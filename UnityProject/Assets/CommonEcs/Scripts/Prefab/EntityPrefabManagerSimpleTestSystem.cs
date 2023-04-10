using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace CommonEcs {
    public partial class EntityPrefabManagerSimpleTestSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem barrier;
 
        protected override void OnCreate() {
            this.barrier = this.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }
 
        protected override void OnUpdate() {
            // Left click
            if (Input.GetMouseButtonDown(0)) {
                // Instantiate 1
                if (!SystemAPI.TryGetSingleton(out EntityPrefabManager prefabManager)) {
                    // No PrefabManager found. Maybe it's not prepared.
                    return;
                }
                
                ValueTypeOption<Entity> prefab = prefabManager.GetPrefab("SamplePrefab");
                if (prefab.IsSome) {
                    this.EntityManager.Instantiate(prefab.value);
                }
                Debug.Log("Instantiated Test");
            }
         
            // Right click
            if (Input.GetMouseButtonDown(1)) {
                // Instantiate using a job
                this.Dependency = new InstantiateJob() {
                    commandBuffer = this.barrier.CreateCommandBuffer(),
                    prefabManager = SystemAPI.GetSingleton<EntityPrefabManager>(),
                    prefabId = "Cube"
                }.Schedule(this.Dependency);
             
                this.barrier.AddJobHandleForProducer(this.Dependency);
            }
        }
     
        [BurstCompile]
        private struct InstantiateJob : IJob {
            public EntityCommandBuffer commandBuffer;
            public EntityPrefabManager prefabManager;
            public FixedString64Bytes prefabId;
         
            public void Execute() {
                ValueTypeOption<Entity> entityPrefab = this.prefabManager.GetPrefab(this.prefabId);
                if (entityPrefab.IsSome) {
                    this.commandBuffer.Instantiate(entityPrefab.value);
                }
            }
        }
    }
}