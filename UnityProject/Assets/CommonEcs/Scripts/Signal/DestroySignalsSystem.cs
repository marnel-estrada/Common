﻿using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DestroySignalsSystem : ComponentSystem {
        private EntityQuery query;
    
        protected override void OnCreateManager() {
            this.query = GetEntityQuery(typeof(Signal), typeof(SignalFramePassed));
        }
    
        protected override void OnUpdate() {
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}