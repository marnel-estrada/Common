﻿////////////////////////////////////////////////////////////////////////////////
// Warning: This file was automatically generated by SmallBufferGenerator.
//          If you edit this by hand, the next run of SmallBufferGenerator
//          will overwrite your edits.
////////////////////////////////////////////////////////////////////////////////

using System;

namespace CommonEcs
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public unsafe struct FixedList4Entries<T> where T : unmanaged {
		public ref struct Enumerator
		{
			private readonly T* elements;
			
			private int index;
			
			private readonly int originalVersion;
			
			private readonly int* version;
			
			private readonly int length;
			
			public Enumerator(T* elements, int* version, int length)
			{
				this.elements = elements;
				this.index = -1;
				this.originalVersion = *version;
				this.version = version;
				this.length = length;
			}
			
			public bool MoveNext()
			{
				RequireVersionMatch();
				this.index++;
				return this.index < this.length;
			}
			
			public ref T Current
			{
				get
				{
					RequireVersionMatch();
					RequireIndexInBounds();
					return ref this.elements[this.index];
				}
			}
			
			[Unity.Burst.BurstDiscard]
			private void RequireVersionMatch()
			{
				if (this.originalVersion != *this.version)
				{
					throw new InvalidOperationException("Buffer modified during enumeration");
				}
			}
			
			[Unity.Burst.BurstDiscard]
			private void RequireIndexInBounds()
			{
				if (this.index < 0 || this.index >= this.length)
				{
					// ReSharper disable once UseStringInterpolation (due to Burst)
					throw new Exception($"Index out of bounds: {this.index}");
				}
			}
		}
		
		private readonly T element0;
		
		private readonly T element1;
		
		private readonly T element2;
		
		private readonly T element3;
		
		private int version;
		
		private int length;
		
		public ref T this[int index]
		{
			get
			{
				RequireIndexInBounds(index);
				return ref GetElement(index);
			}
		}
		
		private ref T GetElement(int index)
		{
			fixed (T* elements = &this.element0)
			{
				return ref elements[index];
			}
		}
		
		private void SetElement(int index, T value)
		{
			fixed (T* elements = &this.element0)
			{
				elements[index] = value;
			}
		}
		
		public int Count => this.length;

		public const int CAPACITY = 4;
		
		public Enumerator GetEnumerator()
		{
			// Safe because Enumerator is a 'ref struct'
			fixed (T* elements = &this.element0)
			{
				fixed (int* versionPointer = &this.version)
				{
					return new Enumerator(elements, versionPointer, this.length);
				}
			}
		}
		
		public void Add(T item)
		{
			RequireNotFull();
			SetElement(this.length, item);
			this.length++;
			this.version++;
		}
		
		public void Clear()
		{
			for (int i = 0; i < this.length; ++i)
			{
				SetElement(i, default);
			}
			this.length = 0;
			this.version++;
		}
		
		public void Insert(int index, T value)
		{
			RequireNotFull();
			RequireIndexInBounds(index);
			for (int i = this.length; i > index; --i)
			{
				SetElement(i, GetElement(i - 1));
			}
			SetElement(index, value);
			this.length++;
			this.version++;
		}
		
		public void RemoveAt(int index)
		{
			RequireIndexInBounds(index);
			for (int i = index; i < this.length - 1; ++i)
			{
				SetElement(i, GetElement(i + 1));
			}
			this.length--;
			this.version++;
		}
		
		public void RemoveRange(int index, int count)
		{
			RequireIndexInBounds(index);
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive: " + count);
			}
			RequireIndexInBounds(index + count - 1);
			int indexAfter = index + count;
			int indexEndCopy = indexAfter + count;
			if (indexEndCopy >= this.length)
			{
				indexEndCopy = this.length;
			}
			int numCopies = indexEndCopy - indexAfter;
			for (int i = 0; i < numCopies; ++i)
			{
				SetElement(index + i, GetElement(index + count + i));
			}
			for (int i = indexAfter; i < this.length - 1; ++i)
			{
				SetElement(i, GetElement(i + 1));
			}
			this.length -= count;
			this.version++;
		}
		
		[Unity.Burst.BurstDiscard]
		private void RequireNotFull()
		{
			if (this.length == 4)
			{
				throw new InvalidOperationException("Buffer overflow");
			}
		}
		
		private void RequireIndexInBounds(int index)
		{
			if (index < 0 || index >= this.length)
			{
				throw new Exception($"Index out of bounds: {index}");
			}
		}
	}
}