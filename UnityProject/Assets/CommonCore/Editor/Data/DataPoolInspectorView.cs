using UnityEditor;

using UnityEngine;

namespace Common {
    public class DataPoolInspectorView<T> where T : IDataPoolItem, new() {
        private readonly DataPoolItemRenderer<T> itemRenderer;
        
        /// <summary>
        ///     Constructor
        /// </summary>
        public DataPoolInspectorView(DataPoolItemRenderer<T> itemRenderer) {
            this.itemRenderer = itemRenderer;
        }

        /// <summary>
        ///     Renders the UI
        /// </summary>
        /// <param name="pool"></param>
        public void Render(DataPool<T> pool, T item) {
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

        private void Delete(DataPool<T> pool, T item) {
            if (EditorUtility.DisplayDialogComplex("Delete Item", $"Are you sure you want to delete {item.Id}?", "Yes", "No", "Cancel") != 0) {
                // Cancelled or No
                return;
            }

            pool.Remove(item.Id);

            EditorUtility.SetDirty(pool);
            DataPoolEditorWindow<T>.REPAINT.Dispatch();
        }
    }
}