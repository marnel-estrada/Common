using Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateInGroup(typeof(ScalableTimeSystemGroup))]
    public partial struct MoveBySpeedSystem : ISystem {
        private EntityQuery query;

        private ComponentTypeHandle<MoveBySpeed> moveBySpeedType;
        private ComponentTypeHandle<LocalTransform> transformType;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<MoveBySpeed>()
                .WithAll<LocalTransform>().Build(ref state);

            this.moveBySpeedType = state.GetComponentTypeHandle<MoveBySpeed>();
            this.transformType = state.GetComponentTypeHandle<LocalTransform>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            this.moveBySpeedType.Update(ref state);
            this.transformType.Update(ref state);

            MoveJob moveJob = new() {
                moveBySpeedType = this.moveBySpeedType,
                transformType = this.transformType,
                deltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = moveJob.ScheduleParallel(this.query, state.Dependency);
        }
        
        [BurstCompile]
        private struct MoveJob : IJobChunk {
            public ComponentTypeHandle<MoveBySpeed> moveBySpeedType;
            public ComponentTypeHandle<LocalTransform> transformType;

            public float deltaTime;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<MoveBySpeed> moveBySpeeds = chunk.GetNativeArray(ref this.moveBySpeedType);
                NativeArray<LocalTransform> transforms = chunk.GetNativeArray(ref this.transformType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    MoveBySpeed moveBySpeed = moveBySpeeds[i];
                    LocalTransform transform = transforms[i];
                    
                    moveBySpeed.timer.Update(this.deltaTime);
                    moveBySpeeds[i] = moveBySpeed; // Modify
                    if (moveBySpeed.timer.HasElapsed) {
                        // Movement is done. Snap position to destination.
                        // Note that we don't disable MoveBySpeed component here
                        // This is so that other systems can check if the movement is done
                        // and they might do something after the movement. They can then choose
                        // to disable this component.
                        transforms[i] = transform.WithPosition(moveBySpeed.destinationPos); // Modify
                        continue;
                    }
                    
                    // Lerp position using the ratio on the timer
                    float3 newPosition = math.lerp(moveBySpeed.startPos, moveBySpeed.destinationPos,
                        moveBySpeed.timer.duration);
                    transforms[i] = transform.WithPosition(newPosition); // Modify
                }
            }
        }
    }
}