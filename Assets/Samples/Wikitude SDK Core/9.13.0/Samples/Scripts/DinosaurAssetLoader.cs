/******************************************************************************
  * File: DinosaurAssetLoader.cs
  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
  *
  * Confidential and Proprietary - Qualcomm Technologies, Inc.
  *
  ******************************************************************************/

using UnityEngine;
using Wikitude;

public class DinosaurAssetLoader : MonoBehaviour {
    public ImageTrackable DiplodocusTrackable;
    public ImageTrackable SpinosaurusTrackable;
    public ImageTrackable TriceratopsTrackable;
    public ImageTrackable TyrannosaurusTrackable;

    private AssetBundle loadedAssetBundle;

    public void Awake() {
        // load dinosaur prefabs from corresponding asset bundle
        var assetBundleName = AssetLoaderHelper.GetAssetBundleNameAccordingToPlatform("assetBundle_dinosaurs");
        loadedAssetBundle = AssetLoaderHelper.LoadAssetBundle(assetBundleName, "Dinosaurs");
        if (loadedAssetBundle != null) {
            AssignTrackableDrawables(loadedAssetBundle);
        }
    }
    
    private void AssignTrackableDrawables(AssetBundle dinosaursBundle) {
        GameObject[] bundleContent = dinosaursBundle.LoadAllAssets<GameObject>();
        foreach (var dinoObject in bundleContent) {
            if (dinoObject.name == "Dino_Diplodocus") {
                DiplodocusTrackable.Drawable = dinoObject;
            } else if (dinoObject.name == "Dino_Spinosaurus") {
                SpinosaurusTrackable.Drawable = dinoObject;
            } else if (dinoObject.name == "Dino_Triceratops") {
                TriceratopsTrackable.Drawable = dinoObject;
            } else if (dinoObject.name == "Dino_Tyrannosaurus") {
                TyrannosaurusTrackable.Drawable = dinoObject;
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