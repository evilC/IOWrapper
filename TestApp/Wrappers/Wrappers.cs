using HidWizards.IOWrapper.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataObjects;

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
        private static IOController instance;

        private IOW() { }

        public static IOController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IOController();
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
        public OutputSubscription()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor
            {
                ProfileGuid = Library.Profiles.Default,
                SubscriberGuid = Guid.NewGuid()
            };
        }
    }

    public class InputSubscription : InputSubscriptionRequest
    {
        public InputSubscription()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor
            {
                ProfileGuid = Library.Profiles.Default,
                SubscriberGuid = Guid.NewGuid()
            };
        }
    }
    #endregion
}
