////////////////////////////////////////////////////////////////////////////////
// Warning: This file was automatically generated by SmallBufferGenerator.
//          If you edit this by hand, the next run of SmallBufferGenerator
//          will overwrite your edits.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

using Unity.Burst;

namespace CommonEcs {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct OptionIntArray4 {
        public ref struct Enumerator {
            private readonly ValueTypeOption<int>* m_Elements;

            private int m_Index;

            public Enumerator(ValueTypeOption<int>* elements) {
                this.m_Elements = elements;
                this.m_Index = -1;
            }

            public bool MoveNext() {
                this.m_Index++;

                return this.m_Index < 4;
            }

            public ref ValueTypeOption<int> Current {
                get {
                    RequireIndexInBounds();

                    return ref this.m_Elements[this.m_Index];
                }
            }

            [BurstDiscard]
            public void RequireIndexInBounds() {
                if (this.m_Index < 0 || this.m_Index >= 4) {
                    // ReSharper disable once UseStringInterpolation (due to Burst)
                    throw new Exception(string.Format("Index out of bounds: {0}", this.m_Index));
                }
            }
        }

        private readonly ValueTypeOption<int> m_Element0;

        private readonly ValueTypeOption<int> m_Element1;

        private readonly ValueTypeOption<int> m_Element2;

        private readonly ValueTypeOption<int> m_Element3;

        public ref ValueTypeOption<int> this[int index] {
            get {
                RequireIndexInBounds(index);

                return ref GetElement(index);
            }
        }

        private ref ValueTypeOption<int> GetElement(int index) {
            fixed (ValueTypeOption<int>* elements = &this.m_Element0) {
                return ref elements[index];
            }
        }

        private void SetElement(int index, ValueTypeOption<int> value) {
            fixed (ValueTypeOption<int>* elements = &this.m_Element0) {
                elements[index] = value;
            }
        }

        public const int Length = 4;

        public Enumerator GetEnumerator() {
            // Safe because Enumerator is a 'ref struct'
            fixed (ValueTypeOption<int>* elements = &this.m_Element0) {
                return new Enumerator(elements);
            }
        }

        private static void RequireIndexInBounds(int index) {
            if (index < 0 || index >= 4) {
                // ReSharper disable once UseStringInterpolation (due to Burst)
                throw new Exception(string.Format("Index out of bounds: {0}", index));
            }
        }
    }
}