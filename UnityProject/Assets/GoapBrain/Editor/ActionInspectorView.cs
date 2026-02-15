using System;
using System.Reflection;
using Common;
using UnityEditor;
using UnityEngine;

namespace GoapBrain {
    /// <summary>
    ///     Handles rendering of the action inspector part
    /// </summary>
    internal class ActionInspectorView {
        private readonly AtomActionsView atomActionsView;
        private readonly ActionConditionsView effectsView;

        private readonly ActionConditionsView preconditionsView;

        private Vector2 mainScrollPos;

        /// <summary>
        ///     Constructor
        /// </summary>
        public ActionInspectorView(EditorWindow parent) {
            this.preconditionsView = new ActionConditionsView(parent, "Precondition", ColorUtils.RED, false);
            this.effectsView = new ActionConditionsView(parent, "Effect", ColorUtils.GREEN, false);
            this.atomActionsView = new AtomActionsView(parent);
        }

        public void OnEnable() {
            this.atomActionsView.OnEnable();
        }

        private const string DIALOG_TITLE = "GOAP Domain Editor";

        private GoapDomainData? copyActionDest;

        /// <summary>
        ///     Renders the view
        /// </summary>
        public void Render(GoapDomainData domain, GoapActionData action) {
            this.mainScrollPos = GUILayout.BeginScrollView(this.mainScrollPos);

            // Utility buttons
            GUILayout.BeginHorizontal();
            
            // Remove action
            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("Remove Action", GUILayout.Width(100))) {
                RemoveAction(domain, action);
            }
            GUI.backgroundColor = ColorUtils.WHITE;

            if (GUILayout.Button("Copy To", GUILayout.Width(90))) {
                ValidateAndCopyAction(domain, action);
            }
            
