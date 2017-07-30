using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginContracts;

namespace IOWrapper
{
    public enum InputTypes { Button, Axis };

    public class IOController
    {
        Dictionary<string, IPlugin> _Plugins;

        public IOController()
        {
            GenericMEFPluginLoader<IPlugin> loader = new GenericMEFPluginLoader<IPlugin>("Plugins");
            _Plugins = new Dictionary<string, IPlugin>();
            IEnumerable<IPlugin> plugins = loader.Plugins;
            foreach (var item in plugins)
            {
                _Plugins[item.PluginName] = item;
            }
        }

        public List<DeviceReport> GetInputList()
        {
            var list = new List<DeviceReport>();
            foreach (var plugin in _Plugins.Values)
            {
                var report = plugin.GetInputList();
                if (report != null)
                {
                    list.Add(report);
                }
            }
            return list;
        }

        public Guid? SubscribeButton(string pluginName, string deviceHandle, uint buttonId, dynamic callback)
        {
            if (_Plugins.ContainsKey(pluginName))
            {
                var subReq = new SubscriptionRequest() {
                    PluginName = pluginName,
                    InputType = InputType.BUTTON,
                    DeviceHandle = deviceHandle,
                    InputIndex = buttonId,
                    Callback = callback
                };
                return _Plugins[pluginName].SubscribeButton(subReq);
            }
            return null;
        }
    }
}
