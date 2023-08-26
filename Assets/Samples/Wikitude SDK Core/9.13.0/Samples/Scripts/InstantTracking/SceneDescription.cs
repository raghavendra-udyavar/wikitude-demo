/******************************************************************************
 * File: SceneDescription.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

/* Simple struct used to serialize augmentations to disk. */
[System.Serializable]
public struct AugmentationDescription {
    /* Corresponds to the index of this augmentation in InstantTrackingController.Models */
    public int ID;
    public Vector3 LocalPosition;
    public Quaternion LocalRotation;
    public Vector3 LocalScale;

    public AugmentationDescription(int id, Transform transform) {
        ID = id;
        LocalPosition = transform.localPosition;
        LocalRotation = transform.localRotation;
        LocalScale = transform.localScale;
    }
}

/* Contains all the augmentations that should be saved along with the Instant Target. */
[System.Serializable]
public class SceneDescription {
    public List<AugmentationDescription> Augmentations = new List<AugmentationDescription>();
}
