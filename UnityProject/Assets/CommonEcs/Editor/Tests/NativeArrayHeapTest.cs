using NUnit.Framework;

using Unity.Collections;
using Unity.Entities.Tests;
using Unity.Mathematics;

namespace CommonEcs.Test {
    [TestFixture]
    [Category("CommonEcs.NativeArrayHeap")]
    public class NativeArrayHeapTest : ECSTestsFixture {
        // [Test]
        // public void TestPush() {
        //     NativeArray<HeapNode<int2>> array = new NativeArray<HeapNode<int2>>(100, Allocator.TempJob);
        //     NativeArrayHeap<int2> heap = new NativeArrayHeap<int2>(array);
        //     heap.Push(new int2(0, 0), 0);
        //     Assert.True(heap.ElementCount == 1);
        //     
        //     heap.Push(new int2(1, 1), 1);
        //     Assert.True(heap.ElementCount == 2);
        //     
        //     array.Dispose();
        // }
        //
        // [Test]
        // public void TestElementCount() {
        //     NativeArray<HeapNode<int2>> array = new NativeArray<HeapNode<int2>>(100, Allocator.TempJob);
        //     NativeArrayHeap<int2> heap = new NativeArrayHeap<int2>(array);
        //     heap.Push(new int2(0, 0), 0);
        //     Assert.True(heap.ElementCount == 1);
        //     
        //     heap.Push(new int2(1, 1), 1);
        //     Assert.True(heap.ElementCount == 2);
        //     
        //     heap.Push(new int2(3, 3), 3);
        //     heap.Push(new int2(4, 4), 4);
        //     heap.Push(new int2(2, 2), 2);
        //     heap.Push(new int2(5, 5), 5);
        //     Assert.True(heap.ElementCount == 6);
        //
        //     heap.Pop();
        //     Assert.True(heap.ElementCount == 5);
        //     
        //     heap.Pop();
        //     Assert.True(heap.ElementCount == 4);
        //     
        //     heap.Pop();
        //     heap.Pop();
        //     Assert.True(heap.ElementCount == 2);
        //     
        //     array.Dispose();
        // }
        //
        // [Test]
        // public void TestPop() {
        //     NativeArray<HeapNode<int2>> array = new NativeArray<HeapNode<int2>>(100, Allocator.TempJob);
        //     NativeArrayHeap<int2> heap = new NativeArrayHeap<int2>(array);
        //     heap.Push(new int2(0, 0), 0);
        //     heap.Push(new int2(1, 1), 1);
        //     heap.Push(new int2(3, 3), 3);
        //     heap.Push(new int2(4, 4), 4);
        //     heap.Push(new int2(2, 2), 2);
        //     heap.Push(new int2(5, 5), 5);
        //
        //     float previousCost = int.MinValue;
        //     while (heap.HasItems) {
        //         int2 top = heap.Top;
        //         Assert.True(previousCost < top.x);
        //         previousCost = top.x;
        //         heap.Pop();
        //     }
        //     
        //     array.Dispose();
        // }
        //
        // [Test]
        // public void TestIndexAccess() {
        //     NativeArray<HeapNode<int2>> array = new NativeArray<HeapNode<int2>>(100, Allocator.TempJob);
        //     NativeArrayHeap<int2> heap = new NativeArrayHeap<int2>(array);
        //     heap.Push(new int2(0, 0), 0);
        //     heap.Push(new int2(1, 1), 1);
        //     heap.Push(new int2(2, 2), 2);
        //     heap.Push(new int2(3, 3), 3);
        //     heap.Push(new int2(4, 4), 4);
        //     heap.Push(new int2(5, 5), 5);
        //
        //     for (int i = 0; i < heap.ElementCount; ++i) {
        //         HeapNode<int2> node = heap[i];
        //         Assert.True(Comparison.TolerantEquals(i, node.cost));
        //     }
        //     
        //     array.Dispose();
        // }
        //
        // [Test]
        // public void TestHasItems() {
        //     NativeArray<HeapNode<int2>> array = new NativeArray<HeapNode<int2>>(100, Allocator.TempJob);
        //     NativeArrayHeap<int2> heap = new NativeArrayHeap<int2>(array);
        //     Assert.False(heap.HasItems);
        //     
        //     heap.Push(new int2(0, 0), 0);
        //     Assert.True(heap.HasItems);
        //     
        //     heap.Push(new int2(1, 1), 1);
        //     heap.Push(new int2(2, 2), 2);
        //     heap.Push(new int2(3, 3), 3);
        //     Assert.True(heap.HasItems);
        //
        //     while (heap.HasItems) {
        //         heap.Pop();
        //     }
        //     Assert.False(heap.HasItems);
        //     
        //     heap.Push(new int2(4, 4), 4);
        //     Assert.True(heap.HasItems);
        //     
        //     heap.Clear();
        //     Assert.False(heap.HasItems);
        //     
        //     array.Dispose();
        // }
        //
        // [Test]
        // public void TestClear() {
        //     const int count = 1000;
        //     NativeArray<HeapNode<int2>> array = new NativeArray<HeapNode<int2>>(count, Allocator.TempJob);
        //     NativeArrayHeap<int2> heap = new NativeArrayHeap<int2>(array);
        //
        //     for (int i = 0; i < count; ++i) {
        //         heap.Push(new int2(i, i), i);
        //     }
        //     Assert.True(heap.ElementCount == count);
        //     
        //     heap.Clear();
        //     Assert.True(heap.ElementCount == 0);
        //     Assert.False(heap.HasItems);
        //     
        //     array.Dispose();
        // }
        //
        // // Test preventing infinite loop while pushing
        // [Test]
        // public void TestNextNotPointingToItself() {
        //     NativeArray<HeapNode<int2>> array = new NativeArray<HeapNode<int2>>(100, Allocator.TempJob);
        //     NativeArrayHeap<int2> heap = new NativeArrayHeap<int2>(array);
        //     
        //     // The values here are derived from a bug that happened during A* search
        //     heap.Push(new int2(-2, 0), 24);
        //     heap.Pop();
        //     heap.Push(new int2(-2, 1), 15);
        //     heap.Push(new int2(-2, -1), 35);
        //     heap.Push(new int2(-1, 0), 29);
        //     heap.Push(new int2(-3, 0), 21);
        //     Assert.True(CheckIntegrity(heap));
        //     heap.Pop();
        //     Assert.True(CheckIntegrity(heap));
        //     heap.Push(new int2(-2, 2), 12);
        //     Assert.True(CheckIntegrity(heap));
        //     heap.Push(new int2(-1, 1), 26);
        //     Assert.True(CheckIntegrity(heap));
        //     heap.Push(new int2(-3, 1), 12);
        //     Assert.True(CheckIntegrity(heap));
        //     heap.Pop();
        //     Assert.True(CheckIntegrity(heap));
        //     
        //     array.Dispose();
        // }
        //
        // private bool CheckIntegrity(in NativeArrayHeap<int2> heap) {
        //     for (int i = 0; i < heap.Length; ++i) {
        //         HeapNode<int2> node = heap[i];
        //         if (node.next == i) {
        //             // This causes infinite loop
        //             // It must be avoided
        //             return false;
        //         }
        //     }
        //     
        //     return true;
        // }
    }
}