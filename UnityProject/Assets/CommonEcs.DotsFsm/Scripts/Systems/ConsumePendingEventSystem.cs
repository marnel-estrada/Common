using Common;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace CommonEcs.DotsFsm {
    [UpdateInGroup(typeof(DotsFsmSystemGroup))]
    [UpdateAfter(typeof(SendEventFromActionsToFsmSystem))]
    public class ConsumePendingEventSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(DotsFsm), ComponentType.ReadOnly<NameReference>(), 
                ComponentType.ReadOnly<Transition>(), ComponentType.ReadOnly<DebugFsm>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ConsumeJob consumeJob = new ConsumeJob() {
                fsmType = GetComponentTypeHandle<DotsFsm>(),
                nameReferenceType = GetComponentTypeHandle<NameReference>(),
                transitionType = GetBufferTypeHandle<Transition>(),
                debugType = GetComponentTypeHandle<DebugFsm>(true),
                allNameReferences = GetComponentDataFromEntity<NameReference>(true),
                allNames = GetComponentDataFromEntity<Name>(true)
            };
            
            return consumeJob.ScheduleParallel(this.query, 1, inputDeps);
        }
        
        [BurstCompile]
        private struct ConsumeJob : IJobEntityBatch {
            public ComponentTypeHandle<DotsFsm> fsmType;
            
            [ReadOnly]
            public ComponentTypeHandle<NameReference> nameReferenceType;

            [ReadOnly]
            public BufferTypeHandle<Transition> transitionType;

            [ReadOnly]
            public ComponentTypeHandle<DebugFsm> debugType;
            
            [ReadOnly]
            public ComponentDataFromEntity<NameReference> allNameReferences;

            [ReadOnly]
            public ComponentDataFromEntity<Name> allNames;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<DotsFsm> fsms = batchInChunk.GetNativeArray(this.fsmType);
                NativeArray<NameReference> nameReferences = batchInChunk.GetNativeArray(this.nameReferenceType);
                BufferAccessor<Transition> transitionLists = batchInChunk.GetBufferAccessor(this.transitionType);
                NativeArray<DebugFsm> debugList = batchInChunk.GetNativeArray(this.debugType);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    DotsFsm fsm = fsms[i];
                    NameReference nameReference = nameReferences[i];
                    DynamicBuffer<Transition> transitions = transitionLists[i];
                    
                    if (fsm.pendingEvent.IsNone) {
                        // No event to consume
                        continue;
                    }

                    // The current state will be updated in the matcher
                    fsm = fsm.currentState.Match<TryChangeState, DotsFsm>(new TryChangeState() {
                        fsm = fsm,
                        fsmName = this.allNames[nameReference.nameEntity].value,
                        fsmEvent = fsm.pendingEvent.ValueOr(default),
                        transitions = transitions,
                        allNameReferences = this.allNameReferences,
                        allNames = this.allNames,
                        isDebug = debugList[i].isDebug
                    });
                    
                    // Modify
                    fsms[i] = fsm;
                }
            }
        }

        private struct TryChangeState : IFuncOptionMatcher<Entity, DotsFsm> {
            public DotsFsm fsm;
            public FixedString64 fsmName;
            public FsmEvent fsmEvent;
            
            [ReadOnly]
            public DynamicBuffer<Transition> transitions;

            [ReadOnly]
            public ComponentDataFromEntity<NameReference> allNameReferences;
            
            [ReadOnly]
            public ComponentDataFromEntity<Name> allNames;

            public bool isDebug;

            public DotsFsm OnSome(Entity currentStateEntity) {
                if (this.fsmEvent.id == 0) {
                    Debug.LogError("Can't use 0 FSM event");
                    return this.fsm;
                }
                
                // Look for the entity with the same from state and eventId
                for (int i = 0; i < this.transitions.Length; ++i) {
                    Transition transition = this.transitions[i];
                    if (!(transition.fromState == currentStateEntity && transition.fsmEvent.Equals(this.fsmEvent))) {
                        continue;
                    }

                    // We found a transition
                    this.fsm.currentState = ValueTypeOption<Entity>.Some(transition.toState);

                    if (this.isDebug) {
                        // Log transition state if it's for debugging
                        FixedString64 stateName = this.allNames[this.allNameReferences[transition.toState].nameEntity].value;

                        if (this.isDebug) {
                            // ReSharper disable once UseStringInterpolation (due to Burst)
                            Debug.Log(string.Format("Current state is now {0}", stateName));
                        }
                    }
                        
                    // Don't forget to clear the pending event so that actions will run
                    this.fsm.ClearPendingEvent();
                        
                    return this.fsm;
                }
                
                // At this point, there are no transitions found
                // We log a warning
                NameReference currentStateNameReference = this.allNameReferences[currentStateEntity];
                Name currentStateName = this.allNames[currentStateNameReference.nameEntity];
                
                // Burst doesn't like any other string format methods
                // ReSharper disable once UseStringInterpolation
                Debug.LogWarning(string.Format("{0}.{1} does not have transition for event {2}", this.fsmName, currentStateName.value, this.fsmEvent));
                
                return this.fsm;
            }

            public DotsFsm OnNone() {
                return this.fsm;
            }
        }
    }
}