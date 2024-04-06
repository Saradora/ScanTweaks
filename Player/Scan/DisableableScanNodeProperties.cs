using UnityMDK.Config;

namespace ScanTweaks;

public class DisableableScanNodeProperties : ScanNodeProperties
{
    [ConfigSection("PingScan")]
    [ConfigDescription("Adds a scan node to every item that didn't already have one.")]
    public static ConfigData<bool> EnableScanNodeForTools = new(true);
}