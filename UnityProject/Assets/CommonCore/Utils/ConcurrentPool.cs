using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Common {
    public class ConcurrentPool<T> where T : class, new() {
        private readonly ConcurrentStack<T> stack = new ConcurrentStack<T>();
        private ConcurrentBag<T> bag;
        
        // Used to check if an instance was already recycled
        private readonly HashSet<T> recycledItems = new HashSet<T>();

        public T Request() {
            if (this.stack.Count > 0 && this.stack.TryPop(out T instance)) {
                // There are recycled instances
                this.recycledItems.Remove(instance);
                return instance;
            }
            
            return new T();
        }

        public void Recycle(T instance) {
            if (this.recycledItems.Contains(instance)) {
                // Already recycled before
                return;
            }
            
            this.stack.Push(instance);
            this.recycledItems.Add(instance);
        }
    }
}