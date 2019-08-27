using Common.Utils;

using UnityEditor;

using UnityEngine;

namespace Common {
    internal class DataClassInspectorView<T> where T : Identifiable, new() {
        private readonly DataClassItemRenderer<T> itemRenderer;

        /// <summary>
        ///     Constructor
        /// </summary>
        public DataClassInspectorView(DataClassItemRenderer<T> itemRenderer) {
            this.itemRenderer = itemRenderer;
        }

        /// <summary>
        ///     Renders the UI
        /// </summary>
        /// <param name="pool"></param>
        public void Render(DataClassPool<T> pool, T item) {
            GUILayout.BeginVertical();

            GUILayout.Label("Item Details:", EditorStyles.boldLabel);
            GUILayout.Space(5);

            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("Delete", GUILayout.Width(60))) {
                Delete(pool, item);
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.BeginHorizontal();
            GUILayout.Label("ID: " + item.Id);
            GUILayout.EndHorizontal();

            this.itemRenderer.Render(pool, item);

            GUILayout.EndVertical();
        }

        private void Delete(DataClassPool<T> pool, T item) {
            if (EditorUtility.DisplayDialogComplex("Delete Item",
                string.Format("Are you sure you want to delete {0}?", item.Id), "Yes", "No", "Cancel") != 0) {
                // Cancelled or No
                return;
            }

            pool.Remove(item.Id);

            EditorUtility.SetDirty(pool);
            DataClassPoolEditorWindow<T>.REPAINT.Dispatch();
        }
    }
}