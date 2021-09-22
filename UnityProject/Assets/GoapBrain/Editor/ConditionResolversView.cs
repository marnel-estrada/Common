using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using Common;

namespace GoapBrain {
    public class ConditionResolversView {
        private readonly EditorWindow parent;

        private Vector2 scrollPos = new Vector2();

        private string newConditionName = "";

        private string newResolverName = "";
        private string newResolverClassName = ""; // This is the full class name

        private readonly ClassPropertiesRenderer propertiesRenderer = new ClassPropertiesRenderer(250);

        private readonly ConditionResolverFiltering filterHandler = new ConditionResolverFiltering();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public ConditionResolversView(EditorWindow parent) {
            this.parent = parent;
        }

        /// <summary>
        /// Renders the UI
        /// </summary>
        /// <param name="domain"></param>
        public void Render(GoapDomainData domain) {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Condition Resolvers", EditorStyles.boldLabel);
            GUILayout.Space(10);

            RenderAddNewResolver(domain);
            GUILayout.Space(5);
            
            this.filterHandler.Prepare(domain.ConditionResolvers);
            RenderFiltering();

            GUILayout.Space(5);

            // Display existing resolvers
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
            IReadOnlyList<ConditionResolverData> resolvers = this.filterHandler.FilteredList;
            for(int i = 0; i < resolvers.Count; ++i) {
                RenderConditionResolver(domain, resolvers[i]);
            }
            
            GUILayout.Space(5);
            
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void RenderAddNewResolver(GoapDomainData domain) {
            GUILayout.Label("New:", GUILayout.Width(40), GUILayout.Height(20));

            // Condition
            GUILayout.BeginHorizontal();
            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent("Choose Condition..."), GUI.skin.button, GUILayout.Width(140), GUILayout.Height(20));
            if (GUI.Button(buttonRect, "Choose Condition...")) {
                GoapEditorUtils.OpenConditionSelector(domain, this.parent, OnConditionSelected, false);
            }

            if (string.IsNullOrEmpty(this.newConditionName)) {
                GUILayout.Box("(no condition selected)");
            } else {
                GUILayout.Box(this.newConditionName);
            }
            GUILayout.EndHorizontal();

            // Resolver action
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Choose Resolver...", GUILayout.Width(140))) {
                OpenResolverBrowser();
            }

            if (string.IsNullOrEmpty(this.newResolverName)) {
                GUILayout.Box("(no resolver selected)");
            } else {
                GUILayout.Box(this.newResolverName);
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = ColorUtils.GREEN;
            if (GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(20))) {
                AddNewConditionResolver(domain);
            }
            GUI.backgroundColor = ColorUtils.WHITE;
        }

        private void RenderFiltering() {
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name Filter:", GUILayout.Width(100));

                string filter = this.filterHandler.NameFilter ?? "";
                filter = EditorGUILayout.TextField(filter, GUILayout.Width(300));

                if (!filter.EqualsFast(this.filterHandler.NameFilter)) {
                    this.filterHandler.NameFilter = filter;
                }
                
                GUILayout.EndHorizontal();
            }

            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Class Filter:", GUILayout.Width(100));
                
                string filter = this.filterHandler.ClassFilter ?? "";
                filter = EditorGUILayout.TextField(filter, GUILayout.Width(300));

                if (!filter.EqualsFast(this.filterHandler.ClassFilter)) {
                    this.filterHandler.ClassFilter = filter;
                }
                
                GUILayout.EndHorizontal();
            }
            
            this.filterHandler.ApplyFilter();
        }

        private void OnConditionSelected(string conditionName) {
            this.newConditionName = conditionName;
        }

        private void OpenResolverBrowser() {
            Rect position = this.parent.position;
            position.x += (this.parent.position.width * 0.5f) - 200;
            position.y += (this.parent.position.height * 0.5f) - 300;
            position.width = 400;
            position.height = 600;

            ConditionResolverBrowserWindow browser = ScriptableObject.CreateInstance<ConditionResolverBrowserWindow>();
            browser.titleContent = new GUIContent("Condition Resolver Browser");
            browser.Init(OnResolverSelected);
            browser.position = position;
            browser.ShowUtility();
            browser.Focus();
        }

        private void OnResolverSelected(Type type) {
            this.newResolverName = type.Name;
            this.newResolverClassName = type.FullName;
        }

        private void AddNewConditionResolver(GoapDomainData domain) {
            if(string.IsNullOrEmpty(this.newConditionName)) {
                // No condition selected
                EditorUtility.DisplayDialog("Add Condition Resolver", "Can't add. No condition selected", "OK");
                return;
            }

            if(string.IsNullOrEmpty(this.newResolverClassName)) {
                // No resolver class selected
                EditorUtility.DisplayDialog("Add Condition Resolver", "Can't add. No resolver selected", "OK");
                return;
            }

            // Check if one already exists
            ConditionResolverData existing = domain.GetConditionResolver(this.newConditionName);
            if(existing != null) {
                // A resolver for the current condition already exists
                EditorUtility.DisplayDialog("Add Condition Resolver", "Can't add. A resolver for the specified condition already exists.", "OK");
                return;
            }

            ConditionResolverData conditionResolverData = new ConditionResolverData();
            conditionResolverData.ConditionName = this.newConditionName;

            ClassData resolverData = new ClassData();
            resolverData.ClassName = this.newResolverClassName;
            conditionResolverData.ResolverClass = resolverData;

            domain.ConditionResolvers.Add(conditionResolverData);

            // blank the chosen ones
            this.newConditionName = "";
            this.newResolverName = "";
            this.newResolverClassName = "";

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private void RenderConditionResolver(GoapDomainData domain, ConditionResolverData resolver) {
            GUILayout.BeginHorizontal();

            // Remove
            GUI.backgroundColor = ColorUtils.RED;
            if(GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(15))) {
                RemoveResolver(domain, resolver);
            }
            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Label(resolver.ConditionName);
            GUILayout.EndHorizontal();

            if (resolver.ResolverClass.ClassType == null) {
                // Cache
                resolver.ResolverClass.ClassType = TypeUtils.GetType(resolver.ResolverClass.ClassName);
                Assertion.NotNull(resolver.ResolverClass.ClassType, resolver.ResolverClass.ClassName);
            }

            GUILayout.Box(resolver.ResolverClass.ClassType.Name, GUILayout.Width(400)); // Display only the simple name

            // Variables
            Assertion.NotNull(resolver.ResolverClass.ClassType);
            ClassData classData = resolver.ResolverClass;

            this.propertiesRenderer.RenderVariables(domain.Variables, classData.Variables, classData.ClassType, classData.ShowHints);

            GUILayout.Space(5);
        }

        private void RemoveResolver(GoapDomainData domain, ConditionResolverData resolver) {
            if(EditorUtility.DisplayDialogComplex("Remove Condition Resolver", string.Format("Are you sure you want to remove resolver for \"{0}\"?", resolver.ConditionName),
                "Yes", "No", "Cancel") != 0) {
                // Cancelled or No
                return;
            }

            domain.ConditionResolvers.Remove(resolver);
            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }
    }
}
