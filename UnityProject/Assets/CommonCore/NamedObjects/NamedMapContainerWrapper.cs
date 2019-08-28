namespace Common {
    /// <summary>
    /// A wrapper class that wraps a NamedMap instance such that it will implement the NamedValueContainer interface
    /// </summary>
    class NamedMapContainerWrapper<T> : NamedValueContainer where T : NamedValueHolder, new() {
        // we use a delegate to resolve the NamedMap instance instead of the actual instance
        // because we can't really know when are the variable maps instantiated in editor time
        public delegate NamedMap<T> MapResolver();

        private readonly MapResolver mapResolver;

        /// <summary>
        /// Constructor with specified resolver
        /// </summary>
        /// <param name="resolver"></param>
        public NamedMapContainerWrapper(MapResolver resolver) {
            this.mapResolver = resolver;
        }

        public void Clear() {
            this.mapResolver().Clear();
        }

        public void Add(string name) {
            T newValue = new T();
            newValue.Name = name;
            this.mapResolver().Add(newValue);
        }

        public bool Contains(string name) {
            return this.mapResolver().Contains(name);
        }

        public void Remove(string name) {
            this.mapResolver().Remove(name);
        }

        public object Get(string name) {
            // note here that the Named* instance is returned. Not its value.
            return this.mapResolver().Get(name);
        }

        public int Count {
            get {
                return this.mapResolver().Count;
            }
        }

        public object GetAt(int index) {
            return this.mapResolver().GetAt(index);
        }

        public string GetNameAt(int index) {
            Named named = GetAt(index) as Named;
            return named.Name;
        }

        public NamedValueHolder GetNamedValueHolderAt(int index) {
            return this.mapResolver().GetNamedValueHolderAt(index);
        }

        public object GetCopy(string name) {
            T variable = this.mapResolver().Get(name);
            T copy = new T();
            copy.Name = variable.Name;
            copy.Set(variable.Get());
            copy.UseOtherHolder = variable.UseOtherHolder;
            copy.OtherHolderName = variable.OtherHolderName;

            return copy;
        }
    }
}
