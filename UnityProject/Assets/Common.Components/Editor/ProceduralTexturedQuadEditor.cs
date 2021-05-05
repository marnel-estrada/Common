using UnityEngine;
using UnityEditor;

namespace Common {
	[CustomEditor(typeof(ProceduralTexturedQuad))]
	public class ProceduralTexturedQuadEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			
			if(GUILayout.Button("Generate Mesh")) {
				ProceduralTexturedQuad quad = (ProceduralTexturedQuad) this.target;
				quad.GenerateMesh();
			}
		}
	}
}

