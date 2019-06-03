using System;
using System.Collections.Generic;

namespace Common.Notification {
	public class NotificationInstance  : Notification {
		
		private string id;
		private bool active;
		private Dictionary<string, Object> parameterMap;

		private bool handled;
		
		/**
		 * This has an empty constructor because instances would be managed in a pool.
		 */
		private NotificationInstance() {
			active = false;
		}
		
		/**
		 * Initializes the notification instance.
		 */
		private void Init(string id) {
			this.id = id;
			
			if(parameterMap != null) {
				parameterMap.Clear();
			}

			this.handled = false;
		}
		
		private void Activate() {
			active = true;
		}
		
		private void Deactivate() {
			active = false;
		}
		
		/**
		 * Returns whether or not the instance is active.
		 */
		public bool IsActive() {
			return active;
		}
		
		/**
		 * Adds a parameter to the notification. We return NotificationInstance so it can be used for chaining.
		 */
		public NotificationInstance AddParameter(string key, object parameterValue) {
			Assertion.Assert(IsActive()); // "This instance is no longer active."
			
			if(parameterMap == null) {
				// we lazy initialize because not all notifications have parameters
				parameterMap = new Dictionary<string, object>();
			}
			
			parameterMap[key] = parameterValue;

			return this;
		}

		#region Notification implementation
		public string GetId() {
			Assertion.Assert(IsActive()); // "This instance is no longer active."
			return id;
		}
		
		public bool HasParameter(string key) {
			// note that parameterMap is lazy initialized
			if(parameterMap == null) {
				return false;
			}

			return parameterMap.ContainsKey(key);
		}

		public Object GetParameter(string key) {
			Assertion.Assert(IsActive()); // "This instance is no longer active."
			return parameterMap[key];
		}
		
		public T GetParameter<T>(string key) {
			return (T)GetParameter(key);
		}

		public void MarkAsHandled() {
			this.handled = true;
		}

		public bool IsHandled() {
			return this.handled;
		}
		#endregion
		
		private static Stack<NotificationInstance> instanceStack = new Stack<NotificationInstance>();
		
		/**
		 * Borrows an instance.
		 */
		public static NotificationInstance Borrow(string id) {
			NotificationInstance instance = null;
			
			if(instanceStack.Count > 0) {
				instance = instanceStack.Pop();
			} else {
				instance = new NotificationInstance();
			}
			
			instance.Init(id);
			instance.Activate();
			return instance;
		}
		
		/**
		 * Returns an instance.
		 */
		public static void Return(NotificationInstance instance) {
			instance.Deactivate();
			instanceStack.Push(instance);
		}

	}
}

