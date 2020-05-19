using Common;

using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(MeshShaderSetter))]
public class MeshShaderSetterEditor : Editor {
    private MeshShaderSetter targetComponent;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        this.targetComponent = (MeshShaderSetter) this.target;

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Change Shaders")) {
            Assertion.IsTrue(
                this.targetComponent.GetShaderToSet() != null && this.targetComponent.GetShaderToSet().Length > 0,
                "A shader must be specified.");
            ChangeShaders();
        }

        EditorGUILayout.EndVertical();
    }

    private void ChangeShaders() {
        string shaderToSet = this.targetComponent.GetShaderToSet();

        Renderer[] renderers = this.targetComponent.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.sharedMaterial.shader = Shader.Find(shaderToSet);
            Material[] materials = renderer.sharedMaterials;
            foreach (Material material in materials) {
                material.shader = Shader.Find(shaderToSet);
            }
        }
    }
}