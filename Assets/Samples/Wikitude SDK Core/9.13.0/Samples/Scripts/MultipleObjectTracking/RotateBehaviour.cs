/******************************************************************************
 * File: RotateBehaviour.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *  2021 Wikitude GmbH.
 * 
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

﻿using UnityEngine;

public class RotateBehaviour : MonoBehaviour
{
    public Vector3 ConstantRotation = Vector3.zero;
    public bool RandomRotation = false;
    public float RandomRange = 45f;

    private void Start() {
        if (RandomRotation) {
            ConstantRotation = new Vector3( Random.Range(-RandomRange, RandomRange),
                                            Random.Range(-RandomRange, RandomRange),
                                            Random.Range(-RandomRange, RandomRange));
        }
    }

    private void Update() {
        transform.Rotate(ConstantRotation * Time.deltaTime, Space.Self);
    }
}
