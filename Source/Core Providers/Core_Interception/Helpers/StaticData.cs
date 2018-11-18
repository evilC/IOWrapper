using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Helpers
{
    public static class StaticData
    {
        public static readonly List<string> MouseButtonNames = new List<string> { "Left Mouse", "Right Mouse", "Middle Mouse", "Side Button 1", "Side Button 2", "Wheel Up", "Wheel Down", "Wheel Left", "Wheel Right" };

        public static readonly BindingReport[] MouseAxisBindingReports = { new BindingReport
            {
                Title = "X",
                Category = BindingCategory.Delta,
                BindingDescriptor =   new BindingDescriptor
                {
                    Index = 0,
                    Type = BindingType.Axis
                }
            },
            new BindingReport
            {
                Title = "Y",
                Category = BindingCategory.Delta,
                BindingDescriptor = new BindingDescriptor
                {
                    Index = 1,
                    Type = BindingType.Axis
                }
            }};

        public static readonly DeviceReportNode MouseAxisList = new DeviceReportNode
        {
            Title = "Axes",
            Bindings = new List<BindingReport> { MouseAxisBindingReports[0], MouseAxisBindingReports[1] }
        };
    }
}
