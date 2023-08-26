/******************************************************************************
 * File: LoadInstantTarget.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Wikitude;

public class LoadInstantTarget : MonoBehaviour {
    public InstantTracker Tracker;
    public InstantTrackingController Controller;
    public Text InfoMessage;
    public Button LoadButton;
    public InstantTrackable Trackable;
    private GameObject TrackingPlane;

    public void OnLoadButtonPressed() {
        LoadTarget();
        LoadScene();
    }

    public void OnChangedState(InstantTrackingState state) {
        if (state == InstantTrackingState.Tracking) {
            LoadButton.gameObject.SetActive(false);
        } else {
            LoadButton.gameObject.SetActive(true);
        }
    }

    public void OnSceneRecognized(InstantTarget target) {
        FindAugmentationTrackingPlane(target);
        if (TrackingPlane != null) {
            TrackingPlane.SetActive(true);
        }
    }

    private void FindAugmentationTrackingPlane(InstantTarget target) {
        if (TrackingPlane == null) {
            Transform planeTransform = target.Drawable.transform.Find(Trackable.Drawable.name + "(Clone)");
            if (planeTransform != null) {
                TrackingPlane = planeTransform.gameObject;
            }
        }
    }
    
    public void OnSceneLost(InstantTarget target) {
        if (TrackingPlane != null) {
            TrackingPlane.SetActive(false);
            TrackingPlane = null;
        }
    }

    /* Loads the instant target from the disk, without any augmentations. */
    private void LoadTarget() {
        /* A TargetCollectionResource is needed to manage file loading. */
        var targetCollectionResource = new TargetCollectionResource();
        /* UseCustomURL is used to specify that the file is not inside the "StreamingAssets" folder */
        targetCollectionResource.UseCustomURL = true;
        /* The "file://" is used to indicate that the file is located on disk, and not on a server. */
        targetCollectionResource.TargetPath = "file://" + Application.persistentDataPath + "/InstantTarget.wto";
        var configuration = new InstantTargetRestorationConfiguration();
        /* Indicate that we allow the target to be expanded after it was loaded. */
        configuration.ExpansionPolicy = InstantTargetExpansionPolicy.Allow;
        Tracker.LoadInstantTarget(targetCollectionResource, configuration);
    }

    /* Loads all augmentations from disk. */
    private void LoadScene() {
        try {
            string json = File.ReadAllText(Application.persistentDataPath + "/InstantScene.json");
            var sceneDescription = JsonUtility.FromJson<SceneDescription>(json);

            foreach (var augmentation in sceneDescription.Augmentations) {
                Controller.LoadAugmentation(augmentation);
            }
        } catch (Exception ex) {
            InfoMessage.text = "Error loading scene augmentations.";
            Debug.LogError("Error loading augmentations: " + ex.Message);
        }
    }
}
