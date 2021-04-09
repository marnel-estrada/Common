using Unity.Collections;
using Unity.Entities;

#nullable enable

namespace CommonEcs.DotsFsm {
    public struct DotsFsmBuilderByEntityManager {
        private EntityManager entityManager;

        private readonly EntityArchetype fsmArchetype;
        private readonly EntityArchetype stateArchetype;

        public DotsFsmBuilderByEntityManager(ref EntityManager entityManager) {
            this.entityManager = entityManager;
            
            this.fsmArchetype = this.entityManager.CreateArchetype(typeof(DotsFsm), 
                typeof(Name), typeof(Transition), typeof(LinkedEntityGroup));
            
            this.stateArchetype = this.entityManager.CreateArchetype(typeof(DotsFsmState), 
                typeof(Name), typeof(LinkedEntityGroup), typeof(EntityBufferElement));
        }
        
        public Entity CreateFsm(FixedString64 name) {
            Entity fsmEntity = this.entityManager.CreateEntity(this.fsmArchetype);
            this.entityManager.SetComponentData(fsmEntity, new Name(name));

            DynamicBuffer<LinkedEntityGroup> linkedEntities = this.entityManager.GetBuffer<LinkedEntityGroup>(fsmEntity);
            linkedEntities.Add(new LinkedEntityGroup() {
                Value = fsmEntity
            });

            return fsmEntity;
        }
        
        public Entity AddState(Entity fsmOwnerEntity, FixedString64 name) {
            Entity stateEntity = this.entityManager.CreateEntity(this.stateArchetype);
            this.entityManager.SetComponentData(stateEntity, new DotsFsmState(fsmOwnerEntity));
            this.entityManager.SetComponentData(stateEntity, new Name(name));
            
            // We added this so that we don't need to add the stateEntity when adding action entities
            DynamicBuffer<LinkedEntityGroup> stateLinkedEntities = this.entityManager.GetBuffer<LinkedEntityGroup>(stateEntity);
            stateLinkedEntities.Add(new LinkedEntityGroup() {
                Value = stateEntity
            });
            
            DynamicBuffer<LinkedEntityGroup> fsmLinkedEntities = this.entityManager.GetBuffer<LinkedEntityGroup>(fsmOwnerEntity);
            fsmLinkedEntities.Add(new LinkedEntityGroup() {
                Value = stateEntity
            });

            return stateEntity;
        }
        
        public Entity AddAction<T>(in Entity fsmEntity, in Entity stateEntity, in T actionComponent) where T : struct, IComponentData {
            Entity actionEntity = this.entityManager.CreateEntity(typeof(DotsFsmAction), 
                typeof(T), typeof(LinkedEntityGroup));
            this.entityManager.SetComponentData(actionEntity, new DotsFsmAction(fsmEntity, stateEntity));
            this.entityManager.SetComponentData(actionEntity, actionComponent);
            
            // Link state owner to this action
            DynamicBuffer<LinkedEntityGroup> stateLinkedEntities = this.entityManager.GetBuffer<LinkedEntityGroup>(stateEntity);
            stateLinkedEntities.Add(new LinkedEntityGroup() {
                Value = actionEntity
            });

            return actionEntity;
        }
        
        public void AddTransition(in Entity fsmEntity, in Entity fromState, in FsmEvent fsmEvent, in Entity toState) {
            DynamicBuffer<Transition> transitions = this.entityManager.GetBuffer<Transition>(fsmEntity);
            transitions.Add(new Transition(fromState, fsmEvent, toState));
        }
        
        public void AddTransition(in Entity fsmEntity, in Entity fromState, in FixedString64 eventAsString, in Entity toState) {
            AddTransition(fsmEntity, fromState, new FsmEvent(eventAsString), toState);
        }
        
        public void Start(Entity fsmEntity, Entity stateEntity) {
            DotsFsm dotsFsm = new DotsFsm(stateEntity);
            this.entityManager.SetComponentData(fsmEntity, dotsFsm);
        }
    }
}