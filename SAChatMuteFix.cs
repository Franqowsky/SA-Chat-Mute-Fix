using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Text.Json;

namespace SAChatMuteFix;

[MinimumApiVersion(80)]
public class SAChatMuteFix : BasePlugin
{
    public override string ModuleName => "SA Chat Mute Fix";
    public override string ModuleVersion => "1.0.4";
    public override string ModuleAuthor => "Franqowsky";  
    public override string ModuleDescription => "Fixes CS2-SimpleAdmin gag/mute bypass with '!' prefix";

    private readonly Dictionary<ulong, DateTime> _penalizedPlayers = new();
    private SimpleAdminConfig? _config;

    public override void Load(bool hotReload)
    {
        // Ładuj konfigurację SimpleAdmin
        LoadSimpleAdminConfig();

        // Rejestruj handler czatu - blokuje WSZYSTKIE wiadomości od ukaranych
        RegisterEventHandler<EventPlayerChat>(OnPlayerChat);
        
        // Hook na user message dla podwójnej ochrony
        HookUserMessage(118, OnChatUserMessage, HookMode.Pre);

        // Timer sprawdzający kary co 5 sekund
        AddTimer(5.0f, CheckPenaltiesInDatabase, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);

        Logger.LogInformation("🚀 SA Chat Mute Fix v1.0.4 loaded with MySQL DATETIME support!");
    }

    private void LoadSimpleAdminConfig()
    {
        try
        {
            string configPath = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", "CS2-SimpleAdmin", "CS2-SimpleAdmin.json");
            
            if (File.Exists(configPath))
            {
                string jsonContent = File.ReadAllText(configPath);
                _config = JsonSerializer.Deserialize<SimpleAdminConfig>(jsonContent);
                Logger.LogInformation("✅ SimpleAdmin config loaded successfully");
            }
            else
            {
                Logger.LogWarning("⚠️ SimpleAdmin config not found, using defaults");
                _config = new SimpleAdminConfig(); // Default config
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("❌ Failed to load SimpleAdmin config: {Error}", ex.Message);
            _config = new SimpleAdminConfig(); // Fallback to defaults
        }
    }

    private HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if (player == null || !player.IsValid) 
            return HookResult.Continue;

        // Sprawdź czy gracz ma aktywną karę
        if (IsPlayerPenalized(player))
        {
            // Blokuj KAŻDĄ wiadomość - także z prefiksem "!" lub "/"
            Logger.LogInformation("🚫 BLOCKED: {PlayerName} (SteamID: {SteamID}) tried to send: '{Message}'", 
                player.PlayerName, player.SteamID, @event.Text);
            return HookResult.Stop;
        }

        return HookResult.Continue;
    }

