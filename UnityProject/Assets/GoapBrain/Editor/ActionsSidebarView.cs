using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;
using Common;

namespace GoapBrain {
    class ActionsSidebarView {

        private ActionSelectionView selectionView = new ActionSelectionView();

        private Vector2 scrollPos = new Vector2();

        private string newActionName = "";

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionsSidebarView() {
        }

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
            EditorGUILayout.BeginVertical(GUILayout.Width(300));

            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);

            GUILayout.Label("Actions", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Add section
            GUILayout.Label("New action:");
            GUILayout.BeginHorizontal();
            this.newActionName = EditorGUILayout.TextField(this.newActionName);
            if(GUILayout.Button("Add", GUILayout.Width(50), GUILayout.Height(15))) {
                AddNewAction(domainData, this.newActionName);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Filter section
            GUILayout.Label("Filter By Name:");
            string newNameFilter = EditorGUILayout.TextField(this.selectionView.NameFilter);
            if(!this.selectionView.NameFilter.Equals(newNameFilter)) {
                // There's a new filter text
                // Apply the filter
                this.selectionView.FilterByName(domainData, newNameFilter);
            }

            GUILayout.Space(5);

            // Filter by atom action
            GUILayout.Label("Filter By Atom Action:");
            string newAtomActionFilter = EditorGUILayout.TextField(this.selectionView.AtomActionFilter);
            if(!this.selectionView.AtomActionFilter.EqualsFast(newAtomActionFilter)) {
                // New filter text by atom action name
                this.selectionView.FilterByAtomAction(domainData, newAtomActionFilter);
            }

            GUILayout.Space(5);

            // Filter by precondition
            GUILayout.Label("Filter By Precondition:");
            string newPreconditionFilter = EditorGUILayout.TextField(this.selectionView.PreconditionFilter);
            if(!this.selectionView.PreconditionFilter.EqualsFast(newPreconditionFilter)) {
                this.selectionView.FilterByPrecondition(domainData, newPreconditionFilter);
            }

            GUILayout.Space(5);

            // Filter by effect
            GUILayout.Label("Filter By Effect:");
            string newEffectFilter = EditorGUILayout.TextField(this.selectionView.EffectFilter);
            if(!this.selectionView.EffectFilter.EqualsFast(newEffectFilter)) {
                this.selectionView.FilterByEffect(domainData, newEffectFilter);
            }

            GUILayout.Space(10);

            this.selectionView.Render(domainData);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void AddNewAction(GoapDomainData data, string actionName) {
            if(string.IsNullOrEmpty(actionName)) {
                // Can't add. Empty name
                return;
            }

            // Check if it already exists
            GoapActionData action = data.GetAction(actionName);
            if(action != null) {
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
        public GoapActionData GetSelectedAction() {
            return this.selectionView.GetSelectedAction();
        }

        /// <summary>
        /// Routines when repainting
        /// </summary>
        public void OnRepaint(GoapDomainData domain) {
            if(domain == null) {
                // There's no domain
                return;
            }

            this.selectionView.ReapplyFilter(domain);
        }

    }
}
