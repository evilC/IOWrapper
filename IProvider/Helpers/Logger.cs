using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HidWizards.IOWrapper.API.Helpers
{
    public class Logger
    {
        private string providerName;

        public Logger(string name)
        {
            providerName = name;
        }

        public void Log(string formatStr, params object[] arguments)
        {
            var str = string.Format(string.Format("IOWrapper| Provider: {0}| {1}", providerName, formatStr), arguments);
            Debug.WriteLine(str);
        }
    }
}
