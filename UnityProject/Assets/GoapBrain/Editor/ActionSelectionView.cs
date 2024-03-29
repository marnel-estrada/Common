﻿using System;
using System.Collections.Generic;
using Common;
using Unity.Collections;
using UnityEngine;

namespace GoapBrain {
    /// <summary>
    ///     Handles the filtering and showing of filtered actions
    /// </summary>
    internal class ActionSelectionView {
        private readonly SimpleList<GoapActionData> filteredList = new SimpleList<GoapActionData>();
        private readonly List<string> filteredNames = new List<string>(); // Used to render list of action buttons

        private string? atomActionFilter = "";
        private string? effectFilter = "";
        private string? nameFilter = "";
        private string? preconditionFilter = "";
        private string? hashCodeFilter = "";

        private int selection;

        public string NameFilter {
            get {
                if (this.nameFilter == null) {
                    return "";
                }

                return this.nameFilter;
            }

            set {
                this.nameFilter = value;
            }
        }

        public string AtomActionFilter {
            get {
                if (this.atomActionFilter == null) {
                    return "";
                }

                return this.atomActionFilter;
            }

            set {
                this.atomActionFilter = value;
            }
        }

        public string PreconditionFilter {
            get {
                if (this.preconditionFilter == null) {
                    return "";
                }

                return this.preconditionFilter;
            }

            set {
                this.preconditionFilter = value;
            }
        }

        public string EffectFilter {
            get {
                if (this.effectFilter == null) {
                    return "";
                }

                return this.effectFilter;
            }

            set {
                this.effectFilter = value;
            }
        }

        public string HashCodeFilter {
            get {
                if (this.hashCodeFilter == null) {
                    return "";
                }

                return this.hashCodeFilter;
            }

            set {
                this.hashCodeFilter = value;
            }
        }

        /// <summary>
        ///     Applies the filter
        /// </summary>
        /// <param name="domainData"></param>
        /// <param name="nameFilter"></param>
        public void FilterByName(GoapDomainData domainData, string nameFilter) {
            this.nameFilter = nameFilter;

            Filter(domainData, nameFilter, delegate(GoapActionData action, string filter) {
                return action.Name.ToLower().Contains(filter.ToLower());
            });
        }

        /// <summary>
        ///     Filter by atom action name
        /// </summary>
        /// <param name="domainData"></param>
        /// <param name="atomActionFilter"></param>
        public void FilterByAtomAction(GoapDomainData domainData, string atomActionFilter) {
            this.atomActionFilter = atomActionFilter;
            Filter(domainData, atomActionFilter, ContainsAtomAction);
        }

        private static bool ContainsAtomAction(GoapActionData action, string actionNameFilter) {
            string loweredNameFilter = actionNameFilter.ToLower();

            for (int i = 0; i < action.AtomActions.Count; ++i) {
                ClassData atom = action.AtomActions[i];
                if (atom.ClassName.ToLower().Contains(loweredNameFilter)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Filters the actions by precondition
        /// </summary>
        /// <param name="domainData"></param>
        /// <param name="preconditionFilter"></param>
        public void FilterByPrecondition(GoapDomainData domainData, string preconditionFilter) {
            this.preconditionFilter = preconditionFilter;
            Filter(domainData, preconditionFilter, ContainsPrecondition);
        }

        private static bool ContainsPrecondition(GoapActionData action, string filter) {
            string loweredFilter = filter.ToLower();

            for (int i = 0; i < action.Preconditions.Count; ++i) {
                ConditionData precondition = action.Preconditions[i];
                if (precondition.Name != null && precondition.Name.ToLower().Contains(loweredFilter)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Filters the actions by containing an effect
        /// </summary>
        /// <param name="domainData"></param>
        /// <param name="effectFilter"></param>
        public void FilterByEffect(GoapDomainData domainData, string effectFilter) {
            this.effectFilter = effectFilter;
            Filter(domainData, effectFilter, ContainsEffect);
        }

        public void FilterByHashCode(GoapDomainData domainData, string hashCodeFilter) {
            this.hashCodeFilter = hashCodeFilter;
            Filter(domainData, hashCodeFilter, ContainsHashCode);
        }

        private static bool ContainsEffect(GoapActionData action, string filter) {
            if (action.Effect == null) {
                return false;
            }

            // We use upper here because it is faster
            string upperFilter = filter.ToUpperInvariant();
            return action.Effect.Name != null && action.Effect.Name.ToUpperInvariant().Contains(upperFilter);
        }

        private static bool ContainsHashCode(GoapActionData action, string filter) {
            return action.ActionId.ToString().Contains(filter);
        }

        private void Filter(GoapDomainData domainData, string filter, Func<GoapActionData, string, bool> predicate) {
            this.filteredList.Clear();
            this.filteredNames.Clear();

            for (int i = 0; i < domainData.ActionCount; ++i) {
                GoapActionData action = domainData.GetActionAt(i);

                string actionName = action.Name;
                string hashCode = new FixedString64Bytes(actionName).GetHashCode().ToString();
                if (string.IsNullOrEmpty(filter)) {
                    // Filter text is empty
                    // Add every action data
                    this.filteredList.Add(action);
                    this.filteredNames.Add($"{actionName}  ({hashCode})");
                } else {
                    // Invoke the predicate
                    if (predicate(action, filter)) {
                        this.filteredList.Add(action);
                        this.filteredNames.Add($"{actionName}  ({hashCode})");
                    }
                }
            }
        }

        /// <summary>
        ///     Renders the UI
        /// </summary>
        /// <param name="domainData"></param>
        public void Render(GoapDomainData domainData) {
            if (this.filteredNames.Count == 0) {
                // There are no filtered actions
                ReapplyFilter(domainData);

                if (this.filteredNames.Count == 0) {
                    // If after checking filter and filtered names is still zero, we show that filtered list is empty
                    GUILayout.Label("(empty)");
                }

                return;
            }

            this.selection = GUILayout.SelectionGrid(this.selection, this.filteredNames.ToArray(), 1);
        }

        // Added Pragma warning here since the strings' null checks here are done inside the string.IsNullOrEmpty
#pragma warning disable 8604
        /// <summary>
        ///     Reapplies the filters
        /// </summary>
        /// <param name="domain"></param>
        public void ReapplyFilter(GoapDomainData domain) {
            if (!string.IsNullOrEmpty(this.nameFilter)) {
                FilterByName(domain, this.nameFilter);
            } else if (!string.IsNullOrEmpty(this.atomActionFilter)) {
                FilterByAtomAction(domain, this.atomActionFilter);
            } else if (!string.IsNullOrEmpty(this.preconditionFilter)) {
                FilterByPrecondition(domain, this.preconditionFilter);
            } else if (!string.IsNullOrEmpty(this.effectFilter)) {
                FilterByEffect(domain, this.effectFilter);
            } else {
                // There are no filters. Show all
                FilterByName(domain, string.Empty);
            }
        }
#pragma warning restore 8604

        /// <summary>
        ///     Returns the currently selected action
        /// </summary>
        /// <returns></returns>
        public GoapActionData? GetSelectedAction() {
            if (this.selection >= 0 && this.filteredList.Count > 0) {
                if (this.selection < this.filteredList.Count) {
                    // Check for valid index
                    return this.filteredList[this.selection];
                }
            }

            // Client code should check for this
            return null;
        }

        /// <summary>
        ///     Adds to the filtered list
        /// </summary>
        /// <param name="actionData"></param>
        public void AddFiltered(GoapActionData actionData) {
            this.filteredList.Add(actionData);
            this.filteredNames.Add(actionData.Name);
        }
    }
}