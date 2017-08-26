using Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core_Interception
{
    [Export(typeof(IProvider))]
    public class Core_Interception : IProvider
    {
        bool disposed = false;
        private IntPtr deviceContext;
        private ProviderReport providerReport;

        // The thread which handles input detection
        private Thread pollThread;
        // Is the thread currently running? This is set by the thread itself.
        private volatile bool pollThreadRunning = false;
        // Do we want the thread to be on or off?
        // This is independent of whether or not the thread is running...
        // ... for example, we may be updating bindings, so the thread may be temporarily stopped
        private bool pollThreadDesired = false;
        // Set to true to cause the thread to stop running. When it stops, it will set pollThreadRunning to false
        private volatile bool pollThreadStopRequested = false;

        private bool filterState = false;

        private Dictionary<int, KeyboardMonitor> MonitoredKeyboards = new Dictionary<int, KeyboardMonitor>();
        private Dictionary<int, MouseMonitor> MonitoredMice = new Dictionary<int, MouseMonitor>();
        private Dictionary<string, int> deviceHandleToId;

        private static BindingInfo keyboardList;
        private static BindingInfo mouseButtonList;
        private static List<string> mouseButtonNames = new List<string>() { "Left Mouse", "Right Mouse", "Middle Mouse", "Side Button 1", "Side Button 2" };

        public Core_Interception()
        {
            deviceContext = CreateContext();

            QueryDevices();

            pollThreadDesired = true;
        }

        ~Core_Interception()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                SetPollThreadState(false);
            }
            disposed = true;
            Log("Provider {0} was Disposed", ProviderName);
        }

        private void SetFilterState(bool state)
        {
            if (state && !filterState)
            {
                //Log("Got DeviceContext " + deviceContext);
                SetFilter(deviceContext, IsKeyboard, Filter.All);
                SetFilter(deviceContext, IsMouse, Filter.All);
            }
            else if (!state && filterState)
            {
                SetFilter(deviceContext, IsKeyboard, Filter.None);
            }
        }

        private void SetPollThreadState(bool state)
        {
            if (state && !pollThreadRunning)
            {
                SetFilterState(true);
                pollThreadStopRequested = false;
                pollThread = new Thread(PollThread);
                pollThread.Start();
                while (!pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
                Log("Started PollThread for {0}", ProviderName);
            }
            else if (!state && pollThreadRunning)
            {
                SetFilterState(false);
                pollThreadStopRequested = true;
                while (pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
                pollThread = null;
                Log("Stopped PollThread for {0}", ProviderName);
            }
        }

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine(String.Format("IOWrapper| " + formatStr, arguments));
        }

        #region IProvider Members
        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_Interception).Namespace; } }

        public bool SetProfileState(Guid profileGuid, bool state)
        {
            return false;
        }

        public ProviderReport GetInputList()
        {
            return providerReport;
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (pollThreadRunning)
                SetPollThreadState(false);

            if (!deviceHandleToId.ContainsKey(subReq.DeviceHandle))
            {
                return false;
            }
            var id = deviceHandleToId[subReq.DeviceHandle];
            if (id < 10)
            {
                if (!MonitoredKeyboards.ContainsKey(id + 1))
                {
                    MonitoredKeyboards.Add(id + 1, new KeyboardMonitor() { });
                }
                MonitoredKeyboards[id + 1].Add(subReq);
            }
            else
            {
                if (!MonitoredMice.ContainsKey(id + 1))
                {
                    MonitoredMice.Add(id + 1, new MouseMonitor() { });
                }
                MonitoredMice[id + 1].Add(subReq);
            }


            if (pollThreadDesired)
            {
                SetPollThreadState(true);
            }
            return true;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return true;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return true;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, InputType inputType, uint inputIndex, int state)
        {
            int devId = deviceHandleToId[subReq.DeviceHandle];
            Stroke stroke = new Stroke();
            if (devId < 11)
            {
                ushort st = (ushort)(1 - state);
                ushort code = (ushort)(inputIndex + 1);
                if (code > 255)
                {
                    st += 2;
                    code -= 256;
                }
                stroke.key.code = code;
                stroke.key.state = st;
            }
            else
            {
                var bit = (int)inputIndex * 2;
                if (state == 0)
                    bit += 1;
                stroke.mouse.state = (ushort)(1 << bit);
            }
            Send(deviceContext, devId, ref stroke, 1);
            return true;
        }
        #endregion

        #region Device Querying
        private void QueryDevices()
        {
            deviceHandleToId = new Dictionary<string, int>();
            providerReport = new ProviderReport();
            UpdateKeyList();
            UpdateMouseButtonList();
            string handle;
            int i = 1;
            while (i < 11)
            {
                handle = GetHardwareStr(deviceContext, i, 1000);
                if (handle != "" && IsKeyboard(i) == 1)
                {
                    handle = @"Keyboard\" + handle;
                    providerReport.Devices.Add(handle, new IOWrapperDevice()
                    {
                        DeviceHandle = handle,
                        DeviceName = "Unknown Keyboard",
                        ProviderName = ProviderName,
                        API = "Interception",
                        Bindings = { keyboardList }
                    });
                    deviceHandleToId.Add(handle, i - 1);
                    Log(String.Format("{0} (Keyboard) = VID/PID: {1}", i, handle));
                }
                i++;
            }
            while (i < 21)
            {
                handle = GetHardwareStr(deviceContext, i, 1000);
                if (handle != "" && IsMouse(i) == 1)
                {
                    handle = @"Mouse\" + handle;
                    providerReport.Devices.Add(handle, new IOWrapperDevice()
                    {
                        DeviceHandle = handle,
                        DeviceName = "Unknown Mouse",
                        ProviderName = ProviderName,
                        API = "Interception",
                        Bindings = { mouseButtonList }
                    });
                    deviceHandleToId.Add(handle, i - 1);
                    Log(String.Format("{0} (Mouse) = VID/PID: {1}", i, handle));
                }
                i++;
            }
        }

        private void UpdateMouseButtonList()
        {
            mouseButtonList = new BindingInfo()
            {
                Title = "Buttons",
                IsBinding = false
            };
            for (int i = 0; i < 5; i++)
            {
                mouseButtonList.SubBindings.Add(new BindingInfo()
                {
                    InputIndex = i,
                    Title = mouseButtonNames[i],
                    InputType = InputType.BUTTON,
                    Category = BindingInfo.InputCategory.Button
                });
            }
            
        }

        private void UpdateKeyList()
        {
            keyboardList = new BindingInfo() {
                Title = "Keys",
                IsBinding = false
            };
            //buttonNames = new Dictionary<int, string>();
            uint lParam = 0;
            StringBuilder sb = new StringBuilder(260);
            string keyName;
            string altKeyName;

            for (int i = 0; i < 256; i++)
            {
                lParam = (uint)(i+1) << 16;
                if (GetKeyNameTextW(lParam, sb, 260) == 0)
                {
                    continue;
                }
                keyName = sb.ToString().Trim();
                if (keyName == "")
                    continue;
                //Log("Button Index: {0}, name: '{1}'", i, keyName);
                keyboardList.SubBindings.Add(new BindingInfo() {
                    InputIndex = i,
                    Title = keyName,
                    InputType = InputType.BUTTON,
                    Category = BindingInfo.InputCategory.Button
                });
                //buttonNames.Add(i, keyName);

                // Check if this button has an extended (Right) variant
                lParam = (0x100 | ((uint)i+1 & 0xff)) << 16;
                if (GetKeyNameTextW(lParam, sb, 260) == 0)
                {
                    continue;
                }
                altKeyName = sb.ToString().Trim();
                if (altKeyName == "" || altKeyName == keyName)
                    continue;
                //Log("ALT Button Index: {0}, name: '{1}'", i + 256, altKeyName);
                keyboardList.SubBindings.Add(new BindingInfo() {
                    InputIndex = i + 256,
                    Title = altKeyName,
                    InputType = InputType.BUTTON,
                    Category = BindingInfo.InputCategory.Button
                });
                //Log("Button Index: {0}, name: '{1}'", i + 256, altKeyName);
                //buttonNames.Add(i + 256, altKeyName);
            }
        }
        #endregion

        #region Input processing
        #region Keyboard
        private class KeyboardMonitor
        {
            private Dictionary<ushort, KeyboardKeyMonitor> monitoredKeys = new Dictionary<ushort, KeyboardKeyMonitor>();
            
            public void Add(InputSubscriptionRequest subReq)
            {
                var code = (ushort)(subReq.InputIndex + 1);
                ushort stateDown = 0;
                ushort stateUp = 1;
                if (code > 256)
                {
                    code -= 256;
                    stateDown = 2;
                    stateUp = 3;
                }
                monitoredKeys.Add(code, new KeyboardKeyMonitor() { code = code, stateDown = stateDown, stateUp = stateUp});
                monitoredKeys[code].Add(subReq);
            }

            public void Poll(Stroke stroke)
            {
                foreach (var monitoredKey in monitoredKeys.Values)
                {
                    monitoredKey.Poll(stroke);
                }
            }
        }

        private class KeyboardKeyMonitor
        {
            public ushort code;
            public ushort stateDown;
            public ushort stateUp;

            private Dictionary<Guid, InputSubscriptionRequest> subReqs = new Dictionary<Guid, InputSubscriptionRequest>();

            public void Add(InputSubscriptionRequest subReq)
            {
                subReqs.Add(subReq.SubscriberGuid, subReq);
            }

            public void Poll(Stroke stroke)
            {
                var isDown = stateDown == stroke.key.state;
                var isUp = stateUp == stroke.key.state;
                if (code == stroke.key.code && ( isDown || isUp))
                {
                    foreach (var subscriptionRequest in subReqs.Values)
                    {
                        subscriptionRequest.Callback(isDown ? 1 : 0);
                        //Log("State: {0}", isDown);
                    }
                }
            }
        }
        #endregion

        #region Mouse
        private class MouseMonitor
        {
            private Dictionary<ushort, MouseButtonMonitor> monitoredStates = new Dictionary<ushort, MouseButtonMonitor>();

            public void Add(InputSubscriptionRequest subReq)
            {
                var i = (ushort)subReq.InputIndex;
                ushort downbit = (ushort)(1 << (i * 2));
                ushort upbit = (ushort)(1 << ((i * 2) + 1));

                Log("Added subscription to mouse button {0}", subReq.InputIndex);
                monitoredStates.Add(downbit, new MouseButtonMonitor() { outputState = 1 });
                monitoredStates[downbit].Add(subReq);
                monitoredStates.Add(upbit, new MouseButtonMonitor() { outputState = 0 });
                monitoredStates[upbit].Add(subReq);
            }

            public void Poll(Stroke stroke)
            {
                if (monitoredStates.ContainsKey(stroke.mouse.state))
                {
                    monitoredStates[stroke.mouse.state].Poll(stroke);
                }
            }
        }

        private class MouseButtonMonitor
        {
            public int outputState;

            private Dictionary<Guid, InputSubscriptionRequest> subReqs = new Dictionary<Guid, InputSubscriptionRequest>();

            public void Add(InputSubscriptionRequest subReq)
            {
                subReqs.Add(subReq.SubscriberGuid, subReq);
                //Log("Added Subscription to Mouse Button {0}", subReq.InputIndex);
            }

            public void Poll(Stroke stroke)
            {
                if ((stroke.mouse.state & (ushort)Filter.MouseButtonAny) != 0)
                {
                    foreach (var subscriptionRequest in subReqs.Values)
                    {
                        Log("State: {0}", outputState);
                        // ToDo: Need thread pool ?
                        //var t = new Thread(() => CallbackThread(subscriptionRequest, outputState));
                        //t.Start();
                    }
                }
            }

            private static void CallbackThread(InputSubscriptionRequest subReq, int value)
            {
                //Log("Callback");
                subReq.Callback(value);
            }
        }
        #endregion

        #endregion

        #region PollThread
        private void PollThread()
        {
            pollThreadRunning = true;

            Stroke stroke = new Stroke();

            while (!pollThreadStopRequested)
            {
                for (int i = 1; i < 11; i++)
                {
                    bool isMonitoredKeyboard = MonitoredKeyboards.ContainsKey(i);

                    while (Receive(deviceContext, i, ref stroke, 1) > 0)
                    {
                        bool block = false;
                        if (isMonitoredKeyboard)
                        {
                            MonitoredKeyboards[i].Poll(stroke);
                        }
                        if (!block)
                        {
                            Send(deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                for (int i = 11; i < 21; i++)
                {
                    bool isMonitoredMouse = MonitoredMice.ContainsKey(i);

                    while (Receive(deviceContext, i, ref stroke, 1) > 0)
                    {
                        bool block = false;
                        if (isMonitoredMouse)
                        {
                            MonitoredMice[i].Poll(stroke);
                        }
                        if (!block)
                        {
                            Send(deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                Thread.Sleep(1);
            }
            pollThreadRunning = false;
        }
        #endregion

        #region Imports
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Predicate(int device);
        /*
        typedef void *InterceptionContext;
        typedef int InterceptionDevice;
        typedef int InterceptionPrecedence;
        typedef unsigned short InterceptionFilter;
        typedef int (*InterceptionPredicate)(InterceptionDevice device);
        */

        [Flags]
        public enum KeyState
        {
            Down = 0x00,
            Up = 0x01,
            E0 = 0x02,
            E1 = 0x04,
            TermsrvSetLED = 0x08,
            TermsrvShadow = 0x10,
            TermsrvVKPacket = 0x20
            /*
            enum InterceptionKeyState
            INTERCEPTION_KEY_DOWN = 0x00,
            INTERCEPTION_KEY_UP = 0x01,
            INTERCEPTION_KEY_E0 = 0x02,
            INTERCEPTION_KEY_E1 = 0x04,
            INTERCEPTION_KEY_TERMSRV_SET_LED = 0x08,
            INTERCEPTION_KEY_TERMSRV_SHADOW = 0x10,
            INTERCEPTION_KEY_TERMSRV_VKPACKET = 0x20
            */
        }

        [Flags]
        public enum MouseState
        {
            None = 0x000,
            LeftButtonDown = 0x001,
            LeftButtonUp = 0x002,
            RightButtonDown = 0x004,
            RightButtonUp = 0x008,
            MiddleButtonDown = 0x010,
            MiddleButtonUp = 0x020,

            Button1Down = LeftButtonDown,
            Button1Up = LeftButtonUp,
            Button2Down = RightButtonDown,
            Button2Up = RightButtonUp,
            Button3Down = MiddleButtonDown,
            Button3Up = MiddleButtonUp,

            Button4Down = 0x040,
            Button4Up = 0x080,
            Button5Down = 0x100,
            Button5Up = 0x200,

            Wheel = 0x400,
            HWheel = 0x800
            /*
            enum InterceptionMouseState
            {
            INTERCEPTION_MOUSE_LEFT_BUTTON_DOWN = 0x001,
            INTERCEPTION_MOUSE_LEFT_BUTTON_UP = 0x002,
            INTERCEPTION_MOUSE_RIGHT_BUTTON_DOWN = 0x004,
            INTERCEPTION_MOUSE_RIGHT_BUTTON_UP = 0x008,
            INTERCEPTION_MOUSE_MIDDLE_BUTTON_DOWN = 0x010,
            INTERCEPTION_MOUSE_MIDDLE_BUTTON_UP = 0x020,

            INTERCEPTION_MOUSE_BUTTON_1_DOWN = INTERCEPTION_MOUSE_LEFT_BUTTON_DOWN,
            INTERCEPTION_MOUSE_BUTTON_1_UP = INTERCEPTION_MOUSE_LEFT_BUTTON_UP,
            INTERCEPTION_MOUSE_BUTTON_2_DOWN = INTERCEPTION_MOUSE_RIGHT_BUTTON_DOWN,
            INTERCEPTION_MOUSE_BUTTON_2_UP = INTERCEPTION_MOUSE_RIGHT_BUTTON_UP,
            INTERCEPTION_MOUSE_BUTTON_3_DOWN = INTERCEPTION_MOUSE_MIDDLE_BUTTON_DOWN,
            INTERCEPTION_MOUSE_BUTTON_3_UP = INTERCEPTION_MOUSE_MIDDLE_BUTTON_UP,

            INTERCEPTION_MOUSE_BUTTON_4_DOWN = 0x040,
            INTERCEPTION_MOUSE_BUTTON_4_UP = 0x080,
            INTERCEPTION_MOUSE_BUTTON_5_DOWN = 0x100,
            INTERCEPTION_MOUSE_BUTTON_5_UP = 0x200,

            INTERCEPTION_MOUSE_WHEEL = 0x400,
            INTERCEPTION_MOUSE_HWHEEL = 0x800
            };
            */
        }

        [Flags]
        public enum Filter : ushort
        {
            None = 0x0000,
            All = 0xFFFF,
            KeyDown = KeyState.Up,
            KeyUp = KeyState.Up << 1,
            KeyE0 = KeyState.E0 << 1,
            KeyE1 = KeyState.E1 << 1,
            KeyTermsrvSetLED = KeyState.TermsrvSetLED << 1,
            KeyTermsrvShadow = KeyState.TermsrvShadow << 1,
            KeyTermsrvVKPacket = KeyState.TermsrvVKPacket << 1,
            /*
            enum InterceptionFilterKeyState
            INTERCEPTION_FILTER_KEY_NONE = 0x0000,
            INTERCEPTION_FILTER_KEY_ALL = 0xFFFF,
            INTERCEPTION_FILTER_KEY_DOWN = INTERCEPTION_KEY_UP,
            INTERCEPTION_FILTER_KEY_UP = INTERCEPTION_KEY_UP << 1,
            INTERCEPTION_FILTER_KEY_E0 = INTERCEPTION_KEY_E0 << 1,
            INTERCEPTION_FILTER_KEY_E1 = INTERCEPTION_KEY_E1 << 1,
            INTERCEPTION_FILTER_KEY_TERMSRV_SET_LED = INTERCEPTION_KEY_TERMSRV_SET_LED << 1,
            INTERCEPTION_FILTER_KEY_TERMSRV_SHADOW = INTERCEPTION_KEY_TERMSRV_SHADOW << 1,
            INTERCEPTION_FILTER_KEY_TERMSRV_VKPACKET = INTERCEPTION_KEY_TERMSRV_VKPACKET << 1
            */

            // enum InterceptionFilterMouseState
            //MOUSE_NONE = 0x0000,
            //MOUSE_ALL = 0xFFFF,
            MouseMove = 0x1000,

            MouseLeftButtonDown = MouseState.LeftButtonDown,
            MouseLeftButtonUp = MouseState.LeftButtonUp,
            MouseRightButtonDown = MouseState.RightButtonDown,
            MouseRightButtonUp = MouseState.RightButtonUp,
            MouseMiddleButtonDown = MouseState.MiddleButtonDown,
            MouseMiddleButtonUp = MouseState.MiddleButtonUp,

            MouseButton1Down = MouseState.Button1Down,
            MouseButton1Up = MouseState.Button1Up,
            MouseButton2Down = MouseState.Button2Down,
            MouseButton2Up = MouseState.Button2Up,
            MouseButton3Down = MouseState.Button3Down,
            MouseButton3Up = MouseState.Button3Up,

            MouseButton4Down = MouseState.Button4Down,
            MouseButton4Up = MouseState.Button4Up,
            MouseButton5Down = MouseState.Button5Down,
            MouseButton5Up = MouseState.Button5Up,
            MouseButtonAnyDown = MouseState.Button1Down | MouseState.Button2Down | MouseState.Button3Down | MouseState.Button4Down | MouseState.Button5Down,
            MouseButtonAnyUp = MouseState.Button1Up | MouseState.Button2Up | MouseState.Button3Up | MouseState.Button4Up | MouseState.Button5Up,
            MouseButtonAny = MouseButtonAnyDown | MouseButtonAnyUp,

            MouseWheel = MouseState.Wheel,
            MouseHWheel = MouseState.HWheel
            /*
            enum InterceptionFilterMouseState
            {
            INTERCEPTION_FILTER_MOUSE_NONE = 0x0000,
            INTERCEPTION_FILTER_MOUSE_ALL = 0xFFFF,

            INTERCEPTION_FILTER_MOUSE_LEFT_BUTTON_DOWN = INTERCEPTION_MOUSE_LEFT_BUTTON_DOWN,
            INTERCEPTION_FILTER_MOUSE_LEFT_BUTTON_UP = INTERCEPTION_MOUSE_LEFT_BUTTON_UP,
            INTERCEPTION_FILTER_MOUSE_RIGHT_BUTTON_DOWN = INTERCEPTION_MOUSE_RIGHT_BUTTON_DOWN,
            INTERCEPTION_FILTER_MOUSE_RIGHT_BUTTON_UP = INTERCEPTION_MOUSE_RIGHT_BUTTON_UP,
            INTERCEPTION_FILTER_MOUSE_MIDDLE_BUTTON_DOWN = INTERCEPTION_MOUSE_MIDDLE_BUTTON_DOWN,
            INTERCEPTION_FILTER_MOUSE_MIDDLE_BUTTON_UP = INTERCEPTION_MOUSE_MIDDLE_BUTTON_UP,

            INTERCEPTION_FILTER_MOUSE_BUTTON_1_DOWN = INTERCEPTION_MOUSE_BUTTON_1_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_1_UP = INTERCEPTION_MOUSE_BUTTON_1_UP,
            INTERCEPTION_FILTER_MOUSE_BUTTON_2_DOWN = INTERCEPTION_MOUSE_BUTTON_2_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_2_UP = INTERCEPTION_MOUSE_BUTTON_2_UP,
            INTERCEPTION_FILTER_MOUSE_BUTTON_3_DOWN = INTERCEPTION_MOUSE_BUTTON_3_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_3_UP = INTERCEPTION_MOUSE_BUTTON_3_UP,

            INTERCEPTION_FILTER_MOUSE_BUTTON_4_DOWN = INTERCEPTION_MOUSE_BUTTON_4_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_4_UP = INTERCEPTION_MOUSE_BUTTON_4_UP,
            INTERCEPTION_FILTER_MOUSE_BUTTON_5_DOWN = INTERCEPTION_MOUSE_BUTTON_5_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_5_UP = INTERCEPTION_MOUSE_BUTTON_5_UP,

            INTERCEPTION_FILTER_MOUSE_WHEEL = INTERCEPTION_MOUSE_WHEEL,
            INTERCEPTION_FILTER_MOUSE_HWHEEL = INTERCEPTION_MOUSE_HWHEEL,

            INTERCEPTION_FILTER_MOUSE_MOVE = 0x1000
            };
            */
        }

        [Flags]
        public enum MouseFlag : ushort
        {
            MouseMoveRelative = 0x000,
            MouseMoveAbsolute = 0x001,
            MouseVirturalDesktop = 0x002,
            MouseAttributesChanged = 0x004,
            MouseMoveNocoalesce = 0x008,
            MouseTermsrvSrcShadow = 0x100
            /*
            enum InterceptionMouseFlag
            {
            INTERCEPTION_MOUSE_MOVE_RELATIVE = 0x000,
            INTERCEPTION_MOUSE_MOVE_ABSOLUTE = 0x001,
            INTERCEPTION_MOUSE_VIRTUAL_DESKTOP = 0x002,
            INTERCEPTION_MOUSE_ATTRIBUTES_CHANGED = 0x004,
            INTERCEPTION_MOUSE_MOVE_NOCOALESCE = 0x008,
            INTERCEPTION_MOUSE_TERMSRV_SRC_SHADOW = 0x100
            };
            */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseStroke
        {
            public ushort state;
            public ushort flags;
            public short rolling;
            public int x;
            public int y;
            public uint information;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyStroke
        {
            public ushort code;
            public ushort state;
            public uint information;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Stroke
        {
            [FieldOffset(0)]
            public MouseStroke mouse;

            [FieldOffset(0)]
            public KeyStroke key;
        }

        [DllImport("interception.dll", EntryPoint = "interception_create_context", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateContext();

        [DllImport("interception.dll", EntryPoint = "interception_destroy_context", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyContext(IntPtr context);

        [DllImport("interception.dll", EntryPoint = "interception_set_filter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFilter(IntPtr context, Predicate predicate, Filter filter);
        // public static extern void SetFilter(IntPtr context, Predicate predicate, ushort filter);

        // InterceptionFilter INTERCEPTION_API interception_get_filter(InterceptionContext context, InterceptionDevice device);
        [DllImport("interception.dll", EntryPoint = "interception_get_filter", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort GetFilter(IntPtr context, int device);

        [DllImport("interception.dll", EntryPoint = "interception_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(IntPtr context, int device, ref Stroke stroke, uint nstroke);

        [DllImport("interception.dll", EntryPoint = "interception_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(IntPtr context, int device, ref Stroke stroke, uint nstroke);

        [DllImport("interception.dll", EntryPoint = "interception_is_keyboard", CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsKeyboard(int device);

        [DllImport("interception.dll", EntryPoint = "interception_is_mouse", CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsMouse(int device);

        [DllImport("interception.dll", EntryPoint = "interception_is_invalid", CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsInvalid(int device);

        //InterceptionDevice INTERCEPTION_API interception_wait(InterceptionContext context);
        [DllImport("interception.dll", EntryPoint = "interception_wait", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Wait(IntPtr context);

        //InterceptionDevice INTERCEPTION_API interception_wait_with_timeout(InterceptionContext context, unsigned long milliseconds);
        [DllImport("interception.dll", EntryPoint = "interception_wait_with_timeout", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int WaitWithTimeout(int device, ulong milliseconds);
        public static extern int WaitWithTimeout(IntPtr context, ulong milliseconds);

        // unsigned int INTERCEPTION_API interception_get_hardware_id(InterceptionContext context, InterceptionDevice device, void *hardware_id_buffer, unsigned int buffer_size);
        [DllImport("interception.dll", EntryPoint = "interception_get_hardware_id", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetHardwareID(IntPtr context, int device, IntPtr hardwareidbuffer, uint buffersize);
        // public static extern uint GetHardwareID(IntPtr context, int device, [MarshalAs(UnmanagedType.ByValArray,SizeConst=500)]char[] hardwareidbuffer, uint buffersize);
        //public static extern uint GetHardwareID(IntPtr context, int device, ref _wchar_t[] hardwareidbuffer, uint buffersize);

        public static string GetHardwareStr(IntPtr context, int device, int chars = 0)
        {
            if (chars == 0)
                chars = 500;
            String result = "";
            IntPtr bufferptr = Marshal.StringToHGlobalUni(new string(new char[chars]));
            uint length = GetHardwareID(context, device, bufferptr, (uint)(chars * sizeof(char)));
            if (length > 0 && length < (chars * sizeof(char)))
                result = Marshal.PtrToStringAuto(bufferptr);
            Marshal.FreeHGlobal(bufferptr);
            return result;
        }

        /*
        InterceptionContext INTERCEPTION_API interception_create_context(void);
        void INTERCEPTION_API interception_destroy_context(InterceptionContext context);
        InterceptionPrecedence INTERCEPTION_API interception_get_precedence(InterceptionContext context, InterceptionDevice device);
        void INTERCEPTION_API interception_set_precedence(InterceptionContext context, InterceptionDevice device, InterceptionPrecedence precedence);
        InterceptionFilter INTERCEPTION_API interception_get_filter(InterceptionContext context, InterceptionDevice device);
        void INTERCEPTION_API interception_set_filter(InterceptionContext context, InterceptionPredicate predicate, InterceptionFilter filter);
        InterceptionDevice INTERCEPTION_API interception_wait(InterceptionContext context);
        InterceptionDevice INTERCEPTION_API interception_wait_with_timeout(InterceptionContext context, unsigned long milliseconds);
        int INTERCEPTION_API interception_send(InterceptionContext context, InterceptionDevice device, const InterceptionStroke *stroke, unsigned int nstroke);
        int INTERCEPTION_API interception_receive(InterceptionContext context, InterceptionDevice device, InterceptionStroke *stroke, unsigned int nstroke);
        unsigned int INTERCEPTION_API interception_get_hardware_id(InterceptionContext context, InterceptionDevice device, void *hardware_id_buffer, unsigned int buffer_size);
        int INTERCEPTION_API interception_is_invalid(InterceptionDevice device);
        int INTERCEPTION_API interception_is_keyboard(InterceptionDevice device);
        int INTERCEPTION_API interception_is_mouse(InterceptionDevice device);
        */

        [DllImport("user32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int GetKeyNameTextW(uint lParam, StringBuilder lpString, int nSize);

        #endregion
    }
}
