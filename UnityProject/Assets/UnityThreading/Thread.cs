using System;
using System.Collections;
using System.Threading;

namespace UnityThreading {
    public abstract class ThreadBase : IDisposable {

        [ThreadStatic]
        private static ThreadBase currentThread;

        protected ManualResetEvent exitEvent = new ManualResetEvent(false);
        protected Dispatcher targetDispatcher;
        protected Thread thread;

        public ThreadBase() : this(true) {
        }

        public ThreadBase(bool autoStartThread) : this(Dispatcher.Current, autoStartThread) {
        }

        public ThreadBase(Dispatcher targetDispatcher) : this(targetDispatcher, true) {
            this.targetDispatcher = targetDispatcher;
        }

        public ThreadBase(Dispatcher targetDispatcher, bool autoStartThread) {
            this.targetDispatcher = targetDispatcher;
            if (autoStartThread) {
                Start();
            }
        }

        /// <summary>
        ///     Returns the currently ThreadBase instance which is running in this thread.
        /// </summary>
        public static ThreadBase CurrentThread {
            get {
                return currentThread;
            }
        }

        /// <summary>
        ///     Returns true if the thread is working.
        /// </summary>
        public bool IsAlive {
            get {
                return this.thread == null ? false : this.thread.IsAlive;
            }
        }

        /// <summary>
        ///     Returns true if the thread should stop working.
        /// </summary>
        public bool ShouldStop {
            get {
                return this.exitEvent.WaitOne(0, false);
            }
        }

        #region IDisposable Members

        /// <summary>
        ///     Disposes the thread and all resources.
        /// </summary>
        public virtual void Dispose() {
            AbortWaitForSeconds(1.0f);
        }

        #endregion

        /// <summary>
        ///     Starts the thread.
        /// </summary>
        public void Start() {
            if (this.thread != null) {
                Abort();
            }

            this.exitEvent.Reset();
            this.thread = new Thread(DoInternal);
            this.thread.Start();
        }

        /// <summary>
        ///     Notifies the thread to stop working.
        /// </summary>
        public void Exit() {
            if (this.thread != null) {
                this.exitEvent.Set();
            }
        }

        /// <summary>
        ///     Notifies the thread to stop working.
        /// </summary>
        public void Abort() {
            Exit();
            if (this.thread != null) {
                this.thread.Join();
            }
        }

        /// <summary>
        ///     Notifies the thread to stop working and waits for completion for the given ammount of time.
        ///     When the thread soes not stop after the given timeout the thread will be terminated.
        /// </summary>
        /// <param name="seconds">The time this method will wait until the thread will be terminated.</param>
        public void AbortWaitForSeconds(float seconds) {
            Exit();
            if (this.thread != null) {
                this.thread.Join((int) (seconds * 1000));
                if (this.thread.IsAlive) {
                    this.thread.Abort();
                }
            }
        }

        /// <summary>
        ///     Creates a new Task for the target Dispatcher (default: the main Dispatcher) based upon the given function.
        /// </summary>
        /// <typeparam name="T">The return value of the task.</typeparam>
        /// <param name="function">The function to process at the dispatchers thread.</param>
        /// <returns>The new task.</returns>
        public Task<T> Dispatch<T>(Func<T> function) {
            return this.targetDispatcher.Dispatch(function);
        }

        /// <summary>
        ///     Creates a new Task for the target Dispatcher (default: the main Dispatcher) based upon the given function.
        ///     This method will wait for the task completion and returns the return value.
        /// </summary>
        /// <typeparam name="T">The return value of the task.</typeparam>
        /// <param name="function">The function to process at the dispatchers thread.</param>
        /// <returns>The return value of the tasks function.</returns>
        public T DispatchAndWait<T>(Func<T> function) {
            Task<T> task = Dispatch(function);
            task.Wait();

            return task.Result;
        }

        /// <summary>
        ///     Creates a new Task for the target Dispatcher (default: the main Dispatcher) based upon the given function.
        ///     This method will wait for the task completion or the timeout and returns the return value.
        /// </summary>
        /// <typeparam name="T">The return value of the task.</typeparam>
        /// <param name="function">The function to process at the dispatchers thread.</param>
        /// <param name="timeOutSeconds">Time in seconds after the waiting process will stop.</param>
        /// <returns>The return value of the tasks function.</returns>
        public T DispatchAndWait<T>(Func<T> function, float timeOutSeconds) {
            Task<T> task = Dispatch(function);
            task.WaitForSeconds(timeOutSeconds);

            return task.Result;
        }

