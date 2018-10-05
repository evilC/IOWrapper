using System.Collections;
using NUnit.Framework;
using Tests.SubscriptionHandler.Helpers;
using Tests.SubscriptionHandler.Lookups;

namespace Tests.SubscriptionHandler.ContainsKey
{
    /// <summary>
    /// Given I have a SubscriptionHandler
    /// When I add or remove subscriptions
    /// Then the ContainsKey(BindingType) method should indicate if there are any subscribed Indexes
    /// </summary>

    [TestFixture]
    public class ContainsKeyType
    {
        private readonly SubscriptionHelper _subHelper = new SubscriptionHelper();

        private class TestData : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                // Add
                yield return new TestCaseData(SubscriptionType.None, SubReqs.Button1).Returns(false).SetName("Before adding Button 1, Buttons should not exist");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Button1).Returns(true).SetName("After adding Button 1, Buttons should exist");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Button2).Returns(true).SetName("After adding Button 2, Buttons should exist");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Button1Subscriber2).Returns(true).SetName("After adding Button 1 Subscriber 2, Buttons should exist");

                yield return new TestCaseData(SubscriptionType.None, SubReqs.Axis1).Returns(false).SetName("Before adding Axis 1, Axes should not exist");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Axis1).Returns(true).SetName("After adding Axis 1, Axes should exist");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Axis2).Returns(true).SetName("After adding Axis 2, Axes should exist");

                yield return new TestCaseData(SubscriptionType.None, SubReqs.Pov1Up).Returns(false).SetName("Before adding POV 1 Up, POVs should not exist");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov1Up).Returns(true).SetName("After adding POV 1 Up, POVs should exist");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov1Right).Returns(true).SetName("After adding POV 1 Right, POVs should exist");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov2Up).Returns(true).SetName("After adding POV 2 Up, there POVs should exist");
                yield return new TestCaseData(SubscriptionType.Subscribe, SubReqs.Pov2Right).Returns(true).SetName("After adding POV 2 Right, POVs should exist");

                // Remove
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov2Right).Returns(true).SetName("After removing POV 2 Right, POVs should exist");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov2Up).Returns(true).SetName("After removing POV 2 Up, POVs should exist");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov1Right).Returns(true).SetName("After removing POV 1 Right, POVs should exist");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Pov1Up).Returns(false).SetName("After removing POV 1 Up, POVs should not exist");

                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Axis2).Returns(true).SetName("After removing Axis 2, Axes should exist");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Axis1).Returns(false).SetName("After removing Axis 1, Axes should not exist");

                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Button2).Returns(true).SetName("After removing Button 2, Buttons should exist");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Button1).Returns(true).SetName("After removing Button 1, Buttons should exist");
                yield return new TestCaseData(SubscriptionType.Unsubscribe, SubReqs.Button1Subscriber2).Returns(false).SetName("After removing Button 1 Subscriber 2, Buttons should not exist");
            }
        }

        [TestCaseSource(typeof(TestData))]
        public bool CountTypesTests(SubscriptionType subscriptionType, InputSubReq sr)
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

            return _subHelper.SubHandler.ContainsKey(sr.BindingDescriptor.Type);
        }
    }
}
