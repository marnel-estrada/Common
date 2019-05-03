using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityThreading;

public class UnityThreadHelper : MonoBehaviour {
    private static UnityThreadHelper instance;

    private readonly List<ThreadBase> finishedThreads = new List<ThreadBase>(4);

    private readonly List<ThreadBase> registeredThreads = new List<ThreadBase>();

    private static UnityThreadHelper Instance {
        get {
            EnsureHelper();

            return instance;
        }
    }

    /// <summary>
    ///     Returns the GUI/Main Dispatcher.
    /// </summary>
    public static Dispatcher Dispatcher {
        get {
            return Instance.CurrentDispatcher;
        }
    }

    /// <summary>
    ///     Returns the TaskDistributor.
    /// </summary>
    public static TaskDistributor TaskDistributor {
        get {
            return Instance.CurrentTaskDistributor;
        }
    }

    public Dispatcher CurrentDispatcher { get; private set; }

    public TaskDistributor CurrentTaskDistributor { get; private set; }

    public static void EnsureHelper() {
        if (null == (object) instance) {
            instance = FindObjectOfType(typeof(UnityThreadHelper)) as UnityThreadHelper;
            if (null == (object) instance) {
                GameObject go = new GameObject("[UnityThreadHelper]");
                go.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                instance = go.AddComponent<UnityThreadHelper>();
                instance.EnsureHelperInstance();
            }
        }
    }

    private void EnsureHelperInstance() {
        if (this.CurrentDispatcher == null) {
            this.CurrentDispatcher = new Dispatcher();
        }

        if (this.CurrentTaskDistributor == null) {
            this.CurrentTaskDistributor = new TaskDistributor();
        }
    }

    /// <summary>
    ///     Creates new thread which runs the given action. The given action will be wrapped so that any exception will be
    ///     catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ActionThread CreateThread(Action<ActionThread> action, bool autoStartThread) {
        Instance.EnsureHelperInstance();

        Action<ActionThread> actionWrapper = currentThread => {
            try {
                action(currentThread);
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        };
        ActionThread thread = new ActionThread(actionWrapper, autoStartThread);
        Instance.RegisterThread(thread);

        return thread;
    }

    /// <summary>
    ///     Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so
    ///     that any exception will be catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ActionThread CreateThread(Action<ActionThread> action) {
        return CreateThread(action, true);
    }

    /// <summary>
    ///     Creates new thread which runs the given action. The given action will be wrapped so that any exception will be
    ///     catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ActionThread CreateThread(Action action, bool autoStartThread) {
        return CreateThread(thread => action(), autoStartThread);
    }

    /// <summary>
    ///     Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so
    ///     that any exception will be catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ActionThread CreateThread(Action action) {
        return CreateThread(thread => action(), true);
    }

    public void RegisterThread(ThreadBase thread) {
        if (this.registeredThreads.Contains(thread)) {
            return;
        }

        this.registeredThreads.Add(thread);
    }

    private void OnDestroy() {
        foreach (ThreadBase thread in this.registeredThreads) {
            thread.Dispose();
        }

        if (this.CurrentDispatcher != null) {
            this.CurrentDispatcher.Dispose();
        }

        this.CurrentDispatcher = null;

        if (this.CurrentTaskDistributor != null) {
            this.CurrentTaskDistributor.Dispose();
        }

        this.CurrentTaskDistributor = null;
    }

    private void Update() {
        if (this.CurrentDispatcher != null) {
            this.CurrentDispatcher.ProcessTasks();
        }

        this.finishedThreads.Clear();

        // Remove finished threads
        int count = this.registeredThreads.Count;
        for (int i = 0; i < count; ++i) {
            ThreadBase thread = this.registeredThreads[i];
            if (!thread.IsAlive) {
                // Thread is finished
                thread.Dispose();
                this.finishedThreads.Add(thread);
            }
        }

        // Remove from registered
        int finishedCount = this.finishedThreads.Count;
        for (int i = 0; i < finishedCount; ++i) {
            this.registeredThreads.Remove(this.finishedThreads[i]);
        }
    }

    #region Enumeratable

    /// <summary>
    ///     Creates new thread which runs the given action. The given action will be wrapped so that any exception will be
    ///     catched and logged.
    /// </summary>
    /// <param name="action">The enumeratable action which the new thread should run.</param>
    /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ThreadBase CreateThread(Func<ThreadBase, IEnumerator> action, bool autoStartThread) {
        Instance.EnsureHelperInstance();

        EnumeratableActionThread thread = new EnumeratableActionThread(action, autoStartThread);
        Instance.RegisterThread(thread);

        return thread;
    }

    /// <summary>
    ///     Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so
    ///     that any exception will be catched and logged.
    /// </summary>
    /// <param name="action">The enumeratable action which the new thread should run.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ThreadBase CreateThread(Func<ThreadBase, IEnumerator> action) {
        return CreateThread(action, true);
    }

    /// <summary>
    ///     Creates new thread which runs the given action. The given action will be wrapped so that any exception will be
    ///     catched and logged.
    /// </summary>
    /// <param name="action">The enumeratable action which the new thread should run.</param>
    /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ThreadBase CreateThread(Func<IEnumerator> action, bool autoStartThread) {
        Func<ThreadBase, IEnumerator> wrappedAction = thread => {
            return action();
        };

        return CreateThread(wrappedAction, autoStartThread);
    }

    /// <summary>
    ///     Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so
    ///     that any exception will be catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ThreadBase CreateThread(Func<IEnumerator> action) {
        Func<ThreadBase, IEnumerator> wrappedAction = thread => {
            return action();
        };

        return CreateThread(wrappedAction, true);
    }

    #endregion

}