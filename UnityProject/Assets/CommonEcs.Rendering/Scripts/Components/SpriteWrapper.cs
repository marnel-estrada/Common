using System;
using Common;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A component that we can use in GameObject to create ECS Sprite instances
    /// </summary>
    [Obsolete]
    public class SpriteWrapper : MonoBehaviour {
        private Entity spriteManagerEntity;

        // We did it this way because we need a copy of the sprite when it is recreated
        [SerializeField]
        private Sprite sprite;

        [SerializeField]
        public float2 pivot = new float2(0.5f, 0.5f);

        // private GameObjectEntity goEntity;
        
        private EntityManager entityManager;

        private void Awake() {
            //this.goEntity = this.GetRequiredComponent<GameObjectEntity>();
            
            this.entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Assertion.NotNull(this.entityManager);
        }

        public Sprite Sprite {
            get {
                return this.sprite;
            }
        }

        public int LayerOrder {
            get {
                return this.sprite.LayerOrder;
            }
            set {
                this.sprite.LayerOrder = value;
            }
        }

        public float Width {
            get {
                return this.sprite.width;
            }
            set {
                this.sprite.width = value;
            }
        }

        public float Height {
            get {
                return this.sprite.height;
            }
            set {
                this.sprite.height = value;
            }
        }

        public Color Color {
            get {
                return this.sprite.color;
            }
            set {
                this.sprite.color = value;
            }
        }

        public Entity SpriteManagerEntity {
            get {
                return this.spriteManagerEntity;
            }
            set {
                this.spriteManagerEntity = value;
            }
        }

        public float RenderOrder {
            get {
                return this.sprite.RenderOrder;
            }

            set {
                this.sprite.RenderOrder = value;
            }
        }

        /// <summary>
        /// Sets the first UV
        /// </summary>
        /// <param name="lowerLeftUv"></param>
        /// <param name="uvDimension"></param>
        public void SetUv(float2 lowerLeftUv, float2 uvDimension) {
            this.sprite.SetUv(lowerLeftUv, uvDimension);
        }

        /// <summary>
        /// Sets the second UV
        /// </summary>
        /// <param name="lowerLeftUv2"></param>
        /// <param name="uvDimension2"></param>
        public void SetUv2(float2 lowerLeftUv2, float2 uvDimension2) {
            this.sprite.SetUv2(lowerLeftUv2, uvDimension2);
        }

        /// <summary>
        /// Synchronizes the sprite from the ECS Sprite Manager
        /// </summary>
        public void SyncSprite() {
            // Entity entity = this.goEntity.Entity;
            // if (entity != Entity.Null && this.entityManager.HasComponent<Sprite>(entity)) {
            //     // This means that the sprite is already added in ECS sprite manager
            //     this.sprite = this.entityManager.GetComponentData<Sprite>(entity);
            // }
        }

        /// <summary>
        /// Commits the sprite unto the one in ECS Sprite Manager
        /// </summary>
        public void Commit() {
            // Entity entity = this.goEntity.Entity;
            // if (entity != Entity.Null && this.entityManager.HasComponent<Sprite>(entity)) {
            //     this.entityManager.SetComponentData(entity, this.sprite);
            // }
        }
    }
}
