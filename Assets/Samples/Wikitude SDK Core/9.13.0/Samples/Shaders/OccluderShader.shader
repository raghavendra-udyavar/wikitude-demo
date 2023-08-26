/******************************************************************************
 * File: OccluderShader.shader
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *  2021 Wikitude GmbH.
 * 
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

Shader "Wikitude/OccluderShader" {
    SubShader {
        Tags { "Queue" = "Geometry-1" }
        ColorMask 0 
        ZWrite On
        Pass { }
    }
}