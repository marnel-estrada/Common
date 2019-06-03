using System;

namespace Common.Notification {
	public interface NotificationHandler {
		
		/**
		 * Returns the notification handler id. Mostly used for debugging.
		 */
		string GetNotificationHandlerId();
		
		/**
		 * Handles the notification
		 */
		void HandleNotification(Notification notification);
		
	}
}

