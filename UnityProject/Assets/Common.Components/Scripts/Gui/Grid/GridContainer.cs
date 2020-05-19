using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /// <summary>
    /// Handles the creation of rows in a grid
    /// Also handles the expansion of the height of the rect whenever a row is added or removed
    /// </summary>
    public class GridContainer : MonoBehaviour {
        [SerializeField]
        private PrefabManager gridRowPool;

        [SerializeField]
        private string gridRowPrefabName;

        [SerializeField]
        private Color oddRowColor = ColorUtils.WHITE;

        [SerializeField]
        private Color evenRowColor = ColorUtils.WHITE;

        private RectTransform selfRect;

        private readonly List<GridRow> allRows = new List<GridRow>(); // maintenance
        private readonly List<GridRow> visibleList = new List<GridRow>();

        private void Awake() {
            Assertion.NotNull(this.gridRowPool);
            Assertion.NotEmpty(this.gridRowPrefabName);

            this.selfRect = GetComponent<RectTransform>();
            Assertion.NotNull(this.selfRect);

            // reset the rect height (0)
            UpdateContainerHeight();
        }

        /// <summary>
        /// Adds a grid row
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns></returns>
        public GridRow AddRow(string rowId) {
            GameObject go = this.gridRowPool.Request(this.gridRowPrefabName);
            GridRow gridRow = go.GetComponent<GridRow>();
            Assertion.NotNull(gridRow);
            gridRow.Init(rowId);

            AddRow(gridRow);

            return gridRow;
        }

        /// <summary>
        /// Adds a row instance that may not be maintained by this grid
        /// </summary>
        /// <param name="gridRow"></param>
        public void AddRow(GridRow gridRow) {
            // maintain
            this.allRows.Add(gridRow);
            this.visibleList.Add(gridRow); // Every row is visible until a filter is specified

            // set the appropriate row color
            // note that the function SetBackgroundColor() accepts a one based number for the row number
            SetBackgroundColor(gridRow, this.visibleList.Count);

            // expand the container's rect height
            UpdateContainerHeight();

            // set the scale back to (1, 1, 1) again because sometimes UI prefabs have large scales
            gridRow.transform.SetParent(this.selfRect);
            gridRow.transform.localScale = VectorUtils.ONE;
        }

        // Note that the specified rowNumber must be one based so that checking for odd or even is not confusing
        private void SetBackgroundColor(GridRow row, int rowNumber) {
            if((rowNumber & 1) == 0) {
                // an even number
                row.SetBackgroundColor(this.evenRowColor);
            } else {
                // an odd number
                row.SetBackgroundColor(this.oddRowColor);
            }
        }

        private void UpdateContainerHeight() {
            // resolve height
            float totalHeight = 0;
            for(int i = 0; i < this.visibleList.Count; ++i) {
                totalHeight += this.visibleList[i].Height;
            }

            this.selfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }

		public void ForceUpdateHeight (float totalHeight) {
			this.selfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
		}

        /// <summary>
        /// Returns the number of rows in the grid
        /// </summary>
        public int RowCount {
            get {
                return this.visibleList.Count;
            }
        }

        /// <summary>
        /// Removes the row with the specified id
        /// </summary>
        /// <param name="rowId"></param>
        public void RemoveRow(string rowId) {
            Option<GridRow> gridRow = GetRow(rowId);
            gridRow.Match(new RemoveRowMatcher(this));
        }

        private struct RemoveRowMatcher : IOptionMatcher<GridRow> {
            private readonly GridContainer gridContainer;

            public RemoveRowMatcher(GridContainer gridContainer) {
                this.gridContainer = gridContainer;
            }
            
            public void OnSome(GridRow gridRow) {
                this.gridContainer.RemoveRow(gridRow);
            }

            public void OnNone() {
                // Does nothing
            }
        }

        /// <summary>
        /// Removes the specified row
        /// </summary>
        /// <param name="gridRow"></param>
        public void RemoveRow(GridRow gridRow) {
            this.allRows.Remove(gridRow);
            this.visibleList.Remove(gridRow);
            gridRow.Recycle();
            UpdateContainerHeight();
        }

        /// <summary>
        /// Returns the row at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GridRow GetRowAt(int index) {
            return this.visibleList[index];
        }

        /// <summary>
        /// Looks for the GridRow with the specified rowId
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns></returns>
        public Option<GridRow> GetRow(string rowId) {
            for(int i = 0; i < this.allRows.Count; ++i) {
                GridRow row = this.allRows[i];
                if(row.RowId.EqualsFast(rowId)) {
                    return Option<GridRow>.Some(row);
                }
            }

            // Can't find a row with the specified rowId
            return Option<GridRow>.NONE;
        }

        private Comparison<GridRow> sortComparison;

        /// <summary>
        /// Sorts the grid according to the specified Comparison
        /// </summary>
        /// <param name="comparison"></param>
        public void Sort(Comparison<GridRow> comparison) {
            this.sortComparison = comparison;
            this.visibleList.Sort(comparison);
            UpdateDisplay();
        }

        private void UpdateDisplay() {
            for(int i = 0; i < this.visibleList.Count; ++i) {
                this.visibleList[i].transform.SetSiblingIndex(i);
                SetBackgroundColor(this.visibleList[i], i + 1); // also change the coloring
            }

            UpdateContainerHeight();
        }

        private GridFilter filter;

        /// <summary>
        /// Applies the specified filter
        /// </summary>
        /// <param name="filter"></param>
        public void ApplyFilter(GridFilter filter) {
            this.filter = filter;

            this.visibleList.Clear();

            // Add rows to visible list those that pass the filter
            for(int i = 0; i < this.allRows.Count; ++i) {
                GridRow row = this.allRows[i];
                if(this.filter.Passed(row)) {
                    row.gameObject.Activate(); // Activate to show it
                    this.visibleList.Add(row);
                } else {
                    row.gameObject.Deactivate(); // Hide it
                }
            }
            
            UpdateDisplay();
        }

        private int ResolveIndex(string rowId) {
            for (int i = 0; i < this.visibleList.Count; ++i) {
                GridRow row = this.visibleList[i];
                if (row.RowId.Equals(rowId)) {
                    return i;
                }
            }

            Assertion.IsTrue(false, string.Format("Can't find GridRow with ID \"{0}.\"", rowId));
            return -1;
        }

        /// <summary>
        /// Clears the grid of its current rows
        /// </summary>
        public void Clear() {
            // recycle each row
            for(int i = 0; i < this.allRows.Count; ++i) {
                this.allRows[i].Recycle();
            }
            this.allRows.Clear();

            this.visibleList.Clear();
        }

    }
}
