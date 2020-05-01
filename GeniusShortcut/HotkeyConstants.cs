﻿namespace Hotkeys
{
    public static class HotKeyConstants
    {
        //Key modifiers

        //No modifier
        public const int NOMOD = 0x0000;

        public const int ALT = 0x0001;
        public const int CTRL = 0x0002;
        public const int SHIFT = 0x0004;
        public const int WIN = 0x0008;

        //Windows message ID for hotkey
        public const int WM_HOTKEY_MSG_ID = 0x0312;
    }
}