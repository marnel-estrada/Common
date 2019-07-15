using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Delver {
    class NeuralNetworkWriterTest : MonoBehaviour {

        [SerializeField]
        private string streamingAssetsDirectory = "Data/NeuralNetwork";

        [SerializeField]
        private string neuralNetworkName = "Test";

        private float[] dataSet = new float[] {
            0,
            1,
            1.5f,
            2.1f,
            2,
            1.1f,
            -0.5f
        };

        void Awake() {
            // Prepare the neural network
            NeuralNetwork neuralNetwork = new NeuralNetwork(1, 10, 0.00001f, false);
            neuralNetwork.AddOutputLayer(110);
            neuralNetwork.AddOutputLayer(60);
            neuralNetwork.AddOutputLayer(7);

            Debug.Log("Before save");
            RunData(neuralNetwork);

            string fullPath = Application.streamingAssetsPath + "/" + this.streamingAssetsDirectory;
            NeuralNetworkXmlWriter writer = new NeuralNetworkXmlWriter(fullPath, this.neuralNetworkName);
            writer.Write(neuralNetwork);

            Debug.LogFormat("Neural Network {0} was written successfully!", this.name);

            // Load
            NeuralNetwork loadedNetwork = NeuralNetworkXmlReader.Read(fullPath + string.Format("/{0}.xml", this.neuralNetworkName));
            Debug.Log("Loaded Network");
            RunData(loadedNetwork);

            // Copy
            NeuralNetwork copy = NeuralNetwork.Copy(loadedNetwork);
            Debug.Log("Copied Network");
            RunData(copy);
        }

        private void PrintLayer(NeuronLayer layer) {
            for(int i = 0; i < layer.NeuronCount; ++i) {
                float[] weights = layer.GetNeuronAt(i).Weights;
                foreach(float weight in weights) {
                    Debug.Log("Weight: " + weight);
                }
            }
        }

        private NeuralInput input = new NeuralInput();

        private void RunData(NeuralNetwork nn) {
            for(int i = 0; i < this.dataSet.Length; ++i) {
                this.input.Clear();
                this.input.Add(this.dataSet[i]);

                nn.Forward(this.input);
                float forwardValue = nn.LastOutputLayer.GetNeuronAt(0).ForwardUnit.Value;

                Debug.LogFormat("Data {0}: {1}", this.dataSet[i], forwardValue);
            }
        }

    }
}
