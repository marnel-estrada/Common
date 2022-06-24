using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace CommonEcs.Goap {
    public partial class GoapTextDbSystem : SystemBase {
        private NativeParallelHashMap<int, FixedString64Bytes> textMap;
        private GoapTextResolver textResolver;

        protected override void OnCreate() {
            base.OnCreate();

            // Need not run per frame
            this.Enabled = false;
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            if (this.textMap.IsCreated) {
                this.textMap.Dispose();
            }
        }

        protected override void OnUpdate() {
        }

        public void CreateTextDb(IReadOnlyDictionary<int, FixedString64Bytes> rawTextMap) {
            this.textMap = new NativeParallelHashMap<int, FixedString64Bytes>(rawTextMap.Count, Allocator.Persistent);
            
            foreach (KeyValuePair<int,FixedString64Bytes> entry in rawTextMap) {
                this.textMap.Add(entry.Key, entry.Value);                
            }

            this.textResolver = new GoapTextResolver(this.textMap);
        }
        
        public ref readonly GoapTextResolver TextResolver {
            get {
                return ref this.textResolver;
            }
        }
    }
}