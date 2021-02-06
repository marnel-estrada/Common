using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace Common.Ecs.DotsFsm {
    /// <summary>
    /// A utility struct that creates an FSM entity
    /// </summary>
    public struct DotsFsmBuilder {
        private EntityCommandBuffer commandBuffer;

        public DotsFsmBuilder(EntityCommandBuffer commandBuffer) {
            this.commandBuffer = commandBuffer;
        }

        public Entity CreateFsm(FixedString64 name) {
            Entity entity = this.commandBuffer.CreateEntity();
            this.commandBuffer.AddComponent(entity, new DotsFsm());
            this.commandBuffer.AddComponent(entity, new Name(name));
            this.commandBuffer.AddBuffer<Transition>(entity);

            return entity;
        }

        public Entity AddState(Entity fsmEntityOwner, FixedString64 name) {
            Entity entity = this.commandBuffer.CreateEntity();
            this.commandBuffer.AddComponent(entity, new DotsFsmState(fsmEntityOwner));
            this.commandBuffer.AddComponent(entity, new Name(name));
            
            // These are the actions
            this.commandBuffer.AddBuffer<EntityBufferElement>(entity);

            return entity;
        }

        public Entity AddAction<T>(Entity stateEntity, T actionComponent) where T : struct, IComponentData {
            Entity entity = this.commandBuffer.CreateEntity();
            this.commandBuffer.AddComponent(entity, new DotsFsmAction(stateEntity));
            this.commandBuffer.AddComponent(entity, actionComponent);

            return entity;
        }

        /// <summary>
        /// We use a list of transitions here so that they will be added in one go without
        /// resetting the dynamic buffer of Transitions. Calling SetBuffer() clears all the items
        /// in that buffer.
        /// </summary>
        /// <param name="fsmEntity"></param>
        /// <param name="transitions"></param>
        public void AddTransitions(Entity fsmEntity, NativeArray<Transition> transitions) {
            DynamicBuffer<Transition> fsmTransitions = this.commandBuffer.SetBuffer<Transition>(fsmEntity);
            for (int i = 0; i < transitions.Length; ++i) {
                fsmTransitions.Add(transitions[i]);
            }
        }

        /// <summary>
        /// Used when the underlying commandBuffer is from an EntityManager
        /// </summary>
        /// <param name="entityManager"></param>
        public void Build(ref EntityManager entityManager) {
            this.commandBuffer.Playback(entityManager);
        }
    }
}