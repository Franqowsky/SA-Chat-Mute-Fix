# Changelog

All notable changes to SA Chat Mute Fix will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.4] - 2025-09-13

### Added
- Real-time MySQL database monitoring every 5 seconds
- Support for DATETIME and Unix timestamp formats in database
- Automatic penalty cache cleanup
- Detailed logging of blocked messages
- UserMessage hook for additional protection
- Thread-safe penalty caching system

### Fixed
- MySQL connection string generation from SimpleAdmin config
- Database record parsing for different column types
- NULL value handling for permanent penalties
- Threading issues in database operations

### Changed
- Improved error handling and logging
- Better performance with penalty cache system
- More robust database connection handling

## [1.0.3] - 2025-09-13

### Added
- MySQL integration with SimpleAdmin database
- JSON configuration file support
- Penalty type detection (GAG/MUTE/SILENCE)

### Fixed
- CommandInfo compilation errors
- Missing using statements

## [1.0.2] - 2025-09-13

### Added
- Command listeners for SimpleAdmin integration
- Local penalty tracking system
- Event handler for player chat

### Fixed
- GameEventHandler attribute errors
- Plugin initialization issues

## [1.0.1] - 2025-09-13

### Added
- Basic chat message blocking
- Simple penalty detection

## [1.0.0] - 2025-09-13

### Added
- Initial plugin structure
- CounterStrikeSharp framework integration
- Basic event handling