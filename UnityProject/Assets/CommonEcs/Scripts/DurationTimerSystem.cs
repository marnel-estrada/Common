using System.Collections.Generic;

using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// Updates duration timers with time scaling
    /// </summary>
    [UpdateInGroup(typeof(ScalableTimeSystemGroup))]
    public class DurationTimerSystem : JobSystemBase {
        private EntityQuery scaledQuery;
        private EntityQuery nonScaledQuery;

        private SharedComponentQuery<TimeReference> timeReferenceQuery;

        protected override void OnCreate() {
            this.scaledQuery = GetEntityQuery(typeof(DurationTimer), typeof(TimeReference));
            this.nonScaledQuery = GetEntityQuery(typeof(DurationTimer), ComponentType.Exclude<TimeReference>());
            
            this.timeReferenceQuery = new SharedComponentQuery<TimeReference>(this, this.EntityManager);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return ScheduleScaled(ScheduleNonScaled(inputDeps));
        }

        private JobHandle ScheduleScaled(JobHandle dependency) {
            this.timeReferenceQuery.Update();
            IReadOnlyList<TimeReference> timeReferences = this.timeReferenceQuery.SharedComponents;

            JobHandle lastHandle = dependency;
            ComponentTypeHandle<DurationTimer> timerType = GetComponentTypeHandle<DurationTimer>();
            
            // Note here that we start counting from 1 since the first entry is always a default one
            for (int i = 1; i < timeReferences.Count; ++i) {
                TimeReference timeReference = timeReferences[i];
                
                this.scaledQuery.SetSharedComponentFilter(timeReference);

                ScaledJob job = new ScaledJob() {
                    timerType = timerType,
                    scaledDeltaTime = UnityEngine.Time.deltaTime * timeReference.TimeScale
                };

                lastHandle = job.Schedule(this.scaledQuery, lastHandle);
            }
            
            return lastHandle;
        }

        private JobHandle ScheduleNonScaled(JobHandle dependency) {
            NonScaledJob job = new NonScaledJob() {
                timerType = GetComponentTypeHandle<DurationTimer>(),
                deltaTime = UnityEngine.Time.deltaTime
            };

            return job.Schedule(this.nonScaledQuery, dependency);
        }

        [BurstCompile]
        private struct ScaledJob : IJobChunk {
            public ComponentTypeHandle<DurationTimer> timerType;
            public float scaledDeltaTime;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<DurationTimer> timers = chunk.GetNativeArray(this.timerType);
                for (int i = 0; i < timers.Length; ++i) {
                    DurationTimer timer = timers[i];
                    if (timer.polledTime < timer.durationTime) {
                        timer.polledTime += this.scaledDeltaTime;
                        timers[i] = timer; // Modify
                    }
                }
            }
        }

        [BurstCompile]
        private struct NonScaledJob : IJobChunk {
            public ComponentTypeHandle<DurationTimer> timerType;
            public float deltaTime;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<DurationTimer> timers = chunk.GetNativeArray(this.timerType);
                for (int i = 0; i < chunk.Count; ++i) {
                    DurationTimer timer = timers[i];
                    if (timer.polledTime < timer.durationTime) {
                        timer.polledTime += this.deltaTime;
                        timers[i] = timer; // Modify
                    }
                }
            }
        }
    }
}
