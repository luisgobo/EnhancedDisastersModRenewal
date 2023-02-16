using HarmonyLib;
using ICities;
using System;
using UnityEngine;

namespace EnhancedDisastersMod.Patches
{
    [HarmonyPatch(typeof(DisasterHelpers))]
    [HarmonyPatch("DestroyBuildings")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(InstanceManager.Group), typeof(Vector3), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float) })]
    class DestroyBuildings2Patch
    {
        static bool Prefix(int seed, InstanceManager.Group group, Vector3 position, float preRadius, float removeRadius,
            float destructionRadiusMin, float destructionRadiusMax, float burnRadiusMin, float burnRadiusMax, float probability)
        {
            DisasterType dt = DisasterType.Empty;

            if (probability == 0.02f)
            {
                dt = DisasterType.Earthquake;
            }
            else if (burnRadiusMin == 0 && burnRadiusMax == 0)
            {
                dt = DisasterType.Tornado;
            }

            if (dt == DisasterType.Earthquake)
            {
                DisasterHelpersModified.DestroyBuildings(seed, group, position, preRadius, removeRadius, destructionRadiusMin,
                    destructionRadiusMax, burnRadiusMin, burnRadiusMax, 0.04f); // Orig = 0.02f

                return false;
            }
            else if (dt == DisasterType.Tornado)
            {
                DisasterHelpersModified.DestroyBuildings(seed, group, position, preRadius, removeRadius, destructionRadiusMin,
                    destructionRadiusMax, burnRadiusMin, burnRadiusMax, 0.5f); // Orig = 1.0f

                return false;
            }

            return true;
        }
    }
}