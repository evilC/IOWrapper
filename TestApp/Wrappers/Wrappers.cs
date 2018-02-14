using HidWizards.IOWrapper.ProviderInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Wrappers
{
    #region IOWrapper Instance
    /// <summary>
    /// IOWrapper Singleton
    /// 
    /// Use InputList / OutputList to see available forms of I/O, find IDs etc
    /// </summary>
    public class IOW
    {
        private static HidWizards.IOWrapper.ProviderInterface.IOController instance;

        private IOW() { }

        public static HidWizards.IOWrapper.ProviderInterface.IOController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HidWizards.IOWrapper.ProviderInterface.IOController();
                }
                return instance;
            }
        }

        public SortedDictionary<string, ProviderReport> InputList { get { return inputList; } }
        public SortedDictionary<string, ProviderReport> OutputList { get { return outputList; } }

        private SortedDictionary<string, ProviderReport> inputList = instance.GetInputList();
        private SortedDictionary<string, ProviderReport> outputList = instance.GetOutputList();
    }
    #endregion

    #region Subscription Helpers
    public class OutputSubscription : OutputSubscriptionRequest
    {
        public OutputSubscription() : base()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = Library.Profiles.Default,
                SubscriberGuid = Guid.NewGuid()
            };
        }
    }

    public class InputSubscription : InputSubscriptionRequest
    {
        public InputSubscription() : base()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = Library.Profiles.Default,
                SubscriberGuid = Guid.NewGuid()
            };
        }
    }
    #endregion
}
