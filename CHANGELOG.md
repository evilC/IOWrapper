# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
### Changed 
### Deprecated
### Removed
### Fixed

#0.9.10 - 2018-12-02
### Fixed
- Fix inverted Interception mouse button values

## [0.9.9] - 2018-11-29
### Removed
- Subscription Mode callbacks are no longer fired on their own thread. Doing so breaks some tests and alters behavior.  
Difference between v0.9.9 and v0.9.6 is solely the "Fix Interception Bind Mode reporting for axes" item.

## [0.9.8] - 2018-11-29
### Removed
- Tasks implementation from v0.9.7 removed due to conflicts with UCR. Subscription Mode callbacks are still fired on their own thread

## [0.9.7] - 2018-11-29
### Changed 
- Fix Interception Bind Mode reporting for axes
- Bind Mode now uses Tasks instead of Threadpool
- Subscription callbacks are now fired on their own thread, using Tasks
- InputSubscriptionRequest's Callback is now an `Action<int>` instead of dynamic

## [0.9.6] - 2018-11-24
### Added
- SpaceMouse and MIDI Providers now support Bind Mode
- Dispose fixes for DI and XI Providers

## [0.9.5] - 2018-11-18
### Fixed
- Bind Mode updates now fired on a thread

## [0.9.3] - 2018-11-18
### Added
- Interception Provider now supports Bind Mode
- BindingReport now has a "Path" property that can be used to get the fully qualified name of the input
### Changed 
- BindModeUpdate now contains a BindingReport instead of a BindingDescriptor
- Refactored DeviceHandlers in Provider Libraries
- BindingDescriptor is now a Struct instead of a Class
### Deprecated
### Removed
### Fixed
- SpaceMouse provider crash fix

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