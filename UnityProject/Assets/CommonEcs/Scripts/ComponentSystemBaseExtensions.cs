namespace CommonEcs {
    using System;

    using Unity.Entities;

    public static class ComponentSystemBaseExtensions {
        /// <summary>
        /// A shortcut for constructing an EntityArchetypeQuery
        /// </summary>
        /// <param name="system"></param>
        /// <param name="any"></param>
        /// <param name="none"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public static EntityQueryDesc ConstructQuery(this ComponentSystemBase system, ComponentType[] any, ComponentType[] none, ComponentType[] all) {
            return new EntityQueryDesc() {
                Any = any ?? Array.Empty<ComponentType>(),
                None = none ?? Array.Empty<ComponentType>(),
                All = all ?? Array.Empty<ComponentType>()
            };
        }

        /// <summary>
        /// A shortcut for constructing an EntityArchetypeQuery
        /// Accepts only All and None
        /// </summary>
        /// <param name="system"></param>
        /// <param name="all"></param>
        /// <param name="none"></param>
        /// <returns></returns>
        public static EntityQueryDesc ConstructQuery(this ComponentSystemBase system, ComponentType[] all, ComponentType[] none) {
            return new EntityQueryDesc() {
                Any = Array.Empty<ComponentType>(),
                None = none ?? Array.Empty<ComponentType>(),
                All = all ?? Array.Empty<ComponentType>()
            };
        }
        
        /// <summary>
        /// A shortcut for constructing an EntityArchetypeQuery
        /// </summary>
        /// <param name="system"></param>
        /// <param name="any"></param>
        /// <param name="none"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public static EntityQueryDesc ConstructQuery(this SystemBase system, ComponentType[]? any, ComponentType[]? none, ComponentType[] all) {
            return new EntityQueryDesc() {
                Any = any ?? Array.Empty<ComponentType>(),
                None = none ?? Array.Empty<ComponentType>(),
                All = all ?? Array.Empty<ComponentType>()
            };
        }

        /// <summary>
        /// A shortcut for constructing an EntityArchetypeQuery
        /// Accepts only All and None
        /// </summary>
        /// <param name="system"></param>
        /// <param name="all"></param>
        /// <param name="none"></param>
        /// <returns></returns>
        public static EntityQueryDesc ConstructQuery(this SystemBase system, ComponentType[] all, ComponentType[]? none) {
            return new EntityQueryDesc() {
                Any = Array.Empty<ComponentType>(),
                None = none ?? Array.Empty<ComponentType>(),
                All = all ?? Array.Empty<ComponentType>()
            };
        }

        public static T GetOrCreateSystemManaged<T>(this ComponentSystemBase self) where T : ComponentSystemBase {
            return self.World.GetOrCreateSystemManaged<T>();
        }
    }
}
