using LethalMDK.World;
using UnityEngine;
using UnityMDK.Injection;

namespace ScanTweaks.World;

[Initializer]
public static class BreakerBoxScanNode
{
    [SerializeField] private static int _minRange = 3;
    [SerializeField] private static int _maxRange = 13;

    private static readonly Vector3 Offset = new(-1f, -0.8f, -1.2f);
    
    [Initializer]
    private static void Initialize()
    {
        ScanNodes.AddScanNode<BreakerBox>(ScanNodes.EType.Utility, "Breaker Box", "Flip the switches! Maybe it'll do something?", _minRange, _maxRange, Offset);
    }
}