using GameNetcodeStuff;
using LethalMDK;
using TMPro;
using UnityEngine;
using UnityMDK.Config;
using UnityMDK.Injection;

namespace ScanTweaks.UI;

[InjectToComponent(typeof(HUDManager))]
public class PingScanUI : MonoBehaviour
{
    [ConfigSection("PingScan")]
    [ConfigDescription("The speed at which the scrap counter updates.")]
    private static ConfigData<int> ScrapCounterUpdateSpeed = new(1000);
    
    [SerializeField] private float _counterInterval = 0.03f;
    private float _nextCounterUpdate;
    
    private HUDManager _hudManager;

    private Canvas _canvas;
    private RectTransform _canvasTransform;
    private RectTransform _nodeContainer;

    private Queue<UIElement> _uiElementsPool = new();

    private Dictionary<ScanNodeProperties, UIElement> _currentScanNodes = new();
    
    private static readonly int AColorNumber = Animator.StringToHash("colorNumber");
    private static readonly int ADisplayBool = Animator.StringToHash("display");
    private static readonly string DollarSign = "$";

    private int _currentScrapValue;

    private readonly SortedList<float, List<UIElement>> _sortedScanNodes = new();
    private readonly List<ScanNodeProperties> _toDelete = new();

    private struct UIElement
    {
        public RectTransform transform;
        public TextMeshProUGUI mainText;
        public TextMeshProUGUI subText;

        public override int GetHashCode()
        {
            return transform.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return transform.Equals(obj);
        }
    }

    private void Awake()
    {
        _hudManager = GetComponent<HUDManager>();

        _canvas = _hudManager.scanElements[0].GetComponentInParent<Canvas>(true);
        _canvasTransform = (RectTransform)_canvas.transform;
        _nodeContainer = (RectTransform)_hudManager.scanElements[0].parent.transform;

        for (int i = 1; i < _hudManager.scanElements.Length; i++)
        {
            _uiElementsPool.Enqueue(GetUIElementFromRectTransform(_hudManager.scanElements[i]));
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

    private void LateUpdate()
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

        _toDelete.Clear();
        _sortedScanNodes.Clear();

        var canvasSize = _canvasTransform.sizeDelta;

        foreach ((ScanNodeProperties scanNode, UIElement element) in _currentScanNodes)
        {
            if (!scanNode)
            {
                _toDelete.Add(scanNode);
                continue;
            }

            element.mainText.text = scanNode.headerText;
            element.subText.text = scanNode.subText;

            Vector3 scanNodePosition = scanNode.transform.position;
            float distance = Vector3.SqrMagnitude(scanNodePosition - cam.transform.position);
            
            if (!_sortedScanNodes.ContainsKey(distance)) _sortedScanNodes.Add(distance, new List<UIElement>());
            
            _sortedScanNodes[distance].Add(element);

            Vector3 pos = cam.WorldToViewportPoint(scanNodePosition);
            pos = ((Vector2)pos - Vector2.one * 0.5f) * canvasSize;
            element.transform.localPosition = pos;
        }

        foreach (var scanNode in _toDelete)
        {
            OnScanNodeRemoved(scanNode);
        }

        int index = 0;
        foreach (var sortedScanNode in _sortedScanNodes.Reverse())
        {
            foreach (var uiNode in sortedScanNode.Value)
            {
                uiNode.transform.SetSiblingIndex(index);
                index++;
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
            float speed = ScrapCounterUpdateSpeed;
            int difference = Math.Abs(PingScan.PingedScrapValue - _currentScrapValue);
            difference = Math.Max(100, difference);
            speed *= difference * 0.01f;
            
            _currentScrapValue = Mathf.RoundToInt(Mathf.MoveTowards(_currentScrapValue, PingScan.PingedScrapValue, speed * Time.deltaTime));
            _hudManager.UIAudio.PlayOneShot(_hudManager.addToScrapTotalSFX);
        }
        else
        {
            _currentScrapValue = PingScan.PingedScrapValue;
        }

        _hudManager.totalValueText.text = DollarSign + _currentScrapValue;

        if (_currentScrapValue == 0)
        {
            _hudManager.scanInfoAnimator.SetBool(ADisplayBool, false);
        }
        else if (PingScan.PingedScrapValue == _currentScrapValue)
        {
            _hudManager.UIAudio.PlayOneShot(_hudManager.finishAddingToTotalSFX);
        }
    }

    private UIElement GetUIElementFromRectTransform(RectTransform instance)
    {
        TextMeshProUGUI[] texts = instance.transform.GetComponentsInChildren<TextMeshProUGUI>(true);
        UIElement newElement = new()
        {
            transform = instance,
            mainText = texts[0],
            subText = texts[1]
        };

        return newElement;
    }

    private UIElement GetFreeUIElement()
    {
        if (_uiElementsPool.Count > 0)
        {
            return _uiElementsPool.Dequeue();
        }
        
        RectTransform instance = Instantiate(_hudManager.scanElements[0], _hudManager.scanElements[0].transform.parent);
        instance.SetAsFirstSibling();
        
        return GetUIElementFromRectTransform(instance);
    }

    private void OnScanNodeAdded(ScanNodeProperties scanNode)
    {
        if (_currentScanNodes.ContainsKey(scanNode)) return;
        
        UIElement uiElement = GetFreeUIElement();
        uiElement.transform.gameObject.SetActive(true);

        uiElement.mainText.text = scanNode.headerText;
        uiElement.subText.text = scanNode.subText;
        
        uiElement.transform.GetComponent<Animator>().SetInteger(AColorNumber, scanNode.nodeType);
        
        _currentScanNodes.Add(scanNode, uiElement);
    }

    private void OnScanNodeRemoved(ScanNodeProperties scanNode)
    {
        if (!_currentScanNodes.ContainsKey(scanNode)) return;

        UIElement uiElement = _currentScanNodes[scanNode];
        
        uiElement.transform.gameObject.SetActive(false);

        _currentScanNodes.Remove(scanNode);
        
        _uiElementsPool.Enqueue(uiElement);
    }
}