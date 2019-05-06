using Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace CommonEcs {
    public class SpriteInLayerHandle {
        private readonly EntityManager entityManager;
        private readonly Entity spriteLayerEntity; // The entity of the layer where the sprite would be added

        private Entity spriteEntity;

        public SpriteInLayerHandle(EntityManager entityManager, Entity spriteLayerEntity) {
            this.entityManager = entityManager;
            Assertion.AssertNotNull(this.entityManager);

            this.spriteLayerEntity = spriteLayerEntity;
            Assertion.Assert(this.spriteLayerEntity != Entity.Null);
        }

        public Entity SpriteEntity {
            get {
                return this.spriteEntity;
            }
        }

        /// <summary>
        /// Creates the sprite entity with the specified sprite data
        /// </summary>
        /// <param name="sprite"></param>
        public void Create(ref Sprite sprite, float3 translation, quaternion rotation, bool isStatic) {
            // Avoid creating a new sprite entity when another one exists
            Assertion.Assert(!this.Exists);
            Assertion.Assert(this.spriteLayerEntity != Entity.Null);

            this.spriteEntity = this.entityManager.CreateEntity();
            
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);
            
            buffer.AddComponent(this.spriteEntity, sprite);

            buffer.AddComponent(this.spriteEntity, new Translation() {
                Value = translation
            });
            
            buffer.AddComponent(this.spriteEntity, new Rotation() {
                Value = rotation
            });

            buffer.AddComponent(this.spriteEntity, new UseYAsSortOrder());

            buffer.AddComponent(this.spriteEntity, new AddToSpriteLayer() {
                layerEntity = this.spriteLayerEntity
            });

            if (isStatic) {
                buffer.AddComponent(this.spriteEntity, new Static());
            }
            
            buffer.Playback(this.entityManager);
            buffer.Dispose();
        }

        /// <summary>
        /// Pulls a copy of the sprite from the entity
        /// </summary>
        /// <returns></returns>
        public Sprite Pull() {
            // Can only pull if sprite was indeed created
            Assertion.Assert(this.Exists);

            return this.entityManager.GetComponentData<Sprite>(this.spriteEntity);
        }

        /// <summary>
        /// Pushes the values of the specified sprite to the entity
        /// </summary>
        /// <param name="sprite"></param>
        public void Push(ref Sprite sprite) {
            // Can only push if sprite was indeed created
            Assertion.Assert(this.Exists);
            this.entityManager.SetComponentData(this.spriteEntity, sprite);
        }

        /// <summary>
        /// Destroys the sprite entity
        /// </summary>
        public void Destroy() {
            // Destroy only if it was created
            if (this.Exists) {
                this.entityManager.DestroyEntity(this.spriteEntity);
                this.spriteEntity = Entity.Null;
            }
        }

        public bool Exists {
            get {
                return this.spriteEntity != Entity.Null;
            }
        }
    }
}