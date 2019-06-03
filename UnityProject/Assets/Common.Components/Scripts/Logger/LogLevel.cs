using System;

namespace Common.Logger {
	/**
	 * This is an enum implemented as a class. As an enum, this is an immutable class.
	 */
	public sealed class LogLevel {
		
		public static LogLevel NORMAL = new LogLevel("NORMAL");
		public static LogLevel WARNING = new LogLevel("WARNING");
		public static LogLevel ERROR = new LogLevel("ERROR");
		
		private string name;
		
		/**
		 * Constructor with specified name
		 */
		private LogLevel(string name) {
			this.name = name;
		}
		
		/**
		 * Returns the name of the log level.
		 */
		public string Name {
			get {
				return name;
			}
		}
		
	}
}

