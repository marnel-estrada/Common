using System;
using System.Collections.Generic;

namespace Common {
    /// <summary>
    /// Common interface for class that can support component data.
    /// We made this so we can create a common editor renderer for such data.
    /// </summary>
    public interface IDataComponentContainer {
        T AddComponent<T>() where T : DataComponent, new();

        DataComponent AddComponent(Type type);

        bool HasComponents { get; }

        IReadOnlyList<DataComponent> Components { get; }

        void RemoveComponent(DataComponent component);
    }
}