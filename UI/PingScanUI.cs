﻿using System.Diagnostics;
using GameNetcodeStuff;
using LethalMDK;
using TMPro;
using UnityEngine;
using UnityMDK.Config;
using UnityMDK.Injection;
using UnityMDK.Logging;

namespace ScanTweaks.UI;

[InjectToComponent(typeof(HUDManager))]
public class PingScanUI : MonoBehaviour
{
    [ConfigSection("PingScan")]
    [ConfigDescription("The speed at which the scrap counter updates.")]
    private static ConfigData<int> ScrapCounterUpdateSpeed = new(1000);

    [ConfigSection("UI")]
    [ConfigDescription("Should the nodes be sorted by distance, enabling this can have an impact on performances when a lot of nodes are displayed")]
    private static ConfigData<bool> SortNodesByDistance = new(true);
    
    [SerializeField] private float _counterInterval = 0.03f;
    private float _nextCounterUpdate;
    
    private HUDManager _hudManager;

    private Canvas _canvas;
    private RectTransform _canvasTransform;
    private RectTransform _nodeContainer;

    private Stack<UIElement> _uiElementsPool = new();

    private Dictionary<ScanNodeProperties, UIElement> _currentScanNodes = new();
    
    private static readonly int AColorNumber = Animator.StringToHash("colorNumber");
    private static readonly int ADisplayBool = Animator.StringToHash("display");
    private static readonly string DollarSign = "$";

    private int _currentScrapValue;

    private readonly List<ScannedObject> _scannedObjects = new();
    private readonly List<ScanNodeProperties> _toDelete = new();

    private readonly ScannedObjectComparer _distanceComparer = new();

    private struct ScannedObject
    {
        public float distance;
        public UIElement element;

        public ScannedObject(float distance, UIElement element)
        {
            this.distance = distance;
            this.element = element;
        }
    }

    private class ScannedObjectComparer : IComparer<ScannedObject>
    {
        public int Compare(ScannedObject x, ScannedObject y)
        {
            if (x.distance > y.distance)
                return -1;
            return 1;
        }
    }

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
            if (obj is Transform trans)
                return transform == trans;
            
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
            _uiElementsPool.Push(GetUIElementFromRectTransform(_hudManager.scanElements[i]));
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
        UpdateScanNodes();
        UpdateScrapValueCounter();
    }

    private void UpdateNodeContainer()
    {
        _nodeContainer.sizeDelta = Vector2.zero;
        _nodeContainer.localPosition = Vector3.zero;
    }

    private void UpdateScanNodes()
    {
        PlayerControllerB player = LethalMDK.Player.LocalPlayer;
        if (!player) return;

        Camera cam = player.gameplayCamera;
        Vector3 camPos = cam.transform.position;

        _toDelete.Clear();
        _scannedObjects.Clear();

        Vector2 canvasSize = _canvasTransform.sizeDelta;

        Vector2 halfOne = Vector2.one * 0.5f;

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
            float distance = Vector3.SqrMagnitude(scanNodePosition - camPos);
            
            _scannedObjects.Add(new ScannedObject(distance, element));

            Vector3 pos = cam.WorldToViewportPoint(scanNodePosition);
            pos = ((Vector2)pos - halfOne) * canvasSize;
            element.transform.localPosition = pos;
        }

        foreach (var scanNode in _toDelete)
        {
            OnScanNodeRemoved(scanNode);
        }

        if (SortNodesByDistance == false)
            return;
        
        _scannedObjects.Sort(_distanceComparer);

        int count = _scannedObjects.Count;
        for (int sortedIndex = 0; sortedIndex < count; sortedIndex++)
        {
            _scannedObjects[sortedIndex].element.transform.SetSiblingIndex(sortedIndex);
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
            return _uiElementsPool.Pop();
        }
        
        RectTransform instance = Instantiate(_hudManager.scanElements[0], _hudManager.scanElements[0].transform.parent);
        //instance.SetAsFirstSibling();
        
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
        
        _uiElementsPool.Push(uiElement);
    }
}