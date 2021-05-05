using System;
using System.Threading;
using System.Collections.Generic;

namespace Common {
    /// <summary>
    /// Queues jobs and executes them in separate threads.
    /// This was taken from http://wiki.unity3d.com/index.php/JobQueue
    /// 
    /// Usage:
    /// - Create the JobItem class that will be used as item.
    /// - Instantiate JobQueue: jobQueue = new JobQueue<AStarSearch>(5); // Integer here is the number of threads
    /// - Add jobs: jobQueue.AddJob(new AStarSearch(posA, posB));
    /// - Call Update on Update:
    ///     void Update() {
    ///         jobQueue.Update();
    ///     }
    /// - Call Shutdown() whenever the object is no longer needed like in OnDisable() or OnDestroy()
    ///     void OnDisable() {
    ///         jobQueue.Shutdown();
    ///     }
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JobQueue<T> : IDisposable where T : JobItem {

        private class ThreadItem {
            private readonly Thread m_Thread;
            private readonly AutoResetEvent m_Event;
            private volatile bool m_Abort = false;

            // simple linked list to manage active threaditems
            public ThreadItem NextActive = null;

            // the job item this thread is currently processing
            public T Data;

            public ThreadItem() {
                this.m_Event = new AutoResetEvent(false);
                this.m_Thread = new Thread(ThreadMainLoop);
                this.m_Thread.Start();
            }

            private void ThreadMainLoop() {
                while (true) {
                    if (this.m_Abort)
                        return;
                    this.m_Event.WaitOne();
                    if (this.m_Abort)
                        return;
                    this.Data.Execute();
                }
            }

            public void StartJob(T aJob) {
                aJob.ResetJobState();
                this.Data = aJob;
                // signal the thread to start working.
                this.m_Event.Set();
            }

            public void Abort() {
                this.m_Abort = true;
                if (this.Data != null) this.Data.AbortJob();
                // signal the thread so it can finish itself.
                this.m_Event.Set();
            }

            public void Reset() {
                this.Data = null;
            }
        }
        // internal thread pool
        private Stack<ThreadItem> m_Threads = new Stack<ThreadItem>();
        private readonly Queue<T> m_NewJobs = new Queue<T>();
        private volatile bool m_NewJobsAdded = false;
        private Queue<T> m_Jobs = new Queue<T>();
        // start of the linked list of active threads
        private ThreadItem m_Active = null;

        public event Action<T> OnJobFinished;

        public JobQueue(int aThreadCount) {
            if (aThreadCount < 1)
                aThreadCount = 1;
            for (int i = 0; i < aThreadCount; i++) this.m_Threads.Push(new ThreadItem());
        }

        public void AddJob(T aJob) {
            if (this.m_Jobs == null)
                throw new InvalidOperationException("AddJob not allowed. JobQueue has already been shutdown");
            if (aJob != null) {
                this.m_Jobs.Enqueue(aJob);
                ProcessJobQueue();
            }
        }

        public void AddJobFromOtherThreads(T aJob) {
            lock (this.m_NewJobs) {
                if (this.m_Jobs == null)
                    throw new InvalidOperationException("AddJob not allowed. JobQueue has already been shutdown");
                this.m_NewJobs.Enqueue(aJob);
                this.m_NewJobsAdded = true;
            }
        }

        public int CountActiveJobs() {
            int count = 0;
            for (var thread = this.m_Active; thread != null; thread = thread.NextActive)
                count++;
            return count;
        }

        private void CheckActiveJobs() {
            ThreadItem thread = this.m_Active;
            ThreadItem last = null;
            while (thread != null) {
                ThreadItem next = thread.NextActive;
                T job = thread.Data;
                if (job.IsAborted) {
                    if (last == null)
                        this.m_Active = next;
                    else
                        last.NextActive = next;
                    thread.NextActive = null;

                    thread.Reset();
                    this.m_Threads.Push(thread);
                } else if (thread.Data.IsDataReady) {
                    job.OnFinished();
                    if (OnJobFinished != null)
                        OnJobFinished(job);

                    if (last == null)
                        this.m_Active = next;
                    else
                        last.NextActive = next;
                    thread.NextActive = null;

                    thread.Reset();
                    this.m_Threads.Push(thread);
                } else
                    last = thread;
                thread = next;
            }
        }

        private void ProcessJobQueue() {
            if (this.m_NewJobsAdded) {
                lock (this.m_NewJobs) {
                    while (this.m_NewJobs.Count > 0)
                        AddJob(this.m_NewJobs.Dequeue());
                    this.m_NewJobsAdded = false;
                }
            }
            while (this.m_Jobs.Count > 0 && this.m_Threads.Count > 0) {
                var job = this.m_Jobs.Dequeue();
                if (!job.IsAborted) {
                    var thread = this.m_Threads.Pop();
                    thread.StartJob(job);
                    // add thread to the linked list of active threads
                    thread.NextActive = this.m_Active;
                    this.m_Active = thread;
                }
            }
        }

        public void Update() {
            CheckActiveJobs();
            ProcessJobQueue();
        }

        public void Shutdown() {
            for (var thread = this.m_Active; thread != null; thread = thread.NextActive)
                thread.Abort();
            while (this.m_Threads.Count > 0) this.m_Threads.Pop().Abort();
            while (this.m_Jobs.Count > 0) this.m_Jobs.Dequeue().AbortJob();
            this.m_Jobs = null;
            this.m_Active = null;
            this.m_Threads = null;
        }

        public void Dispose() {
            Shutdown();
        }

    }
}
