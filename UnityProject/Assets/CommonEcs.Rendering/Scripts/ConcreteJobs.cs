using Unity.Jobs;

namespace CommonEcs.Rendering {
    /// <summary>
    /// Mentions generic jobs so that they will be Burst compiled
    /// </summary>
    static class ConcreteJobs {
        static ConcreteJobs() {
            new MultithreadedSort.Merge<SortedSpriteEntry>().Schedule();
            new MultithreadedSort.QuicksortJob<SortedSpriteEntry>().Schedule();
        }
    }
}