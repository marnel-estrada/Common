namespace Common {
    public struct ReadOnlySimpleList<T> {
        private readonly SimpleList<T> list;

        public ReadOnlySimpleList(SimpleList<T> list) {
            this.list = list;
        }
        
        public T this[int i] {
            get {
                return this.list[i];
            }
        }

        public int Count {
            get {
                return this.list.Count;
            }
        }

        public bool Contains(T item) {
            return this.list.Contains(item);
        }

        public int IndexOf(T item) {
            return this.list.IndexOf(item);
        }

        public bool IsEmpty {
            get {
                return this.list.IsEmpty();
            }
        }
    }
}