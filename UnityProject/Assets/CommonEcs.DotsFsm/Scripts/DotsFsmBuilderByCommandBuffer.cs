using Unity.Collections;
using Unity.Entities;

#nullable enable

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// A utility struct that creates an FSM entity
    /// </summary>
    public struct DotsFsmBuilderByCommandBuffer {
        private EntityCommandBuffer commandBuffer;

        public DotsFsmBuilderByCommandBuffer(EntityCommandBuffer commandBuffer) {
            this.commandBuffer = commandBuffer;
        }

        public Entity CreateFsm(FixedString64 name) {
            Entity fsmEntity = this.commandBuffer.CreateEntity();
            this.commandBuffer.AddComponent(fsmEntity, new DotsFsm());
            this.commandBuffer.AddComponent(fsmEntity, new Name(name));
            this.commandBuffer.AddBuffer<Transition>(fsmEntity);
            this.commandBuffer.AddBuffer<LinkedEntityGroup>(fsmEntity);
            
            this.commandBuffer.AppendToBuffer(fsmEntity, new LinkedEntityGroup() {
                Value = fsmEntity
            });

            return fsmEntity;
        }

        public Entity AddState(Entity fsmEntityOwner, FixedString64 name) {
            Entity stateEntity = this.commandBuffer.CreateEntity();
            this.commandBuffer.AddComponent(stateEntity, new DotsFsmState(fsmEntityOwner));
            this.commandBuffer.AddComponent(stateEntity, new Name(name));
            
            // Link to actions
            this.commandBuffer.AddBuffer<LinkedEntityGroup>(stateEntity);
            
            // These are the actions
            this.commandBuffer.AddBuffer<EntityBufferElement>(stateEntity);
            
            // We added this so that we don't need to add the stateEntity when adding action entities
            this.commandBuffer.AppendToBuffer(stateEntity, new LinkedEntityGroup() {
                Value = stateEntity
            });
            
            // Link fsm owner to this state
            this.commandBuffer.AppendToBuffer(fsmEntityOwner, new LinkedEntityGroup() {
                Value = stateEntity
            });

            return stateEntity;
        }

        public Entity AddAction<T>(Entity stateEntity, T actionComponent) where T : struct, IComponentData {
            Entity actionEntity = this.commandBuffer.CreateEntity();
            this.commandBuffer.AddComponent(actionEntity, new DotsFsmAction(stateEntity));
            this.commandBuffer.AddComponent(actionEntity, actionComponent);
            
            // Link state owner to this action
            this.commandBuffer.AppendToBuffer(stateEntity, new LinkedEntityGroup() {
                Value = actionEntity
            });

            return actionEntity;
        }

        public void AddTransition(Entity fsmEntity, Entity fromState, FixedString64 eventId, Entity toState) {
            this.commandBuffer.AppendToBuffer(fsmEntity, new Transition(fromState, eventId, toState));
        }

        public void StartState(Entity stateEntity) {
            // Start the state by adding the StartState component to the state
            this.commandBuffer.AddComponent<StartState>(stateEntity);
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