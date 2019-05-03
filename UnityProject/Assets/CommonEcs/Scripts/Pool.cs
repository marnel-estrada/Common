using System;
using System.Collections.Generic;

namespace CommonEcs {
    public class Pool<T> where T : struct, IDisposable {
        // The delegate that creates the instance
        public delegate T Creator();
    
        private readonly Creator creator;
        private readonly List<T> instances = new List<T>(10);
        private readonly List<int> unusedIndices = new List<int>(10);
    
        public Pool(Creator creator) {
            this.creator = creator;
        }
    
        public PoolEntry<T> Request() {
            if (this.unusedIndices.Count > 0) {
                // There are inactive entries
                int lastEntry = this.unusedIndices.Count - 1;
                int index = this.unusedIndices[lastEntry];
                this.unusedIndices.RemoveAt(lastEntry);
                
                return new PoolEntry<T>(this.instances[index], index);
            }
            
            // Create a new entry
            int newIndex = this.instances.Count;
            T newInstance = this.creator();
            this.instances.Add(newInstance);
            return new PoolEntry<T>(newInstance, newIndex);
        }
    
        public void Recycle(PoolEntry<T> entry) {
            this.unusedIndices.Add(entry.index);
        }
    
        public void Dispose() {
            for (int i = 0; i < this.instances.Count; ++i) {
                this.instances[i].Dispose();
            }
        }
    }
}