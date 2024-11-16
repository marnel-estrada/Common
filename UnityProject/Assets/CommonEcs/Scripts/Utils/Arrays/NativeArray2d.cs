using Unity.Collections;
using Unity.Jobs;

namespace CommonEcs {
    public struct NativeArray2d<T> where T : unmanaged {
        private NativeArray<T> values;
        public readonly int columns;
        public readonly int rows;

        public NativeArray2d(int columns, int rows, Allocator allocator) {
            this.columns = columns;
            this.rows = rows;
            this.values = new NativeArray<T>(rows * columns, allocator);
        }
        
        public T this[int x, int y] {
            get => this.values[x + y * this.columns];
            set => this.values[x + y * this.columns] = value;
        }

        public void Dispose() {
            this.values.Dispose();
        }

        public JobHandle Dispose(JobHandle dependency) {
            return this.values.Dispose(dependency);
        }
    }
}
