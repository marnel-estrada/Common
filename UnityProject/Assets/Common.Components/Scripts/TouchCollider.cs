using UnityEngine;
using System.Collections.Generic;

namespace Common {
	/**
	 * Listens to clicks.
	 */
	[RequireComponent(typeof(BoxCollider))]
	public class TouchCollider : MonoBehaviour {
		private BoxCollider boxCollider;
		private readonly IList<Command> commandList = new List<Command>(); // the list of command to execute

		private void Start() {
			this.boxCollider = GetComponent<BoxCollider>();
			Assertion.NotNull(this.boxCollider, "boxCollider");
		}

		private void Update() {
			if(Input.GetButtonUp("Fire1")) {
				// check if mouse click collided with collider
				Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit = new RaycastHit();
				if(!this.boxCollider.Raycast(mouseRay, out hit, 1000)) {
					// did not hit
					return;
				}
				
				// we run command
				foreach(Command command in this.commandList) {
					command.Execute();
				}
			}
		}

		/**
		 * Sets the on click command.
		 */
		public void AddCommand(Command command) {
			this.commandList.Add(command);
		}
	}
}


