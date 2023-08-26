/******************************************************************************
 * File: RandomJitterBehaviour.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *  2021 Wikitude GmbH.
 * 
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

﻿using UnityEngine;

public class RandomJitterBehaviour : MonoBehaviour
{
    private Vector3 _origin;
    private Vector3 _randomPosition;
    private float _maxRandomOffset = 0.002f;
    private float _timer;
    private float _travelTime = 0.1f;

    private void Start() {
        _origin = transform.localPosition;
        _randomPosition = _origin;
        _timer = _travelTime;
    }

    private void Update() {
        transform.localPosition = Vector3.Lerp(transform.localPosition, _randomPosition, Time.deltaTime/_travelTime);
        
        _timer += Time.deltaTime;

        if (_timer > _travelTime) {
            _timer = 0f;
            _randomPosition = new Vector3(_origin.x + Random.Range(-_maxRandomOffset, _maxRandomOffset), 
                                         _origin.y + Random.Range(-_maxRandomOffset, _maxRandomOffset),
                                         _origin.z + Random.Range(-_maxRandomOffset, _maxRandomOffset));
        }
    }
}
