using BepInEx;
using HarmonyLib;

namespace ScanTweaks;

[BepInPlugin(ScanTweaks.ModGuid, ScanTweaks.ModName, ScanTweaks.ModVersion)]
public class PluginInitializer : BaseUnityPlugin
{
    private readonly Harmony _harmonyInstance = new(ScanTweaks.ModGuid);
        
    private void Awake()
    {
        _harmonyInstance.PatchAll();
    }
}