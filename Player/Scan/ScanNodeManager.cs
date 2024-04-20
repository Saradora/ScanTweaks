using LethalMDK;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityMDK.Injection;
using UnityMDK.Logging;

namespace ScanTweaks;

[InjectToComponent(typeof(HUDManager))]
public class ScanNodeManager : MonoBehaviour
{
    private static readonly List<ScanNodeEntity> _nodes = new();

    private static readonly QueryParameters _queryParameters = new(LayerMasks.Room, hitTriggers: QueryTriggerInteraction.Ignore);

    private NativeArray<RaycastHit> _hitsArray;
    private NativeArray<RaycastCommand> _raycastsArray;

    private static bool _init;

    private static bool _isRunning;
    private static JobHandle _currentJob;

    private void Awake()
    {
        if (_init)
            return;

        _init = true;
        
        ResizeArrays();
    }

    private void OnDestroy()
    {
        if (_raycastsArray.IsCreated)
        {
            _raycastsArray.Dispose();
            _hitsArray.Dispose();
        }
    }

    private void ResizeArrays()
    {
        int nodesCount = _nodes.Count;
        
        if (_raycastsArray.Length == nodesCount)
            return;

        if (_raycastsArray.IsCreated)
        {
            _hitsArray.Dispose();
            _raycastsArray.Dispose();
        }
        
        _hitsArray = new NativeArray<RaycastHit>(nodesCount, Allocator.Persistent);
        _raycastsArray = new NativeArray<RaycastCommand>(nodesCount, Allocator.Persistent);
    }

    private void Update()
    {
        if (_isRunning)
        {
            _currentJob.Complete();
            _isRunning = false;

            int hitCount = 0;

            int nodesCount = _hitsArray.Length;
            for (int hitIndex = 0; hitIndex < nodesCount; hitIndex++)
            {
                var hit = _hitsArray[hitIndex];
                if (hit.collider)
                {
                    hitCount++;
                }
                else
                {
                    Log.Warning($"Seeing {_nodes[hitIndex].Properties.headerText}");
                }
            }
            
            Log.Warning($"Visible: {nodesCount - hitCount}/{nodesCount}");
        }
        
        if (Player.LocalPlayer == null)
            return;
        
        ResizeArrays();

        Camera camera = Player.LocalPlayer.gameplayCamera;

        Vector3 camPos = camera.transform.position;

        for (int nodeIndex = 0; nodeIndex < _nodes.Count; nodeIndex++)
        {
            var node = _nodes[nodeIndex];

            if (!node.NodeCollider)
            {
                continue;
            }

            Vector3 boundsCenter = node.NodeCollider.bounds.center;
            Vector3 direction = boundsCenter - camPos;
            if (Math.Abs(direction.y) > 0.1f)
            {
                if (direction.y > 0)
                    boundsCenter -= Vector3.up * 0.1f;
                else
                    boundsCenter += Vector3.up * 0.1f;
            }
            direction = boundsCenter - camPos;
            _raycastsArray[nodeIndex] = new(camPos, direction.normalized, _queryParameters, direction.magnitude);
        }

        _currentJob = RaycastCommand.ScheduleBatch(_raycastsArray, _hitsArray, _nodes.Count);
        _isRunning = true;
    }

    public static void Add(ScanNodeEntity scanNode)
    {
        Remove(scanNode);
        _nodes.Add(scanNode);
    }

    public static void Remove(ScanNodeEntity scanNode)
    {
        _nodes.Remove(scanNode);
    }
}