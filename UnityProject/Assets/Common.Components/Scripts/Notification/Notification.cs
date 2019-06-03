using System;

namespace Common.Notification {
	public interface Notification {
		
		/**
		 * Returns the id of the notification.
		 */
		string GetId();
		
		/**
		 * Returns whether or not the notification has the specified parameter.
		 */
		bool HasParameter(string key);
		
		/**
		 * Returns the parameter with the specified key.
		 */
		Object GetParameter(string key);
		
		/**
		 * Returns a casted parameter value.
		 */
		T GetParameter<T>(string key); 

		/**
		 * Marks the notification as handled.
		 */
		void MarkAsHandled();

		/**
		 * Returns whether or not the notification has been handled.
		 */
		bool IsHandled();
		
	}
}
