using Common;

using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace CommonEcs.DotsFsm {
    [UpdateInGroup(typeof(DotsFsmSystemGroup))]
    [UpdateAfter(typeof(SendEventFromActionsToFsmSystem))]
    public partial class ConsumePendingEventSystem : JobSystemBase {
        private EntityQuery query;
        
        // Type handles
        private ComponentTypeHandle<DotsFsm> fsmType;
        private BufferTypeHandle<Transition> transitionType;
        private ComponentTypeHandle<DebugFsm> debugType;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(DotsFsm), ComponentType.ReadOnly<NameReference>(), 
                ComponentType.ReadOnly<Transition>(), ComponentType.ReadOnly<DebugFsm>());

            this.fsmType = GetComponentTypeHandle<DotsFsm>();
            this.transitionType = GetBufferTypeHandle<Transition>();
            this.debugType = GetComponentTypeHandle<DebugFsm>(true);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            this.fsmType.Update(this);
            this.transitionType.Update(this);
            this.debugType.Update(this);
        
            ConsumeJob consumeJob = new ConsumeJob() {
                fsmType = this.fsmType,
                transitionType = this.transitionType,
                debugType = this.debugType,
                allNameReferences = GetComponentLookup<NameReference>(true),
                allNames = GetComponentLookup<Name>(true)
            };
            
            return consumeJob.ScheduleParallel(this.query, inputDeps);
        }
        
        [BurstCompile]
        private struct ConsumeJob : IJobChunk {
            public ComponentTypeHandle<DotsFsm> fsmType;

            [ReadOnly]
            public BufferTypeHandle<Transition> transitionType;

            [ReadOnly]
            public ComponentTypeHandle<DebugFsm> debugType;
            
            [ReadOnly]
            public ComponentLookup<NameReference> allNameReferences;

            [ReadOnly]
            public ComponentLookup<Name> allNames;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<DotsFsm> fsms = chunk.GetNativeArray(ref this.fsmType);
                BufferAccessor<Transition> transitionLists = chunk.GetBufferAccessor(ref this.transitionType);
                NativeArray<DebugFsm> debugList = chunk.GetNativeArray(ref this.debugType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    DotsFsm fsm = fsms[i];
                    DynamicBuffer<Transition> transitions = transitionLists[i];
                    
                    if (fsm.pendingEvent.IsNone) {
                        // No event to consume
                        continue;
                    }

                    // The current state will be updated in the matcher
                    fsm = fsm.currentState.Match<TryChangeState, DotsFsm>(new TryChangeState() {
                        fsm = fsm,
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
            public FsmEvent fsmEvent;
            
            [ReadOnly]
            public DynamicBuffer<Transition> transitions;

            [ReadOnly]
            public ComponentLookup<NameReference> allNameReferences;
            
            [ReadOnly]
            public ComponentLookup<Name> allNames;

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
                        FixedString64Bytes stateName = this.allNames[this.allNameReferences[transition.toState].nameEntity].value;

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
                // We still clear the pending event because it may have already been outdated
                this.fsm.ClearPendingEvent();
                
                // Note here that we removed logging the warning because it's annoying.
                // It pauses the editor when it Burst is enabled.
                
                return this.fsm;
            }

            public DotsFsm OnNone() {
                return this.fsm;
            }
        }
    }
}