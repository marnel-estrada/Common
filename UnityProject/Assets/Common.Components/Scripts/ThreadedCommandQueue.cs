﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

using UnityThreading;

namespace Common {
    /// <summary>
    /// A unity behaviour that accepts commands in a queue and processes them in a separate thread.
    /// This is designed as a singleton
    /// </summary>
    public class ThreadedCommandQueue : MonoBehaviour {

        [Tooltip("For debugging purposes only")]
        [SerializeField]
        private int queueCount;

        private Queue<Command> commandQueue = new Queue<Command>();

        private ActionThread thread;

        /// <summary>
        /// Enqueues the specified command
        /// </summary>
        /// <param name="command"></param>
        public void Enqueue(Command command) {
            this.commandQueue.Enqueue(command);
        }

        void Update() {
            if (thread == null || !thread.IsAlive) {
                if (this.thread != null) {
                    thread.Exit();
                }

                if (this.commandQueue.Count > 0) {
                    //Thread.Sleep(100);

                    // Dequeue and start a new thread if the thread is already done
                    Command command = this.commandQueue.Dequeue();
                    this.thread = UnityThreadHelper.CreateThread((Action)command.Execute);
                }
            }

            this.queueCount = this.commandQueue.Count;
        }

        public int QueueCount {
            get {
                return this.commandQueue.Count;
            }
        }

        private static ThreadedCommandQueue ONLY_INSTANCE;

        public static ThreadedCommandQueue Instance {
            get {
                if(ONLY_INSTANCE == null) {
                    // Create a new instance
                    GameObject go = new GameObject("ThreadedCommandQueue");
                    go.AddComponent<DontDestroyOnLoadComponent>(); // so it won't be gone when loading another scene
                    ONLY_INSTANCE = go.AddComponent<ThreadedCommandQueue>();
                }

                return ONLY_INSTANCE;
            }
        }

    }
}