    private HookResult OnChatUserMessage(UserMessage um)
    {
        try
        {
            var entityIndex = um.ReadInt("entityindex");
            var player = Utilities.GetPlayerFromIndex(entityIndex);
            
            if (player != null && player.IsValid && IsPlayerPenalized(player))
            {
                Logger.LogInformation("🚫 BLOCKED UserMessage from: {PlayerName} (SteamID: {SteamID})", 
                    player.PlayerName, player.SteamID);
                return HookResult.Stop;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("❌ Error in OnChatUserMessage: {Error}", ex.Message);
        }

        return HookResult.Continue;
    }

    private bool IsPlayerPenalized(CCSPlayerController player)
    {
        if (player?.SteamID == null) return false;
        
        ulong steamId = player.SteamID;
        DateTime now = DateTime.UtcNow;

        // Sprawdź cache kar
        if (_penalizedPlayers.ContainsKey(steamId) && _penalizedPlayers[steamId] > now)
        {
            return true;
        }

        return false;
    }

    private void CheckPenaltiesInDatabase()
    {
        if (_config == null) return;

        _ = Task.Run(async () =>
        {
            try
            {
                using var connection = new MySqlConnection(GetConnectionString());
                await connection.OpenAsync();

                // Sprawdź aktywne kary GAG, MUTE, SILENCE - obsługa DATETIME i NULL
                string query = @"
                    SELECT player_steamid, ends, type 
                    FROM sa_mutes 
                    WHERE type IN ('GAG', 'MUTE', 'SILENCE') 
                    AND status = 'ACTIVE' 
                    AND (ends IS NULL OR ends > NOW())";

                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                var newPenalizedPlayers = new Dictionary<ulong, DateTime>();
                int penaltyCount = 0;

                while (await reader.ReadAsync())
                {
                    try
                    {
                        var steamIdStr = reader.GetString("player_steamid");
                        var type = reader.GetString("type");
                        
                        // Obsługa różnych formatów kolumny ends
                        DateTime endTime;
                        if (reader.IsDBNull(reader.GetOrdinal("ends")))
                        {
                            // NULL = permanent
                            endTime = DateTime.MaxValue;
                        }
                        else
                        {
                            // Próbuj przeczytać jako DateTime
                            try
                            {
                                endTime = reader.GetDateTime("ends");
                            }
                            catch
                            {
                                // Jeśli to unix timestamp jako string
                                try
                                {
                                    var timestampStr = reader.GetString("ends");
                                    if (long.TryParse(timestampStr, out long timestamp))
                                    {
                                        endTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                                    }
                                    else
                                    {
                                        endTime = DateTime.MaxValue; // Fallback
                                    }
                                }
                                catch
                                {
                                    endTime = DateTime.MaxValue; // Fallback
                                }
                            }
                        }
                        
                        if (ulong.TryParse(steamIdStr, out ulong steamId))
                        {
                            newPenalizedPlayers[steamId] = endTime;
                            penaltyCount++;

                            // Loguj znalezione kary
                            var playerName = GetPlayerNameBySteamId(steamId);
                            Logger.LogInformation("📋 Found active {Type} for {PlayerName} (SteamID: {SteamID}) until {EndTime}", 
                                type, playerName ?? "Unknown", steamId, 
                                endTime == DateTime.MaxValue ? "PERMANENT" : endTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("❌ Error reading penalty record: {Error}", ex.Message);
                    }
                }

                // Aktualizuj cache kar
                _penalizedPlayers.Clear();
                foreach (var kvp in newPenalizedPlayers)
                {
                    _penalizedPlayers[kvp.Key] = kvp.Value;
                }

                Logger.LogInformation("🔄 Updated penalties cache: {Count} active chat penalties found", penaltyCount);
                
                // Debug: wylistuj wszystkich ukaranych graczy online
                foreach (var player in Utilities.GetPlayers())
                {
                    if (player?.SteamID != null && IsPlayerPenalized(player))
                    {
                        Logger.LogInformation("🔍 PENALIZED PLAYER ONLINE: {PlayerName} (SteamID: {SteamID})", 
                            player.PlayerName, player.SteamID);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("❌ Database check failed: {Error}", ex.Message);
            }
        });
    }

    private string? GetPlayerNameBySteamId(ulong steamId)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player?.SteamID == steamId)
            {
                return player.PlayerName;
            }
        }
        return null;
    }

    private string GetConnectionString()
    {
        if (_config == null)
            return "Server=localhost;Database=simpleadmin;Uid=root;Pwd=;SslMode=none;";

        return $"Server={_config.DatabaseHost};Port={_config.DatabasePort};Database={_config.DatabaseName};Uid={_config.DatabaseUser};Pwd={_config.DatabasePassword};SslMode=none;";
    }

    public class SimpleAdminConfig
    {
        public string DatabaseHost { get; set; } = "localhost";
        public int DatabasePort { get; set; } = 3306;
        public string DatabaseUser { get; set; } = "root"; 
        public string DatabasePassword { get; set; } = "";
        public string DatabaseName { get; set; } = "simpleadmin";
    }
}