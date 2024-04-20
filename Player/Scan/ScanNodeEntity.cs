using UnityEngine;
using UnityMDK.Injection;

namespace ScanTweaks;

[InjectToComponent(typeof(ScanNodeProperties))]
public class ScanNodeEntity : MonoBehaviour
{
    public Collider NodeCollider { get; private set; }
    public ScanNodeProperties Properties { get; private set; }

    private void Awake()
    {
        NodeCollider = GetComponentInChildren<Collider>();
        Properties = GetComponent<ScanNodeProperties>();
    }

    private void OnEnable()
    {
        ScanNodeManager.Add(this);
    }

    private void OnDisable()
    {
        ScanNodeManager.Remove(this);
    }
}