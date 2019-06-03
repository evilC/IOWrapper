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

        private static readonly Dictionary<int, ButtonState> _buttonStateLookupTable = new Dictionary<int, ButtonState>()
        {
            { 1, new ButtonState{Button = 0, State = 1} },
            { 2, new ButtonState{Button = 0, State = 0} },
            { 4, new ButtonState{Button = 1, State = 1} },
            { 8, new ButtonState{Button = 1, State = 0} },
            { 16, new ButtonState{Button = 2, State = 1} },
            { 32, new ButtonState{Button = 2, State = 0} },
            { 64, new ButtonState{Button = 3, State = 1} },
            { 128, new ButtonState{Button = 3, State = 0} },
            { 256, new ButtonState{Button = 4, State = 1} },
            { 512, new ButtonState{Button = 4, State = 0} },
        };

        public static ButtonState[] StrokeToMouseButtonAndState(ManagedWrapper.Stroke stroke)
        {
            int state = stroke.mouse.state;
            ushort btn = 0;
            
            // ToDo: Is it possible to have a wheel and button in same update?
            if (state < 0x400)
            {
                var buttonStates = new List<ButtonState>();
                foreach (var buttonState in _buttonStateLookupTable)
                {
                    if (state < buttonState.Key) break;
                    if ((state & buttonState.Key) == buttonState.Key)
                    {
                        buttonStates.Add(buttonState.Value);
                    }

                    state -= buttonState.Key;
                }
                return buttonStates.ToArray();
            }

            // Wheel
            if (state == 0x400) btn = (ushort) (stroke.mouse.rolling > 0 ? 5 : 6); // Vertical mouse wheel
            else if (state == 0x800) btn = (ushort)(stroke.mouse.rolling > 0 ? 7 : 8); // Horizontal mouse wheel
            //state = stroke.mouse.rolling < 0 ? -1 : 1;
            state = 1;
            return new ButtonState[1] { new ButtonState { Button = btn, State = state } };
        }

        public class ButtonState
        {
            public ushort Button { get; set; }
            public int State { get; set; }
        }

        public static bool IsKeyboard(int devId)
        {
            return devId < 11;
        }

        public static bool IsMouse(int devId)
        {
            return devId > 10;
        }
    }
}
