using System;
using System.Collections.Concurrent;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_ViGEm
{
    public partial class Core_ViGEm
    //public class Core_ViGEm
    {
        /// <summary>
        /// Handler for an individual DS4 controller
        /// </summary>
        private class DS4Handler : DeviceHandler
        {
            private DualShock4Report report = new DualShock4Report();

            private static readonly List<DualShock4Axes> axisIndexes = new List<DualShock4Axes>
            {
                DualShock4Axes.LeftThumbX, DualShock4Axes.LeftThumbY, DualShock4Axes.RightThumbX, DualShock4Axes.RightThumbY,
                DualShock4Axes.LeftTrigger, DualShock4Axes.RightTrigger
            };

            protected override List<string> axisNames { get; set; } = new List<string>
            {
                "LX", "LY", "RX", "RY", "LT", "RT"
            };

            private static readonly List<DualShock4Buttons> buttonIndexes = new List<DualShock4Buttons>
            {
                DualShock4Buttons.Cross, DualShock4Buttons.Circle, DualShock4Buttons.Square, DualShock4Buttons.Triangle,
                DualShock4Buttons.ShoulderLeft, DualShock4Buttons.ShoulderRight, DualShock4Buttons.ThumbLeft, DualShock4Buttons.ThumbRight,
                DualShock4Buttons.Share, DualShock4Buttons.Options,
                DualShock4Buttons.TriggerLeft, DualShock4Buttons.TriggerRight
            };

            private static readonly List<DualShock4SpecialButtons> specialButtonIndexes = new List<DualShock4SpecialButtons>
            {
                DualShock4SpecialButtons.Ps, DualShock4SpecialButtons.Touchpad
            };

            protected override List<string> buttonNames { get; set; } = new List<string>
            {
                "Cross", "Circle", "Square", "Triangle", "L1", "R1", "LS", "RS", "Share", "Options", "L2", "R2", "PS", "TouchPad Click"
            };

            private static readonly List<DualShock4DPadValues> povIndexes = new List<DualShock4DPadValues>
            {
                DualShock4DPadValues.North, DualShock4DPadValues.East, DualShock4DPadValues.South, DualShock4DPadValues.West
            };

            private readonly Dictionary<string, int> _povAxisStates = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                {"x", 0 }, {"y", 0}
            };

            private static readonly Dictionary<int, (string Axis, int Direction)> IndexToVector = new Dictionary<int, (string, int)>()
            {
                {0, ("y", -1)},
                {1, ("x", 1)},
                {2, ("y", 1)},
                {3, ("x", -1)}
            };

            private static readonly Dictionary<(int x, int y), DualShock4DPadValues> AxisStatesToDpadValue = new Dictionary<(int x, int y), DualShock4DPadValues>()
            {
                {(0, 0), DualShock4DPadValues.None},
                {(0, -1), DualShock4DPadValues.North},
                {(1, -1), DualShock4DPadValues.Northeast},
                {(1, 0), DualShock4DPadValues.East},
                {(1, 1), DualShock4DPadValues.Southeast},
                {(0, 1), DualShock4DPadValues.South},
                {(-1, 1), DualShock4DPadValues.Southwest},
                {(-1, 0), DualShock4DPadValues.West},
                {(-1, -1), DualShock4DPadValues.Northwest}
            };

            public DS4Handler(DeviceClassDescriptor descriptor, int index) : base(descriptor, index)
            {
            }

            protected override void AcquireTarget()
            {
                target = new DualShock4Controller(client);
                target.Connect();
            }

            protected override void RelinquishTarget()
            {
                target.Disconnect();
                target = null;
            }

            protected override void SetAxisState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                report.SetAxis(axisIndexes[inputId], (byte)((state + 32768) / 256));
                SendReport();
            }

            protected override void SetButtonState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                if (inputId >= buttonIndexes.Count)
                {
                    report.SetSpecialButtonState(specialButtonIndexes[inputId - buttonIndexes.Count], state != 0);
                }
                else
                {
                    report.SetButtonState(buttonIndexes[inputId], state != 0);
                }
                SendReport();
            }

            protected override void SetPovState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                var mapping = IndexToVector[inputId];
                var axisState = _povAxisStates[mapping.Axis];
                var newState = state == 1 ? mapping.Direction : 0;
                if (axisState == newState) return;
                _povAxisStates[mapping.Axis] = newState;

                var buttons = (int)report.Buttons;
                buttons &= ~15; // Clear all the Dpad bits
                
                buttons |= (int)AxisStatesToDpadValue[(_povAxisStates["x"], _povAxisStates["y"])];
                report.Buttons = (ushort) buttons;
                SendReport();
            }

            private void SendReport()
            {
                ((DualShock4Controller)target).SendReport(report);
            }
        }
    }
}
