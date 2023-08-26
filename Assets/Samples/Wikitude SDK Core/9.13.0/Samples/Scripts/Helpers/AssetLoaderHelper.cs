/******************************************************************************
 * File: DinosaurAssetLoader.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class AssetLoaderHelper {
    public static String GetAssetBundleNameAccordingToPlatform(String fileName) {
        String assetBundleName = null;
        var test = Application.platform;
        switch (Application.platform) {
            case RuntimePlatform.Android:
                assetBundleName = fileName + "-android"; break;
            case RuntimePlatform.IPhonePlayer:
                assetBundleName = fileName + "-ios"; break;
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                assetBundleName = fileName + "-osx"; break;
            case RuntimePlatform.WSAPlayerARM:
            case RuntimePlatform.WSAPlayerX64:
            case RuntimePlatform.WSAPlayerX86:
                assetBundleName = fileName + "-uwp"; break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                assetBundleName = fileName + "-win"; break;
        }
        
        if (assetBundleName == null) {
            Debug.LogError("Could not find asset bundle for currently selected build platform '" + Application.platform + "'. Please switch to a supported platform and try again.");
            return null;
        }
        return assetBundleName;
    }

    public static AssetBundle LoadAssetBundle(String assetBundleName, String streamingAssetSubFolder) {
        var assetBundleFilePath = Path.Combine(streamingAssetSubFolder, assetBundleName);
        return LoadAssetBundle(assetBundleFilePath);
    }

    public static AssetBundle LoadAssetBundle(String assetBundleName) {
        var assetBundleFilePath = Path.Combine(Application.streamingAssetsPath, assetBundleName);
        AssetBundle assetBundle = null;
        Debug.Log("Trying to load " + assetBundleFilePath);

#if UNITY_ANDROID
        var request = UnityWebRequest.Get(assetBundleFilePath);
        request.SendWebRequest();
        while (!request.isDone) {
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
                break;
        }

        if (request.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error loading `" + assetBundleName + "` from StreamingAssets.");
            return null;
        }
        assetBundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
#else
        if (!File.Exists(assetBundleFilePath)) {
            Debug.LogError("Could not find asset bundle `" + assetBundleName + "` in StreamingAssets folder.");
            return null;
        }
        assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);
#endif
        
        if (assetBundle == null) {
            Debug.LogError("Failed to load asset bundle.");
        }
        return assetBundle;
    }
}