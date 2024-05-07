using HarmonyLib;
using ScanTweaks.World;
using Unity.Netcode;

namespace ScanTweaks.Patches;

[HarmonyPatch(typeof(RoundManager))]
public static class RoundManager_Patching
{
    [HarmonyPatch("waitForScrapToSpawnToSync"), HarmonyPrefix]
    private static void waitForScrapToSpawnToSync_Prefix()
    {
        ApparaticeScrapValue.PatchedApparatices.Clear();
    }

    [HarmonyPatch(nameof(RoundManager.SyncScrapValuesClientRpc)), HarmonyPrefix]
    private static void SyncScrapValuesClientRpc_Prefix(ref NetworkObjectReference[] spawnedScrap, ref int[] allScrapValue)
    {
        if (!Player.IsServerOrHost)
            return;
        
        List<NetworkObjectReference> spawnedList = spawnedScrap.ToList();
        List<int> values = allScrapValue.ToList();

        foreach (var patchedApparatice in ApparaticeScrapValue.PatchedApparatices)
        {
            if (patchedApparatice == null)
                continue;

            NetworkObjectReference objectReference = patchedApparatice.NetworkObject;
            
            if (spawnedList.Contains(objectReference))
                continue;
            
            spawnedList.Add(objectReference);
            values.Add(patchedApparatice.scrapValue);
        }

        spawnedScrap = spawnedList.ToArray();
        allScrapValue = values.ToArray();
    }
}