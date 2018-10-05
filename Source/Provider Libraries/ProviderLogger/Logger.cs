using System.Diagnostics;

namespace Hidwizards.IOWrapper.Libraries.ProviderLogger
{
    public class Logger
    {
        private readonly string _providerName;

        public Logger(string name)
        {
            _providerName = name;
        }

        public void Log(string formatStr, params object[] arguments)
        {
            var str = string.Format($"IOWrapper| Provider: {_providerName}| {formatStr}", arguments);
            Debug.WriteLine(str);
        }
    }
}
