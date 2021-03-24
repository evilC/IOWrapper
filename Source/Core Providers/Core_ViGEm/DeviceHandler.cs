using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client;
using System;
using System.Collections.Generic;
using Hidwizards.IOWrapper.Libraries.ProviderLogger;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_ViGEm
{
    public partial class Core_ViGEm
    //public class Core_ViGEm
    {
        /// <summary>
        /// Handles a specific (Xbox, PS etc) kind of device
        /// Base class with methods common to all device types
        /// </summary>
        private abstract class DeviceHandler : IDisposable
        {
            public bool IsRequested { get; set; }
            private Dictionary<Guid, OutputSubscriptionRequest> subscriptions = new Dictionary<Guid, OutputSubscriptionRequest>();

            private Logger logger;

            protected DeviceClassDescriptor deviceClassDescriptor;
            protected int deviceId;
            protected bool isAcquired;
            protected IVirtualGamepad target;

            protected abstract List<string> axisNames { get; set; }
            protected static readonly List<BindingCategory> axisCategories = new List<BindingCategory>
            {
                BindingCategory.Signed, BindingCategory.Signed, BindingCategory.Signed, BindingCategory.Signed, BindingCategory.Unsigned, BindingCategory.Unsigned
            };
            protected abstract List<string> buttonNames { get; set; }
            protected static readonly List<string> povDirectionNames = new List<string>
            {
                "Up", "Right", "Down", "Left"
            };

            public DeviceHandler(DeviceClassDescriptor descriptor,int index)
            {
                logger = new Logger(string.Format("Core_ViGEm ({0})", descriptor.classIdentifier));
                deviceId = index;
                deviceClassDescriptor = descriptor;
            }

            protected bool SubscribeFeedback(EventArgs e)
            {
                // TODO: send feedback to main IOWrapper
                return false;
            }

            protected bool SubscribeOutput(OutputSubscriptionRequest subReq)
            {
                return false;
            }

            public bool AddSubscription(OutputSubscriptionRequest subReq)
            {
                subscriptions.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
                logger.Log("Adding subscription to controller # {0}", subReq.DeviceDescriptor.DeviceInstance);
                IsRequested = true;
                SetAcquireState();
                return true;
            }

            public bool RemoveSubscription(OutputSubscriptionRequest subReq)
            {
                if (subscriptions.ContainsKey(subReq.SubscriptionDescriptor.SubscriberGuid))
                {
                    subscriptions.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
                }
                logger.Log("Removing subscription to controller # {0}", subReq.DeviceDescriptor.DeviceInstance);
                IsRequested = HasSubscriptions();
                SetAcquireState();
                return true;
            }

            public bool HasSubscriptions()
            {
                return subscriptions.Count > 0;
            }

            protected void SetAcquireState()
            {
                if (IsRequested && !isAcquired)
                {
                    AcquireTarget();
                    isAcquired = true;
                }
                else if (!IsRequested && isAcquired)
                {
                    RelinquishTarget();
                    isAcquired = false;
                }
            }

            public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
            {
                switch (bindingDescriptor.Type)
                {
                    case BindingType.Axis:
                        SetAxisState(bindingDescriptor, state);
                        break;
                    /*case BindingType.Slider:
                        SetSliderState(bindingDescriptor, state);
                        break;*/
                    case BindingType.Button:
                        SetButtonState(bindingDescriptor, state);
                        break;
                    case BindingType.POV:
                        SetPovState(bindingDescriptor, state);
                        break;
                }
                return false;
            }

            public DeviceReport GetDeviceReport()
            {
                var report = new DeviceReport
                {
                    DeviceName = String.Format("ViGEm {0} Controller {1}", deviceClassDescriptor.classHumanName, (deviceId + 1)),
                    DeviceDescriptor = new DeviceDescriptor
                    {
                        DeviceHandle = deviceClassDescriptor.classIdentifier,
                        DeviceInstance = deviceId
                    }
                };
                report.Nodes.Add(GetAxisReport());
                report.Nodes.Add(GetButtonReport());
                report.Nodes.Add(GetPovReport());
                return report;
            }

            protected DeviceReportNode GetAxisReport()
            {
                var report = new DeviceReportNode
                {
                    Title = "Axes"
                };
                for (int i = 0; i < axisNames.Count; i++)
                {
                    report.Bindings.Add(new BindingReport
                    {
                        Title = axisNames[i],
                        Category = axisCategories[i],
                        BindingDescriptor = new BindingDescriptor
                        {
                            Index = i,
                            Type = BindingType.Axis
                        }
                    });
                }
                return report;
            }

            protected DeviceReportNode GetButtonReport()
            {
                var report = new DeviceReportNode
                {
                    Title = "Buttons"
                };
                for (int i = 0; i < buttonNames.Count; i++)
                {
                    report.Bindings.Add(new BindingReport
                    {
                        Title = buttonNames[i],
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = new BindingDescriptor
                        {
                            Index = i,
                            Type = BindingType.Button
                        }
                    });
                }
                return report;
            }

            protected DeviceReportNode GetPovReport()
            {
                var report = new DeviceReportNode
                {
                    Title = "DPad"
                };
                for (int i = 0; i < povDirectionNames.Count; i++)
                {
                    report.Bindings.Add(new BindingReport
                    {
                        Title = povDirectionNames[i],
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = new BindingDescriptor
                        {
                            Index = i,
                            SubIndex = 0,
                            Type = BindingType.POV
                        }
                    });
                }
                return report;
            }

            protected abstract void FeedbackEventHandler(object sender, EventArgs e);

            protected abstract void AcquireTarget();
            protected abstract void RelinquishTarget();
            protected abstract void SetAxisState(BindingDescriptor bindingDescriptor, int state);
            // protected abstract void SetSliderState(BindingDescriptor bindingDescriptor, int state);
            protected abstract void SetButtonState(BindingDescriptor bindingDescriptor, int state);
            protected abstract void SetPovState(BindingDescriptor bindingDescriptor, int state);

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    target?.Dispose();
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
