using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Delver {
    /// <summary>
    /// A simple Neural Network test
    /// </summary>
    class LineRegressionTest : MonoBehaviour {

        // We are solving for y = mx + b;
        [SerializeField]
        private float slope; // This is the m

        [SerializeField]
        private float coordinate; // This is the b

        [SerializeField]
        private float stepSize = 0.0001f;

        [SerializeField]
        private int inputLayerNeuronCount = 1;

        [SerializeField]
        private int dataCount = 1;

        [SerializeField]
        private float regularizationStrength = 0.1f;

        [SerializeField]
        private float cutoffCost = 0.16f;

        private NeuralNetwork neuralNetwork;

        private Vector2[] dataList;

        void Awake() {
            this.neuralNetwork = new NeuralNetwork(1, this.inputLayerNeuronCount, this.stepSize, false);
            this.neuralNetwork.AddOutputLayer(1);

            // Prepare data
            this.dataList = new Vector2[dataCount];
            for(int i = 0; i < dataCount; ++i) {
                float x = UnityEngine.Random.Range(-1.0f, 1.0f);
                float y = (this.slope * x) + this.coordinate; // This is our target
                this.dataList[i] = new Vector2(x, y);
            }
        }

        private int iteration = 0;
        private bool finished = false;

        void Update() {
            if(this.finished) {
                return;
            }

            for (int i = 0; i < 20; ++i) {
                Learn();
            }
            
            ++this.iteration;
            if(this.iteration % 20 == 0) {
                CheckAccuracy();
            }
        }

        private void CheckAccuracy() {
            float cost = ComputeCost();
            Debug.LogFormat("Cost at {0}: {1}", this.iteration, cost);

            // Regularization cost
            float regularizationCost = this.regularizationStrength * this.neuralNetwork.RegularizationCost;
            Debug.Log("Regularization Cost: " + regularizationCost);

            this.finished = cost < this.cutoffCost;

            if(this.finished) {
                Debug.Log(this.neuralNetwork.ToString());
            }
        }

        private NeuralInput input = new NeuralInput();

        private void Learn() {
            // Prepare a test data
            float x = UnityEngine.Random.Range(-1.0f, 1.0f);
            float y = ComputeTargetValue(x); // This is our target
            Vector2 data = new Vector2(x, y);

            input.Clear();
            input.Add(data.x);

            this.neuralNetwork.Forward(input);

            this.neuralNetwork.ResetGradients();

            // Set the gradient
            Gate outputGate = this.neuralNetwork.LastOutputLayer.GetNeuronAt(0);
            float forwardValue = outputGate.ForwardUnit.Value;
            outputGate.ForwardUnit.Gradient = 2 * (data.y - forwardValue);

            this.neuralNetwork.Learn();
        }

        private float ComputeTargetValue(float x) {
            return (this.slope * x) + this.coordinate;
            //return Mathf.Sin(x);
        }

        private float ComputeCost() {
            float totalCost = 0;

            for(int i = 0; i < dataCount; ++i) {
                float x = UnityEngine.Random.Range(-1.0f, 1.0f);
                float y = ComputeTargetValue(x); // This is our target
                Vector2 data = new Vector2(x, y);

                // Prepare input
                input.Clear();
                input.Add(data.x);

                this.neuralNetwork.Forward(input);
                Gate outputGate = this.neuralNetwork.LastOutputLayer.GetNeuronAt(0);
                float forwardValue = outputGate.ForwardUnit.Value;
                
                float cost = (data.y - forwardValue);

                Debug.LogFormat("x: {0}; y: {1}; forward: {2}; cost: {3}", data.x, data.y, forwardValue, cost);

                totalCost += cost * cost;
            }

            Debug.Log(this.neuralNetwork.ToString());
            return totalCost;
        }

    }
}
