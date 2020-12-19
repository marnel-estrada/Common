using Unity.Entities;
using Unity.Mathematics;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ExecuteNextAtomActionSystem))]
    [UpdateBefore(typeof(ReplanAfterFinishSystem))]
    public class MoveIntTranslationSystem : AtomActionComponentSystem<MoveIntTranslation> {
        private ComponentDataFromEntity<IntTranslation> allTranslations;

        protected override void OnUpdate() {
            this.allTranslations = GetComponentDataFromEntity<IntTranslation>();
            
            base.OnUpdate();
        }

        protected override GoapStatus Start(ref AtomAction atomAction, ref MoveIntTranslation actionComponent) {
            // Check if already done
            IntTranslation translation = this.allTranslations[atomAction.agentEntity];
            if (Equals(translation.value, actionComponent.target)) {
                // Already done
                return GoapStatus.SUCCESS;
            }
            
            return GoapStatus.RUNNING;
        }

        protected override GoapStatus Update(ref AtomAction atomAction, ref MoveIntTranslation actionComponent) {
            IntTranslation translation = this.allTranslations[atomAction.agentEntity];
            int3 difference = actionComponent.target - translation.value;
            int3 normalizedDiff = Normalize(difference);
            
            // Update the value with normalizedDiff
            translation.value = translation.value + normalizedDiff;
            this.allTranslations[atomAction.agentEntity] = translation; // Modify the value
            
            if (Equals(translation.value, actionComponent.target)) {
                // Already done
                return GoapStatus.SUCCESS;
            }

            return GoapStatus.RUNNING;
        }

        private static bool Equals(int3 a, int3 b) {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        // Each value can only have 1, -1 or zero
        private static int3 Normalize(int3 value) {
            return new int3(Normalize(value.x), Normalize(value.y), Normalize(value.z));
        }

        private static int Normalize(int value) {
            int absValue = math.abs(value);
            return absValue > 1 ? value / absValue : value;
        }
    }
}