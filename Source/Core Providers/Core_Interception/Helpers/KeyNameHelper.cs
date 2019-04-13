using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core_Interception.Helpers
{
    public static class KeyNameHelper
    {
        [DllImport("user32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetKeyNameTextW(uint lParam, StringBuilder lpString, int nSize);

        // GetKeyNameTextW does not seem to return names for these ScanCodes
        private static readonly Dictionary<int, string> MissingKeyNames = new Dictionary<int, string>
        {
            { 100, "F13" }, { 101, "F14" }, { 102, "F15" }, { 103, "F16" }, { 104, "F17" }, { 105, "F18" },
            { 106, "F19" }, { 107, "F20" }, { 108, "F21" }, { 109, "F22" }, { 110, "F23" }, { 111, "F24" }
        };

        public static string GetNameFromScanCode(int code)
        {
            if (MissingKeyNames.ContainsKey(code))
            {
                return MissingKeyNames[code];
            }
            uint lParam;
            if (code > 255)
            {
                code -= 256;
                lParam = (0x100 | ((uint)code & 0xff)) << 16;
            }
            else
            {
                lParam = (uint)(code) << 16;
            }

            var sb = new StringBuilder(260);
            if (GetKeyNameTextW(lParam, sb, 260) == 0)
            {
                return null;
            }
            var keyName = sb.ToString().Trim();
            if (keyName == "") return null;
            return keyName;
        }
    }
}
