using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Interception.Lib;

namespace Core_Interception.Helpers
{
    public static class HelperFunctions
    {
        public static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine("IOWrapper| Core_Interception | " + formatStr, arguments);
        }

        public static ButtonState StrokeToMouseButtonAndState(ManagedWrapper.Stroke stroke)
        {
            int state = stroke.mouse.state;
            ushort btn = 0;
            if (state < 0x400)
            {
                while (state > 2)
                {
                    state >>= 2;
                    btn++;
                }
                state = 2 - state; // 1 = Pressed, 0 = Released
            }
            else
            {
                if (state == 0x400) btn = (ushort) (stroke.mouse.rolling > 0 ? 5 : 6); // Vertical mouse wheel
                else if (state == 0x800) btn = (ushort)(stroke.mouse.rolling > 0 ? 7 : 8); // Horizontal mouse wheel
                //state = stroke.mouse.rolling < 0 ? -1 : 1;
                state = 1;
            }
            return new ButtonState { Button = btn, State = state };
        }

        public class ButtonState
        {
            public ushort Button { get; set; }
            public int State { get; set; }
        }
    }
}
