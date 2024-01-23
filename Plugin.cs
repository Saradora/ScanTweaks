using BepInEx;
using HarmonyLib;
using RuntimeNetcodeRPCValidator;

namespace ScanTweaks;

[BepInDependency(MyPluginInfo.PLUGIN_GUID)]
[BepInDependency(LethalMDK.LethalMDK.ModGuid)]
[BepInDependency(UnityMDK.UnityMDK.ModGuid)]
[BepInPlugin(ScanTweaks.ModGuid, ScanTweaks.ModName, ScanTweaks.ModVersion)]
public class PluginInitializer : BaseUnityPlugin
{
    private readonly Harmony _harmonyInstance = new(ScanTweaks.ModGuid);
    private readonly NetcodeValidator _netcodeValidator = new(ScanTweaks.ModGuid);
        
    private void Awake()
    {
        _harmonyInstance.PatchAll();
        _netcodeValidator.PatchAll();
    }
}