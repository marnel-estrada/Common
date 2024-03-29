﻿using CommonEcs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components {
    /// <summary>
    /// A common component that can be added to entities to move them by speed to a
    /// destination by calling StartMove() and enabling the component.
    /// </summary>
    public struct MoveBySpeed : IComponentData, IEnableableComponent {
        public readonly float3 startPos;
        public readonly float3 destinationPos;

        public Timer timer;

        public MoveBySpeed(float3 startPos, float3 destinationPos, float speed) : this() {
            this.startPos = startPos;
            this.destinationPos = destinationPos;

            DotsAssert.IsTrue(speed > 0); // Prevent divide by zero
            float duration = math.distance(this.destinationPos, this.startPos) / speed;
            this.timer = new Timer(duration);
        }

        public bool IsDone => this.timer.HasElapsed;
    }

    public class MoveBySpeedAuthoring : MonoBehaviour {
        private class Baker : Baker<MoveBySpeedAuthoring> {
            public override void Bake(MoveBySpeedAuthoring authoring) {
                Entity primaryEntity = this.GetPrimaryEntity();
                AddComponent<MoveBySpeed>(primaryEntity);
                SetComponentEnabled<MoveBySpeed>(primaryEntity, false);
            }
        }
    }
}
