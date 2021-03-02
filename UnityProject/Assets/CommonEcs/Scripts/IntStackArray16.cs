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
    public unsafe struct IntStackArray16 {
        public ref struct Enumerator {
            private readonly int* m_Elements;

            private int m_Index;

            public Enumerator(int* elements) {
                this.m_Elements = elements;
                this.m_Index = -1;
            }

            public bool MoveNext() {
                this.m_Index++;

                return this.m_Index < 16;
            }

            public ref int Current {
                get {
                    RequireIndexInBounds();

                    return ref this.m_Elements[this.m_Index];
                }
            }

            [BurstDiscard]
            public void RequireIndexInBounds() {
                if (this.m_Index < 0 || this.m_Index >= 16) {
                    throw new InvalidOperationException("Index out of bounds: " + this.m_Index);
                }
            }
        }

        private readonly int m_Element0;

        private readonly int m_Element1;

        private readonly int m_Element2;

        private readonly int m_Element3;

        private readonly int m_Element4;

        private readonly int m_Element5;

        private readonly int m_Element6;

        private readonly int m_Element7;

        private readonly int m_Element8;

        private readonly int m_Element9;

        private readonly int m_Element10;

        private readonly int m_Element11;

        private readonly int m_Element12;

        private readonly int m_Element13;

        private readonly int m_Element14;

        private readonly int m_Element15;

        public ref int this[int index] {
            get {
                RequireIndexInBounds(index);

                return ref GetElement(index);
            }
        }

        private ref int GetElement(int index) {
            fixed (int* elements = &this.m_Element0) {
                return ref elements[index];
            }
        }

        private void SetElement(int index, int value) {
            fixed (int* elements = &this.m_Element0) {
                elements[index] = value;
            }
        }

        public const int Length = 16;

        public Enumerator GetEnumerator() {
            // Safe because Enumerator is a 'ref struct'
            fixed (int* elements = &this.m_Element0) {
                return new Enumerator(elements);
            }
        }

        [BurstDiscard]
        public void RequireIndexInBounds(int index) {
            if (index < 0 || index >= 16) {
                throw new InvalidOperationException("Index out of bounds: " + index);
            }
        }
    }
}