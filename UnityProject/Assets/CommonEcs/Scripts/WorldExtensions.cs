using Unity.Entities;

namespace CommonEcs {
    public static class WorldExtensions {
        /// <summary>
        /// A utility method that return an unmanaged system (ISystem).
        /// </summary>
        /// <param name="world"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ref T GetUnmanagedSystem<T>(this ref WorldUnmanaged world) where T : unmanaged, ISystem {
            SystemHandle handle = world.GetExistingUnmanagedSystem<T>();
            return ref world.GetUnsafeSystemRef<T>(handle);
        }
    }
}