using Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace CommonEcs {
    public class SpriteInLayerHandle {
        private Entity spriteLayerEntity; // The entity of the layer where the sprite would be added

        private Entity spriteEntity;

        public SpriteInLayerHandle(Entity spriteLayerEntity) {
            this.spriteLayerEntity = spriteLayerEntity;
            Assertion.IsTrue(this.spriteLayerEntity != Entity.Null);
        }

        public void Init(Entity spriteLayerEntity) {
            this.spriteLayerEntity = spriteLayerEntity;
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
        public void Create(ref Sprite sprite, in float3 translation, in quaternion rotation, bool isStatic, float sortOrderOffset = 0) {
            // Avoid creating a new sprite entity when another one exists
            Assertion.IsTrue(!this.Exists);
            Assertion.IsTrue(this.spriteLayerEntity != Entity.Null);

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            this.spriteEntity = entityManager.CreateEntity();
            
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);
            
            buffer.AddComponent(this.spriteEntity, sprite);
            
            buffer.AddComponent(this.spriteEntity, LocalTransform.FromPositionRotation(translation, rotation));
            
            buffer.AddComponent(this.spriteEntity, new LocalToWorld());

            buffer.AddComponent(this.spriteEntity, new UseYAsSortOrder(sortOrderOffset));

            buffer.AddComponent(this.spriteEntity, new AddToSpriteLayer() {
                layerEntity = this.spriteLayerEntity
            });

            if (isStatic) {
                buffer.AddComponent(this.spriteEntity, new Static());
            }
            
            buffer.Playback(entityManager);
            buffer.Dispose();
        }

        /// <summary>
        /// Pulls a copy of the sprite from the entity
        /// </summary>
        /// <returns></returns>
        public Sprite Pull() {
            // Can only pull if sprite was indeed created
            if (!this.Exists) {
                return default;
            }
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            return entityManager.GetComponentData<Sprite>(this.spriteEntity);
        }

        /// <summary>
        /// Pushes the values of the specified sprite to the entity
        /// </summary>
        /// <param name="sprite"></param>
        public void Push(ref Sprite sprite) {
            // Can only push if sprite was indeed created
            if (!this.Exists) {
                return;
            }
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.SetComponentData(this.spriteEntity, sprite);
        }

        /// <summary>
        /// Destroys the sprite entity
        /// </summary>
        public void Destroy() {
            // Destroy only if it was created
            if (this.Exists) {
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                entityManager.DestroyEntity(this.spriteEntity);
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