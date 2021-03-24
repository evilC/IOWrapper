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
            // private Xbox360Report report = new Xbox360Report();

            private static readonly List<Xbox360Axis> axisIndexes = new List<Xbox360Axis>
            {
                Xbox360Axis.LeftThumbX, Xbox360Axis.LeftThumbY, Xbox360Axis.RightThumbX, Xbox360Axis.RightThumbY
            };

            private static readonly List<Xbox360Slider> sliderIndexes = new List<Xbox360Slider>
            {
                Xbox360Slider.LeftTrigger, Xbox360Slider.RightTrigger
            };

            protected override List<string> axisNames { get; set; } = new List<string>
            {
                "LX", "LY", "RX", "RY", "LT", "RT"
            };

            private static readonly List<Xbox360Button> buttonIndexes = new List<Xbox360Button>
            {
                Xbox360Button.A, Xbox360Button.B, Xbox360Button.X, Xbox360Button.Y,
                Xbox360Button.LeftShoulder, Xbox360Button.RightShoulder, Xbox360Button.LeftThumb, Xbox360Button.RightThumb,
                Xbox360Button. Back, Xbox360Button.Start
            };

            protected override List<string> buttonNames { get; set; } = new List<string>
            {
                "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start"
            };

            private static readonly List<Xbox360Button> povIndexes = new List<Xbox360Button>
            {
                Xbox360Button.Up, Xbox360Button.Right, Xbox360Button.Down, Xbox360Button.Left
            };

            public Xb360Handler(DeviceClassDescriptor descriptor, int index) : base(descriptor, index)
            {

            }

            protected override void AcquireTarget()
            {
                target = _client.CreateXbox360Controller();
                ((IXbox360Controller)target).FeedbackReceived += new Xbox360FeedbackReceivedEventHandler(FeedbackEventHandler);
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
                if (bindingDescriptor.Index > 3)
                {
                    SetSliderState(bindingDescriptor, state);
                } else
                {
                    var outState = (short)state;                            // Other axes are regular shorts
                    ((IXbox360Controller)target).SetAxisValue(axisIndexes[inputId], outState);
                    /*SendReport();*/
                }
            }

            protected void SetSliderState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                //var outState = (short)((state / 256) + 128);              // Xbox Triggers are shorts, but 0..255
                var outState = (byte)(state);              // Xbox Triggers are shorts, but 0..255
                ((IXbox360Controller)target).SetSliderValue(sliderIndexes[inputId], outState);
                /*SendReport();*/
            }

            protected override void SetButtonState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                ((IXbox360Controller)target).SetButtonState(buttonIndexes[inputId], state != 0);
                /*SendReport();*/
            }

            protected override void SetPovState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                ((IXbox360Controller)target).SetButtonState(povIndexes[inputId], state != 0);
                /*SendReport();*/
            }

            protected void FeedbackEventHandler(object sender, Xbox360FeedbackReceivedEventArgs e)
            {
                SubscribeFeedback(e);
            }

            /*private void SendReport()
            {
                ((IXbox360Controller)target).SendReport(report);
            }*/
        }
    }
}
