using LethalMDK;
using UnityEngine;
using UnityMDK.Injection;
using UnityMDK.Logging;

namespace ScanTweaks.World;

[InjectToPrefab(Prefabs.PLungApparatus)]
public class ApparatusScrapValueInjector : IPrefabInjector
{
    public void OnInject(GameObject obj)
    {
        if (!ApparaticeScrapValue.ApparaticeMakeRandomValue)
            return;

        LungProp apparatus = obj.GetComponentInChildren<LungProp>();
        if (apparatus == null)
        {
            Log.Error($"Couldn't find apparatus on prefab.");
            return;
        }

        apparatus.gameObject.AddComponent<ApparaticeScrapValue>();
    }
}