using System.Collections;
using System.Linq;
using NUnit.Framework;
using Tests.SubscriptionHandler.Helpers;
using Tests.SubscriptionHandler.Lookups;

namespace Tests.SubscriptionHandler.EmptyHandler
{
    /// <summary>
    /// Given I add a number of subscriptions to a SubscriptionHandler
    /// When I remove subscriptions
    /// Then the EmptyHandler delegate should only fire when the last subscription is removed
    /// </summary>

    [TestFixture]
    public class EmptyHandler
    {
        private readonly SubscriptionHelper _subHelper = new SubscriptionHelper();

        public EmptyHandler()
        {
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Button1));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Button2));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Button1Subscriber2));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Button2));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Axis1));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Axis2));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Pov1Up));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Pov1Right));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Pov2Up));
            _subHelper.SubHandler.Subscribe(_subHelper.BuildSubReq(SubReqs.Pov2Right));
        }

        private class TestData : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new TestCaseData(SubReqs.Pov2Right).Returns(0).SetName("After removing POV 2 Right, EmptyHandler should not fire");
                yield return new TestCaseData(SubReqs.Pov2Up).Returns(0).SetName("After removing POV 2 Up, EmptyHandler should not fire");
                yield return new TestCaseData(SubReqs.Pov1Right).Returns(0).SetName("After removing POV 1 Right, EmptyHandler should not fire");
                yield return new TestCaseData(SubReqs.Pov1Up).Returns(0).SetName("After removing POV 1 Up, EmptyHandler should not fire");
                yield return new TestCaseData(SubReqs.Axis2).Returns(0).SetName("After removing Axis 2, EmptyHandler should not fire");
                yield return new TestCaseData(SubReqs.Axis1).Returns(0).SetName("After removing Axis 1, EmptyHandler should not fire");
                yield return new TestCaseData(SubReqs.Button2).Returns(0).SetName("After removing Button 2, EmptyHandler should not fire");
                yield return new TestCaseData(SubReqs.Button1Subscriber2).Returns(0).SetName("After removing Button 1 Subscriber 2, EmptyHandler should not fire");
                yield return new TestCaseData(SubReqs.Button1).Returns(1).SetName("After removing Button 1, EmptyHandler should fire");
            }
        }

        [TestCaseSource(typeof(TestData))]
        public int CountIndexesTests(InputSubReq sr)
        {
            var subReq = _subHelper.BuildSubReq(sr);
            _subHelper.SubHandler.Unsubscribe(subReq);

            return _subHelper.DeviceEmptyResults.Count();
        }

    }
}
