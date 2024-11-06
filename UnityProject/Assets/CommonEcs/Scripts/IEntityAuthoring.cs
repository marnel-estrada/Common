using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A common interface for objects that can author an entity. Components may be added to the specified
    /// entity or more entities could be created.
    /// </summary>
    public interface IEntityAuthoring {
        void Execute(ref EntityManager entityManager, Entity entity);
    }
}