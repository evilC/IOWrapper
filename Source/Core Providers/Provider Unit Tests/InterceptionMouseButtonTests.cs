using Core_Interception.Helpers;
using Core_Interception.Lib;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Provider_Unit_Tests
{
    [TestClass]
    public class InterceptionMouseButtonTests
    {
        [TestMethod]
        public void BasicMouseStroke()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 1
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(new HelperFunctions.ButtonState { Button = 0, State = 1 });
        }

        [TestMethod]
        public void MultipleMouseButtons ()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 1
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            var bs = new object[2]
            {
                new HelperFunctions.ButtonState { Button = 0, State = 1 },
                new HelperFunctions.ButtonState { Button = 1, State = 1 }
            };
            res.Should().BeEquivalentTo(bs);
        }

    }
}
