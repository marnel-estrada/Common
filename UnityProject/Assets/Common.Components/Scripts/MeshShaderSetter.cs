using UnityEngine;

namespace Common {
	public class MeshShaderSetter : MonoBehaviour {
		[SerializeField]
		private string shaderToSet;
		
		/**
		 * Returns the shader to set.
		 */
		public string GetShaderToSet() {
			return this.shaderToSet;
		}
	}
}

