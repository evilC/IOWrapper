using System.ComponentModel.Composition;
using PluginContracts;
using System;

namespace SharpDX_DirectInput
{
    [Export(typeof(IPlugin))]
    public class SharpDX_DirectInput : IPlugin
    {
        #region IPlugin Members

        public string PluginName
        {
            get
            {
                //Type myType = typeof(SharpDX_DirectInput);
                //return myType.Namespace;
                return "SharpDX_DirectInput";
                //System.Reflection.Assembly.GetExecutingAssembly().EntryPoint.DeclaringType.Namespace;
            }
        }

        public void Do()
        {
            var a = 1;
            //System.Windows.MessageBox.Show("Do Something in First Plugin");
        }

        #endregion
    }
}