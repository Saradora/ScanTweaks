﻿using LethalMDK;
using LethalMDK.World;
using UnityEngine;
using UnityMDK.Config;
using UnityMDK.Injection;
using UnityMDK.Logging;

namespace ScanTweaks.World;

[InjectToPrefab(Prefabs.PBreakerBox)]
public class BreakerBoxPatcher : IPrefabInjector
{
    public void OnInject(GameObject obj)
    {
        BreakerBox breakerBox = obj.GetComponentInChildren<BreakerBox>();
        if (breakerBox == null)
        {
            Log.Error($"Couldn't patch breaker box");
            return;
        }

        breakerBox.gameObject.AddComponent<BreakerBoxScanNode>();
    }
}

public class BreakerBoxScanNode : MonoBehaviour
{
    [ConfigSection("BreakerBox")] 
    [ConfigDescription("Adds a scan node to the breaker box")]
    private static readonly ConfigData<bool> _breakerBoxScanNode = new(true);
    
    [ConfigDescription("Minimum range of the scan node")]
    private static readonly ConfigData<int> _breakerBoxMinRange = new(3);
    
    [ConfigDescription("Maximum range of the scan node")]
    private static readonly ConfigData<int> _breakerBoxMaxRange = new(13);

    private static readonly Vector3 Offset = new(-1f, -0.8f, -1.2f);
    
    private ScanNodeProperties _scanNode;

    private void Start()
    {
        _breakerBoxScanNode.ConfigChanged += OnNodeActiveStateChanged;
        _breakerBoxMinRange.ConfigChanged += OnMinRangeChanged;
        _breakerBoxMaxRange.ConfigChanged += OnMaxRangeChanged;
        
        _scanNode = GetComponentInChildren<ScanNodeProperties>(true);
        
        if (!_scanNode)
        {
            GameObject scanNodeObject = new("Scan Node") { layer = Layers.ScanNode };

            BoxCollider collider = scanNodeObject.AddComponent<BoxCollider>();
            collider.size = Vector3.one * 0.2f;

            _scanNode = scanNodeObject.AddComponent<ScanNodeProperties>();
            _scanNode.headerText = "Breaker Box";
            _scanNode.subText = "Flip the switches! Maybe it'll do something?";
            _scanNode.minRange = _breakerBoxMinRange;
            _scanNode.maxRange = _breakerBoxMaxRange;
            _scanNode.creatureScanID = -1;
            _scanNode.nodeType = (int)ScanNodes.EType.Utility;
            _scanNode.requiresLineOfSight = true;
        
            scanNodeObject.transform.SetParent(transform);
            scanNodeObject.transform.SetLocalPositionAndRotation(Offset, Quaternion.identity);
        }

        _scanNode.enabled = _breakerBoxScanNode;
    }

    private void OnNodeActiveStateChanged(bool value)
    {
        if (!_scanNode)
            return;

        _scanNode.enabled = value;
    }

    private void OnMinRangeChanged(int value)
    {
        if (!_scanNode)
            return;

        _scanNode.minRange = value;
    }

    private void OnMaxRangeChanged(int value)
    {
        if (!_scanNode)
            return;

        _scanNode.maxRange = value;
    }

    private void OnDestroy()
    {
        _breakerBoxScanNode.ConfigChanged -= OnNodeActiveStateChanged;
        _breakerBoxMinRange.ConfigChanged -= OnMinRangeChanged;
        _breakerBoxMaxRange.ConfigChanged -= OnMaxRangeChanged;
    }
}