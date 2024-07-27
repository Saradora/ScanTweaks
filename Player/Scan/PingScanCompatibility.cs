using ReservedItemSlotCore;
using ReservedItemSlotCore.Data;

namespace ScanTweaks;

public interface IPocketedGrabbableComparer
{
    public bool IsPocketedGrabbableScannable(GrabbableObject grabbableObject);
}

public class DefaultPocketedGrabbableComparer : IPocketedGrabbableComparer
{
    public bool IsPocketedGrabbableScannable(GrabbableObject grabbableObject)
    {
        return false;
    }
}

public class ReservedPocketedGrabbableComparer : IPocketedGrabbableComparer
{
    public bool IsPocketedGrabbableScannable(GrabbableObject grabbableObject)
    {
        if (grabbableObject.playerHeldBy == null)
            return false;
        
        if (!SessionManager.TryGetUnlockedItemData(grabbableObject, out ReservedItemData itemData))
            return false;

        if (!itemData.showOnPlayerWhileHolstered)
            return false;

        if (!ReservedPlayerData.allPlayerData.TryGetValue(grabbableObject.playerHeldBy, out var playerData))
            return false;

        return playerData.IsItemInReservedItemSlot(grabbableObject);
    }
}