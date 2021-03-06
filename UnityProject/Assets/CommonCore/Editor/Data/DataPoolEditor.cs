using UnityEditor;

namespace Common {
    public class DataPoolEditor<T> : Editor where T : class, IDataPoolItem, IDuplicable<T>, new() {
        private DataPool<T> dataPool;
        
        void OnEnable() {
            this.dataPool = (DataPool<T>)this.target;
            Assertion.NotNull(this.dataPool);
        }
        
        protected DataPool<T> DataPool {
            get {
                return (DataPool<T>)this.target;
            }
        }
    }
}