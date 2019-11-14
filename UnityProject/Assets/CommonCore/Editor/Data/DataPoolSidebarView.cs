using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Common {
    public class DataPoolSidebarView<T> where T : Identifiable, IntIdentifiable, new() {
        private readonly Dictionary<string, T> filteredMap = new Dictionary<string, T>();
        private readonly List<string> filteredIds = new List<string>(); // Used to render list of action buttons

        private Vector2 scrollPos;

        private int selection;
        
        private readonly SimpleList<DataPoolFilterStrategy<T>> filterStrategies = new SimpleList<DataPoolFilterStrategy<T>>();
        
        /// <summary>
        /// Renders the UI for the specified pool
        /// </summary>
        /// <param name="pool"></param>
        public void Render(DataPool<T> pool) {
            GUILayout.BeginVertical(GUILayout.Width(300));

            // Render existing
            GUILayout.Label("Items: ", EditorStyles.boldLabel);

            GUILayout.Space(5);

            // New item
            RenderNewItemUi(pool);

            GUILayout.Space(5);

            RenderItems(pool);

            GUILayout.EndVertical();
        }

        private string newItemId = "";

        private void RenderNewItemUi(DataPool<T> pool) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("New: ", GUILayout.Width(40));
            this.newItemId = EditorGUILayout.TextField(this.newItemId);

            if(GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(15))) {
                if(!string.IsNullOrEmpty(this.newItemId)) {
                    // See if it already exists
                    if(pool.Contains(newItemId.Trim())) {
                        // An item with the same ID exists
                        EditorUtility.DisplayDialog("Add Data Class", "Can't add. An item with the same ID already exists.", "OK");
                        return;
                    }

                    // Add the new item
                    T newItem = pool.AddNew(this.newItemId);

                    // Add to filtered and select it
                    AddToFiltered(newItem);
                    this.selection = this.filteredIds.Count - 1;
                    this.newItemId = "";

                    EditorUtility.SetDirty(pool);
                    //DataPoolEditorWindow<T>.REPAINT.Dispatch();
                }
            }

            GUILayout.EndHorizontal();
        }

        private void AddToFiltered(T item) {
            this.filteredMap[item.Id] = item;
            this.filteredIds.Add(item.Id);
        }

        private string filterText = "";

        private void RenderItems(DataPool<T> pool) {
            GUILayout.Label("Filters", GUILayout.Width(50));
            
            // The filters
            GUILayout.BeginHorizontal();
            GUILayout.Label("ID:", GUILayout.Width(40));
            string newFilter = EditorGUILayout.TextField(this.filterText);
            if(newFilter != this.filterText) {
                // There's a new filter. Apply the filter
                this.filterText = newFilter;
                Filter(pool);
            }
            GUILayout.EndHorizontal();

            RenderFilterStrategies(pool);
            
            // Sort by ID
            this.filteredIds.Sort();

            if (this.filteredIds.Count == 0) {
                // Try empty filter in the hopes of finding entries
                this.filterText = "";
                Filter(pool);

                if (this.filteredIds.Count == 0) {
                    // It's still empty after trying to search with empty filter
                    // Display (empty)
                    GUILayout.Label("(empty)");
                }
            }

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            this.selection = GUILayout.SelectionGrid(this.selection, this.filteredIds.ToArray(), 1);
            GUILayout.EndScrollView();
        }

        // Render the other filtering strategies
        private void RenderFilterStrategies(DataPool<T> pool) {
            for (int i = 0; i < this.filterStrategies.Count; ++i) {
                RenderFilterStrategy(pool, this.filterStrategies[i]);
            }
        }

        private void RenderFilterStrategy(DataPool<T> pool, DataPoolFilterStrategy<T> strategy) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(strategy.Label + ":", GUILayout.Width(strategy.LabelWidth));
            string newFilter = EditorGUILayout.TextField(strategy.FilterText);
            if (newFilter != strategy.FilterText) {
                strategy.FilterText = newFilter;
                Filter(pool, strategy);
            }
            GUILayout.EndHorizontal();
        }

        private void Filter(DataPool<T> pool, DataPoolFilterStrategy<T> strategy) {
            this.filteredMap.Clear();
            this.filteredIds.Clear();
            
            for (int i = 0; i < pool.Count; ++i) {
                T item = pool.GetAt(i);
                if (strategy.IsFilterMet(item)) {
                    AddToFiltered(item);
                }
            }
        }

        private void Filter(DataPool<T> pool) {
            this.filteredMap.Clear();
            this.filteredIds.Clear();

            if (pool == null) {
                return;
            }
            
            for (int i = 0; i < pool.Count; ++i) {
                T item = pool.GetAt(i);

                if (string.IsNullOrEmpty(this.filterText)) {
                    // Filter text is empty
                    // Add every action data
                    AddToFiltered(item);
                } else if (item.Id.ToLower().Contains(this.filterText.ToLower())) {
                    AddToFiltered(item);
                }
            }
        }

        public bool IsValidSelection {
            get {
                return 0 <= this.selection && this.selection < this.filteredMap.Count;
            }
        }

        public T SelectedItem {
            get {
                if (this.IsValidSelection) {
                    // Valid index
                    string selectedId = this.filteredIds[this.selection];
                    return this.filteredMap[selectedId];
                }

                return default(T);
            }
        }

        /// <summary>
        /// Routines on Repaint()
        /// </summary>
        /// <param name="pool"></param>
        public void OnRepaint(DataPool<T> pool) {
            // Apply filter again
            // Items might have been removed
            Filter(pool);
        }

        /// <summary>
        /// Adds a filtering strategy
        /// </summary>
        /// <param name="strategy"></param>
        public void AddFilterStrategy(DataPoolFilterStrategy<T> strategy) {
            this.filterStrategies.Add(strategy);
        }
    }
}