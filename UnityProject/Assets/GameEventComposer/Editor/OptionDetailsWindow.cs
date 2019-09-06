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

        void OnGUI() {
            GUILayout.BeginVertical();

            string optionLabel = $"{this.eventItem.NameId}.{this.option.NameId}";
            GUILayout.Label($"Option Details: {optionLabel}", EditorStyles.largeLabel);
            GUILayout.Space(10);
            
            GUILayout.EndVertical();
        }
    }
}