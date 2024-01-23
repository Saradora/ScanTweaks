using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityMDK.Config;

namespace ScanTweaks.World;

public class ApparaticeScrapValue : MonoBehaviour
{
    [ConfigSection("Apparatice")]
    [ConfigDescription("Make the apparatice have a random scrap value, also fixes the \"???\" on the scan node")]
    public static ConfigData<bool> MakeRandomValue { get; } = new(true);
    
    [ConfigDescription("Minimum possible scrap value of the apparatice. This value has a x0.4 multiplier in patch 47")]
    private static readonly ConfigData<int> _minValue = new(125); // patch 47 has a 0.4 multiplier on scraps
    
    [ConfigDescription("Maximum possible scrap value of the apparatice. This value has a x0.4 multiplier in patch 47")]
    private static readonly ConfigData<int> _maxValue = new(350);

    private LungProp _lungProp;

    private static bool IsServerOrHost => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;

    private void Awake()
    {
        _lungProp = GetComponent<LungProp>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(11f);

        if (!IsServerOrHost) yield break;
        
        int minValue = _minValue;
        int maxValue = _maxValue;
        if (minValue > maxValue) (minValue, maxValue) = (maxValue, minValue);
        int value = (int)(RoundManager.Instance.AnomalyRandom.Next(minValue, maxValue) * RoundManager.Instance.scrapValueMultiplier);
        _lungProp.SetScrapValue(value);

        NetworkObjectReference lungPropReference = _lungProp.NetworkObject;
        RoundManager.Instance.SyncScrapValuesClientRpc(new []{lungPropReference}, new[]{value});
    }
}