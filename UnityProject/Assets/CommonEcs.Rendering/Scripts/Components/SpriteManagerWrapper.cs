using Common;

using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    
    /// <summary>
    /// A wrapper of SpriteManager in ECS such that we can use it as MonoBehaviour
    /// </summary>
    public class SpriteManagerWrapper : MonoBehaviour {
        [SerializeField]
        private int allocationCount = 1000;

        [SerializeField]
        private Material material;

        [SerializeField]
        private string sortingLayerName;

        [SerializeField]
        private bool alwaysUpdateMesh;
        
        private Entity entity;
        
        private EntityManager entityManager;

        private void Awake() {
            Assertion.NotNull(this.material);
            
            this.entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Assertion.NotNull(this.entityManager);
            
            this.entity = this.entityManager.CreateEntity();

            // Prepare a SpriteManager
            SpriteManager spriteManager = new SpriteManager(this.allocationCount);
            spriteManager.Owner = this.entity;
            spriteManager.SetMaterial(this.material);
            spriteManager.Layer = this.gameObject.layer;
            spriteManager.SortingLayer = SortingLayer.GetLayerValueFromName(this.sortingLayerName); 
            spriteManager.AlwaysUpdateMesh = this.alwaysUpdateMesh;
            this.entityManager.AddSharedComponentData(this.entity, spriteManager);
        }
        
        public Entity Entity {
            get {
                return this.entity;
            }
        }
    }
}
