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

        public Entity CreateFsm(in FixedString64Bytes name, bool isDebug = false) {
            Entity fsmEntity = this.commandBuffer.CreateEntity();
            this.commandBuffer.AddComponent(fsmEntity, new DotsFsm());
            this.commandBuffer.AddComponent(fsmEntity, new DebugFsm(isDebug));
            this.commandBuffer.AddComponent<NameReference>(fsmEntity);
            this.commandBuffer.AddBuffer<Transition>(fsmEntity);
            this.commandBuffer.AddBuffer<LinkedEntityGroup>(fsmEntity);
            
            this.commandBuffer.AppendToBuffer(fsmEntity, new LinkedEntityGroup() {
                Value = fsmEntity
            });
            
            Name.SetupName(ref this.commandBuffer, fsmEntity, name);

            return fsmEntity;
        }

        public Entity AddState(Entity fsmEntityOwner, in FixedString64Bytes name) {
            Entity stateEntity = this.commandBuffer.CreateEntity();
            this.commandBuffer.AddComponent(stateEntity, new DotsFsmState(fsmEntityOwner));
            this.commandBuffer.AddComponent<NameReference>(stateEntity);
            
            // Link to actions
            this.commandBuffer.AddBuffer<LinkedEntityGroup>(stateEntity);
            
            // These are the actions
            this.commandBuffer.AddBuffer<EntityBufferElement>(stateEntity);
            
            // We added this so that we don't need to add the stateEntity when adding action entities
            this.commandBuffer.AppendToBuffer(stateEntity, new LinkedEntityGroup() {
                Value = stateEntity
            });
            
            Name.SetupName(ref this.commandBuffer, stateEntity, name);
            
            // Link fsm owner to this state
            this.commandBuffer.AppendToBuffer(fsmEntityOwner, new LinkedEntityGroup() {
                Value = stateEntity
            });

            return stateEntity;
        }

        /// <summary>
        /// We need the FSM entity to denormalize the data into DotsFsmAction
        /// This is so that we don't need to look it up from the stateOwner if we want
        /// to know the owner fsm directly
        /// </summary>
        /// <param name="fsmEntity"></param>
        /// <param name="stateEntity"></param>
        /// <param name="actionComponent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Entity AddAction<T>(Entity fsmEntity, Entity stateEntity, T actionComponent) where T : struct, IComponentData {
            Entity actionEntity = this.commandBuffer.CreateEntity();
            this.commandBuffer.AddComponent(actionEntity, new DotsFsmAction(fsmEntity, stateEntity));
            this.commandBuffer.AddComponent(actionEntity, actionComponent);
            
            // Link state owner to this action
            this.commandBuffer.AppendToBuffer(stateEntity, new LinkedEntityGroup() {
                Value = actionEntity
            });

            return actionEntity;
        }

        public void AddTransition(in Entity fsmEntity, in Entity fromState, in FsmEvent fsmEvent, in Entity toState) {
            this.commandBuffer.AppendToBuffer(fsmEntity, new Transition(fromState, fsmEvent, toState));
        }
        
        public void AddTransition(in Entity fsmEntity, in Entity fromState, in FixedString64Bytes eventAsString, in Entity toState) {
            AddTransition(fsmEntity, fromState, new FsmEvent(eventAsString), toState);
        }

        /// <summary>
        /// Used when the underlying commandBuffer is from an EntityManager
        /// </summary>
        /// <param name="entityManager"></param>
        public void Build(ref EntityManager entityManager) {
            this.commandBuffer.Playback(entityManager);
        }

        /// <summary>
        /// Starts the specified FSM with the specified state
        /// </summary>
        /// <param name="fsmEntity"></param>
        /// <param name="stateEntity"></param>
        public void Start(Entity fsmEntity, Entity stateEntity) {
            DotsFsm dotsFsm = new DotsFsm(stateEntity);
            this.commandBuffer.SetComponent(fsmEntity, dotsFsm);
        }
    }
}