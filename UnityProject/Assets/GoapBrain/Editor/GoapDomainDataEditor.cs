using Common;

using UnityEngine;
using UnityEditor;

namespace GoapBrain {
    [CustomEditor(typeof(GoapDomainData))]
    class GoapDomainDataEditor : Editor {

        private GoapDomainData domainData;

        void OnEnable() {
            this.domainData = (GoapDomainData)this.target;
            Assertion.NotNull(this.domainData);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if(GUILayout.Button("Open Editor")) {
                GoapDomainEditorWindow window = EditorWindow.GetWindow<GoapDomainEditorWindow>("GOAP Domain Editor");
                window.Init(this.domainData);
            }
        }

    }
}
