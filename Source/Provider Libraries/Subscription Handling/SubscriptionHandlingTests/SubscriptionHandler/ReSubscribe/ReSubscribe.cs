using NUnit.Framework;
using Tests.SubscriptionHandler.Helpers;
using Tests.SubscriptionHandler.Lookups;

namespace Tests.SubscriptionHandler.ReSubscribe
{
    [TestFixture]
    class ReSubscribe
    {
        /// <summary>
        /// Give I subscribe twice with the same SubscriberGuid to the same input of a SubscriptionHandler
        /// When I unsibscribe once
        /// Then the EmptyHandler fires
        /// </summary>
        [TestCase(TestName = "Subscribing twice to the same input and unsubscribing once unsubscribes")]
        public void ResubscribeTest()
        {
            var subHelper = new SubscriptionHelper();
            subHelper.SubHandler.Subscribe(subHelper.BuildSubReq(SubReqs.Button1));
            Assert.That(subHelper.DeviceEmptyResults.Count, Is.EqualTo(0));
            subHelper.SubHandler.Subscribe(subHelper.BuildSubReq(SubReqs.Button1));
            Assert.That(subHelper.DeviceEmptyResults.Count, Is.EqualTo(0));
            subHelper.SubHandler.Unsubscribe(subHelper.BuildSubReq(SubReqs.Button1));
            Assert.That(subHelper.DeviceEmptyResults.Count, Is.EqualTo(1));
        }
    }
}
