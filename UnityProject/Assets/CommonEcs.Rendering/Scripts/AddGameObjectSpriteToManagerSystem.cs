using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// This is the same as AddSpriteToManagerSystem but adds the Sprite that are in GameObject world
    /// (Using Transform instead of TransformMatrix)
    /// </summary>
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(SpriteManagerInstancesSystem))]
    [UpdateAfter(typeof(AddGameObjectSpriteToLayerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AddGameObjectSpriteToManagerSystem : ComponentSystem {
        // Note here that we're not using a common Added component so that each manager knows what 
        // sprite to remove
        public struct Added : ISystemStateComponentData {
            // The entity of the sprite manager to where the sprite is added
            public readonly Entity spriteManagerEntity;
        
            // This is the managerIndex of the sprite so we can determine what index they are if they were somehow
            // removed
            public readonly int managerIndex;

            public Added(Entity spriteManagerEntity, int managerIndex) {
                this.spriteManagerEntity = spriteManagerEntity;
                this.managerIndex = managerIndex;
            }
        }

        private EntityQuery addedQuery;
        private EntityQuery removedQuery;

        private SpriteManagerInstancesSystem spriteManagers;

        private EntityQueryBuilder.F_ECD<Transform, Sprite> addedForEach;
        private EntityQueryBuilder.F_ED<Added> removedForEach;

        protected override void OnCreate() {
            this.addedQuery = GetEntityQuery(this.ConstructQuery(new ComponentType[] {
                typeof(Transform), typeof(Sprite)
            }, new ComponentType[] {
                typeof(Added), typeof(AddToSpriteLayer)
            }));

            this.removedQuery = GetEntityQuery(this.ConstructQuery(new ComponentType[] {
                typeof(Added)
            }, new ComponentType[] {
                typeof(Transform), typeof(Sprite)
            }));

            this.spriteManagers = this.World.GetOrCreateSystem<SpriteManagerInstancesSystem>();

            this.addedForEach = delegate(Entity entity, Transform transform, ref Sprite sprite) {
                if (sprite.spriteManagerEntity == Entity.Null) {
                    // The sprite manager entity might not have been set yet
                    // For example, when a prefab with SpriteWrapper is instantiated, it probably
                    // doesn't have its spriteManagerEntity value set yet.
                    // We skip it for now and process them in the next frame.
                    return;
                }
            
                Maybe<SpriteManager> maybeManager = this.spriteManagers.Get(sprite.spriteManagerEntity);
                float4x4 matrix = new float4x4(transform.rotation, transform.position);
                SpriteManager spriteManager = maybeManager.Value;
                spriteManager.Add(ref sprite, matrix);

                // Add this component so it will no longer be processed by this system
                this.PostUpdateCommands.AddComponent(entity,
                    new Added(sprite.spriteManagerEntity, sprite.managerIndex));
            
                // We add the shared component so that it can be filtered using such shared component
                // in other systems. For example, in SortRenderOrderSystem.
                this.PostUpdateCommands.AddSharedComponent(entity, spriteManager);
                
                if (spriteManager.AlwaysUpdateMesh) {
                    // We add this component so it will be excluded in IdentifySpriteManagerChangedSystem
                    this.PostUpdateCommands.AddComponent(entity, new AlwaysUpdateMesh());
                }
            };

            this.removedForEach = delegate(Entity entity, ref Added added) {
                Maybe<SpriteManager> maybeManager = this.spriteManagers.Get(added.spriteManagerEntity);
                if (maybeManager.HasValue) {
                    maybeManager.Value.Remove(added.managerIndex);
                }

                this.PostUpdateCommands.RemoveComponent<Added>(entity);
            };
        }

        protected override void OnUpdate() {
            // Process added
            this.Entities.With(this.addedQuery).ForEach(this.addedForEach);
            
            // Process removed
            this.Entities.With(this.removedQuery).ForEach(this.removedForEach);
        }
    }
}
