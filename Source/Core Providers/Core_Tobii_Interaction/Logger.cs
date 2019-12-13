using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Tobii_Interaction
{
    public static class Logger
    {
        public static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine("IOWrapper| Tobii | " + formatStr, arguments);
        }
    }
}
