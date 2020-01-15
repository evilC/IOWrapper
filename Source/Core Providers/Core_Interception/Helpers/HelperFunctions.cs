using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Interception.Lib;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Helpers
{
    public static class HelperFunctions
    {
        public static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine("IOWrapper| Core_Interception | " + formatStr, arguments);
        }

        public static readonly Dictionary<int, ButtonState> ButtonStateLookupTable = new Dictionary<int, ButtonState>()
        {
            { 1, new ButtonState{Button = 0, State = 1, Flag = 1} },        // LMB Press
            { 2, new ButtonState{Button = 0, State = 0, Flag = 2} },        // LMB Release
            { 4, new ButtonState{Button = 1, State = 1, Flag = 4} },        // RMB Press
            { 8, new ButtonState{Button = 1, State = 0, Flag = 8} },        // RMB Release
            { 16, new ButtonState{Button = 2, State = 1, Flag = 16} },      // MMB Press
            { 32, new ButtonState{Button = 2, State = 0, Flag = 32} },      // MMB Release
            { 64, new ButtonState{Button = 3, State = 1, Flag = 64} },      // XB1 Press
            { 128, new ButtonState{Button = 3, State = 0, Flag = 128} },    // XB1 Release
            { 256, new ButtonState{Button = 4, State = 1, Flag = 256} },    // XB2 Press
            { 512, new ButtonState{Button = 4, State = 0, Flag = 512} }     // XB2 Release
        };

        public static ButtonState[] StrokeToMouseButtonAndState(ManagedWrapper.Stroke stroke)
        {
            int state = stroke.mouse.state;
            
            // Buttons
            var buttonStates = new List<ButtonState>();
            foreach (var buttonState in ButtonStateLookupTable)
            {
                if (state < buttonState.Key) break;
                if ((state & buttonState.Key) != buttonState.Key) continue;

                buttonStates.Add(buttonState.Value);
                state -= buttonState.Key;
            }

            // Wheel
            if ((state & 0x400) == 0x400) // Wheel up / down
            {
                buttonStates.Add(
                    new ButtonState
                    {
                        Button = (ushort) (stroke.mouse.rolling > 0 ? 5 : 6),
                        State = 1
                    }
                );
            }
            else if ((state & 0x800) == 0x800) // Wheel left / right
            {
                buttonStates.Add(
                    new ButtonState
                    {
                        Button = (ushort) (stroke.mouse.rolling > 0 ? 8 : 7),
                        State = 1
                    }
                );
            }
            return buttonStates.ToArray();
        }

        public static List<BindingUpdate> StrokeToMouseMove(ManagedWrapper.Stroke stroke)
        {
            var bindingUpdates = new List<BindingUpdate>();

            if (stroke.mouse.x != 0)
            {
                bindingUpdates.Add(new BindingUpdate
                {
                    Binding = new BindingDescriptor
                    {
                        Index = 0,
                        SubIndex = 0
                    },
                    Value = stroke.mouse.x
                });
            }

            if (stroke.mouse.y != 0)
            {
                bindingUpdates.Add(new BindingUpdate
                {
                    Binding = new BindingDescriptor
                    {
                        Index = 1,
                        SubIndex = 0
                    },
                    Value = stroke.mouse.y
                });
            }

            return bindingUpdates;
        }

        public class ButtonState
        {
            public ushort Button { get; set; }
            public int State { get; set; }
            public ushort Flag { get; set; } // Preserve original flag, so it can be removed from stroke
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
