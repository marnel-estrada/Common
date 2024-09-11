using System.Collections.Generic;
using Common.Time;
using Unity.Entities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// Updates duration timers with time scaling
    /// </summary>
    [UpdateInGroup(typeof(ScalableTimeSystemGroup))]
    public partial class DurationTimerSystem : JobSystemBase {
        private EntityQuery scaledQuery;
        private EntityQuery nonScaledQuery;

        private SharedComponentQuery<TimeReference> timeReferenceQuery;

        protected override void OnCreate() {
            this.scaledQuery = GetEntityQuery(typeof(DurationTimer), typeof(TimeReference));
            this.nonScaledQuery = GetEntityQuery(typeof(DurationTimer), ComponentType.Exclude<TimeReference>());
            
            this.timeReferenceQuery = new SharedComponentQuery<TimeReference>(this, this.EntityManager);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            // Schedule non scaled first
            NonScaledJob job = new() {
                timerType = GetComponentTypeHandle<DurationTimer>(),
                deltaTime = UnityEngine.Time.deltaTime
            };
            JobHandle handle = job.Schedule(this.nonScaledQuery, inputDeps);
            
            return ScheduleScaled(handle);
        }

        private JobHandle ScheduleScaled(JobHandle dependency) {
            this.timeReferenceQuery.Update();
            IReadOnlyList<TimeReference> timeReferences = this.timeReferenceQuery.SharedComponents;

            JobHandle lastHandle = dependency;
            ComponentTypeHandle<DurationTimer> timerType = GetComponentTypeHandle<DurationTimer>();
            
            // Note here that we start counting from 1 since the first entry is always a default one
            for (int i = 1; i < timeReferences.Count; ++i) {
                TimeReference timeReference = timeReferences[i];
                
                this.scaledQuery.SetSharedComponentFilterManaged(timeReference);
                float timeScale = TimeReferencePool.GetInstance().Get(timeReference.id).TimeScale;

                ScaledJob job = new() {
                    timerType = timerType,
                    scaledDeltaTime = UnityEngine.Time.deltaTime * timeScale
                };

                lastHandle = job.Schedule(this.scaledQuery, lastHandle);
            }
            
            return lastHandle;
        }

        [BurstCompile]
        private struct ScaledJob : IJobChunk {
            public ComponentTypeHandle<DurationTimer> timerType;
            public float scaledDeltaTime;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<DurationTimer> timers = chunk.GetNativeArray(ref this.timerType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    DurationTimer timer = timers[i];
                    if (!(timer.polledTime < timer.durationTime)) {
                        // polledTime has reached the durationTime
                        continue;
                    }

                    timer.polledTime += this.scaledDeltaTime;
                    timers[i] = timer; // Modify
                }
            }
        }

        [BurstCompile]
        private struct NonScaledJob : IJobChunk {
            public ComponentTypeHandle<DurationTimer> timerType;
            public float deltaTime;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<DurationTimer> timers = chunk.GetNativeArray(ref this.timerType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    DurationTimer timer = timers[i];
                    if (!(timer.polledTime < timer.durationTime)) {
                        // polledTime has reached the durationTime
                        continue;
                    }

                    timer.polledTime += this.deltaTime;
                    timers[i] = timer; // Modify
                }
            }
        }
    }
}
