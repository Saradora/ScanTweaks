# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

//

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
