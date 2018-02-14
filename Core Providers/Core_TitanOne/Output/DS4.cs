using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.DataObjects;

namespace Core_TitanOne.Output
{
    class DS4OutputHandler : OutputHandler
    {
        public override string Title { get; set; } = "PS4";
        public override string Handle { get; set; } = "ds4";

        public override List<SlotDescriptor> buttons { get; } = new List<SlotDescriptor>
        {
                new SlotDescriptor { Name = "Cross", Slot = 19, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "Circle", Slot = 18, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "Square", Slot = 20, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "Triangle", Slot = 17, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "L1", Slot = 6, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "R1", Slot = 3, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "L2", Slot = 7, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "R2", Slot = 4, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "L3", Slot = 8, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "R3", Slot = 5, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "Share", Slot = 1, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "Options", Slot = 2, Category = BindingCategory.Momentary },
                new SlotDescriptor { Name = "PS", Slot = 0, Category = BindingCategory.Momentary }
            };

        public override List<SlotDescriptor> axes { get; } = new List<SlotDescriptor>
        {
                new SlotDescriptor { Name = "LX", Slot = 11, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "LY", Slot = 12, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "RX", Slot = 9, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "RY", Slot = 10, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "L2", Slot = 7, Category = BindingCategory.Unsigned },
                new SlotDescriptor { Name = "R2", Slot = 4, Category = BindingCategory.Unsigned },
                new SlotDescriptor { Name = "ACCX", Slot = 21, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "ACCY", Slot = 22, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "ACCZ", Slot = 23, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "GYROX", Slot = 24, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "GYROY", Slot = 25, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "TOUCHX", Slot = 28, Category = BindingCategory.Signed },
                new SlotDescriptor { Name = "TOUCHY", Slot = 29, Category = BindingCategory.Signed }
            };
    }
}
