using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ColossalFramework.Plugins;
using ICities;

namespace NaturalDisastersRenewal.Common
{
    public static class ModCompatibilityService
    {
        private const string CompatibleModsResourceName = "NaturalDisastersRenewal.Resources.Files.CompatibleMods.json";

        private static Dictionary<string, CompatibleModDefinition> _compatibleMods;
        private static List<CompatibleModMatch> _activeMatches;
        private static HashSet<string> _activeKeys;

        public static void Refresh()
        {
            var modDefinitions = GetCompatibleMods();
            _activeMatches = DetectActiveMatches(modDefinitions);
            _activeKeys = new HashSet<string>();

            for (var i = 0; i < _activeMatches.Count; i++)
                _activeKeys.Add(_activeMatches[i].Key);
        }

        public static void Reset()
        {
            _activeMatches = null;
            _activeKeys = null;
        }

        public static bool IsActive(string modKey)
        {
            EnsureActiveMatchesLoaded();
            return _activeKeys != null && _activeKeys.Contains(modKey);
        }

        public static List<CompatibleModMatch> GetActiveMatches()
        {
            EnsureActiveMatchesLoaded();
            return _activeMatches != null
                ? new List<CompatibleModMatch>(_activeMatches)
                : new List<CompatibleModMatch>();
        }

        private static void EnsureActiveMatchesLoaded()
        {
            if (_activeMatches == null || _activeKeys == null)
                Refresh();
        }

        private static List<CompatibleModMatch> DetectActiveMatches(
            Dictionary<string, CompatibleModDefinition> modDefinitions)
        {
            var matches = new List<CompatibleModMatch>();
            if (modDefinitions.Count == 0)
                return matches;

            var pluginManager = Services.Plugins;
            if (!pluginManager)
                return matches;

            foreach (var plugin in pluginManager.GetPluginsInfo())
            {
                if (plugin == null || plugin.userModInstance == null || !plugin.isEnabled)
                    continue;

                var userMod = plugin.userModInstance as IUserMod;
                var publishedFileId = plugin.publishedFileID.AsUInt64;

                AddDetectedMod(modDefinitions, userMod, plugin, publishedFileId, matches);
            }

            return matches;
        }

        private static void AddDetectedMod(
            Dictionary<string, CompatibleModDefinition> modDefinitions,
            IUserMod userMod,
            PluginManager.PluginInfo plugin,
            ulong publishedFileId,
            ICollection<CompatibleModMatch> matches)
        {
            foreach (var modDefinition in modDefinitions)
            {
                if (!MatchesCompatibleMod(userMod != null ? userMod.Name : null, plugin.name, publishedFileId,
                        modDefinition.Value))
                    continue;

                matches.Add(new CompatibleModMatch
                {
                    Key = modDefinition.Key,
                    ModName = userMod != null ? userMod.Name : null,
                    PluginName = plugin.name,
                    WorkshopId = publishedFileId
                });
            }
        }

        private static bool MatchesCompatibleMod(string modName, string pluginName, ulong publishedFileId,
            CompatibleModDefinition modDefinition)
        {
            return ContainsAnyNamePattern(modName, modDefinition.NamePatterns) ||
                   ContainsAnyNamePattern(pluginName, modDefinition.NamePatterns) ||
                   modDefinition.WorkshopIds.Contains(publishedFileId);
        }

        private static Dictionary<string, CompatibleModDefinition> GetCompatibleMods()
        {
            if (_compatibleMods != null)
                return _compatibleMods;

            _compatibleMods = new Dictionary<string, CompatibleModDefinition>();

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(CompatibleModsResourceName))
            {
                if (stream == null)
                    return _compatibleMods;

                using (var reader = new StreamReader(stream))
                {
                    ParseCompatibleModsJson(reader.ReadToEnd(), _compatibleMods);
                }
            }

            return _compatibleMods;
        }

        private static void ParseCompatibleModsJson(string json, IDictionary<string, CompatibleModDefinition> target)
        {
            foreach (Match modMatch in Regex.Matches(json, "\\{\\s*\"key\"\\s*:\\s*\"([^\"]+)\"(.*?)\\}",
                         RegexOptions.Singleline))
            {
                var key = modMatch.Groups[1].Value;
                var body = modMatch.Groups[2].Value;

                target[key] = new CompatibleModDefinition
                {
                    NamePatterns = ParseStringArray(body, "namePatterns"),
                    WorkshopIds = ParseUInt64Array(body, "workshopIds")
                };
            }
        }

        private static List<string> ParseStringArray(string json, string propertyName)
        {
            var result = new List<string>();
            var match = Regex.Match(json, "\"" + propertyName + "\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
            if (!match.Success)
                return result;

            foreach (Match valueMatch in Regex.Matches(match.Groups[1].Value, "\"([^\"]+)\""))
                result.Add(valueMatch.Groups[1].Value);

            return result;
        }

        private static List<ulong> ParseUInt64Array(string json, string propertyName)
        {
            var result = new List<ulong>();
            var match = Regex.Match(json, "\"" + propertyName + "\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
            if (!match.Success)
                return result;

            foreach (Match valueMatch in Regex.Matches(match.Groups[1].Value, "\\d+"))
            {
                ulong value;
                if (ulong.TryParse(valueMatch.Value, out value))
                    result.Add(value);
            }

            return result;
        }

        private static bool ContainsAnyNamePattern(string name, IEnumerable<string> namePatterns)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            foreach (var namePattern in namePatterns)
                if (!string.IsNullOrEmpty(namePattern) &&
                    name.IndexOf(namePattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

            return false;
        }

        public struct CompatibleModMatch
        {
            public string Key;
            public string ModName;
            public string PluginName;
            public ulong WorkshopId;
        }

        private struct CompatibleModDefinition
        {
            public List<string> NamePatterns;
            public List<ulong> WorkshopIds;
        }
    }
}
