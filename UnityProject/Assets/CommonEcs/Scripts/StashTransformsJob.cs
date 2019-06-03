using Unity.Burst;
using Unity.Collections;

using UnityEngine.Jobs;

namespace CommonEcs {
    [BurstCompile]
    public struct StashTransformsJob : IJobParallelForTransform {
        public NativeArray<TransformStash> stashes;

        public void Execute(int index, TransformAccess transform) {
            this.stashes[index] = new TransformStash {
                rotation = transform.rotation, position = transform.position,
            };
        }
    }
}