        /// <summary>
        ///     Creates a new Task for the target Dispatcher (default: the main Dispatcher) based upon the given action.
        /// </summary>
        /// <param name="action">The action to process at the dispatchers thread.</param>
        /// <returns>The new task.</returns>
        public Task Dispatch(Action action) {
            return this.targetDispatcher.Dispatch(action);
        }

        /// <summary>
        ///     Creates a new Task for the target Dispatcher (default: the main Dispatcher) based upon the given action.
        ///     This method will wait for the task completion.
        /// </summary>
        /// <param name="action">The action to process at the dispatchers thread.</param>
        public void DispatchAndWait(Action action) {
            Task task = Dispatch(action);
            task.Wait();
        }

        /// <summary>
        ///     Creates a new Task for the target Dispatcher (default: the main Dispatcher) based upon the given action.
        ///     This method will wait for the task completion or the timeout.
        /// </summary>
        /// <param name="action">The action to process at the dispatchers thread.</param>
        /// <param name="timeOutSeconds">Time in seconds after the waiting process will stop.</param>
        public void DispatchAndWait(Action action, float timeOutSeconds) {
            Task task = Dispatch(action);
            task.WaitForSeconds(timeOutSeconds);
        }

        /// <summary>
        ///     Dispatches the given task to the target Dispatcher (default: the main Dispatcher).
        /// </summary>
        /// <param name="taskBase">The task to process at the dispatchers thread.</param>
        /// <returns>The new task.</returns>
        public TaskBase Dispatch(TaskBase taskBase) {
            return this.targetDispatcher.Dispatch(taskBase);
        }

        /// <summary>
        ///     Dispatches the given task to the target Dispatcher (default: the main Dispatcher).
        ///     This method will wait for the task completion.
        /// </summary>
        /// <param name="taskBase">The task to process at the dispatchers thread.</param>
        public void DispatchAndWait(TaskBase taskBase) {
            TaskBase task = Dispatch(taskBase);
            task.Wait();
        }

        /// <summary>
        ///     Dispatches the given task to the target Dispatcher (default: the main Dispatcher).
        ///     This method will wait for the task completion or the timeout.
        /// </summary>
        /// <param name="taskBase">The task to process at the dispatchers thread.</param>
        /// <param name="timeOutSeconds">Time in seconds after the waiting process will stop.</param>
        public void DispatchAndWait(TaskBase taskBase, float timeOutSeconds) {
            TaskBase task = Dispatch(taskBase);
            task.WaitForSeconds(timeOutSeconds);
        }

        protected void DoInternal() {
            currentThread = this;
            IEnumerator enumerator = Do();
            if (enumerator == null) {
                return;
            }

            do {
                TaskBase task = (TaskBase) enumerator.Current;
                if (task != null) {
                    DispatchAndWait(task);
                }
            } while (enumerator.MoveNext());
        }

        protected abstract IEnumerator Do();
    }

    public class ActionThread : ThreadBase {
        private readonly Action<ActionThread> action;

        /// <summary>
        ///     Creates a new Thread which runs the given action.
        ///     The thread will start running after creation.
        /// </summary>
        /// <param name="action">The action to run.</param>
        public ActionThread(Action<ActionThread> action) : this(action, true) {
        }

        /// <summary>
        ///     Creates a new Thread which runs the given action.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="autoStartThread">Should the thread start after creation.</param>
        public ActionThread(Action<ActionThread> action, bool autoStartThread) : base(Dispatcher.Current, false) {
            this.action = action;
            if (autoStartThread) {
                Start();
            }
        }

        protected override IEnumerator Do() {
            this.action(this);

            return null;
        }
    }

    public class EnumeratableActionThread : ThreadBase {
        private readonly Func<ThreadBase, IEnumerator> enumeratableAction;

        /// <summary>
        ///     Creates a new Thread which runs the given enumeratable action.
        ///     The thread will start running after creation.
        /// </summary>
        /// <param name="action">The enumeratable action to run.</param>
        public EnumeratableActionThread(Func<ThreadBase, IEnumerator> enumeratableAction) : this(enumeratableAction,
            true) {
        }

        /// <summary>
        ///     Creates a new Thread which runs the given enumeratable action.
        /// </summary>
        /// <param name="action">The enumeratable action to run.</param>
        /// <param name="autoStartThread">Should the thread start after creation.</param>
        public EnumeratableActionThread(Func<ThreadBase, IEnumerator> enumeratableAction, bool autoStartThread) : base(
            Dispatcher.Current, false) {
            this.enumeratableAction = enumeratableAction;
            if (autoStartThread) {
                Start();
            }
        }

        protected override IEnumerator Do() {
            return this.enumeratableAction(this);
        }
    }
}