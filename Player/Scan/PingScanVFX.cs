using LethalMDK;
using UnityEngine;
using UnityMDK.Injection;

namespace ScanTweaks;

[InjectToComponent(typeof(PingScanInput))]
public class PingScanVFX : MonoBehaviour
{
    private PingScanInput _pingScanInput;
    
    private static readonly int ATriggerScan = Animator.StringToHash("scan");

    private void Awake()
    {
        _pingScanInput = GetComponent<PingScanInput>();
    }

    private void OnEnable()
    {
        _pingScanInput.DoPingScan += OnDoPingScan;
    }

    private void OnDisable()
    {
        _pingScanInput.DoPingScan -= OnDoPingScan;
    }

    private void OnDoPingScan()
    {
        var player = Player.LocalPlayer;
        if (!player || !HUDManager.Instance) return;
        
        HUDManager.Instance.scanEffectAnimator.transform.position = player.gameplayCamera.transform.position;
        HUDManager.Instance.scanEffectAnimator.SetTrigger(ATriggerScan);
        HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.scanSFX);
    }
}