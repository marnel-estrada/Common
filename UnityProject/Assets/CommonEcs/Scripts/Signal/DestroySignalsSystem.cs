﻿using Unity.Entities;

namespace CommonEcs {
    public partial class DestroySignalsSystem : SystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Signal), typeof(SignalFramePassed));
        }

        protected override void OnUpdate() {
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}