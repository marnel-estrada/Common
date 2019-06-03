using UnityEngine;

using Common.Logger;

/**
 * A component that can be attached to a GameObject so it could run Logger updates.
 */
public class LoggerComponent : MonoBehaviour {
	
	[SerializeField]
	private string loggerName;

	[SerializeField]
	private bool errorOnly;
	
	private Common.Logger.Logger logger;
	
	private void Awake() {
		this.logger = Common.Logger.Logger.GetInstance();
		this.logger.SetName(this.loggerName);
	}

	private void OnEnable() {
		Application.logMessageReceivedThreaded += HandleLog;
	}

	private void OnDisable() {
		Application.logMessageReceivedThreaded -= HandleLog;
	}

	// Update is called once per frame
	private void Update() {
		this.logger.Update();
	}

	private static readonly object SYNC_OBJECT = new object();
	
	private void HandleLog(string logString, string stackTrace, LogType type) {
		lock (SYNC_OBJECT) {
			LogLevel level = Convert(type);
			if (this.errorOnly && level != LogLevel.ERROR) {
				// Error only was specified but the log is not error level
				// We don't log
				// This can save log file space
				return;
			}

			this.logger.Log(Convert(type), logString);
			this.logger.Log(Convert(type), stackTrace);
		}
	}

	private static LogLevel Convert(LogType type) {
		switch (type) {
			case LogType.Log:
				return LogLevel.NORMAL;
			
			case LogType.Assert:
			case LogType.Error:
			case LogType.Exception:
				return LogLevel.ERROR;
			
			case LogType.Warning:
				return LogLevel.WARNING;
		}
		
		return LogLevel.NORMAL;
	}

}
