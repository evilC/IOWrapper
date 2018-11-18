using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates
{
    /// <summary>
    /// Performs two main functions:
    /// 1) Converts from raw values to normalized values (eg in DirectInput, converts 0..65535 to -32768..-32767
    ///     In this case, typically one <see cref="IUpdateProcessor"/> per <see cref="BindingType"/> is required
    /// 2) Calculates states of Logical inputs (eg POVs) from Physical values
    ///     In this case, typically one <see cref="IUpdateProcessor"/> per input is required
    /// Interface for handling Button/Axis value normalization, POV physical to logical values etc
    /// </summary>
    public interface IUpdateProcessor
    {
        // Converts an update for one physical input into one or more updates
        // Eg POV changing angle from 0 to 90 deg would cause two updates - North release and East press
        // In most other cases, the array will be of 1 length
        BindingUpdate[] Process(BindingUpdate update);
    }
}
