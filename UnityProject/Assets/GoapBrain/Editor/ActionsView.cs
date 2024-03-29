﻿using UnityEngine;
using UnityEditor;

namespace GoapBrain {
    public class ActionsView {
        private readonly ActionsSidebarView sidebar = new();
        private readonly ActionInspectorView inspector;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionsView(EditorWindow parent) {
            this.inspector = new ActionInspectorView(parent);
        }

        public void OnEnable() {
            this.inspector.OnEnable();
        }

        /// <summary>
        /// Start routine
        /// </summary>
        /// <param name="domainData"></param>
        public void Start(GoapDomainData domainData) {
            this.sidebar.Start(domainData);
        }

        /// <summary>
        /// Renders the UI
        /// </summary>
        /// <param name="domainData"></param>
        public void Render(GoapDomainData domainData) {
            // Separator for sidebar and inspector
            EditorGUILayout.BeginHorizontal();

            // Sidebar
            this.sidebar.Render(domainData);

            GUILayout.Space(10);

            // Inspector
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Inspector", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GoapActionData? selectedAction = this.sidebar.GetSelectedAction();

            if (selectedAction != null) {
                this.inspector.Render(domainData, selectedAction);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Routines on repaint
        /// </summary>
        /// <param name="domain"></param>
        public void OnRepaint(GoapDomainData domain) {
            this.sidebar.OnRepaint(domain);
        }
    }
}