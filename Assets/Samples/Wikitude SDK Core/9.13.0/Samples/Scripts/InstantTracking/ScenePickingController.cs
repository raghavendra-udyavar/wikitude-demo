/******************************************************************************
 * File: ScenePickingController.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *  2018-2022 Wikitude GmbH.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Wikitude;

public class ScenePickingController : SampleController
{
    public InstantTracker Tracker;
    /* The augmentation prefab that should be placed whenever a successful hit was registered. */
    public GameObject Augmentation;

    /* Button that toggles between Initializing and Tracking state. */
    public Button ToggleStateButton;
    public Text ToggleStateButtonText;
    public ToastStack ToastStack;

    /* The slider used to define the DeviceHeightAboveGround property. */
    public GameObject HeightSlider;
    public Text HeightLabel;

    /* The state in which the tracker currently is. */
    private InstantTrackingState _currentTrackerState = InstantTrackingState.Initializing;
    /* Flag to determine if we are currently in the process of chaning from one InstantTrackingState to another. */
    private bool _changingState = false;

    /* The currently rendered augmentations */
    private List<GameObject> _augmentations = new List<GameObject>();
    private GameObject _instantTarget;
    private GameObject _augmentationParent;
    private SpriteRenderer _initializationSpriteRenderer;
    private int _screenConversionFailedToastID;

    protected override void Awake() {
        base.Awake();
        _screenConversionFailedToastID = ToastStack.CreateToast("");
    }

    protected override void Start() {
        base.Start();
        _augmentationParent = new GameObject("model_parent");
    }

    protected override void Update() {
        base.Update();

        /* If we register a screen tap while we're tracking, convert the screen tap to a coordinate in the map. */
        if (_instantTarget && Input.GetMouseButtonUp(0)) {
            /* The result of this operation is not instantaneous, so we need to wait for
             * the OnScreenConversionComputed callback to get the results. */
            Tracker.ConvertScreenCoordinate(Input.mousePosition);
        }

        /* Change the opacity of the texture to indicate if tracking can be started or not. */
        if (_initializationSpriteRenderer) {
            if (Tracker.CanStartTracking()) {
                _initializationSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                if (ToggleStateButton) {
                    ToggleStateButton.interactable = true;
                }
            } else {
                _initializationSpriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
                if (ToggleStateButton) {
                    ToggleStateButton.interactable = false;
                }
            }
        }
    }


    public void OnStateChanged(InstantTrackingState newState) {
        _currentTrackerState = newState;

        if (_currentTrackerState == InstantTrackingState.Initializing) {
            if (ToggleStateButtonText) {
                ToggleStateButtonText.text = "Start Tracking";
            }
            if (HeightSlider) {
                HeightSlider.SetActive(true);
            }
        } else {
            if (ToggleStateButtonText) {
                ToggleStateButtonText.text = "Start Initialization";
            }
            if (HeightSlider) {
                HeightSlider.SetActive(false);
            }
        }

        _changingState = false;
    }

    public void OnScreenConversionComputed(bool success, Vector2 screenCoordinate, Vector3 pointCloudCoordinate) {
        if (success) {
            /* The pointCloudCoordinate values are in the local space of the trackable's Drawable. */
            var newAugmentation = GameObject.Instantiate(Augmentation, _augmentationParent.transform) as GameObject;
            newAugmentation.transform.localPosition = pointCloudCoordinate;
            newAugmentation.transform.localScale = Vector3.one;
            _augmentations.Add(newAugmentation);
        } else {
            ToastStack.SetToastText(_screenConversionFailedToastID, $"No point found at the touched position: {screenCoordinate.x}, {screenCoordinate.y}.");
            ToastStack.ShowToast(_screenConversionFailedToastID);
        }
    }

    public void OnToggleStateButtonPressed() {
        if (!_changingState) {
            if (_currentTrackerState == InstantTrackingState.Initializing) {
                if (ToggleStateButtonText) {
                    ToggleStateButtonText.text = "Switching State...";
                }
                _changingState = true;
                Tracker.SetState(InstantTrackingState.Tracking);
            } else {
                foreach (var augmentation in _augmentations) {
                    Destroy(augmentation);
                }
                _augmentations.Clear();
                if (ToggleStateButtonText) {
                    ToggleStateButtonText.text = "Switching State...";
                }
                _changingState = true;
                Tracker.SetState(InstantTrackingState.Initializing);
            }
        }
    }

    public void OnInitializationStarted(InstantTarget target) {
        _initializationSpriteRenderer = target.Drawable.GetComponentInChildren<SpriteRenderer>();
    }

    public void OnInitializationStopped(InstantTarget target) {
        _initializationSpriteRenderer = null;
    }

    public void OnSceneRecognized(InstantTarget target) {
        _instantTarget = target.Drawable;
        Transform placementNotification = _instantTarget.GetComponentsInChildren<Transform>(true).Single(t => t.name == "Placement Notification");
        if (placementNotification) {
            placementNotification.gameObject.SetActive(true);
        }
        
        /* realign augmentation parent according to the instant target */
        _augmentationParent.transform.parent = _instantTarget.transform;
        _augmentationParent.transform.localPosition = Vector3.zero;
        _augmentationParent.transform.localRotation = Quaternion.identity;

        foreach(GameObject augmentation in _augmentations) {
            augmentation.SetActive(true);
        }
    }

    public void OnSceneLost(InstantTarget target) {
        _augmentationParent.transform.parent = null;
        _instantTarget = null;
        foreach(GameObject augmentation in _augmentations) {
            augmentation.SetActive(false);
        }
    }

    public void OnHeightValueChanged(float newHeightValue) {
        if (HeightLabel) {
            HeightLabel.text = newHeightValue.ToString("F1") + " m";
        }
        Tracker.DeviceHeightAboveGround = newHeightValue;
    }

    public void OnError(Error error) {
        _changingState = false;
        if (ToggleStateButtonText) {
            ToggleStateButtonText.text = "Start Tracking";
        }
        PrintError("Instant Tracker error!", error, true);
    }

    public void OnFailedStateChange(InstantTrackingState failedState, Error error) {
        PrintError("Failed to change state to " + failedState, error, true);
    }
}
