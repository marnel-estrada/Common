using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Common.Logger {
    /// <summary>
    /// A special kind of logger that stores logs in memory instead of using Debug.Log()
    /// This makes it possible for fast logs then open up the QuickLogger window to show these logs on runtime.
    /// </summary>
    public class QuickLogger {

        private List<Log> logList;

        /// <summary>
        /// Constructor
        /// </summary>
        public QuickLogger() {
            this.logList = new List<Log>(1000); // initialize with buffer so List won't increase its array when adding the first few logs
        }

        private const int MAX_LOGS = 5000;

        /// <summary>
        /// Adds a log
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void Log(LogLevel level, string message) {
#if UNITY_EDITOR
            // Do only in editor to save memory during actual runtime
            this.logList.Add(new Log(level, message));

            while(this.logList.Count > MAX_LOGS) {
                this.logList.RemoveAt(0);
            }
#endif
        }

        private static QuickLogger ONLY_INSTANCE = null; // the only QuickLogger instance (singleton)

        /// <summary>
        /// Returns the only QuickLogger instance
        /// </summary>
        public static QuickLogger Instance {
            get {
                if(ONLY_INSTANCE == null) {
                    ONLY_INSTANCE = new QuickLogger();
                }

                return ONLY_INSTANCE;
            }
        }

        /// <summary>
        /// Logs a normal message
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message) {
            Instance.Log(LogLevel.NORMAL, message);
        }

        /// <summary>
        /// Returns the number of logs currently logged
        /// </summary>
        public static int Count {
            get {
                return Instance.logList.Count;
            }
        }

        /// <summary>
        /// Returns the log at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Log GetAt(int index) {
            return Instance.logList[index];
        }

        /// <summary>
        /// Clears all the logs
        /// Usually used to save memory
        /// </summary>
        public void Clear() {
            this.logList.Clear();
        }

    }
}
