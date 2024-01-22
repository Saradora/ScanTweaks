using UnityEngine;
using UnityMDK.Injection;

namespace ScanTweaks.World;

[InjectToComponent(typeof(StartOfRound))]
public class RadarIcons : MonoBehaviour
{
    private static List<(GrabbableObject, MeshRenderer)> _radarIconList = new();

    private static readonly Vector3 Offset = Vector3.up * 0.5f;

    private void Update()
    {
        for (int iconIndex = _radarIconList.Count - 1; iconIndex >= 0; iconIndex--)
        {
            (GrabbableObject grabbable, MeshRenderer radarIcon) = _radarIconList[iconIndex];

            if (!grabbable)
            {
                Destroy(radarIcon.gameObject);
                _radarIconList.RemoveAt(iconIndex);
                continue;
            }
            
            if (!radarIcon)
            {
                _radarIconList.RemoveAt(iconIndex);
                continue;
            }

            bool active = grabbable.gameObject.activeInHierarchy && !grabbable.deactivated && (grabbable.isHeld || (!grabbable.isInElevator && !grabbable.isInShipRoom));
            
            radarIcon.enabled = active;

            if (active)
            {
                radarIcon.transform.position = grabbable.transform.position + Offset;
            }
        }
    }

    public static void AddRadarIcon(GrabbableObject grabbableObject, MeshRenderer radarIcon)
    {
        if (!grabbableObject || !radarIcon) return;
        
        _radarIconList.Add((grabbableObject, radarIcon));
    }
}