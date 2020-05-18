using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace Common {
    /// <summary>
    /// A component that adds grid row behaviour to an object
    /// </summary>
    [RequireComponent(typeof(LayoutElement))]
    public class GridRow : MonoBehaviour {

        [SerializeField]
        private Image background;

        // used to refer to the row when it is created by the GridContainer
        private string rowId;

        private LayoutElement layoutElement;
        private SwarmItem swarm;

        void Awake() {
            this.layoutElement = GetComponent<LayoutElement>();
            Assertion.NotNull(this.layoutElement);
        }

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="rowId"></param>
        public void Init(string rowId) {
            this.rowId = rowId;
        }

        /// <summary>
        /// Returns the height of the grid row
        /// </summary>
        public float Height {
            get {
                return this.layoutElement.minHeight;
            }
        }

        public string RowId {
            get {
                return rowId;
            }
        }

        /// <summary>
        /// Sets the background color or the row (used for alternating row colors)
        /// The color may not be set if the grid row has no background specified
        /// </summary>
        /// <param name="color"></param>
        public void SetBackgroundColor(Color color) {
            if (this.background != null) {
                this.background.color = color;
            }
        }

        /// <summary>
        /// Recycles the grid row
        /// </summary>
        internal void Recycle() {
            if(this.swarm == null) {
                // we lazy initialize because Recycle() might get invoked before its Awake() is invoked
                this.swarm = GetComponent<SwarmItem>();
                Assertion.NotNull(this.swarm);
            }

            this.swarm.Kill();
        }

    }
}
