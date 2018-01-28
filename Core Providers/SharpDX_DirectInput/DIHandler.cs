using Providers;
using Providers.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX_DirectInput
{

    internal class DIHandler : NodeHandler<BindingType, DIBindingHandler>
    {
        public override BindingType GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Type;
        }

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            return PassToChild(subReq);
        }
    }

    internal class DIBindingHandler : NodeHandler<int, DISubBindingHandler>
    {
        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Index;
        }

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            return PassToChild(subReq);
        }
    }

    internal class DISubBindingHandler : NewBindingHandler
    {
        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }
    }
}
