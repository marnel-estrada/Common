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
        private OptionRequirementsRenderer requirementsRenderer;
        private OptionCostsRenderer costsRenderer;
        private OptionEffectsRenderer effectsRenderer;

        public static readonly Signal REPAINT = new Signal("Repaint");
        public static readonly Signal CLOSE = new Signal("Close");

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
            this.requirementsRenderer = new OptionRequirementsRenderer(this, pool, eventItem, option);
            this.costsRenderer = new OptionCostsRenderer(this, pool, eventItem, option);
            this.effectsRenderer = new OptionEffectsRenderer(this, pool, eventItem, option);
        }

        private void OnEnable() {
            REPAINT.AddListener(Repaint);
            CLOSE.AddListener(Close);
        }

        private void OnDisable() {
            REPAINT.RemoveListener(Repaint);
            CLOSE.RemoveListener(Close);
        }

        private void OnDestroy() {
            // Repaint the parent editor so that the changes will reflect
            EventsEditorWindow.REPAINT.Dispatch();
        }

        private void Repaint(ISignalParameters parameters) {
            Repaint();
        }

        private void Close(ISignalParameters parameters) {
            Close();
        }

        private Vector2 scrollPos;

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
            
            GUILayout.Space(10);

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            
            GUILayout.Label("Properties", EditorStyles.boldLabel);
            this.propertiesRenderer.Render();

            GUILayout.Space(20);
            
            GUILayout.Label("Requirements", EditorStyles.boldLabel);
            this.requirementsRenderer.Render();

            GUILayout.Space(20);
            
            GUILayout.Label("Costs", EditorStyles.boldLabel);
            this.costsRenderer.Render();

            GUILayout.Space(20);
            
            GUILayout.Label("Effects", EditorStyles.boldLabel);
            this.effectsRenderer.Render();
            
            GUILayout.EndScrollView();

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