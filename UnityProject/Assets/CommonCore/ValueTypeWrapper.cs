namespace Common {
    /// <summary>
    /// Used for wrapping value types when passed to receivers with object or interface to avoid boxing
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueTypeWrapper<T> where T : struct {
        private readonly T value;

        public ValueTypeWrapper(T value) {
            this.value = value;
        }

        public T Value {
            get {
                return this.value;
            }
        }
    }
}