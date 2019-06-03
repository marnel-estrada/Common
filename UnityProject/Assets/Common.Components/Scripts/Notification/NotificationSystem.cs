using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Notification {
	public class NotificationSystem {
		
		// we used a linked list because some handlers may be added and removed often
		private LinkedList<NotificationHandler> handlerList;
		
		/**
		 * This can be used for containment or as singleton.
		 */
		public NotificationSystem() {
			handlerList = new LinkedList<NotificationHandler>();
		}
		
		/**
		 * Adds a notification handler.
		 */
		public void AddHandler(NotificationHandler handler) {
			Assertion.Assert(!handlerList.Contains(handler));
			handlerList.AddLast(handler);
		}
		
		/**
		 * Removes the specified handler.
		 */
		public void RemoveHandler(NotificationHandler handler) {
			handlerList.Remove(handler);
		}
		
		/**
		 * Clears all handlers of the notification system.
		 */
		public void ClearHandlers() {
			handlerList.Clear();
		}
		
		/**
		 * Notify with a notification id.
		 */
		public void Post(string notificationId) {
			Post(NotificationInstance.Borrow(notificationId));
		}
		
		public delegate void NotificationInitializer(NotificationInstance notif);
		
		/**
		 * Post with a specified initializer.
		 */
		public void Post(string notificationId, NotificationInitializer initializer) {
			NotificationInstance notif = NotificationInstance.Borrow(notificationId);
			initializer(notif);
			Post(notif); // will return notification instance here
		}

		/**
		 * Utility function to start a notification with parameter. This was made to avoid usage of closures which may instantiate memory.
		 */
		public NotificationInstance Begin(string notificationId) {
			return NotificationInstance.Borrow(notificationId);
		}

		/**
		 * Synonym for Begin()
		 */
		public NotificationInstance Start(string notificationId) {
			return Begin(notificationId);
		}
		
		/**
		 * Notify with a certain Notification instance.
		 */
		public void Post(NotificationInstance notification) {
			if(handlerList.Count == 0) {
				// no notification handlers to handle it
				NotificationInstance.Return(notification);
				return;
			}

			LinkedListNode<NotificationHandler> currentNode = handlerList.First;
			do {
				currentNode.Value.HandleNotification(notification);
				currentNode = currentNode.Next;
			} while(currentNode != null);
			
			// we return the instance after posting so it will be deactivated and may not be used again
			NotificationInstance.Return(notification);
		}
		
		private static NotificationSystem ONLY_INSTANCE;
		
		public static NotificationSystem GetInstance() {
			if(ONLY_INSTANCE == null) {
				ONLY_INSTANCE = new NotificationSystem();
			}
			
			return ONLY_INSTANCE;
		}
	}
}

