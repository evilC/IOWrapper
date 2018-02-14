using IProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_TitanOne.Output
{
    class Xb360OutputHandler : OutputHandler
    {
        public override string Title { get; set; } = "Xb360";
        public override string Handle { get; set; } = "xb360";

        public override List<SlotDescriptor> buttons { get; } = new List<SlotDescriptor>()
            {
                new SlotDescriptor() { Name = "A", Slot = 19, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "B", Slot = 18, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "X", Slot = 20, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "Y", Slot = 17, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "LB", Slot = 6, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "RB", Slot = 3, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "LS", Slot = 8, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "RS", Slot = 5, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "Back", Slot = 1, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "Start", Slot = 2, Category = BindingCategory.Momentary },
                new SlotDescriptor() { Name = "Xbox", Slot = 0, Category = BindingCategory.Momentary },
            };

        public override List<SlotDescriptor> axes { get; } = new List<SlotDescriptor>()
            {
                new SlotDescriptor() { Name = "LX", Slot = 11, Category = BindingCategory.Signed },
                new SlotDescriptor() { Name = "LY", Slot = 12, Category = BindingCategory.Signed },
                new SlotDescriptor() { Name = "RX", Slot = 9, Category = BindingCategory.Signed },
                new SlotDescriptor() { Name = "RY", Slot = 10, Category = BindingCategory.Signed },
                new SlotDescriptor() { Name = "LT", Slot = 7, Category = BindingCategory.Unsigned },
                new SlotDescriptor() { Name = "RT", Slot = 4, Category = BindingCategory.Unsigned },
            };
    }
}
