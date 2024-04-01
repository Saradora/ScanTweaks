using GameNetcodeStuff;
using LethalMDK;
using TMPro;
using UnityEngine;
using UnityMDK.Injection;

namespace ScanTweaks.UI;

[InjectToComponent(typeof(HUDManager))]
public class PingScanUI : MonoBehaviour
{
    [SerializeField] private float _counterUpdateSpeed = 1500f;
    [SerializeField] private float _counterInterval = 0.03f;
    private float _nextCounterUpdate;
    
    private HUDManager _hudManager;

    private Canvas _canvas;
    private RectTransform _canvasTransform;
    private RectTransform _nodeContainer;

    private Queue<RectTransform> _uiElementsPool = new();

    private Dictionary<ScanNodeProperties, RectTransform> _currentScanNodes = new();
    
    private static readonly int AColorNumber = Animator.StringToHash("colorNumber");
    private static readonly int ADisplayBool = Animator.StringToHash("display");

    private int _currentScrapValue;

    private readonly SortedList<float, List<RectTransform>> _sortedScanNodes = new();

    private void Awake()
    {
        _hudManager = GetComponent<HUDManager>();

        _canvas = _hudManager.scanElements[0].GetComponentInParent<Canvas>(true);
        _canvasTransform = (RectTransform)_canvas.transform;
        _nodeContainer = (RectTransform)_hudManager.scanElements[0].parent.transform;

        for (int i = 1; i < _hudManager.scanElements.Length; i++)
        {
            _uiElementsPool.Enqueue(_hudManager.scanElements[i]);
        }
    }

    private void OnEnable()
    {
        PingScan.ScanNodeAdded += OnScanNodeAdded;
        PingScan.ScanNodeRemoved += OnScanNodeRemoved;
    }

    private void OnDisable()
    {
        PingScan.ScanNodeAdded -= OnScanNodeAdded;
        PingScan.ScanNodeRemoved -= OnScanNodeRemoved;
    }

    private void Update()
    {
        UpdateNodeContainer();
        UpdateScanUIPositions();
        UpdateScrapValueCounter();
    }

    private void UpdateNodeContainer()
    {
        _nodeContainer.sizeDelta = Vector2.zero;
        _nodeContainer.localPosition = Vector3.zero;
    }

    private void UpdateScanUIPositions()
    {
        PlayerControllerB player = Player.LocalPlayer;
        if (!player) return;

        Camera cam = player.gameplayCamera;

        List<ScanNodeProperties> toDelete = new();
        var sortedScanNodes = _sortedScanNodes;
        _sortedScanNodes.Clear();

        var canvasSize = _canvasTransform.sizeDelta;

        foreach ((ScanNodeProperties scanNode, RectTransform rect) in _currentScanNodes)
        {
            if (!scanNode)
            {
                toDelete.Add(scanNode);
                continue;
            }

            Vector3 scanNodePosition = scanNode.transform.position;
            float distance = Vector3.SqrMagnitude(scanNodePosition - cam.transform.position);
            
            if (!sortedScanNodes.ContainsKey(distance)) sortedScanNodes.Add(distance, new List<RectTransform>());
            
            sortedScanNodes[distance].Add(rect);

            Vector3 pos = cam.WorldToViewportPoint(scanNodePosition);
            pos = ((Vector2)pos - Vector2.one * 0.5f) * canvasSize;
            rect.localPosition = pos;
        }

        foreach (var scanNode in toDelete)
        {
            OnScanNodeRemoved(scanNode);
        }

        foreach (var sortedScanNode in sortedScanNodes)
        {
            foreach (var uiNode in sortedScanNode.Value)
            {
                uiNode.SetAsFirstSibling();
            }
        }
    }

    private void UpdateScrapValueCounter()
    {
        if (PingScan.PingedScrapValue == _currentScrapValue) return;
        
        if (Time.time < _nextCounterUpdate) return;

        _nextCounterUpdate = Time.time + _counterInterval;

        if (_hudManager.scanInfoAnimator.GetBool(ADisplayBool) is false)
        {
            _hudManager.scanInfoAnimator.SetBool(ADisplayBool, true);
        }

        if (_currentScrapValue < PingScan.PingedScrapValue)
        {
            _currentScrapValue = Mathf.RoundToInt(Mathf.MoveTowards(_currentScrapValue, PingScan.PingedScrapValue, _counterUpdateSpeed * Time.deltaTime));
            _hudManager.UIAudio.PlayOneShot(_hudManager.addToScrapTotalSFX);
        }
        else
        {
            _currentScrapValue = PingScan.PingedScrapValue;
        }

        _hudManager.totalValueText.text = $"${_currentScrapValue}";

        if (_currentScrapValue == 0)
        {
            _hudManager.scanInfoAnimator.SetBool(ADisplayBool, false);
        }
        else if (PingScan.PingedScrapValue == _currentScrapValue)
        {
            _hudManager.UIAudio.PlayOneShot(_hudManager.finishAddingToTotalSFX);
        }
    }

    private RectTransform GetUIElement()
    {
        if (_uiElementsPool.Count > 0)
        {
            return _uiElementsPool.Dequeue();
        }
        
        RectTransform instance = Instantiate(_hudManager.scanElements[0], _hudManager.scanElements[0].transform.parent);
        instance.SetAsFirstSibling();
        return instance;
    }

    private void OnScanNodeAdded(ScanNodeProperties scanNode)
    {
        if (_currentScanNodes.ContainsKey(scanNode)) return;
        
        RectTransform uiElement = GetUIElement();
        uiElement.gameObject.SetActive(true);

        TextMeshProUGUI[] texts = uiElement.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length == 2)
        {
            texts[0].text = scanNode.headerText;
            texts[1].text = scanNode.subText;
        }
        
        uiElement.GetComponent<Animator>().SetInteger(AColorNumber, scanNode.nodeType);
        
        _currentScanNodes.Add(scanNode, uiElement);
    }

    private void OnScanNodeRemoved(ScanNodeProperties scanNode)
    {
        if (!_currentScanNodes.ContainsKey(scanNode)) return;

        RectTransform uiElement = _currentScanNodes[scanNode];
        
        uiElement.gameObject.SetActive(false);

        _currentScanNodes.Remove(scanNode);
        
        _uiElementsPool.Enqueue(uiElement);
    }
}