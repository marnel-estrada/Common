using System.Runtime.InteropServices;

namespace CommonEcs {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IntStackArray10 {
        private int item0;
        private readonly int item1;
        private readonly int item2;
        private readonly int item3;
        private readonly int item4;
        private readonly int item5;
        private readonly int item6;
        private readonly int item7;
        private readonly int item8;
        private readonly int item9;

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