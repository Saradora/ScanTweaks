using UnityMDK.Config;

namespace ScanTweaks;

public class CustomScanNodeProperties : ScanNodeProperties
{
    [ConfigSection("PingScan")]
    [ConfigDescription("Adds a scan node to every item that didn't already have one.")]
    public static ConfigData<bool> EnableScanNodeForTools = new(true);
    
    public bool IsBatteryUpdate { get; private set; }

    private void Awake()
    {
        GrabbableObject grabbableObject = GetComponentInParent<GrabbableObject>(true);
        
        if (grabbableObject != null && grabbableObject.itemProperties.requiresBattery)
        {
            IsBatteryUpdate = true;
            gameObject.AddComponent<BatteryTextUpdater>();
        }
    }
}