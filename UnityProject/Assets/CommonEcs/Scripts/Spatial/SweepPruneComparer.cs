using System.Collections.Generic;
using Unity.Collections;

namespace Common {
    public readonly struct SweepPruneComparer : IComparer<int> {
        private readonly NativeList<SweepPruneItem> masterList;

        public SweepPruneComparer(NativeList<SweepPruneItem> masterList) {
            this.masterList = masterList;
        }
            
        public int Compare(int x, int y) {
            // We compare by min X
            SweepPruneItem xItem = this.masterList[x];
            SweepPruneItem yItem = this.masterList[y];

            if (xItem.box.Min.x < yItem.box.Min.x) {
                return -1;
            }
                
            if (xItem.box.Min.x > yItem.box.Min.x) {
                return 1;
            }
                
            // Equal
            return 0;
        }
    }
}