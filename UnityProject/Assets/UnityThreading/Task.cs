using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections;

namespace UnityThreading
{
    public enum TaskSortingSystem
    {
        NeverReorder,
        ReorderWhenAdded,
        ReorderWhenExecuted
    }

    public abstract class TaskBase
    {
        /// <summary>
        /// Change this when you work with a prioritzable Dispatcher or TaskDistributor to change the execution order
        /// A low value will be executed first.
        /// </summary>
        public volatile int Priority;

        private readonly ManualResetEvent abortEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent endedEvent = new ManualResetEvent(false);
		private bool hasStarted = false;

		protected abstract IEnumerator Do();

		/// <summary>
		/// Returns true if the task should abort. If a Task should abort and has not yet been started
		/// it will never start but indicate an end and failed state.
		/// </summary>
        public bool ShouldAbort
        {
            get
			{
				return this.abortEvent.WaitOne(0, false); 
			}
        }

		/// <summary>
		/// Returns true when processing of this task has been ended or has been skipped due early abortion.
		/// </summary>
        public bool HasEnded
        {
            get 
			{
				return this.endedEvent.WaitOne(0, false); 
			}
        }

		/// <summary>
		/// Returns true when the task has successfully been processed. Tasks which throw exceptions will
		/// not be set to a failed state, also any exceptions will not be catched, the user needs to add
		/// checks for these kind of situation.
		/// </summary>
        public bool IsSucceeded
        {
            get
            {
				return this.endedEvent.WaitOne(0, false) && !this.abortEvent.WaitOne(0, false);
            }
        }

		/// <summary>
		/// Returns true if the task should abort and has been ended. This value will not been set to true
		/// in case of an exception while processing this task. The user needs to add checks for these kind of situation.
		/// </summary>
        public bool IsFailed
        {
            get
            {
				return this.endedEvent.WaitOne(0, false) && this.abortEvent.WaitOne(0, false);
            }
        }

		/// <summary>
		/// Notifies the task to abort and sets the task state to failed. The task needs to check ShouldAbort if the task should abort.
		/// </summary>
        public void Abort()
        {
	        this.abortEvent.Set();
        }

		/// <summary>
		/// Notifies the task to abort and sets the task state to failed. The task needs to check ShouldAbort if the task should abort.
		/// This method will wait until the task has been aborted/ended.
		/// </summary>
        public void AbortWait()
		{
			Abort();
			Wait();
        }

		/// <summary>
		/// Notifies the task to abort and sets the task state to failed. The task needs to check ShouldAbort if the task should abort.
		/// This method will wait until the task has been aborted/ended or the given timeout has been reached.
		/// </summary>
		/// <param name="seconds">Time in seconds this method will max wait.</param>
        public void AbortWaitForSeconds(float seconds)
        {
			Abort();
			WaitForSeconds(seconds);
        }

		/// <summary>
		/// Blocks the calling thread until the task has been ended.
		/// </summary>
        public void Wait()
        {
	        this.endedEvent.WaitOne();
        }

		/// <summary>
		/// Blocks the calling thread until the task has been ended or the given timeout value has been reached.
		/// </summary>
		/// <param name="seconds">Time in seconds this method will max wait.</param>
        public void WaitForSeconds(float seconds)
        {
	        this.endedEvent.WaitOne(TimeSpan.FromSeconds(seconds));
        }

		/// <summary>
		/// Blocks the calling thread until the task has been ended and returns the return value of the task as the given type.
		/// Use this method only for Tasks with return values (functions)!
		/// </summary>
		/// <returns>The return value of the task as the given type.</returns>
		public virtual TResult Wait<TResult>()
        {
            throw new InvalidOperationException("This task type does not support return values.");
        }

		/// <summary>
		/// Blocks the calling thread until the task has been ended and returns the return value of the task as the given type.
		/// Use this method only for Tasks with return values (functions)!
		/// </summary>
		/// <param name="seconds">Time in seconds this method will max wait.</param>
		/// <returns>The return value of the task as the given type.</returns>
        public virtual TResult WaitForSeconds<TResult>(float seconds)
        {
            throw new InvalidOperationException("This task type does not support return values.");
        }

		/// <summary>
		/// Blocks the calling thread until the task has been ended and returns the return value of the task as the given type.
		/// Use this method only for Tasks with return values (functions)!
		/// </summary>
		/// <param name="seconds">Time in seconds this method will max wait.</param>
		/// <param name="defaultReturnValue">The default return value which will be returned when the task has failed.</param>
		/// <returns>The return value of the task as the given type.</returns>
		public virtual TResult WaitForSeconds<TResult>(float seconds, TResult defaultReturnValue)
        {
            throw new InvalidOperationException("This task type does not support return values.");
        }

        internal void DoInternal()
        {
	        this.hasStarted = true;
            if (!this.ShouldAbort)
            {
                var enumerator = Do();
                if (enumerator == null)
                {
	                this.endedEvent.Set();
                    return;
                }

                var currentThread = ThreadBase.CurrentThread;
                do
                {
                    var task = (TaskBase)enumerator.Current;
                    if (task != null && currentThread != null)
                    {
                        currentThread.DispatchAndWait(task);
                    }
                }
                while (enumerator.MoveNext());
            }

            this.endedEvent.Set();
        }

		/// <summary>
		/// Disposes this task and waits for completion if its still running.
		/// </summary>
        public void Dispose()
        {
			if (this.hasStarted)
				Wait();
			this.endedEvent.Close();
			this.abortEvent.Close();
        }
    }

    public class Task : TaskBase
    {
        private readonly Action action;
		
        public Task(Action action)
        {
            this.action = action;
        }

        protected override IEnumerator Do()
        {
	        this.action();
            return null;
        }
    }

    public class Task<T> : TaskBase
    {
        private readonly Func<T> function;
        private T result;

        public Task(Func<T> function)
        {
            this.function = function;
        }

		protected override IEnumerator Do()
        {
	        this.result = this.function();
            return null;
        }

		public override TResult Wait<TResult>()
		{
			return (TResult)(object) this.Result;
		}

		public override TResult WaitForSeconds<TResult>(float seconds)
		{
			return WaitForSeconds(seconds, default(TResult));
		}

		public override TResult WaitForSeconds<TResult>(float seconds, TResult defaultReturnValue)
		{
			if (!this.HasEnded)
				WaitForSeconds(seconds);
			if (this.IsSucceeded)
				return (TResult)(object) this.result;
			return defaultReturnValue;
		}
        
		/// <summary>
		/// Waits till completion and returns the operation result.
		/// </summary>
        public T Result
        {
            get
            {
                if (!this.HasEnded)
                    Wait();
                return this.result;
            }
        }
    }
}

