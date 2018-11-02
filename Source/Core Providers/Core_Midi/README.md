Adds support for MIDI devices  

# Research

## Sample Data

### Behringer Motor 49

#### Keys

Vel is 0..127

Left-most:

```
Channel: 0, Event: 144 (0 NoteOn Ch: 1 C2 Vel:9 Len: 0), Note: 24 (0 NoteOn Ch: 1 C2 Vel:9 Len: 0)
Channel: 0, Event: 128 (0 NoteOff Ch: 1 C2 Vel:127), Note: 24 (0 NoteOff Ch: 1 C2 Vel:127)
```

Right-most:

```
Channel: 0, Event: 144 (0 NoteOn Ch: 1 C6 Vel:67 Len: 0), Note: 72 (0 NoteOn Ch: 1 C6 Vel:67 Len: 0)
Channel: 0, Event: 128 (0 NoteOff Ch: 1 C6 Vel:127), Note: 72 (0 NoteOff Ch: 1 C6 Vel:127)
```

Holding keys:

```
Channel: 0, Event: 144 (0 NoteOn Ch: 1 C2 Vel:10 Len: 0), Note: 24 (0 NoteOn Ch: 1 C2 Vel:10 Len: 0)
Channel: 0, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 0, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 0, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 0, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 0, Event: 208 (0 ChannelAfterTouch Ch: 1)
Channel: 0, Event: 128 (0 NoteOff Ch: 1 C2 Vel:127), Note: 24 (0 NoteOff Ch: 1 C2 Vel:127)
```

#### Pads

Vel is 0..127

P1:

```
Channel: 1, Event: 144 (0 NoteOn Ch: 2 F#5 Vel:67 Len: 0), Note: 66 (0 NoteOn Ch: 2 F#5 Vel:67 Len: 0)
Channel: 1, Event: 128 (0 NoteOff Ch: 2 F#5 Vel:127), Note: 66 (0 NoteOff Ch: 2 F#5 Vel:127)
```

P8:

```
Channel: 1, Event: 144 (0 NoteOn Ch: 2 C#6 Vel:107 Len: 0), Note: 73 (0 NoteOn Ch: 2 C#6 Vel:107 Len: 0)
Channel: 1, Event: 128 (0 NoteOff Ch: 2 C#6 Vel:127), Note: 73 (0 NoteOff Ch: 2 C#6 Vel:127)
```

#### Knobs

0..127

Whilst they endlessly rotate, it does not loop around.

There are LEDs to indicate current level.

E1:

```
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 0)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 1)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 2)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 3)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 4)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 71 Value 5)
```

E8:

```
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 0)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 1)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 2)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 3)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 4)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 78 Value 5)
```

#### Sliders

0..127

F1:

```
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 0)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 1)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 2)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 3)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 21 Value 5)
```

F8:

```
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 0)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 1)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 2)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 4)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 28 Value 5)
```

F9 (Special in some way?)

```
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 0)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 1)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 2)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 3)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 4)
Channel: 1, Event: 176 (0 ControlChange Ch: 2 Controller 53 Value 5)
```

#### Pitch wheels

Pitch Bend (-8192..8191):

```
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8051 (-141))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8073 (-119))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8095 (-97))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8117 (-75))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8138 (-54))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8160 (-32))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8192 (0))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8204 (12))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8226 (34))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8247 (55))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8269 (77))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8291 (99))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8313 (121))
Channel: 0, Event: 224 (0 PitchWheelChange Ch: 1 Pitch 8335 (143))
```

Modulation (0..127):

```
Channel: 0, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 0)
Channel: 0, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 1)
Channel: 0, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 2)
Channel: 0, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 3)
Channel: 0, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 4)
Channel: 0, Event: 176 (0 ControlChange Ch: 1 Controller Modulation Value 5)
```

