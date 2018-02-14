using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Core_TitanOne;
using HidWizards.IOWrapper.API;

namespace Core_TitanOne.Output
{
    abstract class OutputHandler
    {
        public abstract string Title { get; set; }
        public abstract string Handle { get; set; }
        public abstract List<SlotDescriptor> buttons { get; }
        public abstract List<SlotDescriptor> axes { get; }

        public DeviceReport GetOutputReport()
        {
            var deviceReport = new DeviceReport
            {
                DeviceName = String.Format("T1 {0}", Title),
                DeviceDescriptor = new DeviceDescriptor
                {
                    DeviceHandle = Handle,
                    DeviceInstance = 0
                }
            };
            deviceReport.Nodes = GetReportNodes();
            return deviceReport;
        }

        private List<DeviceReportNode> GetReportNodes()
        {
            var deviceReportNodes = new List<DeviceReportNode>();
            var axesNode = new DeviceReportNode
            {
                Title = "Axes"
            };
            for (int a = 0; a < axes.Count; a++)
            {
                axesNode.Bindings.Add(new BindingReport
                {
                    Title = axes[a].Name,
                    Category = axes[a].Category,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = a,
                        Type = BindingType.Axis
                    }
                });
            }
            deviceReportNodes.Add(axesNode);

            var buttonsNode = new DeviceReportNode
            {
                Title = "Buttons"
            };
            for (int b = 0; b < buttons.Count; b++)
            {
                buttonsNode.Bindings.Add(new BindingReport
                {
                    Title = buttons[b].Name,
                    Category = buttons[b].Category,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = b,
                        Type = BindingType.Button
                    }
                });
            }
            deviceReportNodes.Add(buttonsNode);


            return deviceReportNodes;
        }

        public int? GetSlot(BindingDescriptor bindingDescriptor)
        {
            SlotDescriptor slot = null;
            switch (bindingDescriptor.Type)
            {
                case BindingType.Axis:
                    slot = axes[bindingDescriptor.Index];
                    break;
                case BindingType.Button:
                    slot = buttons[bindingDescriptor.Index];
                    break;
            }
            if (slot != null)
            {
                return slot.Slot;
            }
            return null;
        }

        public static sbyte GetValue(BindingDescriptor bindingDescriptor, int state)
        {
            if (bindingDescriptor.Type == BindingType.Axis)
            {
                return (sbyte)(state / 327.67);
            }

            return (sbyte)(state * 100);
        }
    }

    class SlotDescriptor
    {
        public int Slot { get; set; }
        public string Name { get; set; }
        public BindingCategory Category { get; set; }
    }


}
