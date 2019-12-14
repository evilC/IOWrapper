using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.DeviceLibrary
{
    public static class StaticData
    {
        public static readonly List<string> MouseButtonNames = new List<string> { "Left Mouse", "Right Mouse", "Middle Mouse", "Side Button 1", "Side Button 2", "Wheel Up", "Wheel Down", "Wheel Right", "Wheel Left" };

        public static readonly BindingReport[] MouseAxisBindingReports = { new BindingReport
            {
                Title = "X",
                Path = "Delta Axis: X",
                Category = BindingCategory.Delta,
                BindingDescriptor =   new BindingDescriptor
                {
                    Index = 0,
                    Type = BindingType.Axis
                },
                Blockable = false
            },
            new BindingReport
            {
                Title = "Y",
                Path = "Delta Axis: Y",
                Category = BindingCategory.Delta,
                BindingDescriptor = new BindingDescriptor
                {
                    Index = 1,
                    Type = BindingType.Axis
                },
                Blockable = false
            }};

        public static readonly DeviceReportNode MouseAxisList = new DeviceReportNode
        {
            Title = "Axes",
            Bindings = new List<BindingReport> { MouseAxisBindingReports[0], MouseAxisBindingReports[1] }
        };
    }
}