            this.copyActionDest =
                EditorGUILayout.ObjectField(this.copyActionDest, typeof(GoapDomainData), false, GUILayout.Width(200))
                    as GoapDomainData;
            
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Enabled or Disabled
            GUILayout.BeginHorizontal();
            GUILayout.Label("Enabled: ", GUILayout.Width(100));
            action.Enabled = GUILayout.Toggle(action.Enabled, "", GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Cancellable
            GUILayout.BeginHorizontal();
            GUILayout.Label("Cancellable: ", GUILayout.Width(100));
            action.Cancellable = GUILayout.Toggle(action.Cancellable, "", GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Name
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name: ", GUILayout.Width(100));
            GUILayout.Label(action.Name, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            // Action Id
            GUILayout.BeginHorizontal();
            GUILayout.Label("Action ID: ", GUILayout.Width(100));
            GUILayout.Label(action.ActionId.ToString(), GUILayout.Width(150));
            GUILayout.EndHorizontal();

            // Cost
            GUILayout.BeginHorizontal();
            GUILayout.Label("Cost: ", GUILayout.Width(100));
            action.Cost = EditorGUILayout.FloatField(action.Cost, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            // Comment
            RenderComment(action);

            GUILayout.Space(5);

            // Preconditions
            GUI.backgroundColor = ColorUtils.RED;
            GUILayout.Box("Preconditions", GUILayout.Width(100));
            GUI.backgroundColor = ColorUtils.WHITE;

            this.preconditionsView.RenderConditions(domain, action.Preconditions);

            GUILayout.Space(10);

            // Effects
            GUI.backgroundColor = ColorUtils.GREEN;
            GUILayout.Box("Effect", GUILayout.Width(70));
            GUI.backgroundColor = ColorUtils.WHITE;

            this.effectsView.RenderEffect(domain, action);

            GUILayout.Space(10);

            // Actions
            GUILayout.Box("Actions", GUILayout.Width(70));
            this.atomActionsView.Render(domain, action);

            GUILayout.EndScrollView();
        }

        private void ValidateAndCopyAction(GoapDomainData source, GoapActionData action) {
            if (source == this.copyActionDest) {
                EditorUtility.DisplayDialog(DIALOG_TITLE, "The source can't be the same as the destination.", "OK");
                return;
            }

            if (this.copyActionDest) {
                if (CopyAction(source, action, this.copyActionDest)) {
                    EditorUtility.DisplayDialog(DIALOG_TITLE, $"Action {action.Name} was copied to {this.copyActionDest?.name}.", "OK");
                }
            } else {
                EditorUtility.DisplayDialog(DIALOG_TITLE, "A destination GOAP domain must be specified.", "OK");
            }
        }

        // Returns whether the copy was successful
        private static bool CopyAction(GoapDomainData source, GoapActionData action, GoapDomainData destination) {
            if (destination.HasAction(action.Name)) {
                EditorUtility.DisplayDialog(DIALOG_TITLE, $"Destination GoapDomainData {destination.name} already has an action named {action.Name}.", "OK");
                return false;
            }
            
            // Add the conditions to the destination. Note that we only add non-existent ones here.
            destination.AddNonExistentConditions(action.Preconditions);

            if (action.Effect != null && !string.IsNullOrWhiteSpace(action.Effect.Name)) {
                destination.AddConditionName(action.Effect.Name);
            }
            
            // Copy condition resolvers
            CopyConditionResolvers(source, action, destination);

            // Copy variables that are used in atom actions
            CopyAtomActionVariables(source, action, destination);
            
            // Add the non existent variables from the condition resolvers of both preconditions and effect
            CopyConditionResolverVariables(source, action, destination);

            destination.AddAction(action.CreateCopy());
            
            // Save the destination
            EditorUtility.SetDirty(destination);
            AssetDatabase.SaveAssets();

            return true;
        }

        private static void CopyConditionResolvers(GoapDomainData source, GoapActionData action,
            GoapDomainData destination) {
            // Copy condition resolvers of preconditions
            foreach (ConditionData precondition in action.Preconditions) {
                CopyConditionResolver(source, destination, precondition.Name);
            }
            
            // Copy condition resolver for effect
            if (action.Effect != null && !string.IsNullOrWhiteSpace(action.Effect.Name)) {
                CopyConditionResolver(source, destination, action.Effect.Name);
            }
        }

        private static void CopyConditionResolver(GoapDomainData source, GoapDomainData destination,
            string conditionName) {
            if (!source.TryGetConditionResolver(conditionName, out ConditionResolverData? sourceResolver)) {
                // No resolver for the precondition. No variable to copy.
                return;
            }

            if (destination.TryGetConditionResolver(conditionName, out ConditionResolverData? destinationResolver)) {
                // The destination already has a condition resolver for the condition
                // We compare if they have the same resolver class. If not, we display as error.
                if (sourceResolver.ResolverClass.ClassName != destinationResolver.ResolverClass.ClassName) {
                    Debug.LogError($"Destination domain data already has a resolver for condition {conditionName} but the resolver class are different ({sourceResolver.ResolverClass.ClassName} != {destinationResolver.ResolverClass.ClassName})");
                }
                return;
            }
            
            destination.AddConditionResolver(sourceResolver.CreateCopy());
        }

        private static void CopyAtomActionVariables(GoapDomainData source, GoapActionData action,
            GoapDomainData destination) {
            foreach (ClassData atomActionData in action.AtomActions) {
                CopyVariables(source, destination, atomActionData);
            }
        }

        private static void CopyConditionResolverVariables(GoapDomainData source, GoapActionData action, GoapDomainData destination) {
            // Copy variables from preconditions
            foreach (ConditionData precondition in action.Preconditions) {
                CopyConditionResolverVariables(source, destination, precondition.Name);
            }
            
            // Copy variables from effect
            if (action.Effect == null) {
                return;
            }
            
            CopyConditionResolverVariables(source, destination, action.Effect.Name);
        }

        private static void CopyConditionResolverVariables(GoapDomainData source, GoapDomainData destination,
            string conditionName) {
            if (!source.TryGetConditionResolver(conditionName, out ConditionResolverData? conditionResolver)) {
                // No resolver for the precondition. No variable to copy.
                return;
            }

            CopyVariables(source, destination, conditionResolver.ResolverClass);
        }

        private static void CopyVariables(GoapDomainData source, GoapDomainData destination, ClassData classData) {
            Type classType = TypeUtils.GetType(classData.ClassName);
            PropertyInfo[] properties = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties) {
                if (!TypeUtils.IsVariableProperty(property)) {
                    continue;
                }

                if (!NamedValueLibrary.IsSupported(property.PropertyType)) {
                    // not a supported type
                    continue;
                }

                // resolve variable
                NamedValueType namedType = NamedValueType.ConvertFromPropertyType(property.PropertyType);
                ValueHolder? holder = classData.Variables.Get(property.Name, namedType) as ValueHolder;
                if (holder == null) {
                    // This should not happen but we are required to check
                    continue;
                }

                if (!holder.UseOtherHolder) {
                    // Variable is not using another variable for its value
                    continue;
                }

                if (destination.Variables.Contains(holder.OtherHolderName, namedType)) {
                    // The destination already has the variable
                    continue;
                }
                
                destination.Variables.Add(holder.OtherHolderName, namedType);
                
                // Copy the value as well
                ValueHolder sourceVariable = (ValueHolder)source.Variables.Get(holder.OtherHolderName, namedType);
                ValueHolder destVariable = (ValueHolder)destination.Variables.Get(holder.OtherHolderName, namedType);
                object value = sourceVariable.Get();
                destVariable.Set(value);
            }
        }

        private static void RenderComment(GoapActionData action) {
            action.ShowComment = EditorGUILayout.Foldout(action.ShowComment, "Comment");
            if (!action.ShowComment) {
                return;
            }

            if (action.EditComment) {
                EditorStyles.textField.wordWrap = true;
                action.Comment =
                    EditorGUILayout.TextArea(action.Comment, GUILayout.Width(600), GUILayout.Height(200));
                if (GUILayout.Button("Done", GUILayout.Width(70))) {
                    action.EditComment = false;
                }
            } else {
                EditorGUILayout.HelpBox(action.Comment, MessageType.Info);
                if (GUILayout.Button("Edit", GUILayout.Width(70))) {
                    action.EditComment = true;
                }
            }
        }

        private static void RemoveAction(GoapDomainData domain, GoapActionData action) {
            if (EditorUtility.DisplayDialogComplex("Remove Action",
                    $"Are you sure you want to remove action \"{action.Name}\"?", "Yes", "No",
                    "Cancel") !=
                0) {
                // Cancelled or No
                return;
            }

            domain.RemoveAction(action);

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }
    }
}