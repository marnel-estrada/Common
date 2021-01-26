namespace Common {
    /// <summary>
    /// Editor class for DataClassPool objects. Must be derived before it can be used.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataClassPoolEditor<T> : UnityEditor.Editor where T : Identifiable, new() {
        private DataClassPool<T> dataPool;

        void OnEnable() {
            this.dataPool = (DataClassPool<T>)this.target;
            Assertion.NotNull(this.dataPool);
        }
        
        protected DataClassPool<T> DataPool {
            get {
                return (DataClassPool<T>)this.target;
            }
        }
    }
}
