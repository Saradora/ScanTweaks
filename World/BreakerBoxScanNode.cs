using LethalMDK.World;
using UnityEngine;
using UnityMDK.Config;
using UnityMDK.Injection;

namespace ScanTweaks.World;

[Initializer]
public static class BreakerBoxScanNode
{
    [ConfigSection("BreakerBox")] 
    [ConfigDescription("Adds a scan node to the breaker box")]
    private static readonly ConfigData<bool> _breakerBoxScanNode = new(true);
    
    [ConfigDescription("Minimum range of the scan node")]
    private static readonly ConfigData<int> _breakerBoxMinRange = new(3);
    
    [ConfigDescription("Maximum range of the scan node")]
    private static readonly ConfigData<int> _breakerBoxMaxRange = new(13);

    private static readonly Vector3 Offset = new(-1f, -0.8f, -1.2f);
    
    [Initializer]
    private static void Initialize()
    {
        if (!_breakerBoxScanNode) return;
        ScanNodes.AddScanNode<BreakerBox>(ScanNodes.EType.Utility, "Breaker Box", "Flip the switches! Maybe it'll do something?", _breakerBoxMinRange, _breakerBoxMaxRange, Offset);
    }
}