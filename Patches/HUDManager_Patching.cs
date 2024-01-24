using HarmonyLib;
using LethalMDK;
using UnityEngine.InputSystem;
using UnityMDK.Reflection;

namespace ScanTweaks.Patches;

[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManager_Patching
{
    private static bool _insideUpdate;
        
    [HarmonyPatch("CanPlayerScan"), HarmonyPrefix]
    private static bool CanPlayerScan_Prefix(ref bool __result)
    {
        if (!PingScan.DoPatch) return true;

        if (!_insideUpdate) return true;
        
        __result = false;
        return false;
    }

    [HarmonyPatch("PingScan_performed"), HarmonyPostfix]
    private static void PingScan_performed_Postfix(HUDManager __instance)
    {
        if (Player.LocalPlayer == null) return;
        if (__instance.GetField<float>("playerPingingScan") < 0.3f) return;
        
        PingScan.TriggerPingScan();
    }

    [HarmonyPatch("Update"), HarmonyPrefix, HarmonyPriority(Priority.Last)]
    private static void Update_Prefix()
    {
        _insideUpdate = true;
    }

    [HarmonyPatch("Update"), HarmonyPostfix, HarmonyPriority(Priority.First)]
    private static void Update_Postfix()
    {
        _insideUpdate = false;
    }
}