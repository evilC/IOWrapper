using System.Collections;
using NUnit.Framework;
using Tests.SubscriptionHandler.Helpers;
using Tests.SubscriptionHandler.Lookups;

namespace Tests.SubscriptionHandler.Callbacks
{
    class MultiSubscription
    {
        /// <summary>
        /// Given I make multiple subscriptions to the same BindingType (Button)
        /// When the callback is fired for each button
        /// Then only the subscriber for that button should get a callback
        /// </summary>
        [TestFixture]
        class SameType
        {
            private readonly SubscriptionHelper _subHelper = new SubscriptionHelper();

            public SameType()
            {
                _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Button1));
                _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Button2));
            }

            private class TestData : IEnumerable
            {
                public IEnumerator GetEnumerator()
                {
                    yield return new TestCaseData(SubReqs.Button1, 100, 1).SetName("Subscriber to Button 1 should receive it's callback");
                    yield return new TestCaseData(SubReqs.Button2, 200, 2).SetName("Subscriber to Button 2 should receive it's callback");
                }
            }


            [TestCaseSource(typeof(TestData))]
            public void DoTests(InputSubReq sr, short value, int expectedCount)
            {
                _subHelper.SubHandler.FireCallbacks(sr.BindingDescriptor, value);
                Assert.That(_subHelper.CallbackResults[sr.Name].Value, Is.EqualTo(value), "Value should be correct");
                Assert.That(_subHelper.CallbackResults[sr.Name].BindingDescriptor, Is.EqualTo(sr.BindingDescriptor), "BindingDescriptor should match binding");
                Assert.That(_subHelper.CallbackResults.Count, Is.EqualTo(expectedCount), "Number of callbacks should be correct");
            }

        }

        /// <summary>
        /// Given I subscribe once to each type of input, but with same index, subindex etc
        /// When I fire the callback for each input
        /// Then only the callack for the appropriate type fires
        /// </summary>
        [TestFixture]
        class DifferentType
        {
            private readonly SubscriptionHelper _subHelper = new SubscriptionHelper();

            public DifferentType()
            {
                _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Button1));
                _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Axis1));
                _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Pov1Up));
            }

            private class TestData : IEnumerable
            {
                public IEnumerator GetEnumerator()
                {
                    yield return new TestCaseData(SubReqs.Button1, 100, 1).SetName("Subscriber to Button 1 should receive it's callback");
                    yield return new TestCaseData(SubReqs.Axis1, 200, 2).SetName("Subscriber to Axis 1 should receive it's callback");
                    yield return new TestCaseData(SubReqs.Pov1Up, 300, 3).SetName("Subscriber to POV 1 Up should receive it's callback");
                }
            }


            [TestCaseSource(typeof(TestData))]
            public void DoTests(InputSubReq sr, short value, int expectedCount)
            {
                _subHelper.SubHandler.FireCallbacks(sr.BindingDescriptor, value);
                Assert.That(_subHelper.CallbackResults[sr.Name].Value, Is.EqualTo(value), "Value should be correct");
                Assert.That(_subHelper.CallbackResults[sr.Name].BindingDescriptor, Is.EqualTo(sr.BindingDescriptor), "BindingDescriptor should match binding");
                Assert.That(_subHelper.CallbackResults.Count, Is.EqualTo(expectedCount), "Number of callbacks should be correct");
            }

        }
    }
}