using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    public static class BlobAssetUtils {
        public static BlobAssetReference<T> CreateReference<T>(T value, Allocator allocator) where T : struct {
            BlobBuilder builder = new BlobBuilder(Allocator.TempJob);
            ref T data = ref builder.ConstructRoot<T>();
            data = value;
            BlobAssetReference<T> reference = builder.CreateBlobAssetReference<T>(allocator);
            builder.Dispose();

            return reference;
        }
    }
}