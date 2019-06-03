using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ProceduralTexturedQuad))]
public class ProceduralTexturedQuadEditor : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		
		if(GUILayout.Button("Generate Mesh")) {
			ProceduralTexturedQuad quad = (ProceduralTexturedQuad) target;
			quad.GenerateMesh();
		}
	}
}
