using System;
using System.Runtime.InteropServices;

using Unity.Burst;

namespace CommonEcs.Goap {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ConditionList5 {
        public ref struct Enumerator {
            private readonly Condition* m_Elements;

            private int m_Index;

            private readonly int m_OriginalVersion;

            private readonly int* m_Version;

            private readonly int m_Length;

            public Enumerator(Condition* elements, int* version, int length) {
                this.m_Elements = elements;
                this.m_Index = -1;
                this.m_OriginalVersion = *version;
                this.m_Version = version;
                this.m_Length = length;
            }

            public bool MoveNext() {
                RequireVersionMatch();
                this.m_Index++;

                return this.m_Index < this.m_Length;
            }

            public ref Condition Current {
                get {
                    RequireVersionMatch();
                    RequireIndexInBounds();

                    return ref this.m_Elements[this.m_Index];
                }
            }

            [BurstDiscard]
            public void RequireVersionMatch() {
                if (this.m_OriginalVersion != *this.m_Version) {
                    throw new InvalidOperationException("Buffer modified during enumeration");
                }
            }

            [BurstDiscard]
            public void RequireIndexInBounds() {
                if (this.m_Index < 0 || this.m_Index >= this.m_Length) {
                    throw new InvalidOperationException("Index out of bounds: " + this.m_Index);
                }
            }
        }

        private readonly Condition m_Element0;

        private readonly Condition m_Element1;

        private readonly Condition m_Element2;

        private readonly Condition m_Element3;

        private readonly Condition m_Element4;

        private int m_Version;

        public ref Condition this[int index] {
            get {
                RequireIndexInBounds(index);

                return ref GetElement(index);
            }
        }

        private ref Condition GetElement(int index) {
            fixed (Condition* elements = &this.m_Element0) {
                return ref elements[index];
            }
        }

        private void SetElement(int index, Condition value) {
            fixed (Condition* elements = &this.m_Element0) {
                elements[index] = value;
            }
        }

        public int Count { get; private set; }

        public const int Capacity = 5;

        public Enumerator GetEnumerator() {
            // Safe because Enumerator is a 'ref struct'
            fixed (Condition* elements = &this.m_Element0) {
                fixed (int* version = &this.m_Version) {
                    return new Enumerator(elements, version, this.Count);
                }
            }
        }

        public void Add(Condition item) {
            RequireNotFull();
            SetElement(this.Count, item);
            this.Count++;
            this.m_Version++;
        }

        public void Clear() {
            for (int i = 0; i < this.Count; ++i) {
                SetElement(i, default);
            }

            this.Count = 0;
            this.m_Version++;
        }

        public void Insert(int index, Condition value) {
            RequireNotFull();
            RequireIndexInBounds(index);
            for (int i = this.Count; i > index; --i) {
                SetElement(i, GetElement(i - 1));
            }

            SetElement(index, value);
            this.Count++;
            this.m_Version++;
        }

        public void RemoveAt(int index) {
            RequireIndexInBounds(index);
            for (int i = index; i < this.Count - 1; ++i) {
                SetElement(i, GetElement(i + 1));
            }

            this.Count--;
            this.m_Version++;
        }

        public void RemoveRange(int index, int count) {
            RequireIndexInBounds(index);
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "Count must be positive: " + count);
            }

            RequireIndexInBounds(index + count - 1);
            int indexAfter = index + count;
            int indexEndCopy = indexAfter + count;
            if (indexEndCopy >= this.Count) {
                indexEndCopy = this.Count;
            }

            int numCopies = indexEndCopy - indexAfter;
            for (int i = 0; i < numCopies; ++i) {
                SetElement(index + i, GetElement(index + count + i));
            }

            for (int i = indexAfter; i < this.Count - 1; ++i) {
                SetElement(i, GetElement(i + 1));
            }

            this.Count -= count;
            this.m_Version++;
        }

        [BurstDiscard]
        public void RequireNotFull() {
            if (this.Count == 5) {
                throw new InvalidOperationException("Buffer overflow");
            }
        }

        [BurstDiscard]
        public void RequireIndexInBounds(int index) {
            if (index < 0 || index >= this.Count) {
                throw new InvalidOperationException("Index out of bounds: " + index);
            }
        }
    }
}