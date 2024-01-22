using GameNetcodeStuff;
using LethalMDK;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityMDK.Injection;

namespace ScanTweaks;

[InjectToComponent(typeof(IngamePlayerSettings))]
public class PingScanInput : MonoBehaviour
{
    private InputAction _pingScanAction;

    public event Func<PlayerControllerB, bool> TryPingScan;
    public event Action DoPingScan;
    
    private void Awake()
    {
        _pingScanAction = GetComponent<IngamePlayerSettings>().playerInput.actions.FindAction("PingScan");
    }

    private void OnEnable()
    {
        _pingScanAction.performed += OnPingScan;
    }

    private void OnDisable()
    {
        _pingScanAction.performed -= OnPingScan;
    }

    private void OnPingScan(InputAction.CallbackContext ctx)
    {
        PlayerControllerB player = Player.LocalPlayer;
        
        if (TryPingScan is not null)
        {
            foreach (var @delegate in TryPingScan.GetInvocationList())
            {
                var func = (Func<PlayerControllerB, bool>)@delegate;
                if (!func(player)) return;
            }
        }
        
        DoPingScan?.Invoke();
    }
}