using Unity.Entities;
using Unity.Mathematics;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ExecuteNextActionOnFailSystem))]
    public class FailTranslation_OnFailSystem : AtomActionOnFailComponentSystem<FailTranslation> {
        private ComponentDataFromEntity<IntTranslation> allTranslations;

        protected override void OnUpdate() {
            this.allTranslations = GetComponentDataFromEntity<IntTranslation>();
            
            base.OnUpdate();
        }

        protected override void OnFail(ref AtomActionOnFail onFail, ref FailTranslation actionComponent) {
            IntTranslation translation = this.allTranslations[actionComponent.agentEntity];
            translation.value = new int3(100, 100, 100);
            this.allTranslations[actionComponent.agentEntity] = translation; // Modify
        }
    }
}