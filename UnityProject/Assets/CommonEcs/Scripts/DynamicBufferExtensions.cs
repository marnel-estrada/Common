using Unity.Entities;

namespace CommonEcs {
    public static class DynamicBufferExtensions {
        /// <summary>
        /// Sets the value at index 0 if such value was already added. If not, it adds the value.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void SetFirstValueOrAdd<T>(this DynamicBuffer<T> self, T value)
            where T : unmanaged, IBufferElementData {
            if (self.Length == 0) {
                self.Add(value);
                return;
            }

            // Already has content
            self[0] = value;
        }
    }
}