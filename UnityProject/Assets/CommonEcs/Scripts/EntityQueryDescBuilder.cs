using System;
using System.Collections.Generic;

using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A utility class for building EntityQueryDesc using chained calls.
    /// </summary>
    public class EntityQueryDescBuilder {
        private readonly List<ComponentType> all = new List<ComponentType>();
        private readonly List<ComponentType> any = new List<ComponentType>();
        private readonly List<ComponentType> none = new List<ComponentType>();
        
        private EntityQueryDescBuilder() {
        }

        public static EntityQueryDescBuilder New() {
            return new EntityQueryDescBuilder();
        }

        public EntityQueryDescBuilder With<T>() where T : IComponentData {
            this.all.Add(ComponentType.ReadWrite<T>());
            return this;
        }

        public EntityQueryDescBuilder WithReadOnly<T>() where T : unmanaged, IComponentData {
            this.all.Add(ComponentType.ReadOnly<T>());
            return this;
        }

        public EntityQueryDescBuilder WithAny<T>() where T : IComponentData {
            this.any.Add(ComponentType.ReadWrite<T>());
            return this;
        }

        public EntityQueryDescBuilder WithAnyReadOnly<T>() where T : unmanaged, IComponentData {
            this.any.Add(ComponentType.ReadOnly<T>());
            return this;
        }

        public EntityQueryDescBuilder WithNone<T>() where T : IComponentData {
            this.none.Add(typeof(T));
            return this;
        }

        public EntityQueryDesc Complete() {
            return new EntityQueryDesc() {
                Any = this.any.Count > 0 ? this.any.ToArray() : Array.Empty<ComponentType>(),
                None = this.none.Count > 0 ? this.none.ToArray() : Array.Empty<ComponentType>(),
                All = this.all.Count > 0 ? this.all.ToArray() : Array.Empty<ComponentType>()
            };
        }
    }
}