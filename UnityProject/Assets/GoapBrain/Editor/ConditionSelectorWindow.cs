using System;

using UnityEngine;
using UnityEditor;

using Common;

namespace GoapBrain {
    class ConditionSelectorWindow : EditorWindow {
        private GoapDomainData domain;
        private Action<string> onSelect;

        private readonly SimpleList<ConditionName> filteredList = new SimpleList<ConditionName>();
        private string filterText = "";

        private Vector2 scrollPos;

        private bool includeExtensions;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="domain"></param>
        public void Init(GoapDomainData domain, Action<string> onSelect, bool includeExtensions) {
            this.domain = domain;
            this.onSelect = onSelect;
            this.includeExtensions = includeExtensions;

            Filter(null); // No filter
        }

        private void OnGUI() {
            if(this.domain == null) {
                Close();
                return;
            }

            GUILayout.BeginVertical();
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);

            GUILayout.Label($"Condition Selector ({this.domain.name})", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Filter
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter: ", GUILayout.Width(40));
            string newFilter = "";
            newFilter = GUILayout.TextField(this.filterText, GUILayout.Width(200));
            if(!newFilter.Equals(this.filterText)) {
                this.filterText = newFilter;
                Filter(this.filterText);
            }
            GUILayout.EndHorizontal();

            // Display name buttons
            if(this.filteredList.Count == 0) {
                GUILayout.Label("(nothing filtered)");
            } else {
                for(int i = 0; i < this.filteredList.Count; ++i) {
                    if(GUILayout.Button(this.filteredList[i].Name, GUILayout.Width(400))) {
                        this.onSelect(this.filteredList[i].Name);
                        this.Close();
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void Filter(string filter) {
            this.filteredList.Clear();

            // Filter from main domain
            Filter(this.domain, filter);

            // Filter from extensions
            if (this.includeExtensions) {
                for (int i = 0; i < this.domain.Extensions.Count; ++i) {
                    Filter(this.domain.Extensions[i].DomainData, filter);
                }
            }
        }

        private void Filter(GoapDomainData domainData, string filter) {
            for (int i = 0; i < domainData.ConditionNamesCount; ++i) {
                ConditionName name = domainData.GetConditionNameAt(i);

                if (string.IsNullOrEmpty(filter)) {
                    // No filter. Add all
                    this.filteredList.Add(name);
                } else if (name.Name.ToLower().Contains(filter.ToLower())) {
                    // Filter is not empty. Filter accordingly.
                    this.filteredList.Add(name);
                }
            }
        }

        private void OnLostFocus() {
            Close();
        }

        private void Update() {
            // close the window if editor is compiling
            if (EditorApplication.isCompiling) {
                Close();
            }
        }

    }
}
