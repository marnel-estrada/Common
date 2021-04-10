using Common;

using UnityEngine;

namespace GoapBrain {
    class CollectorGameSystem : MonoBehaviour {
        [SerializeField]
        private Camera referenceCamera; // Used for input

        [SerializeField]
        private Collector collector;

        [SerializeField]
        private ShinyObjectPool shinyPool;

        [SerializeField]
        private Transform processor;

        [SerializeField]
        private GameObject collectorPrefab;

        [SerializeField]
        private GameObject shinyObjectPrefab;

        private void Awake() {
            Assertion.NotNull(this.referenceCamera);
            Assertion.NotNull(this.collector);
            Assertion.NotNull(this.shinyPool);
            Assertion.NotNull(this.processor);
            Assertion.NotNull(this.collectorPrefab);
            Assertion.NotNull(this.shinyObjectPrefab);
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                // Add a shiny object
                Vector3 worldPosition = referenceCamera.ScreenToWorldPoint(Input.mousePosition);
                worldPosition.z = 0; // Z doesn't matter

                // Instantiate a new shiny object and add it to the collector
                GameObject go = Instantiate(this.shinyObjectPrefab, worldPosition, Quaternion.identity);
                ShinyObject shinyObject = go.GetComponent<ShinyObject>();
                Assertion.NotNull(shinyObject);

                this.shinyPool.Add(shinyObject);
            }

            if(Input.GetMouseButtonDown(1)) {
                // Add a collector
                Vector3 worldPosition = referenceCamera.ScreenToWorldPoint(Input.mousePosition);
                worldPosition.z = 0; // Z doesn't matter

                GameObject go = Instantiate(this.collectorPrefab, worldPosition, Quaternion.identity);
                Collector collector = go.GetComponent<Collector>();
                Assertion.NotNull(collector);
                collector.Pool = this.shinyPool;
                collector.Processor = this.processor;
            }
        }
    }
}
