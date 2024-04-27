using UnityMDK.Injection;

namespace ScanTweaks.World;

[Initializer]
public class ApparatusScrapValueInjector : ComponentInjector<LungProp>
{
    [Initializer]
    private static void Init()
    {
        if (!ApparaticeScrapValue.ApparaticeMakeRandomValue) return;
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