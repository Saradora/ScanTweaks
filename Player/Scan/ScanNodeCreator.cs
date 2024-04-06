﻿using LethalMDK;
using LethalMDK.World;
using UnityEngine;
using UnityMDK.Injection;

namespace ScanTweaks;

[InjectToComponent(typeof(GrabbableObject))]
public class ScanNodeCreator : MonoBehaviour
{
    private void Start()
    {
        if (GetComponentInChildren<ScanNodeProperties>(true))
            return;

        GrabbableObject item = GetComponent<GrabbableObject>();

        if (item.itemProperties.itemName.Equals("sticky note", StringComparison.InvariantCultureIgnoreCase))
            return;

        bool isScrap = item.itemProperties.isScrap;
        
        GameObject scanNode = new("Scan Node") { layer = Layers.ScanNode };

        DisableableScanNodeProperties properties = scanNode.AddComponent<DisableableScanNodeProperties>();
        properties.headerText = item.itemProperties.itemName;
        properties.subText = isScrap ? $"Value: ${item.scrapValue}" : "???";
        properties.minRange = 1;
        properties.maxRange = 13;
        properties.creatureScanID = -1;
        properties.nodeType = (int)(isScrap ? ScanNodes.EType.Scrap : ScanNodes.EType.Utility);
        properties.requiresLineOfSight = true;

        BoxCollider collider = scanNode.AddComponent<BoxCollider>();
        collider.size = Vector3.one * 0.2f;
        
        scanNode.transform.SetParent(transform);
        scanNode.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}