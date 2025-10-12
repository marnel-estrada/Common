using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Common {
    public class DataPoolSidebarView<T> : IDataPoolSidebarView<T> where T : class, IDataPoolItem, IDuplicable<T>, new() {
        private readonly Dictionary<string, T> filteredMap = new();
        private readonly List<string> filteredIds = new(); // Used to render list of action buttons

        private Vector2 scrollPos;

        private int selection;
        
        private readonly SimpleList<DataPoolFilterStrategy<T>> filterStrategies = new();

        private bool shouldSortItems = true;

        /// <summary>
        /// Renders the UI for the specified pool
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="width"></param>
        public void Render(DataPool<T> pool, int width = 250) {
            GUILayout.BeginVertical(GUILayout.Width(width));

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
        private Option<string> itemToSelect;

        private void RenderNewItemUi(DataPool<T> pool) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("New: ", GUILayout.Width(40));
            this.newItemId = EditorGUILayout.TextField(this.newItemId).Trim();

            if(GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(15))) {
                if(!string.IsNullOrEmpty(this.newItemId)) {
                    // See if it already exists
                    if(pool.Contains(this.newItemId.Trim())) {
                        // An item with the same ID exists
                        EditorUtility.DisplayDialog("Add Data Class", "Can't add. An item with the same ID already exists.", "OK");
                        return;
                    }

                    // Add the new item
                    T newItem = pool.AddNew(this.newItemId);
                    OnItemAdded(newItem);
                    this.newItemId = "";

                    EditorUtility.SetDirty(pool);
                }
            }

            GUILayout.EndHorizontal();
        }

        public void OnItemAdded(T item) {
            // Add to filtered and select it
            AddToFiltered(item);
                    
            // This will trigger a custom selection when selection grid is rendered
            this.itemToSelect = Option<string>.Some(item.Id);
        }

        private void AddToFiltered(T item) {
            this.filteredMap[item.Id] = item;
            this.filteredIds.Add(item.Id);
        }

        private string filterText = "";

        private void RenderItems(DataPool<T> pool) {
            this.filteredIds.Clear();
            this.filteredMap.Clear();
            
            GUILayout.Label("Filters", GUILayout.Width(50));
            
            // The filters
            GUILayout.BeginHorizontal();
            GUILayout.Label("ID:", GUILayout.Width(40));
            
            this.filterText = EditorGUILayout.TextField(this.filterText);
            if (!string.IsNullOrEmpty(this.filterText)) {
                Filter(pool);
            }

            GUILayout.EndHorizontal();

            RenderFilterStrategies(pool);

            if (this.filteredIds.Count == 0) {
                // Add all
                for (int i = 0; i < pool.Count; ++i) {
                    T item = pool.GetAt(i);
                    AddToFiltered(item);
                }

                if (this.filteredIds.Count == 0) {
                    // It's still empty after trying to search with empty filter
                    // Display (empty)
                    GUILayout.Label("(empty)");
                }
            }
            
            if (this.ShouldSortItems) {
                // Sort by ID
                this.filteredIds.Sort();
            }

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            ResolveSelection();
            this.selection = GUILayout.SelectionGrid(this.selection, this.filteredIds.ToArray(), 1);
            GUILayout.EndScrollView();
        }

        private void ResolveSelection() {
            if (this.itemToSelect.IsNone) {
                return;
            }
            
            this.selection = this.filteredIds.IndexOf(this.itemToSelect.ValueOrError());
            this.selection = Mathf.Clamp(this.selection, 0, this.filteredIds.Count - 1);
            
            // We always set it to none to mark that the value has been "processed" already
            this.itemToSelect = Option<string>.NONE;
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
            
            strategy.FilterText = EditorGUILayout.TextField(strategy.FilterText);
            if (!string.IsNullOrEmpty(strategy.FilterText)) {
                Filter(pool, strategy);
            }

            GUILayout.EndHorizontal();
        }

        private void Filter(DataPool<T> pool, DataPoolFilterStrategy<T> strategy) {
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

                if (item.Id.ToLower().Contains(this.filterText.ToLower())) {
                    AddToFiltered(item);
                }
            }
        }

        public bool IsValidSelection(DataPool<T> pool) {
            return 0 <= this.selection && this.selection < this.filteredMap.Count;
        }

        public T GetSelectedItem(DataPool<T> pool) {
            if (IsValidSelection(pool)) {
                // Valid index
                string selectedId = this.filteredIds[this.selection];
                return this.filteredMap[selectedId];
            }

            return default;
        }

        public bool ShouldSortItems {
            get => this.shouldSortItems;
            set => this.shouldSortItems = value;
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

        public void SelectItem(string itemId) {
            this.itemToSelect = Option<string>.Some(itemId);
        }
    }
}