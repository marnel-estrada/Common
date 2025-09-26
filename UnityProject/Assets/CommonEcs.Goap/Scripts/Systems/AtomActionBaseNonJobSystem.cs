using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// Base class for atom actions that don't use jobs.
    /// There may be actions that use reference types.
    /// </summary>
    /// <typeparam name="TActionFilter"></typeparam>
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(IdentifyAtomActionsThatCanExecuteSystem))]
    [UpdateBefore(typeof(EndAtomActionsSystem))]
    public abstract partial class AtomActionBaseNonJobSystem<TActionFilter> : SystemBase 
        where TActionFilter : unmanaged, IAtomActionComponent {
        private EntityQuery query;
        private bool isActionFilterHasArray;

        protected override void OnCreate() {
            base.OnCreate();

            this.query = PrepareQuery();

            // Action filter has native array in chunks if it's not zero sized
            this.isActionFilterHasArray = !TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TActionFilter>()).IsZeroSized;
        }

        protected virtual EntityQuery PrepareQuery() {
            return GetEntityQuery(typeof(AtomAction), typeof(TActionFilter));
        }

        protected override void OnUpdate() {
            int entityCount = this.query.CalculateEntityCount();
            NativeList<Entity> cleanupActionsList = new(entityCount, WorldUpdateAllocator);
            NativeList<Entity> canExecuteActionsList = new(entityCount, WorldUpdateAllocator);

            // We collect action entities that can execute via a job so that it would be faster
            // compared to using non bursted chunk iteration which will check all actions.
            CollectActionEntities collectJob = new() {
                entityType = GetEntityTypeHandle(),
                atomActionType = GetComponentTypeHandle<AtomAction>(true),
                allAgents = GetComponentLookup<GoapAgent>(true),
                allDebugEntities = GetComponentLookup<DebugEntity>(true),
                cleanupResults = cleanupActionsList.AsParallelWriter(),
                canExecuteResults = canExecuteActionsList.AsParallelWriter()
            };
            collectJob.ScheduleParallel(this.query, this.Dependency).Complete();
            
            // Execute each action that can execute
            ComponentLookup<AtomAction> allAtomActions = GetComponentLookup<AtomAction>();
            ComponentLookup<TActionFilter> allFilterActions =
                this.isActionFilterHasArray ? GetComponentLookup<TActionFilter>() : default;
            
            BeforeActionsExecution();

            ExecuteCleanup(ref cleanupActionsList, ref allAtomActions, ref allFilterActions);
            ExecuteActions(ref canExecuteActionsList, ref allAtomActions, ref allFilterActions);
        }

        private void ExecuteCleanup(ref NativeList<Entity> actionsList, ref ComponentLookup<AtomAction> allAtomActions, 
            ref ComponentLookup<TActionFilter> allFilterActions) {
            if (actionsList.IsEmpty) {
                // Nothing to process
                return;
            }
            
            for (int i = 0; i < actionsList.Length; ++i) {
                Entity actionEntity = actionsList[i];
                AtomAction atomAction = allAtomActions[actionEntity];
                TActionFilter actionFilter = this.isActionFilterHasArray ? allFilterActions[actionEntity] : default;
                
                Cleanup(ref atomAction, ref actionFilter);

                // Modify
                allAtomActions[actionEntity] = atomAction;

                if (this.isActionFilterHasArray) {
                    allFilterActions[actionEntity] = actionFilter;
                }
            }
        }

        private void ExecuteActions(ref NativeList<Entity> actionsList, ref ComponentLookup<AtomAction> allAtomActions, 
            ref ComponentLookup<TActionFilter> allFilterActions) {
            if (actionsList.IsEmpty) {
                // There are no actions that can execute. We can skip executing the actions.
                return;
            }

            for (int i = 0; i < actionsList.Length; ++i) {
                Entity actionEntity = actionsList[i];
                AtomAction atomAction = allAtomActions[actionEntity];
                TActionFilter actionFilter = this.isActionFilterHasArray ? allFilterActions[actionEntity] : default;
                
                ExecuteAction(ref atomAction, ref actionFilter);

                // Modify
                allAtomActions[actionEntity] = atomAction;

                if (this.isActionFilterHasArray) {
                    allFilterActions[actionEntity] = actionFilter;
                }
            }
        }

        private void ExecuteAction(ref AtomAction atomAction, ref TActionFilter actionFilter) {
            if (!atomAction.started) {
                // We call Start() if not yet started
                atomAction.result = Start(ref atomAction, ref actionFilter);
                atomAction.started = true;

                if (atomAction.result == GoapResult.FAILED || atomAction.result == GoapResult.SUCCESS) {
                    // No need to proceed to update if the Start already failed or succeeded
                    return;
                }
            }

            atomAction.result = Update(ref atomAction, ref actionFilter);
        }

        protected virtual void BeforeActionsExecution() {
            // Caching of ComponentLookup or BufferLookup can be done here.
        }

        protected abstract GoapResult Start(ref AtomAction atomAction, ref TActionFilter actionComponent);

        protected abstract GoapResult Update(ref AtomAction atomAction, ref TActionFilter actionComponent);

        protected virtual void Cleanup(ref AtomAction atomAction, ref TActionFilter actionComponent) {
        }
    }
}