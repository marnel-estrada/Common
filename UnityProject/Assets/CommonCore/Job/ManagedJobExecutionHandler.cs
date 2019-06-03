using System.Runtime.InteropServices;

using Unity.Jobs;

namespace Common {
    public class ManagedJobExecutionHandler {
        private readonly SimpleList<ManagedJob> jobs;
        private readonly SimpleList<JobHandle> handles;

        public ManagedJobExecutionHandler(int bufferSize) {
            this.jobs = new SimpleList<ManagedJob>(bufferSize);
            this.handles = new SimpleList<JobHandle>(bufferSize);
        }

        /// <summary>
        /// Adds a command to be executed in a separate thread
        /// </summary>
        /// <param name="command"></param>
        public void Add(Command command) {
            ManagedJob job = new ManagedJob() {
                gcHandle = GCHandle.Alloc(command)
            };
                    
            this.jobs.Add(job);
            this.handles.Add(job.Schedule());
        }
        
        /// <summary>
        /// Completes all added commands
        /// This is usually invoked in LateUpdate()
        /// </summary>
        public void Complete() {
            Assertion.Assert(this.handles.Count == this.jobs.Count);
            for (int i = this.handles.Count - 1; i >= 0; --i) {
                JobHandle handle = this.handles[i];
                if (handle.IsCompleted) {
                    handle.Complete();
                    this.jobs[i].Free();
                    
                    this.handles.RemoveAt(i);
                    this.jobs.RemoveAt(i);
                }
            }
        }
    }
}