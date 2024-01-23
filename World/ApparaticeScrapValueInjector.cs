using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityMDK.Injection;
using UnityMDK.Logging;
using NetworkPrefabs = LethalMDK.Network.NetworkPrefabs;

namespace ScanTweaks.World;

[Initializer]
[SceneConstructor]
public class ApparatusScrapValueInjector : ComponentInjector<LungProp>
{
    [Initializer]
    private static void Init()
    {
        if (!ApparaticeScrapValue.MakeRandomValue) return;
        SceneInjection.AddComponentInjector(new ApparatusScrapValueInjector(), true);
    }
    
    protected override bool CanBeInjected(LungProp component)
    {
        return IsLungPropValid(component);
    }

    protected override void Inject(LungProp component)
    {
        component.gameObject.AddComponent<ApparaticeScrapValue>();
    }

    private static bool IsLungPropValid(LungProp lungProp)
    {
        if (lungProp.itemProperties.itemName != "Apparatus") return false;
        if (!lungProp.isLungDocked || !lungProp.isLungPowered) return false;
        return lungProp.isInFactory;
    }
}