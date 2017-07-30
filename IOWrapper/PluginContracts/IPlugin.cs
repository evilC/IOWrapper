using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginContracts
{
    public interface IPlugin
    {
        string PluginName { get; }
        void Do();
    }
}
