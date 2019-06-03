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
        public void LmbPress()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 1
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 0, State = 1 }
            );
        }

        [TestMethod]
        public void LmbRelease()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 2
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 0, State = 0 }
            );
        }

        [TestMethod]
        public void RmbPress()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 4
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 1, State = 1 }
            );
        }

        [TestMethod]
        public void RmbRelease()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 8
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 1, State = 0 }
            );
        }

        [TestMethod]
        public void MmbPress()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 16
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 2, State = 1 }
            );
        }

        [TestMethod]
        public void MmbRelease()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 32
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 2, State = 0 }
            );
        }

        [TestMethod]
        public void Xb1Press()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 64
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 3, State = 1 }
            );
        }

        [TestMethod]
        public void Xb1Release()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 128
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 3, State = 0 }
            );
        }

        [TestMethod]
        public void Xb2Press()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 256
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 4, State = 1 }
            );
        }

        [TestMethod]
        public void Xb2Release()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 512
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 4, State = 0 }
            );
        }

        [TestMethod]
        public void WheelUp()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 1024,
                    rolling = 1
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 5, State = 1 }
            );
        }

        [TestMethod]
        public void WheelDown()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 1024,
                    rolling = -1
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 5, State = -1 }
            );
        }

        [TestMethod]
        public void WheelLeft()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 2048,
                    rolling = -1
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 6, State = -1 }
            );
        }

        [TestMethod]
        public void WheelRight()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 2048,
                    rolling = 1
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 6, State = 1 }
            );
        }

        [TestMethod]
        public void ReleaseLeftAndRightMouse()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 10
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 0, State = 0 },
                new HelperFunctions.ButtonState { Button = 1, State = 0 }
            );
        }

        [TestMethod]
        public void LmbPressAndWheelUp()
        {
            var stroke = new ManagedWrapper.Stroke
            {
                mouse = new ManagedWrapper.MouseStroke
                {
                    state = 1025
                }
            };

            var res = HelperFunctions.StrokeToMouseButtonAndState(stroke);
            res.Should().BeEquivalentTo(
                new HelperFunctions.ButtonState { Button = 0, State = 1 },
                new HelperFunctions.ButtonState { Button = 5, State = 1 }
            );
        }

    }
}
