using System;

namespace Common {
    /// <summary>
    /// The abstract base class for an item that is enqueued to JobQueue.
    /// </summary>
    public abstract class JobItem {

        private volatile bool aborted = false;
        private volatile bool started = false;
        private volatile bool dataReady = false;

        /// <summary>
        /// This is the actual job routine. override it in a concrete Job class
        /// </summary>
        protected abstract void DoWork();

        /// <summary>
        /// This is a callback which will be called from the main thread when
        /// the job has finised. Can be overridden.
        /// </summary>
        public virtual void OnFinished() { }

        public bool IsAborted { get { return aborted; } }
        public bool IsStarted { get { return started; } }
        public bool IsDataReady { get { return dataReady; } }

        public void Execute() {
            started = true;
            DoWork();
            dataReady = true;
        }

        public void AbortJob() {
            aborted = true;
        }

        public void ResetJobState() {
            started = false;
            dataReady = false;
            aborted = false;
        }

    }
}
