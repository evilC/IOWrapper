Adds support for MIDI devices  

# Research

## Useful Links

https://www.midi.org/specifications-old/category/reference-tables

http://www.personal.kent.edu/~sbirch/Music_Production/MP-II/MIDI/midi_protocol.htm

## Encoding Format

### Representing MIDI messages as BindingDescriptors

The challenge is to represent all the MIDI events we are interested in as a `BindingDescriptor`.

```
public class BindingDescriptor
{
    /// <summary>
    /// The Type of the Binding - ie Button / Axis / POV
    /// </summary>
    public BindingType Type { get; set; }

    /// <summary>
    /// The Type-specific Index of the Binding
    /// This is often a Sparse Index (it may often be a BitMask value) ...
    /// ... as it is often refers to an enum value in a Device Report
    /// </summary>
    public int Index { get; set; } = 0;

    /// <summary>
    /// The Type-specific SubIndex of the Binding
    /// This is typically unused, but if used generally represents a derived or optional value
    /// For example:
    ///     With each POV reporting natively as an Angle (Like an Axis)
    ///     But in IOWrapper, bindings are to a *Direction* of a POV (As if it were a button)
    ///     So we need to specify the angle of that direction in SubIndex...
    ///     ... as well as the POV# in Index. Directinput supports 4 POVs
    /// </summary>
    public int SubIndex { get; set; } = 0;
}
```

#### MIDI format

Most data in MIDI is analog, so `Type` will always be `Axis`.

This leaves us with `Index` and `SubIndex` (Two integers) to represent all the bindings we are interested in.

In MIDI, for the items we are interested in, there are 3 pieces of data which identify a specific input:

##### Channel

Each MIDI device can send messages on a number of channels.

Mask: 0xF (0..16)

##### CommandCode

Identifies the type of event that happened. 

Mask: 0xF0 (128..255)

##### Note / Controller Number etc

For Notes, you get a Note Number, for ControlChange (eg faders), you get a controller number.

In either case, this is a number in the range 0..127

#### Mapping Strategy

`Index` maps to a combination of Channel and CommandCode

Note that CommandCodes NoteOn and NoteOff are two separate events, so we cannot use a direct mapping of command code to BindingDescriptor. Probably simplest is to use the `On` code to represent both on and off CommandCodes

`SubIndex` maps to Note / Controller Number





## Sample Data

### Behringer Motor 49

#### Keys

Vel is 0..127

Left-most:

```
Channel: 1, Event: 144 (0 NoteOn Ch: 1 C2 Vel:9 Len: 0), Note: 24 (0 NoteOn Ch: 1 C2 Vel:9 Len: 0)
Channel: 1, Event: 128 (0 NoteOff Ch: 1 C2 Vel:127), Note: 24 (0 NoteOff Ch: 1 C2 Vel:127)
```

Right-most:

```
Channel: 1, Event: 144 (0 NoteOn Ch: 1 C6 Vel:67 Len: 0), Note: 72 (0 NoteOn Ch: 1 C6 Vel:67 Len: 0)
Channel: 1, Event: 128 (0 NoteOff Ch: 1 C6 Vel:127), Note: 72 (0 NoteOff Ch: 1 C6 Vel:127)
```

Holding keys:

```
Channel: 1, Event: 144 (0 NoteOn Ch: 1 C2 Vel:10 Len: 0), Note: 24 (0 NoteOn Ch: 1 C2 Vel:10 Len: 0)
Channel: 1, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 1, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 1, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 1, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 1, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 1, Event: 128 (0 NoteOff Ch: 1 C2 Vel:127), Note: 24 (0 NoteOff Ch: 1 C2 Vel:127)
```

#### Pads

Vel is 0..127

P1:

```
Channel: 2, Event: 144 (0 NoteOn Ch: 2 F#5 Vel:67 Len: 0), Note: 66 (0 NoteOn Ch: 2 F#5 Vel:67 Len: 0)
Channel: 2, Event: 128 (0 NoteOff Ch: 2 F#5 Vel:127), Note: 66 (0 NoteOff Ch: 2 F#5 Vel:127)
```

P8:

```
Channel: 2, Event: 144 (0 NoteOn Ch: 2 C#6 Vel:107 Len: 0), Note: 73 (0 NoteOn Ch: 2 C#6 Vel:107 Len: 0)
Channel: 2, Event: 128 (0 NoteOff Ch: 2 C#6 Vel:127), Note: 73 (0 NoteOff Ch: 2 C#6 Vel:127)
```

#### Knobs

0..127

Whilst they endlessly rotate, it does not loop around.

There are LEDs to indicate current level.

E1:

```
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 0)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 1)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 2)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 3)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 4)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 5)
```

E8:

```
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 0)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 1)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 2)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 3)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 4)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 5)
```

#### Sliders

0..127

F1:

```
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 0)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 1)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 2)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 3)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 5)
```

F8:

```
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 0)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 1)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 2)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 4)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 5)
```

F9 (Special in some way?)

```
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 0)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 1)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 2)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 3)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 4)
Channel: 2, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 5)
```

#### Pitch wheels

Pitch Bend (-8192..8191):

```
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8051 (-141))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8073 (-119))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8095 (-97))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8117 (-75))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8138 (-54))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8160 (-32))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8192 (0))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8204 (12))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8226 (34))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8247 (55))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8269 (77))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8291 (99))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8313 (121))
Channel: 1, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8335 (143))
```

Modulation (0..127):

```
Channel: 1, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 0)
Channel: 1, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 1)
Channel: 1, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 2)
Channel: 1, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 3)
Channel: 1, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 4)
Channel: 1, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 5)
```

