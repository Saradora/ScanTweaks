# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

## [1.7.0] - 2024-07-25

### Added
- Config toggle to allow player held items to be scanned

## [1.6.1] - 2024-07-08

### Fixed
- Tools without a battery should be scannable again (shovels, etc..)

## [1.6.0] - 2024-06-28

### Changed
- Allowed trigger colliders to be hit, hopefully improving compatibility with some mods

### Fixed
- Added a few different checks to avoid allocating each frame for tool nodes

## [1.5.3] - 2024-05-06

### Added
- Fix for scrap in ship not being counted when joining as client

### Changed
- Improved Apparatus patching code for better compatibility

## [1.5.2] - 2024-04-30

### Fixed
- Apparatus patch screwing the total scrap count in the end of moon result panel.
- Bodies creating a new scan node when they shouldn't.

## [1.5.1] - 2024-04-12

### Added
- Config entry for whether door should block scan vision or not

### Fixed
- Not being able to scan items on the company desk (auto fixed, was a compilation error?)

## [1.5.0] - 2024-04-07

### Added
- Battery charge on scan node of items with a battery
- Config entry for scrap counter speed
- Scrap counter speed is now relative to the amount of scrap, no more waiting ages for the counter to update when there is a lot of scrap

### Changed
- Default scan speed is faster

### Fixed
- Some performance improvements
- Fixed small scan node UI latency

## [1.4.0] - 2024-04-06

### Added
- Added Scan node for all items that didn't previously have one (can be disabled in config)
- Config to toggle scrap visibility inside the ship on the monitor radar

### Changed
- Breaker Box config should now reflect changes immediately

### Fixed
- Dead bodies shouldn't appear as scrap anymore in the monitor radar

## [1.3.0] - 2024-04-01

### Added
- Added an offset to item detection for better scanning of objects close to the ground
- Made the scan progress over time
- Made the scan pick up even more items at the same time

### Changed
- Improved UI code, scan nodes should now always appear correctly

## [1.2.2] - 2024-03-09

Re-released because the build seemed corrupted

## [1.2.1] - 2024-03-02

### Fixed
- Fixed a bug if two scanned items were at the exact same distance from the player
- Fixed a bug where the radar icons kept sending error messages to the console.

## [1.2.0] - 2024-01-24

### Added
- ConfigEntry for the radar patch

### Changed
- Reworked input listening for improved compatibility with other scan mods

## [1.1.0] - 2024-01-22

### Added
- The Apparatus now has a random value ranging 50-140 and is immediately scannable (Host-Side)
- Config data for ping scan, breaker box and apparatice changes

### Changed
- Updated to 1.0.0 of UnityMDK (which fixed the breaker box not being scannable sometimes)

### Fixed
- Fixed being unable to scan items on the company desk
- Fixed a pool not being correctly cleared
- Added a few safeguards to avoid nullrefs

## [1.0.0] - 2024-01-20

### Added
- Made the ping scan more consistent, allows to scan more than 13 nodes at the same time.
- Made the item radar icons disappear when the player gets eaten.
- Added a scan node on the breaker box
