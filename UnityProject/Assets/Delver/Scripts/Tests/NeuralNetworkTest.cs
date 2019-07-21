using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Delver {
    class NeuralNetworkTest : MonoBehaviour {

        [SerializeField]
        private int hiddenLayerNeuronCount = 2;

        [SerializeField]
        private float stepSize = 0.01f;

        // Note here that Z is the label
        private Vector3[] dataList = new Vector3[] {
            new Vector3(1.2f, 0.7f, 1),
            new Vector3(-0.3f, -0.5f, -1),
            new Vector3(3.0f, 0.1f, 1),
            new Vector3(-0.1f, -1.0f, -1),
            new Vector3(-1.0f, 1.1f, -1),
            new Vector3(2.1f, -3.0f, 1)
        };

        private NeuralNetwork neuralNetwork;

        void Awake() {
            // Prepare the neural network
            this.neuralNetwork = new NeuralNetwork(2, this.hiddenLayerNeuronCount, this.stepSize, true);
            this.neuralNetwork.AddOutputLayer(1);
        }

        private int iteration = 0;
        private bool finished = false;

        void Update() {
            if(finished) {
                return;
            }

            ++this.iteration;

            foreach (Vector3 data in this.dataList) {
                Learn(data);
            }

            if (iteration % 20 == 0) {
                float accuracy = ComputeTrainingAccuracy();
                Debug.LogFormat("Accuracy at {0}: {1}", this.iteration, accuracy);

                this.finished = Comparison.TolerantEquals(accuracy, 1.0f);
            }
        }

        private void Learn(Vector3 testData) {
            // Single forward
            Forward(testData);

            // Single backward
            this.neuralNetwork.ResetGradients();

            // Use the difference as the tug
            float forwardValue = this.neuralNetwork.LastOutputLayer.GetNeuronAt(0).ForwardUnit.Value;
            float pull = 0;
            int label = (int)testData.z;
            if (label == 1 && forwardValue < 1) {
                // Pull up
                pull = 1.0f;
            }

            if (label == -1 && forwardValue > -1) {
                // Pull down
                pull = -1.0f;
            }
            
            this.neuralNetwork.LastOutputLayer.GetNeuronAt(0).ForwardUnit.Gradient = pull;
            this.neuralNetwork.Learn();
        }

        private NeuralInput input = new NeuralInput();

        private void Forward(Vector3 testData) {
            this.input.Clear();
            this.input.Add(testData.x);
            this.input.Add(testData.y);

            this.neuralNetwork.Forward(input);
        }

        private float ComputeTrainingAccuracy() {
            int correctCount = 0;
            for (int i = 0; i < this.dataList.Length; ++i) {
                Forward(this.dataList[i]);
                float forwardValue = this.neuralNetwork.LastOutputLayer.GetNeuronAt(0).ForwardUnit.Value;
                int predictedLabel = forwardValue > 0 ? 1 : -1;

                Debug.LogFormat("Data {0}: {1}", this.dataList[i], forwardValue);

                if (predictedLabel == (int)this.dataList[i].z) {
                    ++correctCount;
                }
            }

            return (float)correctCount / (float)this.dataList.Length;
        }

    }
}
