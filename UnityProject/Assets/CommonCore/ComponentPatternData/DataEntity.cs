using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common {
    /// <summary>
    /// Holds the DataComponents
    /// </summary>
    [Serializable]
    public sealed class DataEntity {
        [SerializeReference]
        private List<DataComponent> components = new(); 
        
        private readonly Dictionary<int, DataComponent> componentMap = new();

        public T AddComponent<T>() where T : DataComponent, new() {
            PopulateMap();
            
            int id = ResolveId<T>();
            
            // Must not have the component yet
            Assertion.IsTrue(!this.componentMap.ContainsKey(id));

            T component = new();
            this.componentMap.Add(id, component);
            this.components.Add(component);

            return component;
        }

        // Adds a component by Type
        public DataComponent AddComponent(Type type) {
            PopulateMap();
            
            Assertion.NotNull(type);
            Assertion.IsTrue(typeof(DataComponent).IsAssignableFrom(type));
            int id = type.FullName.GetHashCode();
            
            // Must not have the component yet
            Assertion.IsTrue(!this.componentMap.ContainsKey(id));

            DataComponent? component = Activator.CreateInstance(type) as DataComponent;
            Assertion.NotNull(component);
            this.componentMap.Add(id, component);
            this.components.Add(component);

            return component;
        }

        public Option<T> GetComponent<T>() where T : DataComponent, new() {
            PopulateMap();
            
            int id = ResolveId<T>();
            Option<DataComponent> component = this.componentMap.Find(id);
            if (component.IsNone) {
                return Option<T>.NONE;
            }

            T castedComponent = (T)component.ValueOrError();
            return Option<T>.AsOption(castedComponent);
        }

        private static int ResolveId<T>() {
            Type type = typeof(T);
            Assertion.NotNull(type);
            return type.FullName.GetHashCode();
        }

        private void PopulateMap() {
            if (this.components.Count == this.componentMap.Count) {
                // Already populated
                return;
            }
            
            this.componentMap.Clear();
            foreach (DataComponent component in this.components) {
                Type type = component.GetType();
                int id = type.FullName.GetHashCode();
                this.componentMap[id] = component;
            }
        }

        public IReadOnlyList<DataComponent> Components => this.components;

        public bool HasComponents => this.components.Count > 0;

        public void RemoveComponent(DataComponent component) {
            this.components.Remove(component);
            this.componentMap.Remove(component.GetType().FullName.GetHashCode());
        }

        public DataEntity CreateCopy() {
            DataEntity copy = new();
            
            // Copy components
            foreach (DataComponent component in this.components) {
                copy.components.Add(component.CreateCopy());
            }
            
            copy.PopulateMap();

            return copy;
        }
    }
}
