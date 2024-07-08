using UnityEngine;

namespace ScanTweaks;

public class BatteryTextUpdater : MonoBehaviour
{
    private GrabbableObject _grabbableObject;
    private ScanNodeProperties _scanNode;

    private static readonly string NoBatteryMsg = "No Battery";
    private static readonly string[] BatteryPercentMsg = { "Battery: ", "%" };

    private int _currentBattery = int.MaxValue;
    
    private void Awake()
    {
        _grabbableObject = GetComponentInParent<GrabbableObject>(true);
        _scanNode = GetComponent<ScanNodeProperties>();
    }

    private void Update()
    {
        if (_scanNode.enabled == false)
            return;
        
        if (_grabbableObject.insertedBattery is null)
        {
            _scanNode.subText = NoBatteryMsg;
            return;
        }

        int currentBattery = _grabbableObject.insertedBattery.empty ? 0 : (int)Math.Ceiling(Math.Clamp(_grabbableObject.insertedBattery.charge * 100f, 0f, 100f));

        if (_currentBattery == currentBattery)
            return;

        _currentBattery = currentBattery;

        _scanNode.subText = string.Join(currentBattery.ToString(), BatteryPercentMsg);
    }
}