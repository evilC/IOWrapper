using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_ESP8266
{
    public static class EspUtility
    {

        public static BindingDescriptor GetBindingDescriptor(BindingCategory bindingCategory, int value)
        {
            switch (bindingCategory)
            {
                case BindingCategory.Momentary:
                    return new BindingDescriptor()
                    {
                        Index = value,
                        SubIndex = 0,
                        Type = BindingType.Button
                    };
                case BindingCategory.Event:
                    return new BindingDescriptor()
                    {
                        Index = value,
                        SubIndex = 1,
                        Type = BindingType.Button
                    };
                case BindingCategory.Signed:
                case BindingCategory.Unsigned:
                    return new BindingDescriptor()
                    {
                        Index = value,
                        SubIndex = 0,
                        Type = BindingType.Axis
                    };
                case BindingCategory.Delta:
                    return new BindingDescriptor()
                    {
                        Index = value,
                        SubIndex = 1,
                        Type = BindingType.Axis
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(bindingCategory), bindingCategory, $"Unable to get BindingDescriptor for category: {bindingCategory}");
            }
        }

        public static BindingCategory GetBindingCategory(BindingDescriptor bindingDescriptor)
        {
            switch (bindingDescriptor.Type)
            {
                case BindingType.Button:
                    return bindingDescriptor.SubIndex == 0
                        ? BindingCategory.Momentary
                        : BindingCategory.Event;
                case BindingType.Axis:
                    return bindingDescriptor.SubIndex == 0
                        ? BindingCategory.Signed
                        : BindingCategory.Delta;
                default:
                    throw new ArgumentOutOfRangeException( $"Unable to get BindingCategory for BindingDescriptor");
            }
        }
    }
}
