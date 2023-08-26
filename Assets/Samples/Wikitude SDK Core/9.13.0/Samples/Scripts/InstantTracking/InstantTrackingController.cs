/******************************************************************************
 * File: InstantTrackingController.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Wikitude;
using System.Linq;
using Plane = UnityEngine.Plane;

public class InstantTrackingController : SampleController
{
    /* The GameObject that contains the all the furniture buttons. */
    public GameObject ButtonDock;
    /* The GameObject that contains the UI elements used to initialize instant tracking. */
    public GameObject InitializationControls;

    /* Button that initializes the tracking state. */
    public Button InitializeButton;

    /* The label indicating the current DeviceHeightAboveGround. */
    public Text HeightLabel;

    public InstantTracker Tracker;

    public Button ResetButton;

    /* The order in theses arrays indicate which button corresponds to which model. */
    public List<Button> Buttons;
    public List<GameObject> Models;

    public Text MessageBox;

    /* Status bar at the bottom of the screen, indicating if the scene is being tracked or not. */
    public Image ActivityIndicator;

    /* The colors of the bottom status status bar */
    public Color EnabledColor = new Color(0.2f, 0.75f, 0.2f, 0.8f);
    public Color DisabledColor = new Color(1.0f, 0.2f, 0.2f, 0.8f);
    
    [HideInInspector]
    public GameObject InstantTarget;

    /* Controller that moves the furniture based on user input. */
    private MoveController _moveController;

    private GameObject _modelParent;
    /* The currently rendered augmentations. */
    private HashSet<GameObject> _activeModels = new HashSet<GameObject>();
    /* The state in which the tracker currently is. */
    private InstantTrackingState _currentState = InstantTrackingState.Initializing;
    public InstantTrackingState CurrentState {
        get { return _currentState; }
    }
    private bool _isTracking = false;

    public HashSet<GameObject> ActiveModels {
        get {
            return _activeModels;
        }
    }

    protected override void Awake() {
        base.Awake();
        
        Application.targetFrameRate = 60;

        _moveController = GetComponent<MoveController>();
    }

    protected override void Start() {
        base.Start();
        QualitySettings.shadowDistance = 4.0f;
        _modelParent = new GameObject("model_parent");

        MessageBox.text = "Starting the SDK";
    }

    protected override void Update() {
        base.Update();
        if (_currentState == InstantTrackingState.Initializing) {
            /* Change the color of the grid to indicate if tracking can be started or not. */
            if (Tracker.CanStartTracking()) {
                if(InitializeButton != null) {
                    InitializeButton.interactable = true;
                }
            } else {
                if(InitializeButton != null) {
                    InitializeButton.interactable = false;
                }
            }
        }
    }

    #region UI Events
    public void OnInitializeButtonClicked() {
        Tracker.SetState(InstantTrackingState.Tracking);
        MessageBox.text = "Tracking the environment";
    }

    public void OnHeightValueChanged(float newHeightValue) {
        HeightLabel.text = newHeightValue.ToString("F1") + " m";
        Tracker.DeviceHeightAboveGround = newHeightValue;
    }

    public void OnAugmentationButtonPressed (int modelIndex) {
        if (_isTracking) {
            /* If we're tracking, instantiate a new model prefab based on the button index and */
            GameObject modelPrefab = Models[modelIndex];
            Transform model = Instantiate(modelPrefab).transform;
            _activeModels.Add(model.gameObject);
            /* Set model position at touch position */
            var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane p = new Plane(_modelParent.transform.up, _modelParent.transform.position);

            float enter;
            if (p.Raycast(cameraRay, out enter)) {
                model.position = cameraRay.GetPoint(enter);
                model.parent = _modelParent.transform;
            }

            /* Set model orientation to face toward the camera */
            Quaternion modelRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-_modelParent.transform.forward, _modelParent.transform.up), _modelParent.transform.up);
            model.rotation = modelRotation;

            /* Assign the new model to the move controller, so that it can be further dragged after it leaves the button area. */
            if (_moveController != null) {
                _moveController.SetMoveObject(model);
            }
        }
    }

    public void OnResetButtonClicked() {
        Tracker.SetState(InstantTrackingState.Initializing);
        ResetButton.gameObject.SetActive(false);
        MessageBox.text = "Starting the SDK";
    }
    #endregion

    #region Tracker Events
    public void OnSceneRecognized(InstantTarget target) {
        InstantTarget = target.Drawable;
        
        /* realign augmentation parent according to the instant target */
        _modelParent.transform.parent = InstantTarget.transform;
        _modelParent.transform.localPosition = Vector3.zero;
        _modelParent.transform.localRotation = Quaternion.identity;

        Transform placementNotification = InstantTarget.GetComponentsInChildren<Transform>(true).Single(t => t.name == "Placement Notification");
        if (placementNotification) {
            placementNotification.gameObject.SetActive(true);
        }
        
        SetSceneActive(true);
    }

    public void OnSceneLost(InstantTarget target) {
        _modelParent.transform.parent = null;
        
        InstantTarget = null;
        
        SetSceneActive(false);
    }

    private void SetSceneActive(bool active) {
        /* Because SetSceneActive(false) can be called when the scene is destroyed,
         * first check if the GameObjects and Components are still valid.
         */
        foreach (var button in Buttons) {
            if (button) {
                button.interactable = active;
            }
        }

        foreach (var model in _activeModels) {
            if (model) {
                model.SetActive(active);
            }
        }

        if (ActivityIndicator) {
            ActivityIndicator.color = active ? EnabledColor : DisabledColor;
        }

        _isTracking = active;
    }

    public void OnStateChanged(InstantTrackingState newState) {
        _currentState = newState;
        if (newState == InstantTrackingState.Tracking) {
            if (InitializationControls != null) {
                InitializationControls.SetActive(false);
            }

            if (ButtonDock != null) {
                ButtonDock.SetActive(true);
            }

            ResetButton.gameObject.SetActive(true);
        } else {
            /* When the state is changed back to initialization, make sure that all the previous augmentations are cleared */
            foreach (var model in _activeModels) {
                Destroy(model);
            }
            _activeModels.Clear();

            if (InitializationControls != null) {
                InitializationControls.SetActive(true);
            }

            if (ButtonDock != null) {
                ButtonDock.SetActive(false);
            }
        }
    }

    /* Used when augmentations are loaded from disk. Please see SaveInstantTarget and LoadInstantTarget for more information. */
    internal void LoadAugmentation(AugmentationDescription augmentation) {
        GameObject modelPrefab = Models[augmentation.ID];
        Transform model = Instantiate(modelPrefab).transform;
        _activeModels.Add(model.gameObject);

        model.parent = _modelParent.transform;
        model.localPosition = augmentation.LocalPosition;
        model.localRotation = augmentation.LocalRotation;
        model.localScale = augmentation.LocalScale;

        model.gameObject.SetActive(false);
    }

    public void OnError(Error error) {
        PrintError("Instant Tracker error!", error, true);
    }

    public void OnFailedStateChange(InstantTrackingState failedState, Error error) {
        PrintError("Failed to change state to " + failedState, error, true);
    }
    #endregion
}
