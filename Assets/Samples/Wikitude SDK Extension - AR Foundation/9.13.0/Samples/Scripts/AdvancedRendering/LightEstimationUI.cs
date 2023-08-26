/******************************************************************************
 * File: LightEstimationUI.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *  2021 Wikitude GmbH.
 * 
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

﻿using UnityEngine;
using UnityEngine.UI;

public class LightEstimationUI : MonoBehaviour {

    public Light MainLight = null;

    [Header("UI Text Fields")]
    public Text BrightnessText = null;
    public Text TemperatureText = null;
    public Text ColorText = null;
    public Text DirectionText = null;

    private void Update() {
        BrightnessText.text = $"B: {MainLight.intensity}";
        TemperatureText.text = $"T: {MainLight.colorTemperature}";
        ColorText.text = $"C: {MainLight.color}";
        DirectionText.text = $"D: {MainLight.transform.rotation}";
    }
}
