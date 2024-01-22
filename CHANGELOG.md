# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

//

## [1.1.0] - 2024-01-22

### Changed
- Updated to 1.0.0 of UnityMDK (which fixed the breaker box not being scannable sometimes)

### Fixed
- Fixed a pool not being correctly cleared
- Added a few safeguards to avoid nullrefs

## [1.0.0] - 2024-01-20

### Added
- Made the ping scan more consistent, allows to scan more than 13 nodes at the same time.
- Made the item radar icons disappear when the player gets eaten.
- Added a scan node on the breaker box