# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- Interception Provider now supports Bind Mode
### Changed 
- BindModeUpdate now contains a BindingReport instead of a BindingDescriptor
- Refactored DeviceHandlers in Provider Libraries
### Deprecated
### Removed
### Fixed

## [0.8.8] - 2018-11-04
### Added
- MIDI Provider - Add support for Output (ControlChange only for now)
- Provider Libraries - Split DeviceLibrary into Input/Output variants

## [0.8.7] - 2018-11-04
### Added

- MIDI Provider "Unsigned" values now report as Positive only Signed. UCR currently does not support eg AxisToButton for unsigned, and resolution is only 7-bit, so we lose nothing and gain compatibility

## [0.8.6] - 2018-10-29
### Added

- Add experimental MIDI provider

## [0.8.5] - 2018-10-29
### Added
- Added experimental SpaceMouse provider (Currently only supports SpaceMouse Pro)
### Fixed
- Provider DLL loading improved - PluginLoader no longer loads all DLLs in folder into Container
- Tobii Eye Tracker Provider build events missing causing provider to be absent from builds

## [0.8.4] - 2018-10-14
### Changed
- Internal build, no changes

## [0.8.3] - 2018-10-08
### Fixed
- Do not exit XI poll thread if device disconnected
- XInput LT reporting wrong scale
- Axes not being processed if previous ones were unsubscribed