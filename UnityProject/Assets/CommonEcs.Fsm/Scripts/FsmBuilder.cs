using Unity.Entities;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// A utility class that helps build FSM graphs
    /// </summary>
    public class FsmBuilder {
        private readonly EntityManager entityManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityManager"></param>
        public FsmBuilder(EntityManager entityManager) {
            this.entityManager = entityManager;
        }

        /// <summary>
        /// Creates an entity with the fsm
        /// </summary>
        /// <returns></returns>
        public Entity CreateFsm(ref Entity owner) {
            Entity entity = this.entityManager.CreateEntity();

            Fsm fsm = new Fsm();
            fsm.owner = owner;
            fsm.currentState = Entity.Null;
            fsm.currentEvent = Fsm.NULL_EVENT;
            this.entityManager.AddComponentData(entity, fsm);

            this.entityManager.AddComponent(entity, ComponentType.ReadWrite<FsmTransition>());

            return entity;
        }

        /// <summary>
        /// Adds a state to the specified FSM
        /// </summary>
        /// <param name="fsmOwner"></param>
        /// <returns></returns>
        public Entity AddState<T>(Entity fsmOwner, byte stateId) where T : struct, IComponentData {
            Entity stateEntity = this.entityManager.CreateEntity();

            FsmState state = new FsmState();
            state.entityOwner = stateEntity;
            state.fsmOwner = fsmOwner;
            state.stateId = stateId;
            this.entityManager.AddComponentData(stateEntity, state);

            // Add the tag component to group such state
            this.entityManager.AddComponentData(stateEntity, new T());

            return stateEntity;
        }

        /// <summary>
        /// Adds an action to the specified state
        /// </summary>
        /// <param name="stateOwnerEntity"></param>
        /// <returns></returns>
        public void AddAction(EntityCommandBuffer postCommandBuffer, Entity stateOwnerEntity) {
            postCommandBuffer.CreateEntity();

            FsmAction action = new FsmAction();
            action.stateOwner = stateOwnerEntity;
            postCommandBuffer.AddComponent(action);
        }

        /// <summary>
        /// Adds a transition to an FSM
        /// </summary>
        /// <param name="fsmOwner"></param>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        /// <param name="transitionEvent"></param>
        /// <returns></returns>
        public void AddTransition(Entity fsmOwner, Entity fromState, uint transitionEvent, Entity toState) {
            DynamicBuffer<FsmTransition> transitions = this.entityManager.GetBuffer<FsmTransition>(fsmOwner);

            FsmTransition transition = new FsmTransition {
                fsmOwner = fsmOwner,
                fromState = fromState,
                toState = toState,
                transitionEvent = transitionEvent
            };

            Assertion.Assert(transition.transitionEvent != Fsm.NULL_EVENT);
            
            transitions.Add(transition);
        }

        /// <summary>
        /// Starts the FSM with the specified state
        /// </summary>
        /// <param name="fsm"></param>
        /// <param name="state"></param>
        public void Start(Entity fsm, Entity state) {
            this.entityManager.AddComponentData(state, new StartState());
        }
    }
}
