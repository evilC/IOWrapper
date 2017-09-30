using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_TitanOne.Output
{
    class DS4OutputHandler : OutputHandler
    {
        public override List<SlotDescriptor> buttons { get; } = new List<SlotDescriptor>()
            {
                new SlotDescriptor() { Name = "Cross", Slot = 19 },
                new SlotDescriptor() { Name = "Circle", Slot = 18 },
                new SlotDescriptor() { Name = "Square", Slot = 20 },
                new SlotDescriptor() { Name = "Triangle", Slot = 17 },
                new SlotDescriptor() { Name = "L1", Slot = 6 },
                new SlotDescriptor() { Name = "R1", Slot = 3 },
                new SlotDescriptor() { Name = "L2", Slot = 7 },
                new SlotDescriptor() { Name = "R2", Slot = 4 },
                new SlotDescriptor() { Name = "L3", Slot = 8 },
                new SlotDescriptor() { Name = "R3", Slot = 5 },
                new SlotDescriptor() { Name = "Share", Slot = 1 },
                new SlotDescriptor() { Name = "Options", Slot = 2 },
                new SlotDescriptor() { Name = "PS", Slot = 0 },
            };

        public override List<SlotDescriptor> axes { get; } = new List<SlotDescriptor>()
            {
                new SlotDescriptor() { Name = "LX", Slot = 11 },
                new SlotDescriptor() { Name = "LY", Slot = 12 },
                new SlotDescriptor() { Name = "RX", Slot = 9 },
                new SlotDescriptor() { Name = "RY", Slot = 10 },
                new SlotDescriptor() { Name = "L2", Slot = 7 },
                new SlotDescriptor() { Name = "R2", Slot = 4 },
                new SlotDescriptor() { Name = "ACCX", Slot = 21 },
                new SlotDescriptor() { Name = "ACCY", Slot = 22 },
                new SlotDescriptor() { Name = "ACCZ", Slot = 23 },
                new SlotDescriptor() { Name = "GYROX", Slot = 24 },
                new SlotDescriptor() { Name = "GYROY", Slot = 25 },
                new SlotDescriptor() { Name = "TOUCHX", Slot = 28 },
                new SlotDescriptor() { Name = "TOUCHY", Slot = 29 },
            };
    }
}
