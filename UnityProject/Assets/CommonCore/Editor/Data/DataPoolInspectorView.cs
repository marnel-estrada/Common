using UnityEditor;

using UnityEngine;

namespace Common {
    public class DataPoolInspectorView<T> where T : class, IDataPoolItem, IDuplicable<T>, new() {
        private readonly EditorWindow parentWindow;
        private readonly DataPoolSidebarView<T> sidebarView;
        private readonly DataPoolItemRenderer<T> itemRenderer;
        
        /// <summary>
        ///     Constructor
        /// </summary>
        public DataPoolInspectorView(EditorWindow parentWindow, DataPoolSidebarView<T> sidebarView, DataPoolItemRenderer<T> itemRenderer) {
            this.parentWindow = parentWindow;
            this.sidebarView = sidebarView;
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
            
            RenderDuplicateSection(pool, item);
            
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("ID: " + item.Id);
            GUILayout.EndHorizontal();

            this.itemRenderer.Render(pool, item);

            GUILayout.EndVertical();
        }

        private string duplicateId = "";

        private void RenderDuplicateSection(DataPool<T> pool, T item) {
            GUILayout.Label("Duplicate", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Duplicate ID:", GUILayout.Width(150));
            this.duplicateId = EditorGUILayout.TextField(this.duplicateId, GUILayout.Width(300));
            
            GUI.backgroundColor = ColorUtils.YELLOW;
            if (GUILayout.Button("Duplicate", GUILayout.Width(100))) {
                Duplicate(pool, item);
            }
            GUI.backgroundColor = ColorUtils.WHITE;
            GUILayout.EndHorizontal();
        }

        private void Duplicate(DataPool<T> pool, T item) {
            // Check that duplicate ID is specified
            if (string.IsNullOrEmpty(this.duplicateId)) {
                EditorUtility.DisplayDialog("Duplicate", "Duplicate ID must not be empty", "OK");
                return;
            }
            
            // Check that duplicate ID should not exist yet
            Maybe<T> existingItem = pool.Find(this.duplicateId);
            if (existingItem.HasValue) {
                // This means that there's an existing item with the same id
                EditorUtility.DisplayDialog("Duplicate", $"An item with ID \"{this.duplicateId}\" already exists. Choose a new ID.", "OK");
                return;
            }
            
            // Add the duplicate item
            T duplicateItem = item.Duplicate();
            duplicateItem.Id = this.duplicateId; // Don't forget to override. Can't add if it's the same ID.
            pool.Add(duplicateItem); // New integer ID is assigned here
            
            EditorUtility.SetDirty(pool);
            
            // Select the duplicate (show the duplicate in inspector view)
            this.sidebarView.SelectItem(this.duplicateId);
            this.parentWindow.Repaint();
            
            this.duplicateId = string.Empty;
        }

        private static void Delete(DataPool<T> pool, T item) {
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