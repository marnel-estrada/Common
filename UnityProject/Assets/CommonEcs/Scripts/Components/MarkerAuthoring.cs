using CommonEcs;
using Unity.Entities;
using UnityEngine;

namespace Components {
    public class MarkerAuthoring : MonoBehaviour {
        public string label;

        private class Baker : Baker<MarkerAuthoring> {
            public override void Bake(MarkerAuthoring authoring) {
                AddComponent(this.GetPrimaryEntity(), new Marker(authoring.label));
            }
        }
    }
}