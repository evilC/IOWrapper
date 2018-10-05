using System.Collections;
using NUnit.Framework;
using Tests.SubscriptionHandler.Helpers;
using Tests.SubscriptionHandler.Lookups;

namespace Tests.SubscriptionHandler.Callbacks
{
    /// <summary>
    /// Given I make multiple subscription requests to the same input, for different subscribers
    /// When the callback is fired for that input
    /// Then each subscriber receives it's own callback
    /// </summary>
    [TestFixture]
    class MultiSubscriber
    {
        private readonly SubscriptionHelper _subHelper = new SubscriptionHelper();

        public MultiSubscriber()
        {
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Button1));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Button1Subscriber2));
            _subHelper.SubHandler.FireCallbacks(SubReqs.Button1.BindingDescriptor, 100);
        }

        private class TestData : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new TestCaseData(SubReqs.Button1, 100).SetName("Subscriber 1 to Button 1 should receive the callback");
                yield return new TestCaseData(SubReqs.Button1Subscriber2, 100).SetName("Subscriber 2 to Button 1 should receive the callback");
            }
        }

        [TestCaseSource(typeof(TestData))]
        public void DoTests(InputSubReq sr, int value)
        {
            Assert.That(_subHelper.CallbackResults[sr.Name].Value, Is.EqualTo(value), "Value should be correct");
            Assert.That(_subHelper.CallbackResults[sr.Name].BindingDescriptor, Is.EqualTo(sr.BindingDescriptor), "BindingDescriptor should match binding");
        }
    }
}
