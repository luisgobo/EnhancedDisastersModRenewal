using System;
using System.Collections.Generic;
using NaturalDisastersRenewal.Common.enums;
using UnityEngine;

namespace NaturalDisastersRenewal.Common
{
    public static class Helper
    {
        private const float RealTimeCompatibilityFactor = 365f;
        private const float VanillaSimulationDaysPerFrame = 1f / 585f;

        private static float GameDaysPerFrame => (float)Services.Simulation.m_timePerFrame.TotalDays;

        public static float GetDaysPerFrame(TimeBehaviorMode mode)
        {
            switch (mode)
            {
                case TimeBehaviorMode.RealTimeCompatible:
                    return GameDaysPerFrame * RealTimeCompatibilityFactor;
                case TimeBehaviorMode.VanillaSimulationCompatible:
                    return VanillaSimulationDaysPerFrame;
                default:
                    return GameDaysPerFrame;
            }
        }
        
        public static string[] GetMonths()
        {
            return LocalizationService.GetMonths();
        }

        public static string[] GetAllEvacuationOptions(bool allowsFocusedEvacuation = false)
        {
            string[] evacuationOptions =
            [
                LocalizationService.Get("evacuation.manual"),
                LocalizationService.Get("evacuation.auto")
            ];

            if (!allowsFocusedEvacuation) return evacuationOptions;
            
            Array.Resize(ref evacuationOptions, evacuationOptions.Length + 1);
            evacuationOptions[evacuationOptions.Length - 1] = LocalizationService.Get("evacuation.focused");

            return evacuationOptions;
        }
        
        public static string[] GetManualAndFocusedEvacuationOptions()
        {
            string[] evacuationOptions =
            [
                LocalizationService.Get("evacuation.manual"),
                LocalizationService.Get("evacuation.focused")
            ];

            return evacuationOptions;
        }

        public static string[] GetCrackModes()
        {
            string[] crackModes =
            [
                LocalizationService.Get("cracks.none"),
                LocalizationService.Get("cracks.always"),
                LocalizationService.Get("cracks.byIntensity")
            ];            
            return crackModes;
        }

        public static float FramesPerDay => 1.0f / DaysPerFrame;

        public static float DaysPerFrame => (float)Services.Simulation.m_timePerFrame.TotalDays;

        public static string FormatTimeSpan(float daysFloat)
        {
            switch (daysFloat)
            {
                case <= 0:
                    return FormatValue(0, "time.day.singular", "time.day.plural");
                case < 1:
                    return LocalizationService.Get("time.lessThanOneDay");
                case < 60:
                    return FormatValue(daysFloat, "time.day.singular", "time.day.plural");
            }

            var months = Mathf.FloorToInt(daysFloat / 30);
            if (months < 13)
            {
                if (months > 3) return FormatValue(months, "time.month.singular", "time.month.plural");
                
                var days = Mathf.FloorToInt(daysFloat - months * 30);
                if (days == 0) return FormatValue(months, "time.month.singular", "time.month.plural");

                return FormatValue(months, "time.month.singular", "time.month.plural") + " " + LocalizationService.Get("time.and") + " " +
                       FormatValue(days, "time.day.singular", "time.day.plural");
            }

            var years = Mathf.FloorToInt(daysFloat / 365);
            months = Mathf.FloorToInt((daysFloat - years * 365) / 30);

            if (years > 5 || months == 0) return FormatValue(years, "time.year.singular", "time.year.plural");

            return FormatValue(years, "time.year.singular", "time.year.plural") + " " + LocalizationService.Get("time.and") + " " +
                   FormatValue(months, "time.month.singular", "time.month.plural");
        }

        private static string FormatValue(float value, string singularKey, string pluralKey)
        {
            return FormatValue(Mathf.FloorToInt(value), singularKey, pluralKey);
        }

        private static string FormatValue(int value, string singularKey, string pluralKey)
        {
            return value + " " + LocalizationService.Get(value == 1 ? singularKey : pluralKey);
        }

        public static int GetPopulation()
        {
            var districts = Services.Districts;
            if (districts)
            {
                return (int)districts.m_districts.m_buffer[0].m_populationData.m_finalCount;
            }

            return 0;
        }

        public static EventModifiers GetSupportedHotkeyModifiers(EventModifiers modifiers)
        {
            return modifiers & (EventModifiers.Control | EventModifiers.Shift | EventModifiers.Alt | EventModifiers.Command);
        }

        public static int CountHotkeyModifiers(EventModifiers modifiers)
        {
            var normalized = GetSupportedHotkeyModifiers(modifiers);
            var count = 0;

            if ((normalized & EventModifiers.Control) != 0)
                count++;
            if ((normalized & EventModifiers.Alt) != 0)
                count++;
            if ((normalized & EventModifiers.Shift) != 0)
                count++;
            if ((normalized & EventModifiers.Command) != 0)
                count++;

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

        public static bool MatchesHotkey(KeyCode configuredKeyCode, EventModifiers configuredModifiers, KeyCode keyCode,
            EventModifiers modifiers)
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

            var parts = new List<string>();
            var normalized = GetSupportedHotkeyModifiers(modifiers);

            if ((normalized & EventModifiers.Control) != 0)
                parts.Add(LocalizationService.Get("key.ctrl"));
            if ((normalized & EventModifiers.Alt) != 0)
                parts.Add(LocalizationService.Get("key.alt"));
            if ((normalized & EventModifiers.Shift) != 0)
                parts.Add(LocalizationService.Get("key.shift"));
            if ((normalized & EventModifiers.Command) != 0)
                parts.Add(LocalizationService.Get("key.command"));

            parts.Add(GetKeyDisplayName(keyCode));
            return string.Join(" + ", parts.ToArray());
        }

        private static string GetKeyDisplayName(KeyCode keyCode)
        {
            var keyName = keyCode.ToString();

            if (keyName.StartsWith("Alpha"))
                return keyName.Substring("Alpha".Length);

            if (keyName.StartsWith("Keypad"))
                return LocalizationService.Format("settings.hotkey.keypad", keyName.Substring("Keypad".Length));

            switch (keyCode)
            {
                case KeyCode.BackQuote:
                    return "`";
                case KeyCode.Minus:
                    return "-";
                case KeyCode.Equals:
                    return "=";
                case KeyCode.LeftBracket:
                    return "[";
                case KeyCode.RightBracket:
                    return "]";
                case KeyCode.Backslash:
                    return "\\";
                case KeyCode.Semicolon:
                    return ";";
                case KeyCode.Quote:
                    return "'";
                case KeyCode.Comma:
                    return ",";
                case KeyCode.Period:
                    return ".";
                case KeyCode.Slash:
                    return "/";
                case KeyCode.UpArrow:
                    return LocalizationService.Get("key.up");
                case KeyCode.DownArrow:
                    return LocalizationService.Get("key.down");
                case KeyCode.LeftArrow:
                    return LocalizationService.Get("key.left");
                case KeyCode.RightArrow:
                    return LocalizationService.Get("key.right");
                case KeyCode.PageUp:
                    return LocalizationService.Get("key.pageUp");
                case KeyCode.PageDown:
                    return LocalizationService.Get("key.pageDown");
                default:
                    return keyName;
            }
        }
    }
}
