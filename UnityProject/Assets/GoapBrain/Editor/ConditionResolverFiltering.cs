using System.Collections.Generic;

using Common;

namespace GoapBrain {
    public class ConditionResolverFiltering {
        private IReadOnlyList<ConditionResolverData> resolvers;
        private readonly List<ConditionResolverData> filteredList = new List<ConditionResolverData>();

        private string nameFilter;
        private string classFilter;

        public void Prepare(IReadOnlyList<ConditionResolverData> resolvers) {
            this.resolvers = resolvers;
            Assertion.NotNull(this.resolvers);
            
            ApplyFilter();
        }

        public void ApplyFilter() {
            this.filteredList.Clear();

            if (string.IsNullOrEmpty(this.nameFilter) && string.IsNullOrEmpty(this.classFilter)) {
                this.filteredList.AddRange(this.resolvers);
            } else {
                FilterByName();
                FilterByClass();
            }
        }

        public string NameFilter {
            get {
                return this.nameFilter;
            }
            set {
                this.nameFilter = value;
            }
        }

        public string ClassFilter {
            get {
                return this.classFilter;
            }
            set {
                this.classFilter = value;
            }
        }

        public IReadOnlyList<ConditionResolverData> FilteredList {
            get {
                return this.filteredList;
            }
        }
 
        private void FilterByName() {
            if (string.IsNullOrEmpty(this.nameFilter)) {
                // No need to filter
                return;
            }
            
            for (int i = 0; i < this.resolvers.Count; ++i) {
                ConditionResolverData resolver = this.resolvers[i];
                if (resolver.ConditionName.Contains(this.nameFilter)) {
                    this.filteredList.Add(resolver);
                }
            }
        }

        private void FilterByClass() {
            if (string.IsNullOrEmpty(this.classFilter)) {
                // No need to filter
                return;
            }
            
            for (int i = 0; i < this.resolvers.Count; ++i) {
                ConditionResolverData resolver = this.resolvers[i];
                if (resolver.ResolverClass.ClassName.Contains(this.classFilter)) {
                    this.filteredList.Add(resolver);
                }
            }
        }
    }
}