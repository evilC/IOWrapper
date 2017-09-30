using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Core_TitanOne;
using Providers;

namespace Core_TitanOne.Output
{
    abstract class OutputHandler
    {
        public abstract List<SlotDescriptor> buttons { get; }
        public abstract List<SlotDescriptor> axes { get; }

        public OutputHandler()
        {
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
            else
            {
                return (sbyte)(state * 100);
            }
        }
    }

    class SlotDescriptor
    {
        public int Slot { get; set; }
        public string Name { get; set; }
    }


}
