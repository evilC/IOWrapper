using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HidWizards.IOWrapper.DataTransferObjects
{
    // Reports allow the back-end to tell the front-end what capabilities are available
    // Reports comprise of two parts:
    // Descriptors allow the front-end to subscribe to Bindings
    // Other meta-data allows the front-end to interpret capabilities, report style etc

    #region Provider Report

    /// <summary>
    /// Contains information about each provider
    /// </summary>
    public class ProviderReport
    {
        /// <summary>
        /// The human-friendly name of the Provider
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A description of what the Provider does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Contains information needed to subscribe to this Provider
        /// </summary>
        public ProviderDescriptor ProviderDescriptor { get; set; }

        /// <summary>
        /// The underlying API that handles this input
        /// It is intended that many providers could support a given API
        /// This meta-data is intended to allow the front-end to present a user with alternatives
        /// </summary>
        public string API { get; set; }

        public List<DeviceReport> Devices { get; set; } = new List<DeviceReport>();
    }

    #endregion

    #region Device Reports

    /// <summary>
    /// Contains information about each Device on a Provider
    /// </summary>
    public class DeviceReport
    {
        /// <summary>
        /// The human-friendly name of the device
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// If set, this is a HID device, and this string uniquely identifies the device, even among duplicate devices.
        /// If port order of devices change, or system reboots etc, may change, unlike the DeviceDescriptor, which should remain constant
        /// </summary>
        public string HidPath { get; set; }

        /// <summary>
        /// Contains information needed to subscribe to this Device via the Provider
        /// </summary>
        public DeviceDescriptor DeviceDescriptor { get; set; }

        //public List<BindingInfo> Bindings { get; set; } = new List<BindingInfo>();

        /// <summary>
        /// Nodes give the device report structure and allow the front-end to logically group items
        /// </summary>
        public List<DeviceReportNode> Nodes { get; set; } = new List<DeviceReportNode>();
    }

    #region Binding Report

    /// <summary>
    /// Contains information about each Binding on a Device on a Provider
    /// </summary>
    public class BindingReport
    {
        /// <summary>
        /// Used by the front-end to build the menu entry for this button
        /// eg "Up"
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Used by the front-end to display a full name for the binding
        /// eg "POVs > POV1 > Up"
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Meta-Data to allow the front-end to interpret the Binding
        /// </summary>
        public BindingCategory Category { get; set; }

        /// <summary>
        /// Contains information needed to subscribe to this Binding
        /// </summary>
        public BindingDescriptor BindingDescriptor { get; set; }
    }
    #endregion

    /// <summary>
    /// Used as a sub-node, to logically group Bindings
    /// </summary>
    public class DeviceReportNode
    {
        /// <summary>
        /// Human-friendly name of this node
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Sub-Nodes
        /// </summary>
        public List<DeviceReportNode> Nodes { get; set; } = new List<DeviceReportNode>();

        /// <summary>
        /// Bindings contained in this node
        /// </summary>
        public List<BindingReport> Bindings { get; set; } = new List<BindingReport>();
    }

    #endregion
}
