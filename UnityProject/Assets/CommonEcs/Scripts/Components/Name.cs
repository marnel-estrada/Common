using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A common component for named entities
    /// </summary>
    public readonly struct Name : IComponentData {
        public readonly FixedString64 value;

        public Name(FixedString64 value) {
            this.value = value;
        }

        /// <summary>
        /// We do it this way to avoid garbage when specifying a params[] of ComponentType passed
        /// in EntityManager.CreateEntity().
        /// </summary>
        private static readonly ComponentType[] NAME_ARCHETYPE = {
            typeof(Name)
        };

        /// <summary>
        /// Utility method for setting a name
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        public static void SetupName(ref EntityManager entityManager, Entity owner, FixedString64 name) {
            entityManager.SetName(owner, name.ToString());
            
            Entity nameEntity = entityManager.CreateEntity(NAME_ARCHETYPE);
            entityManager.SetComponentData(nameEntity, new Name(name));
            
            // Set the reference to owner
            // This assumes that the owner has NameReference in its archetype
            entityManager.SetComponentData(owner, new NameReference(nameEntity));
            
            // It also assumes that the owner entity has LinkedEntityGroup in its archetype
            DynamicBuffer<LinkedEntityGroup> linkedEntities = entityManager.GetBuffer<LinkedEntityGroup>(owner);
            
            // Ensure that the first entity of linked entities is the owner
            if (linkedEntities.Length == 0) {
                linkedEntities.Add(new LinkedEntityGroup() {
                    Value = owner
                });    
            }
            
            linkedEntities.Add(new LinkedEntityGroup() {
                Value = nameEntity
            });
        }

        public static void SetupName(ref EntityCommandBuffer commandBuffer, Entity owner, FixedString64 name) {
            Entity nameEntity = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(nameEntity, new Name(name));
            
            // Set the reference to owner
            // This assumes that the owner has NameReference in its archetype
            commandBuffer.SetComponent(owner, new NameReference(nameEntity));
            
            // It also assumes that the owner entity has LinkedEntityGroup in its archetype
            // and that the owner itself is already added to the list
            commandBuffer.AppendToBuffer(owner, new LinkedEntityGroup() {
                Value = nameEntity
            });
        }
    }
}