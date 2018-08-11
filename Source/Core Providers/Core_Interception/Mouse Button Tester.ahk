#SingleInstance force
#MaxHotkeysPerInterval 9999

~LButton::ToolTip % A_ThisHotkey
~LButton up::ToolTip % A_ThisHotkey

~RButton::ToolTip % A_ThisHotkey
~RButton up::ToolTip % A_ThisHotkey

~MButton::ToolTip % A_ThisHotkey
~MButton up::ToolTip % A_ThisHotkey

~XButton1::ToolTip % A_ThisHotkey
~XButton1 up::ToolTip % A_ThisHotkey

~XButton2::ToolTip % A_ThisHotkey
~XButton2 up::ToolTip % A_ThisHotkey

~WheelUp::ToolTip % A_ThisHotkey
~WheelDown::ToolTip % A_ThisHotkey
~WheelLeft::ToolTip % A_ThisHotkey
~WheelRight::ToolTip % A_ThisHotkey

^Esc::ExitApp