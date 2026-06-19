using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.Common
{
    public static class HotkeyHelper
    {
        public static EventModifiers GetSupportedHotkeyModifiers(EventModifiers modifiers)
        {
            return modifiers & (EventModifiers.Control | EventModifiers.Shift | EventModifiers.Alt | EventModifiers.Command);
        }

        public static int CountHotkeyModifiers(EventModifiers modifiers)
        {
            EventModifiers normalized = GetSupportedHotkeyModifiers(modifiers);
            int count = 0;

            if ((normalized & EventModifiers.Control) != 0) count++;
            if ((normalized & EventModifiers.Alt) != 0) count++;
            if ((normalized & EventModifiers.Shift) != 0) count++;
            if ((normalized & EventModifiers.Command) != 0) count++;

            return count;
        }

        public static bool IsModifierKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.LeftShift:
                case KeyCode.RightShift:
                case KeyCode.LeftControl:
                case KeyCode.RightControl:
                case KeyCode.LeftAlt:
                case KeyCode.RightAlt:
                case KeyCode.LeftCommand:
                case KeyCode.RightCommand:
                case KeyCode.AltGr:
                    return true;
                default:
                    return false;
            }
        }

        public static bool MatchesHotkey(KeyCode configuredKeyCode, EventModifiers configuredModifiers, KeyCode keyCode, EventModifiers modifiers)
        {
            if (configuredKeyCode == KeyCode.None)
                return false;

            return keyCode == configuredKeyCode &&
                   GetSupportedHotkeyModifiers(modifiers) == GetSupportedHotkeyModifiers(configuredModifiers);
        }

        public static string FormatHotkey(KeyCode keyCode, EventModifiers modifiers)
        {
            if (keyCode == KeyCode.None)
                return LocalizationService.Get("settings.hotkey.none");

            List<string> parts = new List<string>();
            EventModifiers normalized = GetSupportedHotkeyModifiers(modifiers);

            if ((normalized & EventModifiers.Control) != 0) parts.Add(LocalizationService.Get("key.ctrl"));
            if ((normalized & EventModifiers.Alt) != 0) parts.Add(LocalizationService.Get("key.alt"));
            if ((normalized & EventModifiers.Shift) != 0) parts.Add(LocalizationService.Get("key.shift"));
            if ((normalized & EventModifiers.Command) != 0) parts.Add(LocalizationService.Get("key.command"));

            parts.Add(GetKeyDisplayName(keyCode));
            return string.Join(" + ", parts.ToArray());
        }

        private static string GetKeyDisplayName(KeyCode keyCode)
        {
            string keyName = keyCode.ToString();

            if (keyName.StartsWith("Alpha"))
                return keyName.Substring("Alpha".Length);

            if (keyName.StartsWith("Keypad"))
                return LocalizationService.Format("settings.hotkey.keypad", keyName.Substring("Keypad".Length));

            switch (keyCode)
            {
                case KeyCode.UpArrow: return LocalizationService.Get("key.up");
                case KeyCode.DownArrow: return LocalizationService.Get("key.down");
                case KeyCode.LeftArrow: return LocalizationService.Get("key.left");
                case KeyCode.RightArrow: return LocalizationService.Get("key.right");
                case KeyCode.PageUp: return LocalizationService.Get("key.page_up");
                case KeyCode.PageDown: return LocalizationService.Get("key.page_down");
                case KeyCode.BackQuote: return "`";
                case KeyCode.Minus: return "-";
                case KeyCode.Equals: return "=";
                case KeyCode.LeftBracket: return "[";
                case KeyCode.RightBracket: return "]";
                case KeyCode.Backslash: return "\\";
                case KeyCode.Semicolon: return ";";
                case KeyCode.Quote: return "'";
                case KeyCode.Comma: return ",";
                case KeyCode.Period: return ".";
                case KeyCode.Slash: return "/";
                default: return keyName;
            }
        }
    }
}
