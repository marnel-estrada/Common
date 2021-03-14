using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// This database is a just an array for now.
    /// User will maintain what each index represents. 
    /// </summary>
    public struct GoapDomainDatabase {
        public BlobArray<GoapDomain> domains;
    }
}