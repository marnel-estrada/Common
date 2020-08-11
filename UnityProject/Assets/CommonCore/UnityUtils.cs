using UnityEngine;

using System.IO;

namespace Common {
    /**
     * General class for utility functions related to game object or game transform manipulations
     */
    public static class UnityUtils {
        /**
         * Sets parenthood among the specified transforms.
         */
        public static void SetAsParent(Transform parentTransform, Transform childTransform) {
            childTransform.parent = parentTransform;
        }

        /**
         * Returns a randomized boolean.
         */
        public static bool RandomBoolean() {
            int random = UnityEngine.Random.Range(0, 2); // zero or one

            return random > 0; // returns true if one
        }

        /**
         * Hides the specified object.
         */
        public static void HideObject(GameObject gameObject, bool recurseChildren = true) {
            Renderer[] renderers = null;

            if (recurseChildren) {
                renderers = gameObject.GetComponentsInChildren<Renderer>();
            } else {
                renderers = gameObject.GetComponents<Renderer>();
            }

            foreach (Renderer renderer in renderers) {
                renderer.enabled = false;
            }
        }

        /**
      * Shows the specified object.
      */
        public static void ShowObject(GameObject gameObject, bool recurseChildren = true) {
            Renderer[] renderers = null;

            if (recurseChildren) {
                renderers = gameObject.GetComponentsInChildren<Renderer>();
            } else {
                renderers = gameObject.GetComponents<Renderer>();
            }

            foreach (Renderer renderer in renderers) {
                renderer.enabled = true;
            }
        }

        /**
      * Clamps a float value.
      */
        public static float Clamp(float aValue, float min, float max) {
            if (aValue < min) {
                return min;
            }

            if (aValue > max) {
                return max;
            }

            return aValue;
        }

        /**
          * Clamps an int value.
          */
        public static int Clamp(int aValue, int min, int max) {
            if (aValue < min) {
                return min;
            }

            if (aValue > max) {
                return max;
            }

            return aValue;
        }

        /// <summary>
        /// Looks for a certain component in the specified object.
        /// (We use transform for the hierarchy.)
        /// </summary>
        /// <param name="transform"></param>
        /// <typeparam name="TComponentType"></typeparam>
        /// <returns></returns>
        public static Option<TComponentType> FindComponentThroughParent<TComponentType>(Transform transform) where TComponentType : Component {
            while (true) {
                TComponentType component = transform.GetComponent<TComponentType>();
                if (component == null) {
                    // component was not found, we search up through its parents
                    if (transform.parent == null) {
                        // no more parent, so there's no more path to search with
                        return Option<TComponentType>.NONE;
                    }

                    transform = transform.parent;

                    continue;
                }

                return Option<TComponentType>.Some(component);
            }
        }

        /**
      * Returns whether or not any game object in the transform heirarchy contains the specified name.
      */
        public static bool ContainsObjectWithName(Transform transform, string name) {
            if (name.Equals(transform.gameObject.name)) {
                return true;
            }

            // search down through children
            Transform foundInChildren = transform.Find(name);
            if (foundInChildren != null) {
                return true;
            }

            // search up through its parent
            Transform currentParent = transform.parent;
            do {
                if (currentParent != null) {
                    if (name.Equals(currentParent.gameObject.name)) {
                        return true;
                    }

                    currentParent = currentParent.parent;
                }
            } while (currentParent != null);

            // it is not found if it comes at this point
            return false;
        }

        /**
      * Searches for the transform with the specified name in the specified transform heirarchy.
      * This only searches down the heirarchy.
      */
        public static Option<Transform> FindTransformByName(Transform transformRoot, string name) {
            if (name == transformRoot.name) {
                return Option<Transform>.Some(transformRoot);
            }

            // search in children
            foreach (Transform childTransform in transformRoot) {
                Option<Transform> searchResult = FindTransformByName(childTransform, name);
                if (searchResult.IsSome) {
                    return searchResult;
                }
            }

            return Option<Transform>.NONE;
        }

        /**
         * Looks for the object with the specified name and retrieves the specified component.
         */
        public static TComponentType GetRequiredComponent<TComponentType>(string objectName)
            where TComponentType : Component {
            GameObject gameObject = GameObject.Find(objectName);
            Assertion.NotNull(gameObject, objectName);

            TComponentType component = gameObject.GetComponent<TComponentType>();
            Assertion.NotNull(component);

            return component;
        }

        /**
         * Computes the travel time based on the distance of two vectors and the specified velocity.
         */
        public static float ComputeTravelTime(Vector3 start, Vector3 destination, float velocity) {
            float distance = (destination - start).magnitude;

            return distance / velocity;
        }

        /**
         * Returns whether or not the two vectors are approximately equal.
         */
        public static bool TolerantEquals(Vector3 a, Vector3 b) {
            return Comparison.TolerantEquals(a.x, b.x) && Comparison.TolerantEquals(a.y, b.y) &&
                Comparison.TolerantEquals(a.z, b.z);
        }

        /**
        * Returns a random sign.
        */
        public static float RandomSign() {
            float random = UnityEngine.Random.Range(-1, 1);
            if (random < 0) {
                return -1;
            }

            return 1;
        }

        /**
         * Enables or disable all scripts in a game object.
         */
        public static void SetAllScriptsEnabled(GameObject gameObject, bool enabled, bool traverseChildren = false) {
            Assertion.NotNull(gameObject);

            MonoBehaviour[] scripts = gameObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts) {
                script.enabled = enabled;
            }

            // enable/disable renderers
            if (enabled) {
                ShowObject(gameObject);
            } else {
                HideObject(gameObject);
            }

            // enable/disable colliders
            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders) {
                collider.enabled = enabled;
            }

            if (traverseChildren) {
                Transform objectTransform = gameObject.transform;
                foreach (Transform child in objectTransform) {
                    SetAllScriptsEnabled(child.gameObject, enabled, traverseChildren);
                }
            }
        }

        /**
         * Sets the layer of a certain game object.
         */
        public static void SetLayer(GameObject go, string layerName, bool recurseToChildren = true) {
            int layerId = LayerMask.NameToLayer(layerName);
            SetLayer(go, layerId, recurseToChildren);
        }

        /**
         * Sets the layer of a certain game object.
         */
        public static void SetLayer(GameObject go, int layer, bool recurseToChildren = true) {
            Assertion.NotNull(go);
            go.layer = layer;

            if (!recurseToChildren) {
                return;
            }

            // recurse to children
            Transform transform = go.transform;
            foreach (Transform child in transform) {
                SetLayer(child.gameObject, layer, recurseToChildren);
            }
        }

        /**
         * Reads the contents of the specified text file path.
         */
        public static string ReadTextFile(string path) {
            Assertion.IsTrue(File.Exists(path), "File path does not exist: " + path);
            StreamReader reader = new StreamReader(path);
            try {
                return reader.ReadToEnd();
            } finally {
                reader.Close();
            }
        }

        /// <summary>
        /// Sets the sorting order of all renderers in the hierarchy starting with the specified root.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="order"></param>
        public static void SetSortingOrderInHierarchy(Transform root, int order) {
            Renderer renderer = root.GetComponent<Renderer>();
            if (renderer != null) {
                renderer.sortingOrder = order;
            }

            // Recurse to children (active only)
            foreach (Transform child in root) {
                if (child.gameObject.activeSelf) {
                    SetSortingOrderInHierarchy(child, order);
                }
            }
        }
    }
}