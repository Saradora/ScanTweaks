using BepInEx;
using HarmonyLib;
using UnityMDK.Config;

namespace ScanTweaks;

[BepInDependency(PluginInfo.ReservedItemSlotsCoreGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalMDK.LethalMDK.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(UnityMDK.UnityMDK.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(PluginInfo.ModGuid, PluginInfo.ModName, PluginInfo.ModVersion)]
public class PluginInitializer : BaseUnityPlugin
{
    private readonly Harmony _harmonyInstance = new(PluginInfo.ModGuid);
        
    private void Awake()
    {
        ConfigBinder.BindAll(Config);
        _harmonyInstance.PatchAll();

        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(PluginInfo.ReservedItemSlotsCoreGuid))
        {
            PingScan.SetPocketedGrabbableComparer(new ReservedPocketedGrabbableComparer());
        }
    }
}

public static class PluginInfo
{
    public const string ModGuid = "Saradora.ScanTweaks";
    public const string ModVersion = "1.8.0";
    public const string ModName = "Scan Tweaks";

    public const string ReservedItemSlotsCoreGuid = "FlipMods.ReservedItemSlotCore";
}