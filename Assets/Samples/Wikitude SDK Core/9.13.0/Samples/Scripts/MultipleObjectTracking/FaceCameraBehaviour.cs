/******************************************************************************
 * File: FaceCameraBehaviour.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *  2021 Wikitude GmbH.
 * 
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

﻿using UnityEngine;

public class FaceCameraBehaviour : MonoBehaviour
{
    private Transform _cameraTransform;
    
    public bool LockXAxis = false;

    private void Start() {
        _cameraTransform = Camera.main.transform;
    }

    private void Update() {
        transform.LookAt(_cameraTransform.position, _cameraTransform.rotation * Vector3.up);

        /* Lock rotation of certain axis if needed. */
        if (LockXAxis) {
            Vector3 euler = transform.localRotation.eulerAngles;
            euler.x = 0f;
            transform.localEulerAngles = euler;
        }
    }
}
