using BepInEx;
using HarmonyLib;
using UnityMDK.Config;

namespace ScanTweaks;

[BepInDependency(LethalMDK.LethalMDK.ModGuid)]
[BepInDependency(UnityMDK.UnityMDK.ModGuid)]
[BepInPlugin(ScanTweaks.ModGuid, ScanTweaks.ModName, ScanTweaks.ModVersion)]
public class PluginInitializer : BaseUnityPlugin
{
    private readonly Harmony _harmonyInstance = new(ScanTweaks.ModGuid);
        
    private void Awake()
    {
        ConfigBinder.BindAll(Config);
        _harmonyInstance.PatchAll();
    }
}

public static class ScanTweaks
{
    public const string ModGuid = "Saradora.ScanTweaks";
    public const string ModVersion = "1.3.1";
    public const string ModName = "Scan Tweaks";
}