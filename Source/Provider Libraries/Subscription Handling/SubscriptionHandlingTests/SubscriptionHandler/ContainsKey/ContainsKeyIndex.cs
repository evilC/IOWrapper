using System.Collections;
using NUnit.Framework;
using SubscriptionHandlingTests.SubscriptionHandler.Helpers;
using SubscriptionHandlingTests.SubscriptionHandler.Lookups;

namespace SubscriptionHandlingTests.SubscriptionHandler.ContainsKey
{
    /// <summary>
    /// Given I have a SubscriptionHandler
    /// When I add or remove subscriptions
    /// Then the Count(BindingType, Index) method should indicate if there are any subscribed SubIndexes
    /// </summary>
    [TestFixture]
    public class ContainsKeyIndex
    {
        private readonly SubscriptionHelper _subHelper = new SubscriptionHelper();

        private class TestData : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                // Add
                yield return new TestCaseData(SubscriptionType.None, SubReqs.Button1).Returns(false).SetName("Before adding Button 1, there should not be Button1 subscriptions");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Button1).Returns(true).SetName("After adding Button 1, there should be Button1 subscriptions");
                yield return new TestCaseData(SubscriptionType.None, SubReqs.Button2).Returns(false).SetName("Before adding Button 2, there should not be Button2 subscriptions");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Button2).Returns(true).SetName("After adding Button 2, there should be Button2 subscriptions");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Button1Subscriber2).Returns(true).SetName("After adding Button 1 Subscriber 2, there should be Button1 subscriptions");

                yield return new TestCaseData(SubscriptionType.None, SubReqs.Axis1).Returns(false).SetName("Before adding Axis 1, there should not be Axis1 subscriptions");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Axis1).Returns(true).SetName("After adding Axis 1, there should be Axis1 subscriptions");
                yield return new TestCaseData(SubscriptionType.None, SubReqs.Axis2).Returns(false).SetName("Before adding Axis 2, there should not be Axis2 subscriptions");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Axis2).Returns(true).SetName("After adding Axis 2, there should be Axis2 subscriptions");

                yield return new TestCaseData(SubscriptionType.None, SubReqs.Pov1Up).Returns(false).SetName("Before adding POV 1 Up, there should not be POV1 subscriptions");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov1Up).Returns(true).SetName("After adding POV 1 Up, there should be POV1 subscriptions");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov1Right).Returns(true).SetName("After adding POV 1 Right, there should be POV1 subscriptions");
                yield return new TestCaseData(SubscriptionType.None, SubReqs.Pov2Up).Returns(false).SetName("Before adding POV 2 Up, there should not be POV2 subscriptions");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov2Up).Returns(true).SetName("After adding POV 2 Up, there should be POV2 subscriptions");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov2Right).Returns(true).SetName("After adding POV 2 Right, there should be POV2 subscriptions");

                // Remove
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov2Right).Returns(true).SetName("After removing POV 2 Right, there should be POV2 subscriptions");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov2Up).Returns(false).SetName("After removing POV 2 Up, there should not be POV2 subscriptions");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov1Right).Returns(true).SetName("After removing POV 1 Right, there should be POV1 subscriptions");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov1Up).Returns(false).SetName("After removing POV 1 Up, there should not be POV1 subscriptions");

                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Axis2).Returns(false).SetName("After removing Axis 2, there should not be Axis2 subscriptions");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Axis1).Returns(false).SetName("After removing Axis 1, there should not be Axis1 subscriptions");

                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Button2).Returns(false).SetName("After removing Button 2, there should not be Button2 subscriptions");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Button1).Returns(true).SetName("After removing Button 1, there should be Button1 subscriptions");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Button1Subscriber2).Returns(false).SetName("After removing Button 1 Subscriber 2, there should not be Button1 subscriptions");
            }
        }

        [TestCaseSource(typeof(TestData))]
        public bool CountIndexesTests(SubscriptionType subscriptionType, InputSubReq sr)
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

            return _subHelper.SubHandler.ContainsKey(subReq.BindingDescriptor.Type, subReq.BindingDescriptor.Index);
        }

    }
}
