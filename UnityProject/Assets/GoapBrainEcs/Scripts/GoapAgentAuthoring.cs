using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace GoapBrainEcs {
    public class GoapAgentAuthoring : MonoBehaviour {
        [SerializeField]
        private List<AuthoringCondition> goals;
        
        [SerializeField]
        private List<AuthoringCondition> fallbackGoals;

        // public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        //     // TODO Get domain ID
        //     GoapAgent agent = new GoapAgent(1); // Dummy domain ID here
        //     
        //     // TODO Add goals and fallback goals (Implement when condition ID is already FixedString)
        //
        //     dstManager.AddComponentData(entity, agent);
        // }
    }
}