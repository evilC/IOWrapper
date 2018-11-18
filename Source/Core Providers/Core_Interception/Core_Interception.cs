using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using Core_Interception.DeviceLibrary;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace Core_Interception
{
    [Export(typeof(IProvider))]
    public class Core_Interception : IInputProvider, IOutputProvider, IBindModeProvider
    {
        private readonly IInputOutputDeviceLibrary<int> _deviceLibrary;
        private Action<ProviderDescriptor, DeviceDescriptor, BindingReport, int> _bindModeCallback;
        private readonly ProviderDescriptor _providerDescriptor;

        public bool IsLive { get; } = false;

        private bool _disposed;
        private readonly IntPtr _deviceContext;
        //private ProviderReport providerReport;

        // The thread which handles input detection
        private Thread _pollThread;
        // Is the thread currently running? This is set by the thread itself.
        private volatile bool _pollThreadRunning;
        // Do we want the thread to be on or off?
        // This is independent of whether or not the thread is running...
        // ... for example, we may be updating bindings, so the thread may be temporarily stopped
        private bool _pollThreadDesired;
        // Set to true to cause the thread to stop running. When it stops, it will set pollThreadRunning to false
        private volatile bool pollThreadStopRequested;

        private bool _filterState = false;

        private bool _blockingEnabled;
        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private readonly ConcurrentDictionary<int, IDeviceHandler<ManagedWrapper.Stroke>> _monitoredKeyboards = new ConcurrentDictionary<int, IDeviceHandler<ManagedWrapper.Stroke>>();
        private readonly ConcurrentDictionary<int, IDeviceHandler<ManagedWrapper.Stroke>> _monitoredMice = new ConcurrentDictionary<int, IDeviceHandler<ManagedWrapper.Stroke>>();

        public Core_Interception()
        {
            _providerDescriptor = new ProviderDescriptor
            {
                ProviderName = ProviderName
            };
            _deviceLibrary = new IceptDeviceLibrary(_providerDescriptor);

            ProcessSettingsFile();

            _deviceContext = ManagedWrapper.CreateContext();

            _pollThreadDesired = true;
        }

        ~Core_Interception()
        {
            Dispose();
        }

        #region Poll Thread Management and Device Filtering

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                SetPollThreadState(false);
            }
            _disposed = true;
            HelperFunctions.Log("Provider {0} was Disposed", ProviderName);
        }

        /// <summary>
        /// Turns on / off filtering of devices
        /// Any filtered device will be blocked if the provider crashes
        /// Also controls which devices are filtered when filtering is on
        /// </summary>
        /// <param name="state">Set to true to turn filtering on</param>
        private void SetFilterState(bool state)
        {
            if (state && !_filterState)
            {
                ManagedWrapper.SetFilter(_deviceContext, IsMonitoredKeyboard, ManagedWrapper.Filter.All);
                ManagedWrapper.SetFilter(_deviceContext, IsMonitoredMouse, ManagedWrapper.Filter.All);
            }
            else if (!state && _filterState)
            {
                ManagedWrapper.SetFilter(_deviceContext, IsMonitoredKeyboard, ManagedWrapper.Filter.None);
                ManagedWrapper.SetFilter(_deviceContext, IsMonitoredMouse, ManagedWrapper.Filter.None);
            }
        }

        private int IsMonitoredKeyboard(int device)
        {
            return Convert.ToInt32(_monitoredKeyboards.ContainsKey(device));
        }

        private int IsMonitoredMouse(int device)
        {
            return Convert.ToInt32(_monitoredMice.ContainsKey(device));
        }

        private void SetPollThreadState(bool state)
        {
            if (state && !_pollThreadRunning)
            {
                SetFilterState(true);
                pollThreadStopRequested = false;
                _pollThread = new Thread(() => PollThread(_blockingEnabled));
                _pollThread.Start();
                while (!_pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
                HelperFunctions.Log("Started PollThread for {0}", ProviderName);
            }
            else if (!state && _pollThreadRunning)
            {
                SetFilterState(false);
                pollThreadStopRequested = true;
                while (_pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
                _pollThread = null;
                HelperFunctions.Log("Stopped PollThread for {0}", ProviderName);
            }
        }
        #endregion

        #region Querying

        public ProviderReport GetInputList()
        {
            return _deviceLibrary.GetInputList();
        }

        public ProviderReport GetOutputList()
        {
            return _deviceLibrary.GetOutputList();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return _deviceLibrary.GetInputDeviceReport(subReq.DeviceDescriptor);
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return _deviceLibrary.GetOutputDeviceReport(subReq.DeviceDescriptor);
        }

        #endregion

        #region Input Subscriptions

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            var ret = false;
            try
            {
                if (_pollThreadRunning)
                    SetPollThreadState(false);

                var devId = _deviceLibrary.GetInputDeviceIdentifier(subReq.DeviceDescriptor);
                if (HelperFunctions.IsKeyboard(devId))
                {
                    EnsureMonitoredKeyboardExists(devId, subReq.DeviceDescriptor);
                    _monitoredKeyboards[devId].SubscribeInput(subReq);
                    ret = true;
                }
                else
                {
                    EnsureMonitoredMouseExists(devId, subReq.DeviceDescriptor);
                    _monitoredMice[devId].SubscribeInput(subReq);
                    ret = true;
                }

            }
            catch
            {
                ret = false;
            }
            if (_pollThreadDesired)
                SetPollThreadState(true);
            return ret;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            var ret = false;

            try
            {
                var devId = _deviceLibrary.GetInputDeviceIdentifier(subReq.DeviceDescriptor);
                if (_pollThreadRunning)
                    SetPollThreadState(false);

                if (HelperFunctions.IsKeyboard(devId))
                {
                    ret = true;
                    _monitoredKeyboards[devId].UnsubscribeInput(subReq);
                }
                else
                {
                    ret = true;
                    _monitoredMice[devId].UnsubscribeInput(subReq);
                }

                if (_pollThreadDesired)
                    SetPollThreadState(true);

            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        #endregion

        #region Output Subscriptions

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return true;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return true;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            int devId;
            try
            {
                devId = _deviceLibrary.GetInputDeviceIdentifier(subReq.DeviceDescriptor);
            }
            catch
            {
                return false;
            }
            //Log("SetOutputState. Type: {0}, Index: {1}, State: {2}, Device: {3}", inputType, inputIndex, state, devId);
            var stroke = new ManagedWrapper.Stroke();
            if (HelperFunctions.IsKeyboard(devId))
            {
                var st = (ushort)(1 - state);
                var code = (ushort)(bindingDescriptor.Index + 1);
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
                switch (bindingDescriptor.Type)
                {
                    case BindingType.Axis:
                        var mouse = new ManagedWrapper.MouseStroke
                        {
                            //ToDo: This only implements mouse relative mode - can we allow absolute mode too?
                            flags = (ushort)ManagedWrapper.MouseFlag.MouseMoveRelative
                        };
                        if (bindingDescriptor.Index != 0)
                            if (bindingDescriptor.Index == 1)
                            {
                                mouse.y = state;
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        else
                            mouse.x = state;

                        stroke.mouse = mouse;
                        break;
                    case BindingType.Button:
                        var btn = bindingDescriptor.Index;
                        var flag = (int)ManagedWrapper.MouseButtonFlags[btn];
                        if (btn < 5)
                        {
                            // Regular buttons
                            if (state == 0) flag *= 2;
                        }
                        else
                        {
                            // Wheel
                            stroke.mouse.rolling = (short)((btn == 5 || btn == 8) ? 120 : -120);
                        }

                        stroke.mouse.state = (ushort)flag;
                        break;
                    case BindingType.POV:
                    default:
                        throw new NotImplementedException();
                }
            }
            ManagedWrapper.Send(_deviceContext, devId, ref stroke, 1);
            return true;
        }


        #endregion

        #region Event Handlers

        private void BindModeHandler(object sender, BindModeUpdate e)
        {
            _bindModeCallback?.Invoke(_providerDescriptor, e.Device, e.Binding, e.Value);
        }

        private void MouseEmptyHandler(object sender, DeviceDescriptor e)
        {
            var id = _deviceLibrary.GetInputDeviceIdentifier(e);
            if (_pollThreadRunning)
                SetPollThreadState(false);
            _monitoredMice[id].Dispose();
            _monitoredMice.TryRemove(id, out _);
            if (_pollThreadDesired)
                SetPollThreadState(true);
        }

        private void KeyboardEmptyHandler(object sender, DeviceDescriptor e)
        {
            var id = _deviceLibrary.GetInputDeviceIdentifier(e);
            if (_pollThreadRunning)
                SetPollThreadState(false);
            _monitoredKeyboards[id].Dispose();
            _monitoredKeyboards.TryRemove(id, out _);
            if (_pollThreadDesired)
                SetPollThreadState(true);
        }
        #endregion

        #region Misc IProvider Members

        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName => typeof(Core_Interception).Namespace;

        public void RefreshLiveState()
        {

        }

        public void RefreshDevices()
        {
            _deviceLibrary.RefreshConnectedDevices();
        }

        #endregion

        #region Bind Mode

        public void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingReport, int> callback = null)
        {
            if (_pollThreadRunning)
                SetPollThreadState(false);

            int devId;
            try
            {
                devId = _deviceLibrary.GetInputDeviceIdentifier(deviceDescriptor);
            }
            catch
            {
                return;
            }
            if (HelperFunctions.IsKeyboard(devId))
            {
                if (detectionMode == DetectionMode.Bind)
                {
                    EnsureMonitoredKeyboardExists(devId, deviceDescriptor);
                    _bindModeCallback = callback;
                }
                else if (!_monitoredKeyboards.ContainsKey(devId))
                {
                    return;
                }

                _monitoredKeyboards[devId].SetDetectionMode(detectionMode);
            }
            else
            {
                if (detectionMode == DetectionMode.Bind)
                {
                    EnsureMonitoredMouseExists(devId, deviceDescriptor);
                    _bindModeCallback = callback;
                }
                else if (!_monitoredMice.ContainsKey(devId))
                {
                    return;
                }

                _monitoredMice[devId].SetDetectionMode(detectionMode);
            }

            if (_pollThreadDesired)
                SetPollThreadState(true);
        }

        #endregion

        #region PollThread
        private void PollThread(bool blockingEnabled)
        {
            _pollThreadRunning = true;

            var stroke = new ManagedWrapper.Stroke();

            while (!pollThreadStopRequested)
            {
                for (var i = 1; i < 11; i++)
                {
                    var isMonitoredKeyboard = _monitoredKeyboards.ContainsKey(i);

                    while (ManagedWrapper.Receive(_deviceContext, i, ref stroke, 1) > 0)
                    {
                        var block = false;
                        if (isMonitoredKeyboard)
                        {
                            //block = _monitoredKeyboards[i].Poll(stroke);
                            block = _monitoredKeyboards[i].ProcessUpdate(stroke);
                        }
                        if (!(blockingEnabled && block))
                        {
                            ManagedWrapper.Send(_deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                for (var i = 11; i < 21; i++)
                {
                    var isMonitoredMouse = _monitoredMice.ContainsKey(i);

                    while (ManagedWrapper.Receive(_deviceContext, i, ref stroke, 1) > 0)
                    {
                        var block = false;
                        if (isMonitoredMouse)
                        {
                            block = _monitoredMice[i].ProcessUpdate(stroke);
                        }
                        if (!(blockingEnabled && block))
                        {
                            ManagedWrapper.Send(_deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                Thread.Sleep(1);
            }
            _pollThreadRunning = false;
        }
        #endregion

        #region Dictionary management

        private void EnsureMonitoredKeyboardExists(int devId, DeviceDescriptor deviceDescriptor)
        {
            if (_monitoredKeyboards.ContainsKey(devId)) return;
            _monitoredKeyboards.TryAdd(devId, new IceptKeyboardHandler(deviceDescriptor, KeyboardEmptyHandler, BindModeHandler, _deviceLibrary));
        }

        private void EnsureMonitoredMouseExists(int devId, DeviceDescriptor deviceDescriptor)
        {
            if (_monitoredMice.ContainsKey(devId)) return;
            _monitoredMice.TryAdd(devId, new IceptMouseHandler(deviceDescriptor, MouseEmptyHandler, BindModeHandler, _deviceLibrary));
        }


        #endregion

        #region XML Settings
        private void ProcessSettingsFile()
        {
            var settingsFile = Path.Combine(AssemblyDirectory, "Settings.xml");
            _blockingEnabled = false;
            if (File.Exists(settingsFile))
            {
                var doc = new XmlDocument();
                doc.Load(settingsFile);

                try
                {
                    _blockingEnabled = Convert.ToBoolean(doc.SelectSingleNode("/Settings/Setting[Name = \"BlockingEnabled\"]")
                        ?.SelectSingleNode("Value")
                        ?.InnerText);
                }
                catch
                {
                    // ignored
                }
            }
            HelperFunctions.Log("Blocking Enabled: {0}", _blockingEnabled);
        }
        #endregion
    }
}
