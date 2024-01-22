using HarmonyLib;
using ScanTweaks.World;
using UnityEngine;

namespace ScanTweaks.Patches;

[HarmonyPatch(typeof(GrabbableObject))]
internal static class GrabbableObject_Patching
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void Start_PostFix(GrabbableObject __instance)
    {
        if (!__instance.radarIcon) return;
        
        RadarIcons.AddRadarIcon(__instance, __instance.radarIcon.GetComponent<MeshRenderer>());
        __instance.radarIcon.transform.SetParent(StartOfRound.Instance.transform);
        __instance.radarIcon = null;
    }
}