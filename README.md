# IOWrapper
IOWrapper is the "Back End" for Universal Control Remapper (UCR) 2 , handling all the device interaction.  
Technically, however, IOWrapper is a stand-alone project which is not inherently coupled to the UCR front-end.  

IOWrapper is extensible through "Providers" which are C# classes which derive from an `IProvider` interface.  
A Provider typically targets a specific API - eg DirectInput, XInput, vJoy, ViGEm etc.  

**Unless you are a developer, this project is probably of no use to you. End Users should download UCR instead**

[UCR Repository](https://github.com/Snoothy/UCR)
