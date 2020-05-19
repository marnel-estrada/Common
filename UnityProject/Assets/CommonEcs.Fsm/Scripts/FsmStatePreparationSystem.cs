using System;
using System.Collections.Generic;

using CommonEcs;

using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// A common base class for state action preparation systems
    /// T here is the component tag used to filter states that belong to a certain FSM (domain related)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [UpdateAfter(typeof(FsmConsumeEventSystem))]
    [UpdateBefore(typeof(FsmActionStartSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class FsmStatePreparationSystem<T> : TemplateComponentSystem
        where T : struct, IComponentData {
        private ArchetypeChunkComponentType<FsmState> stateType;

        protected delegate void StatePrepareAction(ref Entity stateEntity);

        protected readonly Dictionary<byte, StatePrepareAction> statePrepareMap = new Dictionary<byte, StatePrepareAction>();

        private FsmBuilder fsmBuilder;

        protected FsmBuilder FsmBuilder {
            get {
                if (this.fsmBuilder == null) {
                    this.fsmBuilder = new FsmBuilder(this.EntityManager);
                }

                return this.fsmBuilder;
            }
        }

        /// <summary>
        /// Adds a prepare action
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="action"></param>
        protected void AddPrepareAction(byte stateId, StatePrepareAction action) {
            Assertion.IsTrue(!this.statePrepareMap.ContainsKey(stateId)); // Prevent replace of prepare action
            this.statePrepareMap[stateId] = action;
        }
        
        /// <summary>
        /// Routines for preparing the specified state
        /// </summary>
        /// <param name="state"></param>
        protected virtual void Prepare(ref FsmState state) {
            // The preparation action must exist for the specified state
            StatePrepareAction prepareAction = this.statePrepareMap[state.stateId];
            prepareAction(ref state.entityOwner); // Invoke the action
        }

        /// <summary>
        /// Utility method for adding an action
        /// </summary>
        /// <param name="stateEntity"></param>
        protected void AddAction(ref Entity stateEntity) {
            this.FsmBuilder.AddAction(this.PostUpdateCommands, stateEntity);
        }

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(FsmState), typeof(StateJustTransitioned), typeof(T));            
        }

        protected override void BeforeChunkTraversal() {
            this.stateType = GetArchetypeChunkComponentType<FsmState>();
        }

        private NativeArray<FsmState> states;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.states = chunk.GetNativeArray(this.stateType);
        }

        protected override void Process(int index) {
            FsmState state = this.states[index];
            Prepare(ref state);

            // Remove StateJustTransitioned so it will not be processed again
            this.PostUpdateCommands.RemoveComponent<StateJustTransitioned>(state.entityOwner);
        }
    }
}
