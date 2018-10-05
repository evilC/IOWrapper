using System;
using HidWizards.IOWrapper.DataTransferObjects;

namespace SubscriptionHandlingTests.SubscriptionHandler.Lookups
{
    public static class Devices
    {
        public static DeviceDescriptor One = new DeviceDescriptor { DeviceHandle = "Test Device 1" };
    }

    public static class Subscribers
    {
        public static SubscriptionDescriptor One = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid() };
        public static SubscriptionDescriptor Two = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid() };
    }

    public static class Bindings
    {
        public static BindingDescriptor Button1 = new BindingDescriptor { Type = BindingType.Button, Index = 0 };
        public static BindingDescriptor Button2 = new BindingDescriptor { Type = BindingType.Button, Index = 1 };
        public static BindingDescriptor Axis1 = new BindingDescriptor { Type = BindingType.Axis, Index = 0 };
        public static BindingDescriptor Axis2 = new BindingDescriptor { Type = BindingType.Axis, Index = 1 };
        public static BindingDescriptor Pov1Up = new BindingDescriptor { Type = BindingType.POV, Index = 0, SubIndex = 0 };
        public static BindingDescriptor Pov1Right = new BindingDescriptor { Type = BindingType.POV, Index = 0, SubIndex = 1 };
        public static BindingDescriptor Pov2Up = new BindingDescriptor { Type = BindingType.POV, Index = 1, SubIndex = 0 };
        public static BindingDescriptor Pov2Right = new BindingDescriptor { Type = BindingType.POV, Index = 1, SubIndex = 1 };
    }

    public static class SubReqs
    {
        public static InputSubReq Button1 = new InputSubReq { Name = nameof(Button1), BindingDescriptor = Bindings.Button1, DeviceDescriptor = Devices.One, SubscriptionDescriptor = Subscribers.One };
        public static InputSubReq Button1Subscriber2 = new InputSubReq { Name = nameof(Button1Subscriber2), BindingDescriptor = Bindings.Button1, DeviceDescriptor = Devices.One, SubscriptionDescriptor = Subscribers.Two };
        public static InputSubReq Button2 = new InputSubReq { Name = nameof(Button2), BindingDescriptor = Bindings.Button2, DeviceDescriptor = Devices.One, SubscriptionDescriptor = Subscribers.One };
        public static InputSubReq Axis1 = new InputSubReq { Name = nameof(Axis1), BindingDescriptor = Bindings.Axis1, DeviceDescriptor = Devices.One, SubscriptionDescriptor = Subscribers.One };
        public static InputSubReq Axis2 = new InputSubReq { Name = nameof(Axis2), BindingDescriptor = Bindings.Axis2, DeviceDescriptor = Devices.One, SubscriptionDescriptor = Subscribers.One };
        public static InputSubReq Pov1Up = new InputSubReq { Name = nameof(Pov1Up), BindingDescriptor = Bindings.Pov1Up, DeviceDescriptor = Devices.One, SubscriptionDescriptor = Subscribers.One };
        public static InputSubReq Pov1Right = new InputSubReq { Name = nameof(Pov1Right), BindingDescriptor = Bindings.Pov1Right, DeviceDescriptor = Devices.One, SubscriptionDescriptor = Subscribers.One };
        public static InputSubReq Pov2Up = new InputSubReq { Name = nameof(Pov2Up), BindingDescriptor = Bindings.Pov2Up, DeviceDescriptor = Devices.One, SubscriptionDescriptor = Subscribers.One };
        public static InputSubReq Pov2Right = new InputSubReq { Name = nameof(Pov2Right), BindingDescriptor = Bindings.Pov2Right, DeviceDescriptor = Devices.One, SubscriptionDescriptor = Subscribers.One };
    }

    public class InputSubReq
    {
        public string Name { get; set; }
        public DeviceDescriptor DeviceDescriptor { get; set; }
        public BindingDescriptor BindingDescriptor { get; set; }
        public SubscriptionDescriptor SubscriptionDescriptor { get; set; }
    }

}
