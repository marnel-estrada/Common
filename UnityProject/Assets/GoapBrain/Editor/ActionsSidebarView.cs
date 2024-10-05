using UnityEngine;
using UnityEditor;
using Common;

namespace GoapBrain {
    internal class ActionsSidebarView {
        private readonly ActionSelectionView selectionView = new ActionSelectionView();

        private Vector2 scrollPos;

        private bool filtersFoldOutOpened;

        private string newActionName = string.Empty;
        private string newNameFilter = string.Empty;
        private string newAtomActionFilter = string.Empty;
        private string newPreconditionFilter = string.Empty;
        private string newEffectFilter = string.Empty;
        private string hashCodeFilter = string.Empty;

        /// <summary>
        /// Start routines
        /// </summary>
        /// <param name="domainData"></param>
        public void Start(GoapDomainData domainData) {
            // Display all actions
            this.selectionView.NameFilter = "";
            this.selectionView.FilterByName(domainData, this.selectionView.NameFilter);
        }

        /// <summary>
        /// Renders the specified domainData for the sidebar
        /// </summary>
        /// <param name="domainData"></param>
        public void Render(GoapDomainData domainData) {
            EditorGUILayout.BeginVertical(GUILayout.Width(350));

            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);

            GUILayout.Label("Actions", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Add section
            GUILayout.Label("New action:");
            GUILayout.BeginHorizontal();
            this.newActionName = EditorGUILayout.TextField(this.newActionName);
            if (GUILayout.Button("Add", GUILayout.Width(50), GUILayout.Height(15))) {
                AddNewAction(domainData, this.newActionName);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            this.filtersFoldOutOpened = EditorGUILayout.Foldout(this.filtersFoldOutOpened, "Filters");

            if (this.filtersFoldOutOpened) {
                GUILayout.Space(5);

                // Filter section
                GUILayout.Label("Filter By Name:");
                this.newNameFilter = EditorGUILayout.TextField(this.selectionView.NameFilter);

                GUILayout.Space(5);

                // Filter by atom action
                GUILayout.Label("Filter By Atom Action:");
                this.newAtomActionFilter = EditorGUILayout.TextField(this.selectionView.AtomActionFilter);

                GUILayout.Space(5);

                // Filter by precondition
                GUILayout.Label("Filter By Precondition:");
                this.newPreconditionFilter = EditorGUILayout.TextField(this.selectionView.PreconditionFilter);

                GUILayout.Space(5);

                // Filter by effect
                GUILayout.Label("Filter By Effect:");
                this.newEffectFilter = EditorGUILayout.TextField(this.selectionView.EffectFilter);

                GUILayout.Space(5);

                // Filter by effect
                GUILayout.Label("Filter By Action Id (Hashcode):");
                this.hashCodeFilter = EditorGUILayout.TextField(this.selectionView.HashCodeFilter);

                GUILayout.Space(5);
            }

            ApplyFilters(domainData);

            GUILayout.Space(5);

            this.selectionView.Render(domainData);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void ApplyFilters(GoapDomainData domainData) {
            if (!this.selectionView.NameFilter.Equals(this.newNameFilter)) {
                // There's a new filter text
                // Apply the filter
                this.selectionView.FilterByName(domainData, this.newNameFilter);
            }

            if (!this.selectionView.AtomActionFilter.EqualsFast(this.newAtomActionFilter)) {
                // New filter text by atom action name
                this.selectionView.FilterByAtomAction(domainData, this.newAtomActionFilter);
            }

            if (!this.selectionView.PreconditionFilter.EqualsFast(this.newPreconditionFilter)) {
                this.selectionView.FilterByPrecondition(domainData, this.newPreconditionFilter);
            }

            if (!this.selectionView.EffectFilter.EqualsFast(this.newEffectFilter)) {
                this.selectionView.FilterByEffect(domainData, this.newEffectFilter);
            }

            if (!this.selectionView.HashCodeFilter.EqualsFast(this.hashCodeFilter)) {
                this.selectionView.FilterByHashCode(domainData, this.hashCodeFilter);
            }
        }

        private void AddNewAction(GoapDomainData data, string actionName) {
            if (string.IsNullOrEmpty(actionName)) {
                // Can't add. Empty name
                return;
            }

            // Check if it already exists
            GoapActionData? action = data.GetAction(actionName);
            if (action != null) {
                // An action with the same name already exists
                EditorUtility.DisplayDialog("Can't add", "An action with the same name already exists.", "OK");
                return;
            }

            GoapActionData newAction = data.AddAction(actionName);

            // Add the selection so that it will be displayed
            this.selectionView.AddFiltered(newAction);

            EditorUtility.SetDirty(data);
            GoapEditorSignals.REPAINT.Dispatch();

            // Return to blank so user could type a new action name again
            this.newActionName = "";
        }

        /// <summary>
        /// Returns the selected action
        /// </summary>
        /// <returns></returns>
        public GoapActionData? GetSelectedAction() {
            return this.selectionView.GetSelectedAction();
        }

        /// <summary>
        /// Routines when repainting
        /// </summary>
        public void OnRepaint(GoapDomainData domain) {
            if (domain == null) {
                // There's no domain
                return;
            }

            this.selectionView.ReapplyFilter(domain);
        }
    }
}