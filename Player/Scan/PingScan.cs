using GameNetcodeStuff;
using LethalMDK;
using UnityEngine;
using UnityMDK.Config;
using UnityMDK.Injection;
using UnityMDK.Reflection;

namespace ScanTweaks;

[InjectToComponent(typeof(IngamePlayerSettings))]
public class PingScan : MonoBehaviour
{
    [ConfigSection("PingScan")]
    [ConfigDescription("Enable/Disable the ping scan tweaks")]
    public static readonly ConfigData<bool> DoPatch = new(true);
    
    // Parameters
    [SerializeField] private float _range = 80f;
    [SerializeField] private float _outOfFrustumPaddingHorizontal = 0.2f;
    private float _outOfFrustumPaddingVertical => _outOfFrustumPaddingHorizontal * (Screen.width / (float)Screen.height);

    // Private members
    private readonly List<Collider> _currentScannedColliders = new();
    private readonly List<ScanNodeProperties> _currentScannedNodes = new();
    private readonly List<int> _currentScannedValues = new();
    private readonly List<GrabbableObject> _currentScannedGrabbables = new();
    private readonly Collider[] _hitAlloc = new Collider[30];
    private static readonly int _physicsMask = LayerMasks.Room | LayerMasks.InteractableObject;
    
    // Public members
    public static int PingedScrapValue { get; private set; }

    public static int NodeUtility => 0;
    public static int NodeCreature => 1;
    public static int NodeScrap => 2;

    // Events
    public static event Action<ScanNodeProperties> ScanNodeAdded;
    public static event Action<ScanNodeProperties> ScanNodeRemoved;
    
    public static event Action DoPing;

    internal static void TriggerPingScan()
    {
        DoPing?.Invoke();
    }

    private void Update()
    {
        PlayerControllerB player = Player.LocalPlayer;
        if (!player) return;
        if (player.inSpecialInteractAnimation && _currentScannedNodes.Count > 0)
        {
            RemoveAll();
        }
        
        float paddingX = _outOfFrustumPaddingHorizontal;
        float paddingY = _outOfFrustumPaddingVertical;
        
        for (int scanIndex = _currentScannedNodes.Count - 1; scanIndex >= 0; scanIndex--)
        {
            ScanNodeProperties scanNode = _currentScannedNodes[scanIndex];
            Collider collider = _currentScannedColliders[scanIndex];
            GrabbableObject grabbableObject = _currentScannedGrabbables[scanIndex];

            if (grabbableObject && grabbableObject.isHeld && !grabbableObject.isHeldByEnemy)
            {
                RemoveScanNodeAt(scanIndex);
                continue;
            }
            
            if (!IsNodeVisible(scanNode, collider, player.gameplayCamera, paddingX, paddingY)) 
                RemoveScanNodeAt(scanIndex);
        }
    }

    private void OnEnable()
    {
        DoPing += OnPingScan;
    }

    private void OnDisable()
    {
        DoPing -= OnPingScan;
        RemoveAll();
    }

    private void RemoveAll()
    {
        for (int scanIndex = _currentScannedNodes.Count - 1; scanIndex >= 0; scanIndex--)
        {
            RemoveScanNodeAt(scanIndex);
        }
    }

