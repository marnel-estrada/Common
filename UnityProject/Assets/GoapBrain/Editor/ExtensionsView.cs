using System.Collections.Generic;

using Common;

using UnityEditor;

using UnityEngine;

namespace GoapBrain {
    internal class ExtensionsView {
        private GoapDomainData? newExtensionDomainData;

        private readonly EditorWindow parent;

        private readonly Dictionary<GoapExtensionData, ActionConditionsView> preconditionsViewMap =
            new Dictionary<GoapExtensionData, ActionConditionsView>();

        private Vector2 scrollPos;

        /// <summary>
        ///     Constructor
        /// </summary>
        public ExtensionsView(EditorWindow parent) {
            this.parent = parent;
        }

        /// <summary>
        ///     Renders the editor view
        /// </summary>
        /// <param name="domainData"></param>
        public void Render(GoapDomainData domainData) {
            GUILayout.BeginVertical();

            GUILayout.Label("Extensions", EditorStyles.boldLabel);

            GUILayout.Space(10);

            RenderAddNewExtension(domainData);

            GUILayout.Space(10);

            RenderExistingExtensions(domainData);

            GUILayout.EndVertical();
        }

        private void RenderAddNewExtension(GoapDomainData domainData) {
            GUILayout.Label("New Extension", EditorStyles.miniBoldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.Label("GoapDomainData:", GUILayout.Width(120));
            this.newExtensionDomainData =
                EditorGUILayout.ObjectField(this.newExtensionDomainData, typeof(GoapDomainData), false,
                    GUILayout.Width(200)) as GoapDomainData;

            if (GUILayout.Button("Add", GUILayout.Width(40))) {
                if (this.newExtensionDomainData != null) {
                    GoapExtensionData newExtension = new GoapExtensionData();
                    newExtension.DomainData = this.newExtensionDomainData;
                    domainData.Extensions.Add(newExtension);

                    this.newExtensionDomainData = null;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void RenderExistingExtensions(GoapDomainData domainData) {
            GUILayout.Label("Current Extensions", EditorStyles.miniBoldLabel);

            GUILayout.Space(10);

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);

            for (int i = 0; i < domainData.Extensions.Count; ++i) {
                RenderExtension(domainData, domainData.Extensions[i]);
                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();
        }

        private ActionConditionsView ResolvePreconditionsView(GoapExtensionData extension) {
            if (this.preconditionsViewMap.TryGetValue(extension, out ActionConditionsView view)) {
                // Already exists
                return view;
            }

            // Create a new one
            view = new ActionConditionsView(this.parent, "Preconditions", ColorUtils.RED, false);
            this.preconditionsViewMap[extension] = view;

            return view;
        }

        private void RenderExtension(GoapDomainData domainData, GoapExtensionData extension) {
            if (extension.DomainData == null) {
                return;
            }

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20))) {
                if (EditorUtility.DisplayDialogComplex("Delete Extension",
                    "Are you sure you want to delete \"{0}\"?".FormatWith(extension.DomainData.name), "Yes", "No",
                    "Cancel") == 0) {
                    // Chosen Yes
                    domainData.Extensions.Remove(extension);
                    EditorUtility.SetDirty(domainData);
                    this.parent.Repaint();
                }
            }

            GUI.backgroundColor = ColorUtils.WHITE;
            GUILayout.Label(extension.DomainData.name, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("GoapDomainData:", GUILayout.Width(120));
            extension.DomainData =
                EditorGUILayout.ObjectField(extension.DomainData, typeof(GoapDomainData), false, GUILayout.Width(200))
                    as GoapDomainData;
            GUILayout.EndHorizontal();
        }
    }
}