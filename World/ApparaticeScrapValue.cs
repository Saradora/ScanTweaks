using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityMDK.Injection;
using UnityMDK.Logging;
using NetworkPrefabs = LethalMDK.Network.NetworkPrefabs;

namespace ScanTweaks.World;

public abstract class ComponentPostInjector<TComponent> : MonoBehaviour where TComponent : Component
{
    private void Awake()
    {
        TComponent component = GetComponent<TComponent>();
        if (component is null) return;
        if (CanBeInjected(component))
        {
            Inject(component);
        }
    }

    protected abstract bool CanBeInjected(TComponent component);
    protected abstract void Inject(TComponent component);
}

[SceneConstructor]
public class ApparaticeValueSetter : NetworkBehaviour
{
    [SerializeField] private Vector2Int _valueRange = new Vector2Int(125, 350); // path 47 has a 0.4 multiplier

    private LungProp _lungProp;

    private static bool _initialized;
    private static GameObject _networkPrefab;

    private bool isServerOrHost => IsHost || IsServer;

    [InjectToComponent(typeof(LungProp))]
    private class ApparatusValueSetterInjector : ComponentPostInjector<LungProp>
    {
        protected override bool CanBeInjected(LungProp component)
        {
            if (!IsLungPropValid(component)) return false;
            if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost) return false;
            if (component.GetComponentInChildren<ApparaticeValueSetter>(true)) return false;
            return true;
        }

        protected override void Inject(LungProp component)
        {
            Log.Error($"Instantiating apparatice prefab");
            var go = Instantiate(_networkPrefab, component.transform);
            var networkObject = go.GetComponent<NetworkObject>();
            networkObject.Spawn();
        }

        private static bool IsLungPropValid(LungProp lungProp)
        {
            if (lungProp.itemProperties.itemName != "Apparatus") return false;
            if (!lungProp.isLungDocked || !lungProp.isLungPowered) return false;
            return lungProp.isInFactory;
        }
    }

    private static void SceneConstructor(Scene scene)
    {
        if (_initialized) return;
        if (!NetworkManager.Singleton) return;

        _networkPrefab = NetworkPrefabs.GetEmptyPrefab("ApparatusValueSetter");
        _networkPrefab.AddComponent<ApparaticeValueSetter>();
        _initialized = true;
        Log.Error($"Created apparatice injector");
    }

    private void Awake()
    {
        // only available here and on server, otherwise unparented
        _lungProp = GetComponentInParent<LungProp>(true);
    }

    private IEnumerator Start()
    {
        if (isServerOrHost)
        {
            yield return new WaitForSeconds(1f);
            int value = (int)(RoundManager.Instance.AnomalyRandom.Next(_valueRange.x, _valueRange.y) * RoundManager.Instance.scrapValueMultiplier);
            SetApparaticeValueClientRpc(_lungProp.NetworkObject, value);
            yield break;
        }
        
        yield return new WaitForSeconds(2f);
        RequestSyncServerRpc();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        if (IsServer || IsHost)
        {
            NetworkObject.Despawn();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSyncServerRpc()
    {
        Log.Error($"Ping request");
        if (!isServerOrHost) return;
        Log.Error($"Did ping");
        
        SetApparaticeValueClientRpc(_lungProp.GetComponent<NetworkObject>(), _lungProp.scrapValue);
    }

    [ClientRpc]
    private void SetApparaticeValueClientRpc(NetworkObjectReference objectReference, int value)
    {
        if (objectReference.TryGet(out NetworkObject netObject) && netObject.TryGetComponent(out LungProp lungProp))
        {
            lungProp.SetScrapValue(value);
            Log.Error($"Updated {lungProp.name} value to {value}");
        }
    }
}