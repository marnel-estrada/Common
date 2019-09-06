using Common;
using Common.Signal;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class OptionDetailsWindow : EditorWindow {
        private DataPool<EventData> pool;
        private EventData eventItem;
        private OptionData option;
        
        public static readonly Signal REPAINT = new Signal("Repaint");

        private static readonly string[] TABS = {
            "Properties", "Requirements", "Costs", "Effects"
        };

        private const int PROPERTIES_TAB = 0;
        private const int REQUIREMENTS_TAB = 1;
        private const int COSTS_TAB = 2;
        private const int EFFECTS_TAB = 3;

        public void Init(DataPool<EventData> pool, EventData eventItem, OptionData option) {
            this.pool = pool;
            this.eventItem = eventItem;
            this.option = option;
        }
        
        void OnEnable() {
            REPAINT.AddListener(Repaint);
        }

        void OnDisable() {
            REPAINT.RemoveListener(Repaint);
        }

        private void Repaint(ISignalParameters parameters) {
            Repaint();
        }

        private int tabSelected;

        private void OnGUI() {
            GUILayout.BeginVertical();

            // Title
            string optionLabel = $"{this.eventItem.NameId}.{this.option.NameId}";
            GUILayout.Label($"Option Details: {optionLabel}", EditorStyles.largeLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Save", GUILayout.Width(40))) {
                EditorUtility.SetDirty(this.pool);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog("Save", "Save Successful", "OK");
            }
            
            // Render tabs
            this.tabSelected = GUILayout.Toolbar(this.tabSelected, TABS, GUILayout.Width(400));
            
            GUILayout.Space(10);

            switch (this.tabSelected) {
                case PROPERTIES_TAB:
                    GUILayout.Label("Properties");
                    break;
                
                case REQUIREMENTS_TAB:
                    GUILayout.Label("Requirements");
                    break;
                
                case COSTS_TAB:
                    GUILayout.Label("Costs");
                    break;
                
                case EFFECTS_TAB:
                    GUILayout.Label("Effects");
                    break;
            }
            
            GUILayout.EndVertical();
        }
        
        private void Update() {
            // close the window if editor is compiling
            if (EditorApplication.isCompiling) {
                Close();
            }
        }
    }
}