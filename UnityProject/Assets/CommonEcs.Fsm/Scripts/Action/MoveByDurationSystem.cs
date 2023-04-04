using Common;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateAfter(typeof(DurationTimerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class MoveByDurationSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Transform), typeof(Move), typeof(DurationTimer), 
                typeof(Active));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            NativeArray<Move> moves = this.query.ToComponentDataArrayAsync<Move>(Allocator.TempJob, out JobHandle movesHandle);
            NativeArray<DurationTimer> timers =
                this.query.ToComponentDataArrayAsync<DurationTimer>(Allocator.TempJob, out JobHandle timersHandle);
            JobHandle combinedHandle = JobHandle.CombineDependencies(movesHandle, timersHandle, inputDeps);
            
            Job job = new Job() {
                moveArray = moves,
                timers = timers
            };
            
            return job.Schedule(this.query.GetTransformAccessArray(), combinedHandle);
        }

        [BurstCompile]
        private struct Job : IJobParallelForTransform {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Move> moveArray;
            
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<DurationTimer> timers;
            
            public void Execute(int index, TransformAccess transform) {
                DurationTimer timer = this.timers[index];
                Move move = this.moveArray[index];
                transform.position = math.lerp(move.positionFrom, move.positionTo, timer.Ratio);
            }
        }
    }
}