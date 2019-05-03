using Unity.Entities;
using UnityEngine;

namespace CommonEcs.Scripts {
    public static class GameObjectEntityExtensions {
        /// <summary>
        /// Add Component to GameObject and associated entity (if any).
        /// </summary>
        public static T AddEntityComponent<T>(this GameObject go) where T : Component {
            T newComponent = go.GetComponent<T>();
            if (newComponent) {
                // Prevent user from adding more than one of the same type of
                // component since entities do not supported this.
                Debug.LogWarning(go + " already has a " + newComponent.GetType() +
                                 ". Only one component of each type is supported.");

                // Return already existing component.
                return newComponent;
            }

            // Add new component to GameObject.
            newComponent = go.AddComponent<T>();

            // Only execute entity related code if required GameObjectEntity component exist.
            GameObjectEntity goEntity = go.GetComponent<GameObjectEntity>();
            if (!goEntity) {
                return newComponent;
            }

            // Get the entity associated with this GameObject.
            Entity entity = goEntity.Entity;

            // Get entity manager.
            EntityManager manager = World.Active.EntityManager;

            // Add component to associated entity.
            manager.AddComponent(entity, newComponent.GetType());


            return newComponent;
        }

        /// <summary>
        /// Remove Component from GameObject and it's associated entity (if any).
        /// </summary>
        public static void RemoveEntityComponent<T>(this GameObject go) where T : Component {
            T componentToRemove = go.GetComponent<T>();
            if (componentToRemove == null) {
                // Component is null so just exit.
                return;
            }

            // Remove component from GameObject.
            Object.Destroy(componentToRemove);

            // Only execute entity related code if required GameObjectEntity component exist.
            GameObjectEntity goEntity = go.GetComponent<GameObjectEntity>();
            if (goEntity == null) {
                return;
            }

            // Get the entity associated with this GameObject.
            Entity entity = goEntity.Entity;

            // Get entity manager.
            EntityManager manager = World.Active.EntityManager;

            // Remove component from associated entity.
            manager.RemoveComponent(entity, componentToRemove.GetType());
        }
    }
}