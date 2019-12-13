# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
### Changed 
### Deprecated
### Removed
### Fixed

## 0.10.11 - 2019-12-13
### Changed 
- [Tobii Provider] IsLive now reflects state of driver
- [Tobii Provider] Only show Tobii devices in ProviderReport that are currently connected

## 0.10.8 - 2019-12-13
### Changed 
- [XInput Provider] Only show Xinput devices in ProviderReport that are currently connected
- [DS4WindowsApi Provider] Only show DS4 devices in ProviderReport that are currently connected
- [SpaceMouse Provider] Only show SpaceMouse devices in ProviderReport that are currently connected

## 0.10.7 - 2019-12-12
### Changed 
- [Interception Provider] Blockable property of BindingDescriptor now indicates if input is blockable or not  
This is controlled by whether BlockingEnabled in the settings file is true or not
### Removed
- [Interception Provider] BlockingControlledByUi setting removed

## 0.10.6 - 2019-08-10
### Fixed
- Fix for IOWrapper Issue #33 / UCR issue #98  
DirectInput should no longer freeze when PollThreads are started or stopped

## 0.10.5 - 2019-06-24
### Changed 
- [Interception Provider] Polling now uses Multimedia Timer, allowing poll rates down to 1ms. Controlled via Settings.xml
- [Interception Provider] (Dev Option) Allowing selecting block via UI is controlled via Settings.xml
- [Interception Provider] (Tester Option) Allowing selecting threaded or non-threaded pass-through of stroke is controlled via Settings.xml
- [Interception Provider] Keyboard now supports per-mapping blocking (Block one button, but not another)

## 0.10.4 - 2019-06-16 (PRE-RELEASE)
### Changed 
- [DirectInput Provider] Fix #31 - Axis values are no longer inverted
- [Interception Provider] Mouse now supports per-mapping blocking (Block one button, but not another)

## 0.10.3 - 2019-06-08
### Changed 
- [ViGEm Provider] Rename DS4 LT / RT to L2 (LT) and R2 (RT)
### Fixed
- [Interception Provider] Fix #30  
When multiple mouse buttons changed state in one update of the mouse, only one event would be fired for one of the buttons

## 0.10.2 - 2019-03-13
### Added
- [DirectInput Provider] Duplicate devices now have #2, #3 etc after their name
### Fixed
- If a provider crashes on load, it no longer stops IOWrapper from loading
- [Interception Provider] Windows keys are now mappable. Previously, if the non-extended scancode did not have a key name, the extended version of the scancode was not checked
- [Interception Provider] F13-F24 are now mappable.
- [Interception Provider] Pause is now mappable.

## 0.10.1 - 2019-01-27
### Changed 
- [MIDI Provider] Note path shortened, now selected note displays better in UI
- [MIDI Provider] CC now uses the full -32768..32767 range
### Fixed
- [MIDI Provider] Note naming fixed. Now starts at Octave -2, and goes up to Octave 8, ending at G8
- [MIDI Provider] Pitch Wheel now works in Bind Mode
- [MIDI Provider] Fix notes, CCs etc only reaching 32766 instead of 32767
- [MIDI Provider] ProcessUpdate no longer crashes if preProcessedUpdates is null
- [Interception Provider] Left/Right Mouse Wheel labels are no longer switched

## 0.10.0 - 2019-01-03
### Changed 
- Subscription and Bind Mode callbacks are now executed as Tasks and are an Action<short> rather than dynamic
- Default blocking to true while UCR GUI does  not support selecting block

## 0.9.12 - 2018-12-06
### Added
- Added Block property to InputSubscriptionRequest
- Interception now blocks inputs which have the block property set to true
- Added Blockable property to BindingReport to indicate that the block property is supported for this input

## 0.9.11 - 2018-12-03
### Fixed
- Fixed XInput additional attempt to dispose empty device
- Made SetDetectionMode and Subscribe / Unsubscribe calls thread-safe

## 0.9.10 - 2018-12-02
### Added
- DeviceReport now has optional HidPath parameter
- Added GetInputDeviceReport
- GetInputDeviceReport / GetOutputDeviceReport now take a DeviceDescriptor and BindingDescriptor
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
