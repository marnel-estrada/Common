using System;

using UnityEngine;
using UnityEditor;
using Common;

namespace GoapBrain {
    /// <summary>
    /// Handles rendering of atom actions UI
    /// </summary>
    class AtomActionsView {
        private readonly EditorWindow parent;

        private GoapDomainData domain;
        private GoapActionData action;

        private readonly ClassPropertiesRenderer propertiesRenderer;

        /// <summary>
        /// Constructor
        /// </summary>
        public AtomActionsView(EditorWindow parent) {
            this.parent = parent;
            this.propertiesRenderer = new ClassPropertiesRenderer(300);
        }

        /// <summary>
        /// Renders the atom actions of the specified action
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="action"></param>
        public void Render(GoapDomainData domain, GoapActionData action) {
            this.domain = domain;
            this.action = action;

            // Add new atomic action
            GUI.backgroundColor = ColorUtils.GREEN;
            if(GUILayout.Button("Add Atomic Action...", GUILayout.Width(140))) {
                OpenAtomActionsBrowser();
            }
            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Space(5);

            // Render atomic actions
            if (action.AtomActions.Count <= 0) {
                GUILayout.Label("(no atomic actions yet)");
            } else {
                RenderAtomActions(domain, action);
            }
        }

        private void RenderAtomActions(GoapDomainData domain, GoapActionData action) {
            for(int i = 0; i < action.AtomActions.Count; ++i) {
                RenderAtomAction(domain, action, action.AtomActions[i], i);
                GUILayout.Space(5);
            }
        }

        private void RenderAtomAction(GoapDomainData domain, GoapActionData action, ClassData data, int index) {
            if(data.ClassType == null) {
                // Cache
                data.ClassType = TypeUtils.GetType(data.ClassName);
                Assertion.NotNull(data.ClassType);
            }

            GUILayout.BeginHorizontal();

            // delete button
            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20))) {
                Remove(domain, action, data);
            }
            GUI.backgroundColor = ColorUtils.WHITE;

            // up button 
            if (GUILayout.Button("Up", GUILayout.Width(35), GUILayout.Height(20))) {
                MoveUp(domain, action, index);
            }

            // down button
            if (GUILayout.Button("Down", GUILayout.Width(45), GUILayout.Height(20))) {
                MoveDown(domain, action, index);
            }
            
            GUILayout.Box(data.ClassType.Name, GUILayout.Width(300));

            GUILayout.EndHorizontal();

            // Variables
            Assertion.NotNull(data.ClassType);
            this.propertiesRenderer.RenderVariables(domain.Variables, data.Variables, data.ClassType, data.ShowHints);
        }

        private void OpenAtomActionsBrowser() {
            Rect position = this.parent.position;
            position.x += (this.parent.position.width * 0.5f) - 200;
            position.y += (this.parent.position.height * 0.5f) - 300;
            position.width = 400;
            position.height = 600;

            AtomActionBrowserWindow actionsBrowser = EditorWindow.CreateInstance<AtomActionBrowserWindow>();
            actionsBrowser.titleContent = new GUIContent("Atom Actions Browser");
            actionsBrowser.Init(OnAdd);
            actionsBrowser.position = position;
            actionsBrowser.ShowModalUtility();
            actionsBrowser.Focus();
        }

        private void OnAdd(Type type) {
            ClassData classData = new ClassData();
            classData.ClassName = type.FullName;

            this.action.AtomActions.Add(classData);

            EditorUtility.SetDirty(this.domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private void Remove(GoapDomainData domain, GoapActionData action, ClassData data) {
            if(EditorUtility.DisplayDialogComplex("Remove Atomic Action", "Are you sure you want to remove this atomic action?", "Yes", "No", "Cancel") != 0) {
                // Cancelled or No
                return;
            }

            action.AtomActions.Remove(data);

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private void MoveUp(GoapDomainData domain, GoapActionData action, int index) {
            if(index <= 0) {
                // Can no longer move up
                return;
            }

            Swap(action, index, index - 1);

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private void MoveDown(GoapDomainData domain, GoapActionData action, int index) {
            if (index + 1 >= action.AtomActions.Count) {
                // Can no longer move down
                return;
            }

            Swap(action, index, index + 1);

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private static void Swap(GoapActionData action, int a, int b) {
            ClassData temp = action.AtomActions[a];
            action.AtomActions[a] = action.AtomActions[b];
            action.AtomActions[b] = temp;
        }
    }
}
