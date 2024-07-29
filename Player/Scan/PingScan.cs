using System.Collections;
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
    public static readonly ConfigData<bool> PingScanDoPatch = new(true);

    [ConfigDescription("The delay between each ping scan step. A larger value will make the scan take longer to reach great distances.")]
    private static readonly ConfigData<float> PingScanStepDuration = new(0.01f);
    
    [ConfigDescription("Should dynamic objects (e.g. doors) block the ping scan")]
    private static readonly ConfigData<bool> DynamicObjectsBlockRaycast = new(true);

    [ConfigDescription("Defines how player held items should react to the scanner.\nNone: The scanner doesn't see the items\nEquipped: The scanner only sees equipped items\nAll: The scanner sees all items")]
    private static readonly ConfigData<EHeldScanType> PlayerHeldItemsScannability = new(EHeldScanType.Equipped);

    public enum EHeldScanType
    {
        None,
        Equipped,
        All
    }
    
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
    private static readonly int _physicsMask = LayerMasks.Room;
    private static readonly int _doorMask = LayerMasks.InteractableObject;
    private Terminal _terminal;

    private static IPocketedGrabbableComparer _pocketedGrabbableComparer = new DefaultPocketedGrabbableComparer();
    
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

    public static void SetPocketedGrabbableComparer(IPocketedGrabbableComparer comparer) =>
        _pocketedGrabbableComparer = comparer;

    private void Update()
    {
        PlayerControllerB player = LethalMDK.Player.LocalPlayer;
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

            if (grabbableObject)
            {
                if (!IsGrabbableScannable(grabbableObject))
                {
                    RemoveScanNodeAt(scanIndex);
                    continue;
                }
            }
            
            if (!IsNodeVisible(scanNode, collider, player.gameplayCamera, paddingX, paddingY)) 
                RemoveScanNodeAt(scanIndex);
        }
    }

    private static bool IsGrabbableScannable(GrabbableObject grabbableObject)
    {
        if (!grabbableObject.isHeld)
            return true;

        if (grabbableObject.isHeldByEnemy)
            return true;

        if (grabbableObject.playerHeldBy == LethalMDK.Player.LocalPlayer)
            return false;

        EHeldScanType scanType = PlayerHeldItemsScannability;
        switch (scanType)
        {
            case EHeldScanType.None:
                return false;
            case EHeldScanType.Equipped:
                return !grabbableObject.isPocketed || _pocketedGrabbableComparer.IsPocketedGrabbableScannable(grabbableObject);
            case EHeldScanType.All:
                return true;
            default:
                throw new ArgumentOutOfRangeException();
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
        Camera camera = LethalMDK.Player.LocalPlayer.gameplayCamera;
        StartCoroutine(Pinging(camera));
    }

    private IEnumerator Pinging(Camera camera)
    {
        var cameraTransform = camera.transform;
        Vector3 currentTestPos = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;
        Quaternion boxOrientation = Quaternion.LookRotation(direction);
        Vector3 screenPos = Vector3.one;
        Vector3 extents = Vector3.one;

        const float stepLength = 0.5f;
        float currentLength = 0f;
        float nextStep = Time.time;

        while (currentLength < _range)
        {
            yield return null;

            while (Time.time >= nextStep)
            {
                UpdateExtents();
                DoPing();
                
                currentTestPos += direction * stepLength;
                currentLength += stepLength;
                nextStep += PingScanStepDuration;
            }
        }

        void UpdateExtents()
        {
            screenPos.z = currentLength;
            Vector3 localSize = camera.transform.InverseTransformPoint(camera.ViewportToWorldPoint(screenPos));
            extents = localSize;
            extents.z = stepLength * 0.5f;
        }

        void DoPing()
        {
            int hitCount = Physics.OverlapBoxNonAlloc(currentTestPos, extents, _hitAlloc, boxOrientation, LayerMasks.ScanNode, QueryTriggerInteraction.Collide);

            for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            {
                Collider hitCollider = _hitAlloc[hitIndex];
                if (!hitCollider.TryGetComponent(out ScanNodeProperties scanNode)) continue;
            
                if (!IsNodeVisible(scanNode, hitCollider, camera, 0f, 0f)) continue;
            
                AddScanNode(scanNode);
            }
        }
    }

    private static bool PointOutOfViewport(float point, float padding)
    {
        return point > 1 + padding || point < 0 - padding;
    }

    private static bool IsNodeVisible(ScanNodeProperties scanNode, Collider nodeCollider, Camera camera, float paddingX, float paddingY)
    {
        if (scanNode == null) return false;
        if (scanNode.enabled == false) return false;
        if (scanNode is CustomScanNodeProperties && !CustomScanNodeProperties.EnableScanNodeForTools)
            return false;
        if (nodeCollider == null) return false;
        
        Vector3 camPos = camera.transform.position;
        float distanceToCam = (camPos - nodeCollider.bounds.center).magnitude;

        if (distanceToCam < scanNode.minRange) return false;
        if (distanceToCam > scanNode.maxRange) return false;
            
        Vector3 viewPosition = camera.WorldToViewportPoint(nodeCollider.bounds.center);
        if (PointOutOfViewport(viewPosition.x, paddingX) || PointOutOfViewport(viewPosition.y, paddingY) || viewPosition.z < 0) return false;

        var mask = _physicsMask;
        if (DynamicObjectsBlockRaycast)
            mask |= _doorMask;

        if (!scanNode.requiresLineOfSight)
            return true;

        Bounds colliderBounds = nodeCollider.bounds;
        Vector3 endpoint = colliderBounds.center;
        float upPoint = colliderBounds.extents.y > 0 ? colliderBounds.max.y : colliderBounds.min.y;
        endpoint.y += (upPoint - endpoint.y) * 0.5f;
        
        return !Linecast(camPos, endpoint, mask);
    }

    private static bool Linecast(Vector3 start, Vector3 end, int layerMask)
    {
        if (!Physics.Linecast(start, end, out RaycastHit hitInfo, layerMask, QueryTriggerInteraction.Collide)) 
            return false;

        if (hitInfo.collider.gameObject.CompareTag(Tags.InteractTrigger))
            return hitInfo.collider.GetComponent<AnimatedObjectTrigger>();

        return true;
    }

    private Terminal GetTerminal()
    {
        if (_terminal == null)
        {
            if (!HUDManager.Instance || !HUDManager.Instance.TryGetField("terminalScript", out Terminal terminal))
                return null;

            _terminal = terminal;
        }

        return _terminal;
    }

    private void AttemptScanNewCreature(int creatureID)
    {
        if (creatureID < 0) return;
        Terminal terminal = GetTerminal();
        if (terminal == null)
            return;
        
        if (terminal.scannedEnemyIDs.Contains(creatureID)) return;
        
        HUDManager.Instance.ScanNewCreatureServerRpc(creatureID);
    }

    private void AddScanNode(ScanNodeProperties scanNodeProperties)
    {
        if (!scanNodeProperties) return;
        if (_currentScannedNodes.Contains(scanNodeProperties)) return;

        GrabbableObject grabbableObject = scanNodeProperties.GetComponentInParent<GrabbableObject>();

        if (grabbableObject)
        {
            if (!IsGrabbableScannable(grabbableObject))
                return;
        }

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