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
            private Thread m_Thread;
            private AutoResetEvent m_Event;
            private volatile bool m_Abort = false;

            // simple linked list to manage active threaditems
            public ThreadItem NextActive = null;

            // the job item this thread is currently processing
            public T Data;

            public ThreadItem() {
                m_Event = new AutoResetEvent(false);
                m_Thread = new Thread(ThreadMainLoop);
                m_Thread.Start();
            }

            private void ThreadMainLoop() {
                while (true) {
                    if (m_Abort)
                        return;
                    m_Event.WaitOne();
                    if (m_Abort)
                        return;
                    Data.Execute();
                }
            }

            public void StartJob(T aJob) {
                aJob.ResetJobState();
                Data = aJob;
                // signal the thread to start working.
                m_Event.Set();
            }

            public void Abort() {
                m_Abort = true;
                if (Data != null)
                    Data.AbortJob();
                // signal the thread so it can finish itself.
                m_Event.Set();
            }

            public void Reset() {
                Data = null;
            }
        }
        // internal thread pool
        private Stack<ThreadItem> m_Threads = new Stack<ThreadItem>();
        private Queue<T> m_NewJobs = new Queue<T>();
        private volatile bool m_NewJobsAdded = false;
        private Queue<T> m_Jobs = new Queue<T>();
        // start of the linked list of active threads
        private ThreadItem m_Active = null;

        public event Action<T> OnJobFinished;

        public JobQueue(int aThreadCount) {
            if (aThreadCount < 1)
                aThreadCount = 1;
            for (int i = 0; i < aThreadCount; i++)
                m_Threads.Push(new ThreadItem());
        }

        public void AddJob(T aJob) {
            if (m_Jobs == null)
                throw new System.InvalidOperationException("AddJob not allowed. JobQueue has already been shutdown");
            if (aJob != null) {
                m_Jobs.Enqueue(aJob);
                ProcessJobQueue();
            }
        }

        public void AddJobFromOtherThreads(T aJob) {
            lock (m_NewJobs) {
                if (m_Jobs == null)
                    throw new System.InvalidOperationException("AddJob not allowed. JobQueue has already been shutdown");
                m_NewJobs.Enqueue(aJob);
                m_NewJobsAdded = true;
            }
        }

        public int CountActiveJobs() {
            int count = 0;
            for (var thread = m_Active; thread != null; thread = thread.NextActive)
                count++;
            return count;
        }

        private void CheckActiveJobs() {
            ThreadItem thread = m_Active;
            ThreadItem last = null;
            while (thread != null) {
                ThreadItem next = thread.NextActive;
                T job = thread.Data;
                if (job.IsAborted) {
                    if (last == null)
                        m_Active = next;
                    else
                        last.NextActive = next;
                    thread.NextActive = null;

                    thread.Reset();
                    m_Threads.Push(thread);
                } else if (thread.Data.IsDataReady) {
                    job.OnFinished();
                    if (OnJobFinished != null)
                        OnJobFinished(job);

                    if (last == null)
                        m_Active = next;
                    else
                        last.NextActive = next;
                    thread.NextActive = null;

                    thread.Reset();
                    m_Threads.Push(thread);
                } else
                    last = thread;
                thread = next;
            }
        }

        private void ProcessJobQueue() {
            if (m_NewJobsAdded) {
                lock (m_NewJobs) {
                    while (m_NewJobs.Count > 0)
                        AddJob(m_NewJobs.Dequeue());
                    m_NewJobsAdded = false;
                }
            }
            while (m_Jobs.Count > 0 && m_Threads.Count > 0) {
                var job = m_Jobs.Dequeue();
                if (!job.IsAborted) {
                    var thread = m_Threads.Pop();
                    thread.StartJob(job);
                    // add thread to the linked list of active threads
                    thread.NextActive = m_Active;
                    m_Active = thread;
                }
            }
        }

        public void Update() {
            CheckActiveJobs();
            ProcessJobQueue();
        }

        public void Shutdown() {
            for (var thread = m_Active; thread != null; thread = thread.NextActive)
                thread.Abort();
            while (m_Threads.Count > 0)
                m_Threads.Pop().Abort();
            while (m_Jobs.Count > 0)
                m_Jobs.Dequeue().AbortJob();
            m_Jobs = null;
            m_Active = null;
            m_Threads = null;
        }

        public void Dispose() {
            Shutdown();
        }

    }
}
