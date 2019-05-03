using Unity.Entities;

using Common;

namespace CommonEcs {
    public struct TimeReference : ISharedComponentData {
        private readonly Internal instance;

        public TimeReference(byte id) {
            this.instance = new Internal(id);
        }

        public float TimeScale {
            get {
                return this.instance.timeScale;
            }

            set {
                this.instance.timeScale = value;
            }
        }
        
        public bool IsNull {
            get {
                return this.instance == null;
            }
        }

        // We use a class internally so we don't have to access it through a chunk if we want to modify it.
        private class Internal {
            public readonly byte id;
            public float timeScale;

            public Internal(byte id) {
                this.id = id;
                Assertion.Assert(this.id > 0); // Zero is reserved to identify a non existing TimeReference
            
                this.timeScale = 1.0f;
            }    
        }
    }
}