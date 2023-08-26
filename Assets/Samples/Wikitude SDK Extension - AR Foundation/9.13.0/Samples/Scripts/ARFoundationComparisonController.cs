/******************************************************************************
 * File: MoveController.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Wikitude;

public class ARFoundationComparisonController : MonoBehaviour {

    public InstantTracker Tracker;
    public ARPlaneManager ARPlaneManager;

    /* The state in which the tracker currently is. */
    private InstantTrackingState _currentTrackerState = InstantTrackingState.Initializing;

    private readonly List<PlaneTarget> _trackedPlanes = new List<PlaneTarget>();

    [SerializeField]
    private Text _detectionModeLabel;

    private String[] _detectionModeDisplayText = {"AR Foundation tracking", "Wikitude tracking"};
    private bool _initialized;
    
    private enum PlaneDetectionMode : uint {
        ARFoundation = 0,
        Wikitude = 1
    }

    private PlaneDetectionMode _detectionMode = PlaneDetectionMode.ARFoundation;

    private void Awake() {
        /* Disable the ARPlaneManager until the InstantTracker
         * can start Tracking
         */
        ARPlaneManager.enabled = false;
        _detectionModeLabel.text = _detectionModeDisplayText[(int) _detectionMode];
    }

    public void OnToggleButtonPressed() {
        _detectionMode = (PlaneDetectionMode) (((int)_detectionMode + 1) % 2);
        _detectionModeLabel.text = _detectionModeDisplayText[(int) _detectionMode];

        SetARPlaneDetectionActive(_detectionMode == PlaneDetectionMode.ARFoundation && _initialized);
        SetWikitudePlaneDetectionActive(_detectionMode == PlaneDetectionMode.Wikitude && _initialized);
    }

    private void SetARPlaneDetectionActive(bool active) {
        foreach (var trackable in ARPlaneManager.trackables) {
            trackable.gameObject.SetActive(active);
        }
    }

    private void SetWikitudePlaneDetectionActive(bool active) {
        foreach (var plane in _trackedPlanes) {
            plane.Drawable.SetActive(active);
        }
    }

    public void OnStateChanged(InstantTrackingState newState) {
        _currentTrackerState = newState;
        if (!_initialized && _currentTrackerState == InstantTrackingState.Initializing) {
            Tracker.SetState(InstantTrackingState.Tracking);
            ARPlaneManager.enabled = true;
            ARPlaneManager.planesChanged += OnARFoundationPlanesChanged;
            _initialized = true;
        }
    }

    public void OnFailedStateChange(InstantTrackingState failedState, Error error) {
        Debug.Log("Failed to change state to " + failedState);
    }

    private void OnARFoundationPlanesChanged(ARPlanesChangedEventArgs args) {
        if (_detectionMode == PlaneDetectionMode.Wikitude) {
            foreach (var plane in args.added) {
                plane.gameObject.SetActive(false);
            }
        }
    }

    public void OnWikitudePlaneRecognized(PlaneTarget recognizedPlane) {
        _trackedPlanes.Add(recognizedPlane);
        if (_detectionMode == PlaneDetectionMode.ARFoundation) {
            recognizedPlane.Drawable.SetActive(false);
        }
    }

    public void OnWikitudePlaneLost(PlaneTarget lostPlane) {
        _trackedPlanes.Remove(lostPlane);
    }
}
