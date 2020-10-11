using Unity.Collections;

namespace CommonEcs {
    /// <summary>
    /// Uses a NativeArray internally to wrap a single value. This is temporary for now.
    /// It should use the more sophisticated implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct NativeUnit<T> where T : struct {
        private NativeArray<T> internalContainer;

        public NativeUnit(Allocator allocator) {
            this.internalContainer = new NativeArray<T>(1, allocator);
        }

        public void Dispose() {
            this.internalContainer.Dispose();
        }

        public T Value {
            get {
                return this.internalContainer[0];
            }

            set {
                this.internalContainer[0] = value;
            }
        }
    }
}