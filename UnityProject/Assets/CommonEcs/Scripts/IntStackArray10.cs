using System.Runtime.InteropServices;

namespace Common {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IntStackArray10 {
        private int item0;
        private int item1;
        private int item2;
        private int item3;
        private int item4;
        private int item5;
        private int item6;
        private int item7;
        private int item8;
        private int item9;

        public const int Length = 10;

        public ref int this[int index] {
            get {
                RequireIndexInBounds(index);
                fixed (int* elements = &this.item0) {
                    return ref elements[index];
                }
            }
        }

        public void Clear() {
            for (int i = 0; i < Length; ++i) {
                this[i] = 0;
            }
        }

        [Unity.Burst.BurstDiscard]
        public void RequireIndexInBounds(int index) {
            if (index < 0 || index >= Length) {
                throw new System.InvalidOperationException("Index out of bounds: " + index);
            }
        }
    }
}