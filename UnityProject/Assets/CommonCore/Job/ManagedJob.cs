using System.Runtime.InteropServices;

using Unity.Jobs;

namespace Common {
    public struct ManagedJob : IJob {
        public GCHandle gcHandle;
        
        public void Execute() {
            Command command = (Command) this.gcHandle.Target;
            command.Execute();
        }

        public void Free() {
            this.gcHandle.Free();
        }
    }
}