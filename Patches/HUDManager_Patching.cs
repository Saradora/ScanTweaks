using HarmonyLib;

namespace ScanTweaks.Patches;

[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManager_Patching
{
    [HarmonyPatch("CanPlayerScan")]
    [HarmonyPrefix]
    private static bool CanPlayerScan_Prefix(ref bool __result)
    {
        if (!PingScan.DoPatch) return true;
        
        __result = false;
        return false;
    }
}