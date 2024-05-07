using Unity.Netcode;

namespace ScanTweaks;

public static class Player
{
    public static bool IsServerOrHost => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
}