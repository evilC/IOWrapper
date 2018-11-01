# IOWrapper
**Unless you are a developer, this project is probably of no use to you. End Users should download [UCR](https://github.com/Snoothy/UCR) instead**

## Project Overview

IOWrapper is the "Back End" for [Universal Control Remapper (UCR) 2](https://github.com/Snoothy/UCR) , handling all the device interaction.  
Technically, however, IOWrapper is a stand-alone project which is not inherently coupled to the UCR front-end.  

The purpose of the IOWrapper library is to present a list of devices to the consumer; both input (The user typed a key, moved a joystick etc) and output (Faking user input). The consumer can make "subscriptions" to devices in order to receive input or send output.

## Primer

### APIs

Whilst Windows has standard APIs for I/O - for example DirectInput (Flight sticks, steering wheels etc), XInput (Xbox controllers) and RawInput (Keyboards, mice, joysticks), there are also other devices which require custom APIs (eg vJoy for faking DirectInput joysticks), so IOWrapper needs to be extensible.

### Providers

Support for new forms of I/O can be added to IOWrapper via plugins dubbed "Providers". A Provider typically wraps one API, however this does not need to be the case.

Providers are MEF plugins that expose `IProvider` or one of it's derived interfaces. Each has their own folder, which can contain dependent DLLs etc.

A "Provider Libraries" project is provided, with helper classes etc to simplify the process of writing new Providers - their use is *entirely optional*.

### Normalization

UCR (The front end which sits on top of IOWrapper) is *API Agnostic* - that is to say that it does not care how a device is identified, or in what format data from the device comes in, everything is normalized to a consistent way of reporting.

However, the current system will probably need to be overhauled at some point, as 

#### Devices

The primary way of identifying devices is by a string - the "Handle", which for APIs which is often the USB VID/PID of the device.

Duplicate device disambiguation is handled by the "DeviceInstance", which is a positive integer.
Providers, if possible, should make attempts to make instance ordering consistent, ideally between boots.
ie, as long as you do not plug the devices into different ports, Instance 0 should remain 0, 1 should remain 1 etc.

#### Buttons / Axes etc

Inputs on a device are referred to by Type (Axis, Button or POV), Index and SubIndex.  
SubIndex is optional, and is typically used to denote a derived value (eg an input which represents a direction on a POV hat)

#### Input Values

All input values in IOWrapper are currently normalized to signed 16-bit integers for axes, or 0 (Unpressed) / 1 (Pressed) for buttons. In this way, plugins in the UCR front end are always dealing with a consistent range of values.

### Writing a Provider

So you have some funky new input device, and you want to make it work with UCR?
Here is a handy sequence of steps you can take to incrementally make progress, even with minimal C# knowledge.

#### Starting Out

1. Write some Proof-of-Concept C# code that can read the device
2. Create a new `Class Library`project in IOWrapper.
3. Copy the Pre/Post build events from one of the other providers (eg DirectInput)
4. Decorate your class with `[Export(typeof(IProvider))]`
5. Implement `IInputProvider`
   All of the methods can just be left empty, or just `return null`
6. Reference the needed projects (You will probably only need the provider interface and DTOs)
7. Set a breakpoint in your constructor and hit F5, you should hit your breakpoint.
   For now, the constructor can be used to kick off whatever POC code you desire

#### Normalizing input

Until now, your POC code is probably just logging out raw values etc - you now need to normalize what is coming from the device into a language that IOWrapper understands.

You will need to invent a translation scheme to describe each possible kind of input in terms of a `BindingDescriptor` (`Type`/`Index`/`SubIndex`.)
For example, DirectInput reports all input with an `Offset` (An integer), so the provider uses a look-up dictionary to find the `Type` (Axis/Button/POV) given the `Offset`, and the `Index` property is used to denote which axis or button, so we can just use the raw `Offset` value for that.
DirectInput can have 4 POV hats (which each natively report 0..360), but the front-end allows subscribing to a *direction* of a POV (Up/Down/Left/Right) as if it were a button, so this being a derived value, we use `SubIndex` to encode which of the four directions of the hat that the subscription is for.

The other task here is to normalize the values reported for axes and buttons. Axes should report in the range -32768...32767 and buttons should report 0 for released and 1 for pressed.

#### Setting up the Test App

Edit the console app in the solution that is used to test providers.

There are numerous helper classes and pre-built descriptors in there, but ultimately all you need to do is call `SubscribeInput` on the IOController and pass it a `ProviderDescriptor` that matches your Provider.
The other descriptors that you pass can just be empty for now.

#### Handling Subscriptions

Your next goal is to handle subscription requests.

When a user selects an input in the front end, `SubscribeInput` is called.
It is the Provider's job to fire the contained callback when the input described by the subscription request happens. Conversely, `UnsubscribeInput` should cancel that subscription.
It is also generally desirable to stop any threads (eg poll threads) when there are no subscriptions.

The subscription request will contain a `BindingDescriptor`, which describes the input to be subscribed to, using the translation scheme you came up with in the previous step.
The `Provider Libraries` project contains a useful `SubscriptionHandler` class which can store your subscription requests for you, and will also enable you to determine whether a given button or axis has any subscriptions. This class uses `ConcurrentDictionary` and so is thread-safe, so when your poll loop receives input, you can just look up in the dictionary to tell if that input has any subscriptions or not, and whether to fire the callback.

By this point, you should be able to use the Test App to subscribe to your various inputs, and Unsubscribe.

#### Reporting

Now you can subscribe to stuff, but if you integrated it into UCR right now, it would be useless, as the user would have no way of selecting the input to bind to - the Device Group window would not contain anything, and even if it did, the Input selection control would not contain any axes or buttons for that device.
This is handled via Reports - you need to implement `GetInputList` and `GetInputDeviceReport`. These basically populate the menus in the front end with text, and tell the front end what `BindingDescriptor` to pass to the back end when the user selects that input.

#### Multiple Devices

When starting out, you can hard-wire everything to your test device, but often with a Provider it will need to support many different devices.

In this instance, you will probably want to take the code you have at some point, and move it into a device class containing it's own poll thread and subscription handler.

If the device is identified by USB Vendor Id and Product Id (VID / PID), then it is convention to encode this as the `DeviceHandle` string of the `DeviceDescriptor`. See DirectInput for an example.

If you wish to support multiple identical devices, you need to use the `DeviceInstance ` property of the `DeviceDescriptor`.

#### Tidying Up

In order for the provider to play nice, it **must** properly implement `IDisposable`. When the provider is Disposed, **kill all threads**. If you do not do this, UCR may well hang on exit.

Try to consider performance, especially if working with high frequency data (eg mouse movement).