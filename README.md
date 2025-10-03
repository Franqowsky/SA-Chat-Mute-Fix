NEW-->https://github.com/Franqowsky/SA-Chat-Mute-Fix-v2

# SA Chat Mute Fix

A Counter-Strike 2 plugin that fixes the "!" and "/" prefix bypass in CS2-SimpleAdmin's GAG/MUTE/SILENCE punishments.

## 🚫 Problem

CS2-SimpleAdmin allows gagged players to bypass chat restrictions by using "!" or "/" prefixes:
- Player gets gagged with `css_gag player 60 reason`
- Player can still send messages like `!admin`, `/help`, etc.
- Other players see these messages, making the punishment ineffective

## ✅ Solution

This plugin blocks **ALL** chat messages from penalized players, including:
- Messages with "!" prefix (`!admin`, `!help`)
- Messages with "/" prefix (`/admin`, `/help`) 
- Regular chat messages
- Team chat messages

## 📋 Features

- **Real-time MySQL monitoring** - Checks CS2-SimpleAdmin database every 5 seconds
- **Complete chat blocking** - No bypass possible with any prefix
- **Multiple punishment types** - Works with GAG, MUTE, and SILENCE
- **Automatic cleanup** - Expired penalties are removed from cache
- **Detailed logging** - Shows exactly what messages are blocked
- **Zero configuration** - Uses SimpleAdmin's existing database settings

## 🔧 Requirements

- **CounterStrikeSharp**https://github.com/roflmuffin/CounterStrikeSharp
 (latest version)
- **CS2-SimpleAdmin**https://github.com/daffyyyy/CS2-SimpleAdmin
 plugin installed and configured
- **MySQL/MariaDB** database (same as SimpleAdmin uses)

## 📦 Installation

1. **Download the plugin**
   ```bash
   # Download from releases or compile from source
   ```

2. **Install to server**
   ```bash
   # Copy to your CS2 server
   /game/csgo/addons/counterstrikesharp/plugins/SAChatMuteFix/
   ├── SAChatMuteFix.dll
   ├── MySqlConnector.dll  # Important: Include MySQL dependency
   └── [other dependencies]
   ```

3. **Restart server**
   ```bash
   # Restart your CS2 server
   ```

4. **Verify installation**
   ```
   css_plugins list
   # Should show: "SA Chat Mute Fix" (1.0.4) by Franqowsky
   ```

## 🎯 Usage

The plugin works automatically once installed:

1. **Admin issues punishment**:
   ```
   css_gag PlayerName 60 Spamming
   css_mute PlayerName 30 Toxic behavior  
   css_silence PlayerName 120 Breaking rules
   ```

2. **Plugin detects and blocks**:
   - Monitors database for active penalties
   - Blocks all chat messages from penalized players
   - Logs blocked messages for transparency

3. **Penalty expires**:
   - Plugin automatically detects expiration
   - Player can chat normally again
   - Cache is cleaned up

## 📊 Console Output

**Plugin loaded successfully**:
```
[INFO] 🚀 SA Chat Mute Fix v1.0.4 loaded with MySQL DATETIME support!
```

**Penalty detected**:
```
[INFO] 📋 Found active GAG for PlayerName (SteamID: 76561198...) until 2025-09-13 18:30:00
[INFO] 🔄 Updated penalties cache: 1 active chat penalties found
```

**Message blocked**:
```
[INFO] 🚫 BLOCKED: PlayerName (SteamID: 76561198...) tried to send: '!admin'
[INFO] 🚫 BLOCKED UserMessage from: PlayerName (SteamID: 76561198...)
```

## ⚙️ Configuration

No configuration needed! The plugin automatically:
- Reads CS2-SimpleAdmin's database configuration
- Connects to the same MySQL database
- Uses the same `sa_mutes` table structure

## 🐛 Troubleshooting

**Plugin not loading**:
- Ensure CounterStrikeSharp is properly installed
- Check that MySqlConnector.dll is present
- Verify CS2-SimpleAdmin is working

**Database connection issues**:
- Ensure CS2-SimpleAdmin database is accessible
- Check MySQL credentials and permissions
- Verify `sa_mutes` table exists

**Messages still showing in logs**:
- This is normal - server logs all attempted messages
- Plugin blocks them from reaching other players
- Check for `🚫 BLOCKED` entries to confirm blocking

## 🔧 Technical Details

**Architecture**:
- Written in C# using CounterStrikeSharp API
- Uses MySQL async queries for database monitoring
- Implements both EventPlayerChat and UserMessage hooks
- Thread-safe penalty caching system

**Database Integration**:
- Queries `sa_mutes` table every 5 seconds
- Supports both DATETIME and Unix timestamp formats
- Handles NULL values for permanent penalties
- Compatible with SimpleAdmin 1.7.7+

**Performance**:
- Lightweight: <1ms processing per message
- Efficient: Only queries active penalties
- Scalable: Handles hundreds of players

## 🤝 Contributing

1. **Fork the repository**
2. **Create feature branch** (`git checkout -b feature/improvement`)
3. **Commit changes** (`git commit -am 'Add improvement'`)
4. **Push to branch** (`git push origin feature/improvement`)
5. **Create Pull Request**

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Credits

- **Author**: [Franqowsky](https://github.com/Franqowsky)
- **Inspiration**: CS2-SimpleAdmin team for the excellent admin plugin
- **Framework**: CounterStrikeSharp for the powerful C# API

## 🆘 Support

- **Issues**: [GitHub Issues](https://github.com/Franqowsky/SA-Chat-Mute-Fix/issues)
- **Discord**: [Our Discord Server](https://discord.gg/77Hmw23YfW)
- **Steam**: 

---

### 🌟 Star this repository if it helped fix your server's chat bypass issues!
