using UnityEngine;
using System.Collections.Generic;

using Common;

/**
 * Listens to clicks.
 */
[RequireComponent(typeof(BoxCollider))]
public class TouchCollider : MonoBehaviour {
	
	private BoxCollider boxCollider;
	private IList<Command> commandList = new List<Command>(); // the list of command to execute
	
	void Start() {
		this.boxCollider = this.GetComponent<BoxCollider>();
		Assertion.AssertNotNull(boxCollider, "boxCollider");
	}
	
	void Update() {
		if(Input.GetButtonUp("Fire1")) {
			// check if mouse click collided with collider
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit = new RaycastHit();
			if(!boxCollider.Raycast(mouseRay, out hit, 1000)) {
				// did not hit
				return;
			}
			
			// we run command
			foreach(Command command in commandList) {
				command.Execute();
			}
		}
	}
	
	/**
	 * Sets the on click command.
	 */
	public void AddCommand(Command command) {
		commandList.Add(command);
	}
	
}

