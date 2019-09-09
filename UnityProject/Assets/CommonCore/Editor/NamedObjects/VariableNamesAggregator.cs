using System.Collections.Generic;

namespace Common {
    /// <summary>
    ///     A class that aggregates variable names for each type
    ///     Such names will then be used in dropdown lists
    /// </summary>
    public class VariableNamesAggregator {

        // We use list internally
        // We'll just use ToArray() when needed
        private readonly Dictionary<NamedValueType, List<string>> namesMap;

        /// <summary>
        ///     Constructor
        /// </summary>
        public VariableNamesAggregator() {
            this.namesMap = new Dictionary<NamedValueType, List<string>>();

            // add an entry for each supported type
            for (int i = 0; i < NamedValueType.ALL_TYPES.Length; ++i) {
                this.namesMap[NamedValueType.ALL_TYPES[i]] = new List<string>();
            }
        }

        /// <summary>
        ///     Updates the aggregated names
        /// </summary>
        public void Update(NamedValueLibrary library) {
            foreach (KeyValuePair<NamedValueType, List<string>> entry in this.namesMap) {
                entry.Value.Clear();

                // We add an empty entry so that popup still works even if there are no variables for such type
                entry.Value.Add("");

                NamedValueContainer container = library.GetContainer(entry.Key);

                // populate for each named instance found in container
                for (int i = 0; i < container.Count; ++i) {
                    entry.Value.Add(container.GetNameAt(i));
                }
            }
        }

        /// <summary>
        ///     Returns the set of names of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string[] GetVariablesNames(NamedValueType type) {
            return this.namesMap[type].ToArray();
        }
    }
}