using System.Collections;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityMDK.Config;

namespace ScanTweaks.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public static class StartOfRound_Patching
{
    [ConfigSection("Patches")]
    [ConfigDescription("Patches the loot not being properly registered as inside the ship when joining a game as client.")]
    public static readonly ConfigData<bool> PatchLootInShip = new(true);
    
    [HarmonyPatch(nameof(StartOfRound.SyncAlreadyHeldObjectsServerRpc)), HarmonyPrefix]
    private static void SyncAlreadyHeldObjectsServerRpc_Prefix(int joiningClientId)
    {
        if ((int)NetworkManager.Singleton.LocalClientId != joiningClientId)
            return;

        try
        {
            GameObject ship = GameObject.Find("/Environment/HangarShip");
            var eventManager = ship.GetComponent<ElevatorAnimationEvents>();
            eventManager.StartCoroutine(WaitForSyncItems(ship));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private static IEnumerator WaitForSyncItems(GameObject shipObject)
    {
        yield return new WaitForSeconds(2f);
        if (!PatchLootInShip)
            yield break;
        
        GrabbableObject[] gObjects = shipObject.GetComponentsInChildren<GrabbableObject>();

        foreach (var grabbableObject in gObjects)
        {
            if (!grabbableObject.isInShipRoom)
            {
                grabbableObject.scrapPersistedThroughRounds = true;
                GameNetworkManager.Instance.localPlayerController.SetItemInElevator(true, true, grabbableObject);
            }
        }
    }
}