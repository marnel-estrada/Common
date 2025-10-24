using UnityEngine;

namespace GoapBrain {
    public static class AtomActionIdGenerator {
        private static int IdCounter = 1;

        public static int NextAtomActionId() {
            int nextId = IdCounter;
            ++IdCounter;
            return nextId;
        }
        
        // Reset since IdCounter is static
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic() {
            IdCounter = 0;
        }
    }
}