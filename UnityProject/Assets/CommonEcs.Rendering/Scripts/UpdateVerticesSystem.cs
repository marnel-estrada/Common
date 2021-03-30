using System.Collections.Generic;

using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// Common base class for systems that updates vertices
    /// </summary>
    [UpdateAfter(typeof(TransformGameObjectSpriteVerticesSystem))]
    [UpdateAfter(typeof(TransformVerticesSystem))]
    [UpdateAfter(typeof(IdentifySpriteManagerChangedSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateBefore(typeof(SpriteManagerJobsFinisher))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class UpdateVerticesSystem : JobComponentSystem {        
        private SharedComponentQuery<SpriteManager> spriteManagerQuery;

        private EntityQuery query;
        private ComponentTypeHandle<Sprite> spriteType;

        protected override void OnCreate() {
            this.spriteManagerQuery = new SharedComponentQuery<SpriteManager>(this, this.EntityManager);
            this.query = ResolveQuery(); 
        }

        /// <summary>
        /// This is to be overriden by deriving class to provide the ComponentGroup with some filtering
        /// </summary>
        /// <returns></returns>
        protected abstract EntityQuery ResolveQuery();
        
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            this.spriteManagerQuery.Update();
            IReadOnlyList<SpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;

            this.spriteType = GetComponentTypeHandle<Sprite>(true);
            
            JobHandle lastHandle = inputDeps;
            
            // Note here that we start counting from 1 since the first entry is always a default one
            // In this case, SpriteManager.internal has not been allocated. So we get a NullPointerException
            // if we try to access the default entry at 0.
            for (int i = 1; i < spriteManagers.Count; ++i) {
                SpriteManager manager = spriteManagers[i];

                if (manager.Owner == Entity.Null) {
                    // Note that the owner of a SpriteManager might be set in SetOwnerToSpriteManagerSystem
                    // If it's null, then it hasn't been assigned yet
                    continue;
                }

                if (!ShouldProcess(manager)) {
                    continue;
                }

                this.query.SetSharedComponentFilter(manager);
    
                UpdateVerticesJob job = new UpdateVerticesJob() {
                    spriteType = this.spriteType,
                    vertices = manager.NativeVertices,
                    uv = manager.NativeUv,
                    uv2 = manager.NativeUv2,
                    colors = manager.NativeColors
                };

                lastHandle = job.Schedule(this.query, lastHandle);
            }
            
            return lastHandle;
        }

        /// <summary>
        /// To be overridden by deriving class to filter which SpriteManager to process
        /// </summary>
        /// <param name="spriteManager"></param>
        /// <returns></returns>
        protected abstract bool ShouldProcess(in SpriteManager spriteManager);
    }
}