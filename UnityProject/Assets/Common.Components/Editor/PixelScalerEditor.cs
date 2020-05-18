using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Common;

[CustomEditor(typeof(PixelScaler))]
public class PixelScalerEditor : Editor {

    private PixelScaler scaler;

    void OnEnable() {
        scaler = this.target as PixelScaler;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(this.scaler == null) {
            this.scaler = this.target as PixelScaler;
            Assertion.NotNull(this.scaler);
        }

        if(GUILayout.Button("Apply Scale")) {
            this.scaler.ApplyPixelScale();
        }
    }

}
