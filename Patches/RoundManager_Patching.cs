using System.Collections;
using HarmonyLib;
using ScanTweaks.World;
using Unity.Netcode;
using UnityEngine;
using UnityMDK.Logging;

namespace ScanTweaks.Patches;

[HarmonyPatch(typeof(RoundManager))]
public static class RoundManager_Patching
{
    [HarmonyPatch(nameof(waitForScrapToSpawnToSync)), HarmonyPrefix]
    private static bool waitForScrapToSpawnToSync_Prefix(NetworkObjectReference[] spawnedScrap, int[] scrapValues, ref IEnumerator __result)
    {
        ApparaticeScrapValue.PatchedApparatices.Clear();
        
        if (!ApparaticeScrapValue.ApparaticeMakeRandomValue)
            return true;

        __result = waitForScrapToSpawnToSync(spawnedScrap, scrapValues);

        return false;
    }

    private static IEnumerator waitForScrapToSpawnToSync(NetworkObjectReference[] spawnedScrap, int[] scrapValues)
    {
        yield return new WaitForSeconds(11f);

        List<NetworkObjectReference> spawnedList = spawnedScrap.ToList();
        List<int> values = scrapValues.ToList();
        
        Log.Warning($"Scrap count before patch: {spawnedList.Count}");

        foreach (var patchedApparatice in ApparaticeScrapValue.PatchedApparatices)
        {
            if (patchedApparatice == null)
                continue;
            
            spawnedList.Add(patchedApparatice.NetworkObject);
            values.Add(patchedApparatice.scrapValue);
        }
        
        RoundManager.Instance.SyncScrapValuesClientRpc(spawnedList.ToArray(), values.ToArray());
        
        Log.Warning($"Scrap count after patch: {spawnedList.Count}");
        Log.Error($"Patches scrap values");
    }
}