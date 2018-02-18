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
                if (state == 0)
                    report.SetDPad(DualShock4DPadValues.None);
                else
                    report.SetDPad(povIndexes[inputId]);
                SendReport();
            }

            private void SendReport()
            {
                ((DualShock4Controller)target).SendReport(report);
            }
        }
    }
}
