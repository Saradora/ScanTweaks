using UnityMDK.Config;

namespace ScanTweaks;

public class CustomScanNodeProperties : ScanNodeProperties
{
    [ConfigSection("PingScan")]
    [ConfigDescription("Adds a scan node to every item that didn't already have one.")]
    public static ConfigData<bool> EnableScanNodeForTools = new(true);

    private GrabbableObject _grabbableObject;

    private static readonly string NoBatteryMsg = "No Battery";
    private static readonly string[] BatteryPercentMsg = { "Battery: ", "%" };

    private int _currentBattery = int.MaxValue;

    private void Awake()
    {
        _grabbableObject = GetComponentInParent<GrabbableObject>(true);
        
        if (_grabbableObject == null || _grabbableObject.itemProperties.requiresBattery is false)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (_grabbableObject.insertedBattery is null)
        {
            subText = NoBatteryMsg;
            return;
        }

        int currentBattery = _grabbableObject.insertedBattery.empty ? 0 : (int)Math.Ceiling(Math.Clamp(_grabbableObject.insertedBattery.charge * 100f, 0f, 100f));

        if (_currentBattery == currentBattery)
            return;

        _currentBattery = currentBattery;

        subText = string.Join(currentBattery.ToString(), BatteryPercentMsg);
    }
}