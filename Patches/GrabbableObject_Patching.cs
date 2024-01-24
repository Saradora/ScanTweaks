using HarmonyLib;
using LethalMDK;
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
        if (RadarIcons.PatchRadarIcons)
        {
            PatchRadarIcon(__instance);
        }
        PatchPropColliders(__instance); // todo add config for this?
    }

    private static void PatchRadarIcon(GrabbableObject grabbableObject)
    {
        if (!grabbableObject.radarIcon) return;
        
        RadarIcons.AddRadarIcon(grabbableObject, grabbableObject.radarIcon.GetComponent<MeshRenderer>());
        grabbableObject.radarIcon.transform.SetParent(StartOfRound.Instance.transform);
        grabbableObject.radarIcon = null;
    }

    private static void PatchPropColliders(GrabbableObject grabbableObject)
    {
        List<Collider> newPropColliders = new();
        foreach (var propCollider in grabbableObject.propColliders)
        {
            if (propCollider.gameObject.layer != Layers.ScanNode)
            {
                newPropColliders.Add(propCollider);
            }
        }

        grabbableObject.propColliders = newPropColliders.ToArray();
    }
}