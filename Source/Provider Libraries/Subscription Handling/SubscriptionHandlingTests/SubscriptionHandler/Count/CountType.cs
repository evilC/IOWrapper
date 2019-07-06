using System.Collections;
using NUnit.Framework;
using SubscriptionHandlingTests.SubscriptionHandler.Helpers;
using SubscriptionHandlingTests.SubscriptionHandler.Lookups;

namespace SubscriptionHandlingTests.SubscriptionHandler.Count
{
    /// <summary>
    /// Given I have a SubscriptionHandler
    /// When I add or remove subscriptions
    /// Then the Count() method should return the correct number of subscribed BindingTypes
    /// </summary>

    [TestFixture]
    public class CountType
    {
        private readonly SubscriptionHelper _subHelper = new SubscriptionHelper();

        private class TestData : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                // Add
                yield return new TestCaseData(SubscriptionType.None, SubReqs.Button1).Returns(0).SetName("Before adding Button 1, there should be 0 Types");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Button1).Returns(1).SetName("After adding Button 1, there should be 1 Type");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Button2).Returns(1).SetName("After adding Button 2, there should be 1 Type");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Button1Subscriber2).Returns(1).SetName("After adding Button 1 Subscriber 2, there should be 1 Type");

                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Axis1).Returns(2).SetName("After adding Axis 1, there should be 2 Types");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Axis2).Returns(2).SetName("After adding Axis 2, there should be 2 Types");

                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov1Up).Returns(3).SetName("After adding POV 1 Up, there should be 3 Types");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov1Right).Returns(3).SetName("After adding POV 1 Right, there should be 3 Types");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov2Up).Returns(3).SetName("After adding POV 2 Up, there should be 3 Types");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov2Right).Returns(3).SetName("After adding POV 2 Right, there should be 3 Types");

                // Remove
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov2Right).Returns(3).SetName("After removing POV 2 Right, there should be 3 Types");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov2Up).Returns(3).SetName("After removing POV 2 Up, there should be 3 Types");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov1Right).Returns(3).SetName("After removing POV 1 Right, there should be 3 Types");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov1Up).Returns(2).SetName("After removing POV 1 Up, there should be 2 Types");

                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Axis2).Returns(2).SetName("After removing Axis 2, there should be 2 Types");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Axis1).Returns(1).SetName("After removing Axis 1, there should be 1 Type");

                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Button2).Returns(1).SetName("After removing Button 2, there should be 1 Type");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Button1Subscriber2).Returns(1).SetName("After removing Button 1 Subscriber 2, there should be 1 Type");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Button1).Returns(0).SetName("After removing Button 1, there should be 0 Types");
            }
        }

        [TestCaseSource(typeof(TestData))]
        public int CountTypesTests(SubscriptionType subscriptionType, InputSubReq sr)
        {
            var subReq = _subHelper.BuildSubReq(sr);
            if (subscriptionType == SubscriptionType.Subscribe)
            {
                _subHelper.SubHandler.Subscribe(subReq);
            }
            else
            {
                _subHelper.SubHandler.Unsubscribe(subReq);
            }

            return _subHelper.SubHandler.Count();
        }
    }
}
