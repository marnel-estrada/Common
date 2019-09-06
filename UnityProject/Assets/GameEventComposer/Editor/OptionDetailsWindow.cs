using Common;
using Common.Signal;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class OptionDetailsWindow : EditorWindow {
        private EditorWindow parent;
        
        private DataPool<EventData> pool;
        private EventData eventItem;
        private OptionData option;

        private OptionPropertiesRenderer propertiesRenderer;
        
        public static readonly Signal REPAINT = new Signal("Repaint");

        private static readonly string[] TABS = {
            "Properties", "Requirements", "Costs", "Effects"
        };

        private const int PROPERTIES_TAB = 0;
        private const int REQUIREMENTS_TAB = 1;
        private const int COSTS_TAB = 2;
        private const int EFFECTS_TAB = 3;

        public void Init(EditorWindow parent, DataPool<EventData> pool, EventData eventItem, OptionData option) {
            this.parent = parent;
            
            this.pool = pool;
            this.eventItem = eventItem;
            this.option = option;
            
            this.propertiesRenderer = new OptionPropertiesRenderer(this, pool, eventItem, option);
        }

        private void OnEnable() {
            REPAINT.AddListener(Repaint);
        }

        private void OnDisable() {
            REPAINT.RemoveListener(Repaint);
        }

        private void OnDestroy() {
            // Repaint the parent editor so that the changes will reflect
            EventsEditorWindow.REPAINT.Dispatch();
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
                    GUILayout.Label("Properties", EditorStyles.boldLabel);
                    this.propertiesRenderer.Render();
                    break;
                
                case REQUIREMENTS_TAB:
                    GUILayout.Label("Requirements", EditorStyles.boldLabel);
                    break;
                
                case COSTS_TAB:
                    GUILayout.Label("Costs", EditorStyles.boldLabel);
                    break;
                
                case EFFECTS_TAB:
                    GUILayout.Label("Effects", EditorStyles.boldLabel);
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