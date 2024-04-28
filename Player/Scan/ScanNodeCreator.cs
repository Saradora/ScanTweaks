using LethalMDK;
using UnityEngine;
using UnityMDK.Injection;

namespace ScanTweaks;

[Initializer]
public class ScanNodeCreator : ComponentInjector<GrabbableObject>
{
    private static readonly string[] excludes = new[]
    {
        "sticky note",
    };

    [Initializer]
    private static void Init()
    {
        SceneInjection.AddComponentInjector(new ScanNodeCreator(), true);
    }

    protected override bool CanBeInjected(GrabbableObject component)
    {
        if (component.GetComponentInChildren<ScanNodeProperties>(true))
            return false;

        if (component is RagdollGrabbableObject)
            return false;

        foreach (var exclude in excludes)
        {
            if (component.itemProperties.itemName.Equals(exclude, StringComparison.InvariantCultureIgnoreCase))
                return false;
        }

        return true;
    }

    protected override void Inject(GrabbableObject component)
    {
        var transform = component.transform;

        bool isScrap = component.itemProperties.isScrap;
        
        GameObject scanNode = new("Scan Node") { layer = Layers.ScanNode };
        
        scanNode.transform.SetParent(transform);
        scanNode.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        BoxCollider collider = scanNode.AddComponent<BoxCollider>();
        collider.size = Vector3.one * 0.2f;

        CustomScanNodeProperties properties = scanNode.AddComponent<CustomScanNodeProperties>();
        properties.headerText = component.itemProperties.itemName;
        properties.subText = isScrap ? $"Value: ${component.scrapValue}" : "???";
        properties.minRange = 1;
        properties.maxRange = 13;
        properties.creatureScanID = -1;
        properties.nodeType = isScrap ? PingScan.NodeScrap : PingScan.NodeUtility;
        properties.requiresLineOfSight = true;
    }
}