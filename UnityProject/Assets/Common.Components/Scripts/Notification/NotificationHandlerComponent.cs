using UnityEngine;
using System.Collections;

using Common.Notification;

/**
 * A notification handler that is a component. It can be assigned to NotificationSystemComponent
 */
public abstract class NotificationHandlerComponent : MonoBehaviour, NotificationHandler {
	
	#region NotificationHandler implementation
	public abstract string GetNotificationHandlerId();

	public abstract void HandleNotification(Notification notification);
	#endregion
	
}

