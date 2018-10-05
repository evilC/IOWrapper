using System.Collections.Generic;
using Hidwizards.IOWrapper.Libraries.EmptyEventDictionary;
using NUnit.Framework;

namespace Tests.EmptyEventDictionary
{
    [TestFixture()]
    public class EmptyEvent
    {
        private List<string> _callbacks;
        private const string EventArgs = "Test";

        [SetUp]
        public void Setup()
        {
            _callbacks = new List<string>();
        }

        [TestCase(TestName = "Empty event does not fire on item removal when items remain")]
        public void TestMethod1()
        {
            var dict = new EmptyEventDictionary<int, int, string>(EventArgs, EmptyEventHandler);
            dict.TryAdd(1, 100);
            dict.TryAdd(2, 200);
            dict.TryRemove(2, out _);
            Assert.That(_callbacks.Count, Is.EqualTo(0));
        }

        [TestCase(TestName = "Empty event fires on item removal when no items remain")]
        public void TestMethod2()
        {
            var dict = new EmptyEventDictionary<int, int, string>(EventArgs, EmptyEventHandler);
            dict.TryAdd(1, 100);
            dict.TryAdd(2, 200);
            dict.TryRemove(2, out _);
            dict.TryRemove(1, out _);
            Assert.That(_callbacks.Count, Is.EqualTo(1));
        }

        [TestCase(TestName = "When the Empty event fires, it provides the correct arguments")]
        public void TestMethod3()
        {
            var dict = new EmptyEventDictionary<int, int, string>(EventArgs, EmptyEventHandler);
            dict.TryAdd(1, 100);
            dict.TryRemove(1, out _);
            Assert.That(_callbacks[0] == EventArgs);
        }

        private void EmptyEventHandler(object sender, string e)
        {
            _callbacks.Add(e);
        }
    }
}
