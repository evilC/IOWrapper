using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX_DirectInput;

namespace Provider_Unit_Tests
{
    [TestClass]
    public class UnsignedToSigned
    {
        private DiAxisProcessor _axisProcessor = new DiAxisProcessor();

        [TestMethod]
        public void LowBound()
        {
            var inValue = ushort.MinValue;
            var outValue = _axisProcessor.UnsignedToSigned(inValue);
            outValue.Should().Be(-32768);
        }

        [TestMethod]
        public void HighBound()
        {
            var inValue = ushort.MaxValue;
            var outValue = _axisProcessor.UnsignedToSigned(inValue);
            outValue.Should().Be(32767);
        }

        [TestMethod]
        public void MidPoint()
        {
            var inValue = 32767;
            var outValue = _axisProcessor.UnsignedToSigned(inValue);
            outValue.Should().Be(0);
        }
    }
}
