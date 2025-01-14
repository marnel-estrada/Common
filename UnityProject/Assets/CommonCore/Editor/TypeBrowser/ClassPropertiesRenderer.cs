﻿using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Common {
    /// <summary>
    ///     A class that handles rendering of named properties of a certain class
    /// </summary>
    public class ClassPropertiesRenderer {
        private const int DEFAULT_LABEL_WIDTH = 200;

        private readonly int fieldWidth;

        private readonly VariableFieldRenderer variableFieldRenderer;

        private readonly VariableNamesAggregator aggregator = new();
        private readonly PopupValueSet variablesValueSet = new();

        /// <summary>
        ///     Constructor
        /// </summary>
        public ClassPropertiesRenderer(int fieldWidth) {
            this.fieldWidth = fieldWidth;
            this.variableFieldRenderer = new VariableFieldRenderer(fieldWidth);
        }

        /// <summary>
        ///     Renders the variables of the specified class type
        ///     The specified parentVariables is the one the variable would refer to if it was set as using another variable
        /// </summary>
        /// <param name="parentVariables"></param>
        /// <param name="localVariables"></param>
        /// <param name="classType"></param>
        /// <param name="showHints"></param>
        /// <param name="labelWidth"></param>
        public void RenderVariables(NamedValueLibrary parentVariables, NamedValueLibrary localVariables, Type classType,
                                    bool showHints, int labelWidth = DEFAULT_LABEL_WIDTH) {
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
                ValueHolder? holder = localVariables.Get(property.Name, namedType) as ValueHolder;
                if (holder == null) {
                    // this means that the variable is not existing yet
                    // we add it to the variable library
                    localVariables.Add(property.Name, namedType);

                    // resolve again
                    holder = localVariables.Get(property.Name, namedType) as ValueHolder;
                    Assertion.NotNull(holder, "holder");
                }

                RenderVariableField(parentVariables, property, holder, namedType, showHints, labelWidth);

                GUILayout.Space(5);
            }
        }

        private void RenderVariableField(NamedValueLibrary parentVariables, PropertyInfo property, ValueHolder? holder,
                                         NamedValueType namedType, bool showHint, int labelWidth) {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(property.Name + ": ", GUILayout.Width(labelWidth));

            bool hasSelectionHint = HasEditorHint(property, EditorHint.SELECTION);

            if (holder != null) {
                if (holder.UseOtherHolder || hasSelectionHint) {
                    // render pop-up of event node variables here
                    this.aggregator.Update(parentVariables);
                    string[] variableNames = this.aggregator.GetVariablesNames(namedType);
                    this.variablesValueSet.Update(variableNames, variableNames);
                    holder.OtherHolderName =
                        EditorRenderUtils.Dropdown(holder.OtherHolderName, this.variablesValueSet, this.fieldWidth);
                    holder.UseOtherHolder =
                        true; // We set to true here because EditorHint.SELECTION does not automatically set it to true
                } else {
                    VariableFieldRenderer.FieldRenderer fieldRenderer =
                        this.variableFieldRenderer.GetFieldRenderer(namedType);
                    holder.Set(fieldRenderer(holder));
                }

                if (!hasSelectionHint) {
                    // Render this button only if property does not have the selection hint
                    // This is because we force variable selection if it has such hint
                    // Note also that we only render the var toggle if there's parent variables specified
                    holder.UseOtherHolder = GUILayout.Toggle(holder.UseOtherHolder, "var", EditorStyles.miniButton,
                        GUILayout.Width(40));
                }
            }

            if (showHint) {
                RenderHint(property);
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool HasEditorHint(PropertyInfo property, string editorHint) {
            Attribute[] attributes = Attribute.GetCustomAttributes(property);
            foreach (Attribute attribute in attributes) {
                if (attribute is EditorHint hint) {
                    string editorHintValue = hint.Hint;
                    if (editorHint.Equals(editorHintValue)) {
                        return true;
                    }
                }
            }

            return false;
        }

        private void RenderHint(PropertyInfo property) {
            Attribute[] attributes = Attribute.GetCustomAttributes(property);
            foreach (Attribute attribute in attributes) {
                if (attribute is TextHint textHint) {
                    string hint = textHint.Text;
                    EditorGUILayout.HelpBox(hint, MessageType.None);

                    break;
                }
            }
        }
    }
}