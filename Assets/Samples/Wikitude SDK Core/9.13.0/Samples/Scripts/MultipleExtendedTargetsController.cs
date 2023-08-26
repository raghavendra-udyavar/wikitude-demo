/******************************************************************************
 * File: MultipleExtendedTargetsController.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *  2021 Wikitude GmbH.
 * 
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Wikitude;

public class MultipleExtendedTargetsController : MonoBehaviour
{
    private float _distanceThreshold;
    private Dictionary<GameObject, GameObject> _dinosaursWithTarget = new Dictionary<GameObject, GameObject>();
    private List<ImageTarget> _targets = new List<ImageTarget>();
    private AssetBundle loadedAssetBundle;

    private void Awake() {
        // load dinosaur prefabs from corresponding asset bundle
        var assetBundleName = AssetLoaderHelper.GetAssetBundleNameAccordingToPlatform("assetBundle_dinosaurs");
        loadedAssetBundle = AssetLoaderHelper.LoadAssetBundle(assetBundleName, "Dinosaurs");
        if (loadedAssetBundle != null) {
            GameObject[] bundleContent = loadedAssetBundle.LoadAllAssets<GameObject>();
            foreach (GameObject dinoObject in bundleContent) {
                if (dinoObject.name.StartsWith("Dino_")) {
                    _distanceThreshold = Dinosaur.DistanceThreshold;
                    GameObject dinosaur = Instantiate(dinoObject);
                    dinosaur.SetActive(false);
                    // rename "Dino_XXX to XXX
                    var dinosaurName = dinosaur.name.Substring(5);
                    _dinosaursWithTarget.Add(dinosaur, new GameObject(dinosaurName + "_target"));
                    dinosaur.GetComponent<Dinosaur>().SetAlignDinosaur(true);
                }
            }
        } 
    }

    public void OnImageRecognized(ImageTarget target) {
        _targets.Add(target);
    }

    public void OnImageLost(ImageTarget target) {
        _targets.Remove(target);
    }

    private void Update() {
        foreach(var dinosaur in _dinosaursWithTarget) {
            ImageTarget target = _targets.LastOrDefault(obj => dinosaur.Key.name.IndexOf(obj.Name, StringComparison.OrdinalIgnoreCase) >= 0);
            if (target != null) {
                if (!dinosaur.Key.activeSelf || (dinosaur.Value.transform.position - target.Drawable.transform.position).magnitude > _distanceThreshold) {
                    if (!dinosaur.Key.activeSelf) {
                        dinosaur.Key.transform.SetPositionAndRotation(target.Drawable.transform.position, target.Drawable.transform.rotation);
                        dinosaur.Key.transform.up = Vector3.up;
                        dinosaur.Key.SetActive(true);
                    }
                    dinosaur.Value.transform.SetPositionAndRotation(target.Drawable.transform.position, target.Drawable.transform.rotation);
                    dinosaur.Key.GetComponent<Dinosaur>().StartWalkCoroutine(dinosaur.Value.transform);
                } else if ((dinosaur.Key.gameObject.transform.position - dinosaur.Value.transform.position).magnitude < _distanceThreshold) {
                    dinosaur.Key.GetComponent<Dinosaur>().StopIfWalking();
                }
            } else {
                dinosaur.Value.transform.SetPositionAndRotation(dinosaur.Key.transform.position, dinosaur.Key.transform.rotation);
                dinosaur.Key.GetComponent<Dinosaur>().StopIfWalking();
            }
        }
    }

    public void OnDestroy() {
        // unload asset bundle after usage
        if (loadedAssetBundle != null) {
            loadedAssetBundle.Unload(true);
        }
    }
}