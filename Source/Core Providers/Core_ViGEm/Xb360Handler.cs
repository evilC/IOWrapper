using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_ViGEm
{
    public partial class Core_ViGEm
    //public class Core_ViGEm
    {
        /// <summary>
        /// Handler for an individual Xb360 controller
        /// </summary>
        private class Xb360Handler : DeviceHandler
        {
            private Xbox360Report report = new Xbox360Report();

            private static readonly List<Xbox360Axes> axisIndexes = new List<Xbox360Axes>
            {
                Xbox360Axes.LeftThumbX, Xbox360Axes.LeftThumbY, Xbox360Axes.RightThumbX, Xbox360Axes.RightThumbY,
                Xbox360Axes.LeftTrigger, Xbox360Axes.RightTrigger
            };

            protected override List<string> axisNames { get; set; } = new List<string>
            {
                "LX", "LY", "RX", "RY", "LT", "RT"
            };

            private static readonly List<Xbox360Buttons> buttonIndexes = new List<Xbox360Buttons>
            {
                Xbox360Buttons.A, Xbox360Buttons.B, Xbox360Buttons.X, Xbox360Buttons.Y,
                Xbox360Buttons.LeftShoulder, Xbox360Buttons.RightShoulder, Xbox360Buttons.LeftThumb, Xbox360Buttons.RightThumb,
                Xbox360Buttons. Back, Xbox360Buttons.Start
            };

            protected override List<string> buttonNames { get; set; } = new List<string>
            {
                "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start"
            };

            private static readonly List<Xbox360Buttons> povIndexes = new List<Xbox360Buttons>
            {
                Xbox360Buttons.Up, Xbox360Buttons.Right, Xbox360Buttons.Down, Xbox360Buttons.Left
            };

            public Xb360Handler(DeviceClassDescriptor descriptor, int index) : base(descriptor, index)
            {

            }

            protected override void AcquireTarget()
            {
                target = new Xbox360Controller(_client);
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
                var outState = bindingDescriptor.Index > 3      // If Index is 4 or 5 (Triggers)...
                    ? (short)((state / 256) + 128)              // Xbox Triggers are shorts, but 0..255
                    : (short) state;                            // Other axes are regular shorts
                report.SetAxis(axisIndexes[inputId], outState);
                SendReport();
            }

            protected override void SetButtonState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                report.SetButtonState(buttonIndexes[inputId], state != 0);
                SendReport();
            }

            protected override void SetPovState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                report.SetButtonState(povIndexes[inputId], state != 0);
                SendReport();
            }

            private void SendReport()
            {
                ((Xbox360Controller)target).SendReport(report);
            }
        }
    }
}
