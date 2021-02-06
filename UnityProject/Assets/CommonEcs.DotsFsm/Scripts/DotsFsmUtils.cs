using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public static class DotsFsmUtils {
        public static void SendEvent(EntityCommandBuffer commandBuffer, Entity fsmEntity, FixedString64 eventId) {
            Signal.Dispatch(commandBuffer, new SendEvent(fsmEntity, eventId));
        }
    }
}