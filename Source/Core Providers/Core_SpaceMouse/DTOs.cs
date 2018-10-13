using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_SpaceMouse
{
    public struct SpaceMouseUpdate
    {
        public BindingType BindingType { get; set; }
        public int Index { get; set; }
        public int Value { get; set; }
    }
}
