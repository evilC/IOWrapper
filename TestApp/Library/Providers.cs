using IProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Library
{
    /// <summary>
    /// A Lookup table of Providers
    /// </summary>
    class Providers
    {
        public static ProviderDescriptor DirectInput = new ProviderDescriptor() { ProviderName = "SharpDX_DirectInput" };
        public static ProviderDescriptor XInput = new ProviderDescriptor() { ProviderName = "SharpDX_XInput" };
        public static ProviderDescriptor vJoy = new ProviderDescriptor() { ProviderName = "Core_vJoyInterfaceWrap" };
        public static ProviderDescriptor Interception = new ProviderDescriptor() { ProviderName = "Core_Interception" };
        public static ProviderDescriptor Tobii = new ProviderDescriptor() { ProviderName = "Core_Tobii_Interaction" };
        public static ProviderDescriptor ViGEm = new ProviderDescriptor() { ProviderName = "Core_ViGEm" };
        public static ProviderDescriptor TitanOne = new ProviderDescriptor() { ProviderName = "Core_TitanOne" };
        public static ProviderDescriptor DS4Windows = new ProviderDescriptor() { ProviderName = "Core_DS4WindowsApi" };

    }
}
