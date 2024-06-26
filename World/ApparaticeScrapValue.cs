﻿using System.Collections;
using UnityEngine;
using UnityMDK.Config;

namespace ScanTweaks.World;

public class ApparaticeScrapValue : MonoBehaviour
{
    [ConfigSection("Apparatice")]
    [ConfigDescription("Make the apparatice have a random scrap value, also fixes the \"???\" on the scan node, Host-only")]
    public static ConfigData<bool> ApparaticeMakeRandomValue { get; } = new(true);
    
    [ConfigDescription("Minimum possible scrap value of the apparatice. This value has a x0.4 multiplier")]
    private static readonly ConfigData<int> ApparaticeMinValue = new(125); // patch 47 has a 0.4 multiplier on scraps
    
    [ConfigDescription("Maximum possible scrap value of the apparatice. This value has a x0.4 multiplier")]
    private static readonly ConfigData<int> ApparaticeMaxValue = new(350);

    private LungProp _lungProp;

    public static readonly List<LungProp> PatchedApparatices = new();

    private void Awake()
    {
        _lungProp = GetComponent<LungProp>();
    }
    
    private static bool IsLungPropValid(LungProp lungProp)
    {
        if (!lungProp.isLungDocked || !lungProp.isLungPowered) return false;
        return lungProp.isInFactory;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(5f);
        
        if (!IsLungPropValid(_lungProp))
            yield break;

        if (!ApparaticeMakeRandomValue)
            yield break;

        if (!Player.IsServerOrHost) yield break;
        
        int minValue = ApparaticeMinValue;
        int maxValue = ApparaticeMaxValue;
        if (minValue > maxValue) (minValue, maxValue) = (maxValue, minValue);
        int value = (int)(RoundManager.Instance.AnomalyRandom.Next(minValue, maxValue) * RoundManager.Instance.scrapValueMultiplier);
        _lungProp.SetScrapValue(value);
        
        PatchedApparatices.Add(_lungProp);
    }
}