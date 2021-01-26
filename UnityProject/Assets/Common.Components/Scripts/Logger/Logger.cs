using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

using UnityEngine;

namespace Common.Logger {
    /**
     * Manages the logging and writing of logs to a log file. This is implemented as a singleton.
     */
    public class Logger {
        private string logFilePath;

        public string LogFilePath {
            get {
                return this.logFilePath;
            }
        }

        private readonly Queue<Log> bufferQueue; // the primary queue where logs would be enqueued
        private readonly Queue<Queue<Log>> writeQueue; // this is the work queue

        private bool currentlyWriting;

        private const int WRITE_TIME_INTERVAL = 5;
        private readonly CountdownTimer writeTimer;

        private static Logger ONLY_INSTANCE = null;

        public static Logger GetInstance() {
            if (ONLY_INSTANCE == null) {
                ONLY_INSTANCE = new Logger();
            }

            return ONLY_INSTANCE;
        }

        private Logger() {
            this.bufferQueue = new Queue<Log>();
            this.writeQueue = new Queue<Queue<Log>>();
            this.currentlyWriting = false;

            this.writeTimer = new CountdownTimer(WRITE_TIME_INTERVAL);
        }

        /**
         * Sets the name.
         */
        public void SetName(string name) {
            this.logFilePath = Application.persistentDataPath + "/" + name + "Log.txt";
            Debug.Log("logFilePath: " + this.logFilePath);
        }

        /**
         * Logs a normal message.
         */
        public void Log(string message) {
            Log(LogLevel.NORMAL, message);
        }

        /**
         * Logs a warning message.
         */
        public void LogWarning(string message) {
            Log(LogLevel.WARNING, message);
        }

        /**
         * Logs an error message.
         */
        public void LogError(string message) {
            Log(LogLevel.ERROR, message);
        }

        private static readonly object SYNC_OBJECT = new object();
        
        /**
         * Adds a log with a specified level.
         */
        public void Log(LogLevel level, string message) {
            // We did it this way so it will be thread safe and can be called in multithreaded environment
            lock(SYNC_OBJECT) {
                this.bufferQueue.Enqueue(new Log(level, message));
            }
        }

        private void EnqueueWriteTask() {
            Queue<Log> logsToWriteQueue = new Queue<Log>();
            while (this.bufferQueue.Count > 0) {
                Log log = this.bufferQueue.Dequeue();
                logsToWriteQueue.Enqueue(log);
            }

            this.writeQueue.Enqueue(logsToWriteQueue);
        }

        /**
         * Writes the remaining logs. Usually called when the application is exiting.
         */
        public void WriteRemainingLogs() {
            EnqueueWriteTask();

            // wait for all logs to be written
            while (this.writeQueue.Count > 0) {
                ProcessTaskQueue();
            }
        }

        /**
         * Update routines of the logger.
         */
        public void Update() {
            if (this.currentlyWriting) {
                // still writing
                return;
            }

            if (this.bufferQueue.Count == 0) {
                // nothing to write
                return;
            }

            this.writeTimer.Update();
            if (this.writeTimer.HasElapsed()) {
                EnqueueWriteTask();
                Action action = ProcessTaskQueue;
                UnityThreadHelper.CreateThread(action);
                this.writeTimer.Reset();
            }
        }

        // Allow up to 5MB only
        private const int MAX_LOG_SIZE = 5000000;

        /**
         * Writes the latest log queue to the file.
         */
        private void ProcessTaskQueue() {
#if UNITY_WEBPLAYER
// we don't support logging since System.IO.File.AppendText() is not supported in web player
#else
            this.currentlyWriting = true;

            // check if file already exists
            Assertion.IsTrue(!string.IsNullOrEmpty(this.logFilePath),
                "logFilePath should not be empty. Try setting a name for the log.");
            StreamWriter writer = null;
            try {
                if (File.Exists(this.logFilePath)) {
                    // Check if the logFilePath is already too big
                    FileInfo fileInfo = new FileInfo(this.logFilePath);
                    if (fileInfo.Length > MAX_LOG_SIZE) {
                        ResizeLogFile();
                    }
                    
                    writer = File.AppendText(this.logFilePath);
                } else {
                    writer = File.CreateText(this.logFilePath);
                }

                Queue<Log> frontLogQueue = this.writeQueue.Dequeue();
                while (frontLogQueue.Count > 0) {
                    Log log = frontLogQueue.Dequeue();
                    string logLine = log.Level.Name + ": " + log.Timestamp.ToString(CultureInfo.InvariantCulture.NumberFormat) +
                        " " + log.Message;
                    writer.WriteLine(logLine);
                }
            } finally {
                if (writer != null) {
                    writer.Flush();
                    writer.Close();
                }
            }

            this.currentlyWriting = false;
#endif
        }

        private readonly SimpleList<string> lastLines = new SimpleList<string>();
        private const int LINES_TO_SAVE = 2000;
        
        private void ResizeLogFile() {
            ReverseLineReader reader = new ReverseLineReader(this.logFilePath);

            int counter = 0;
            foreach (string line in reader) {
                this.lastLines.Add(line);

                ++counter;
                if (counter >= LINES_TO_SAVE) {
                    break;
                }
            }
            
            File.Delete(this.logFilePath);

            StreamWriter writer = null;
            try {
                // Write the stored lines
                writer = File.CreateText(this.logFilePath);

                // Write in reverse since the lines were read from the last
                for (int i = this.lastLines.Count - 1; i >= 0; --i) {
                    writer.WriteLine(this.lastLines[i]);
                }
            } finally {
                if (writer != null) {
                    writer.Flush();
                    writer.Close();
                }
            }

            this.lastLines.Clear();
        }
    }
}