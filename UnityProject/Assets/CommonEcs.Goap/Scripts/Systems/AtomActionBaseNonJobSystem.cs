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
    public abstract partial class AtomActionBaseNonJobSystem<TActionFilter> : SystemBase where TActionFilter : struct, IAtomActionComponent {
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
            NativeList<Entity> actionsThatCanExecuteList =
                new NativeList<Entity>(this.query.CalculateEntityCount(), Allocator.TempJob);

            // We collect action entities that can execute via a job so that it would be faster
            // compared to using non bursted chunk iteration which will check all actions.
            CollectActionsThatCanExecuteJob collectJob = new CollectActionsThatCanExecuteJob() {
                entityType = GetEntityTypeHandle(),
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                allDebugEntities = GetComponentDataFromEntity<DebugEntity>(),
                resultList = actionsThatCanExecuteList.AsParallelWriter()
            };
            collectJob.ScheduleParallel(this.query, 1, this.Dependency).Complete();

            if (actionsThatCanExecuteList.IsEmpty) {
                // There are no actions that can execute. We can skip executing the actions.

                // Don't forget to dispose
                actionsThatCanExecuteList.Dispose();
                return;
            }

            // Execute each action that can execute
            ComponentDataFromEntity<AtomAction> allAtomActions = GetComponentDataFromEntity<AtomAction>();
            ComponentDataFromEntity<TActionFilter> allFilterActions =
                this.isActionFilterHasArray ? GetComponentDataFromEntity<TActionFilter>() : default;

            BeforeActionsExecution();

            for (int i = 0; i < actionsThatCanExecuteList.Length; ++i) {
                Entity actionEntity = actionsThatCanExecuteList[i];
                AtomAction atomAction = allAtomActions[actionEntity];
                TActionFilter actionFilter = this.isActionFilterHasArray ? allFilterActions[actionEntity] : default;
                ExecuteAction(ref atomAction, ref actionFilter);

                // Modify
                allAtomActions[actionEntity] = atomAction;

                if (this.isActionFilterHasArray) {
                    allFilterActions[actionEntity] = actionFilter;
                }
            }

            // Don't forget to dispose
            actionsThatCanExecuteList.Dispose();
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
            // Caching of ComponentDataFromEntity or BufferFromEntity can be done here.
        }

        protected abstract GoapResult Start(ref AtomAction atomAction, ref TActionFilter actionComponent);

        protected abstract GoapResult Update(ref AtomAction atomAction, ref TActionFilter actionComponent);

        // Note that we don't have Cleanup() here yet because it's really needed. Very few actions should
        // use this and they may not need Cleanup()
    }
}