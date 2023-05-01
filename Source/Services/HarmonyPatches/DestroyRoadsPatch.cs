using HarmonyLib;
using ICities;
using NaturalDisastersRenewal.DisasterServices.HarmonyPatches;
using System;
using UnityEngine;

namespace NaturalDisastersRenewal.HarmonyPatches
{
    [HarmonyPatch(typeof(DisasterHelpers))]
    [HarmonyPatch("DestroyNetSegments")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(InstanceManager.Group), typeof(Vector3), typeof(float), typeof(float), typeof(float), typeof(float) })]

    class DestroyRoadsPatch
    {
        static bool Prefix(int seed, InstanceManager.Group group, Vector3 position, float totalRadius, float removeRadius, float destructionRadiusMin, float destructionRadiusMax)
        {
            DisasterType disasterType = DisasterHelpersModified.DisasterType;            

            if(!DisasterHelpersModified.EnableDestruction)
                return false;
            
            if (disasterType == DisasterType.Tornado && DisasterHelpersModified.DisasterIntensity <= DisasterHelpersModified.IntensityStartDestruction)
                return false;

            return true;
        }
    }
}