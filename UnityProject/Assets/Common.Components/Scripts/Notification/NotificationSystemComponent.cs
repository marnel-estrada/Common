using UnityEngine;
using System.Collections;

using Common.Notification;

/**
 * A game object level notification system. Usually used for communication between components where components don't want to be coupled with each other.
 */
public class NotificationSystemComponent : MonoBehaviour {
	
	// handlers may be added as components
	[SerializeField]
	private NotificationHandlerComponent[] handlerList;
	
	// note that NotificationSystem can be used for containment or as singleton
	private NotificationSystem system;
	
	void Awake() {
		foreach(NotificationHandlerComponent handler in handlerList) {
			AddHandler(handler);
		}
	}
	
	/**
	 * Adds a notification handler.
	 */
	public void AddHandler(NotificationHandler handler) {
		System.AddHandler(handler);
	}
	
	/**
	 * Removes the specified handler.
	 */
	public void RemoveHandler(NotificationHandler handler) {
		System.RemoveHandler(handler);
	}
	
	/**
	 * Clears all handlers of the notification system.
	 */
	public void ClearHandlers() {
		System.ClearHandlers();
	}
	
	/**
	 * Notify with a notification id.
	 */
	public void Post(string notificationId) {
		System.Post(notificationId);
	}
	
	/**
	 * Notify with a certain Notification instance.
	 */
	public void Post(NotificationInstance notification) {
		System.Post(notification);
	}
	
	private NotificationSystem System {
		get {
			// we lazy initialize because we don't know the order in which Awake() is invoked
			// system may be null when another component adds a handler on its Awake()
			if(system == null) {
				system = new NotificationSystem();
			}
			return system;
		}
	}
	
}