    private void OnPingScan()
    {
        Camera camera = Player.LocalPlayer.gameplayCamera;
        Transform cameraTransform = camera.transform;
        Vector3 camPos = cameraTransform.position;
        Vector3 camFwd = cameraTransform.forward;

        float radius = _range * 0.5f;
        int hitCount = Physics.OverlapSphereNonAlloc(camPos + (camFwd * radius), radius, _hitAlloc, LayerMasks.ScanNode, QueryTriggerInteraction.Ignore);

        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            Collider hitCollider = _hitAlloc[hitIndex];
            if (!hitCollider.TryGetComponent(out ScanNodeProperties scanNode)) continue;
            
            if (!IsNodeVisible(scanNode, hitCollider, camera, 0f, 0f)) continue;
            
            AddScanNode(scanNode);
        }
    }

    private static bool PointOutOfViewport(float point, float padding)
    {
        return point > 1 + padding || point < 0 - padding;
    }

    private static bool IsNodeVisible(ScanNodeProperties scanNode, Collider nodeCollider, Camera camera, float paddingX, float paddingY)
    {
        if (scanNode == false) return false;
        if (nodeCollider == false) return false;
        
        Vector3 camPos = camera.transform.position;
        float distanceToCam = (camPos - nodeCollider.bounds.center).magnitude;

        if (distanceToCam < scanNode.minRange) return false;
        if (distanceToCam > scanNode.maxRange) return false;
            
        Vector3 viewPosition = camera.WorldToViewportPoint(nodeCollider.bounds.center);
        if (PointOutOfViewport(viewPosition.x, paddingX) || PointOutOfViewport(viewPosition.y, paddingY) || viewPosition.z < 0) return false;

        return !scanNode.requiresLineOfSight || !Linecast(camPos, nodeCollider.bounds.center, _physicsMask);
    }

    private static bool Linecast(Vector3 start, Vector3 end, int layerMask)
    {
        bool hit = Physics.Linecast(start, end, out RaycastHit hitInfo, layerMask, QueryTriggerInteraction.Ignore);

        if (!hit) return false;

        if (hitInfo.collider.gameObject.CompareTag(Tags.InteractTrigger))
        {
            return hitInfo.collider.GetComponent<AnimatedObjectTrigger>();
        }

        return true;
    }

    private void AttemptScanNewCreature(int creatureID)
    {
        if (creatureID < 0) return;
        if (!HUDManager.Instance || !HUDManager.Instance.TryGetField("terminalScript", out Terminal terminal)) return;
        
        if (terminal.scannedEnemyIDs.Contains(creatureID)) return;
        
        HUDManager.Instance.ScanNewCreatureServerRpc(creatureID);
    }

    private void AddScanNode(ScanNodeProperties scanNodeProperties)
    {
        if (!scanNodeProperties) return;
        if (_currentScannedNodes.Contains(scanNodeProperties)) return;

        GrabbableObject grabbableObject = scanNodeProperties.GetComponentInParent<GrabbableObject>();
        if (grabbableObject && grabbableObject.isHeld && !grabbableObject.isHeldByEnemy) return;

        //Log.Print($"Scanned {scanNodeProperties.headerText}, type {scanNodeProperties.nodeType}");

        Collider coll = scanNodeProperties.GetComponent<Collider>();
        coll.enabled = false;
        
        AttemptScanNewCreature(scanNodeProperties.creatureScanID);
        
        _currentScannedValues.Add(scanNodeProperties.nodeType == NodeScrap ? scanNodeProperties.scrapValue : 0);
        _currentScannedColliders.Add(coll);
        _currentScannedNodes.Add(scanNodeProperties);
        _currentScannedGrabbables.Add(grabbableObject);
        
        CalculateTotalValue();
        
        ScanNodeAdded?.Invoke(scanNodeProperties);
    }

    private void RemoveScanNodeAt(int index)
    {
        if (index < 0 || index >= _currentScannedNodes.Count) return;

        Collider coll = _currentScannedColliders[index];
        if (coll) coll.enabled = true;

        _currentScannedValues.RemoveAt(index);
        CalculateTotalValue();
        ScanNodeRemoved?.Invoke(_currentScannedNodes[index]);
        _currentScannedColliders.RemoveAt(index);
        _currentScannedNodes.RemoveAt(index);
        _currentScannedGrabbables.RemoveAt(index);
    }

    private void CalculateTotalValue()
    {
        PingedScrapValue = 0;
        foreach (int value in _currentScannedValues)
        {
            PingedScrapValue += value;
        }
    }
}