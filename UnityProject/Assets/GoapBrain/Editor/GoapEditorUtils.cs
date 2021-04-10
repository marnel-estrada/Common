using System;

using UnityEditor;
using UnityEngine;

namespace GoapBrain {
    public static class GoapEditorUtils {
        /// <summary>
        /// Opens the common condition selector
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="parent"></param>
        /// <param name="onConditionSelected"></param>
        public static void OpenConditionSelector(GoapDomainData domain, EditorWindow parent, Action<string> onConditionSelected, bool includeExtensions) {
            Rect position = parent.position;
            position.x += (parent.position.width * 0.5f) - 125;
            position.y += (parent.position.height * 0.5f) - 75;
            position.width = 500;
            position.height = 200;

            ConditionSelectorWindow conditionSelector = ScriptableObject.CreateInstance<ConditionSelectorWindow>();
            conditionSelector.Init(domain, onConditionSelected, includeExtensions);
            conditionSelector.position = position;
            conditionSelector.ShowUtility();
            conditionSelector.Focus();
        }
    }
}
