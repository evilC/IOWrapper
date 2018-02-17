﻿using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace SharpDX_DirectInput.Handlers
{
    public class DiAxisBindingHandler : BindingHandler
    {
        public DiAxisBindingHandler(InputSubscriptionRequest subReq) : base(subReq) { }

        public override void Poll(int pollValue)
        {
            // Normalization of Axes to standard scale occurs here
            BindingDictionary[0].State = (65535 - pollValue) - 32768;
        }
    }
}