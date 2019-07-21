using System.IO;

using Common;
using Common.Xml;

namespace Delver {
    /// <summary>
    /// Utility class that reads a neural network from an XML file
    /// </summary>
    public static class NeuralNetworkXmlReader {
        /// <summary>
        /// Reads the XML and returns the NeuralNetwork it represents
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static NeuralNetwork Read(string xmlPath) {
            string xmlText = File.ReadAllText(xmlPath);
            SimpleXmlReader reader = new SimpleXmlReader();

            SimpleXmlNode rootNode = reader.Read(xmlText).FindFirstNodeInChildren("NeuralNetwork");

            int inputCount = rootNode.GetAttributeAsInt("inputCount");
            int inputLayerNeuronCount = rootNode.GetAttributeAsInt("inputLayerNeuronCount");
            float stepSize = rootNode.GetAttributeAsFloat("stepSize");
            bool rectified = rootNode.GetAttributeAsBool("rectified");

            NeuralNetwork neuralNetwork = new NeuralNetwork(inputCount, inputLayerNeuronCount, stepSize, rectified);
            AddOutputLayers(rootNode, neuralNetwork);
            ProcessLayers(rootNode, neuralNetwork);

            return neuralNetwork;
        }

        private static void AddOutputLayers(SimpleXmlNode node, NeuralNetwork neuralNetwork) {
            for(int i = 0; i < node.Children.Count; ++i) {
                SimpleXmlNode child = node.Children[i];
                if("OutputLayer".Equals(child.TagName)) {
                    int neuronCount = child.GetAttributeAsInt("neuronCount");
                    neuralNetwork.AddOutputLayer(neuronCount);
                }
            }
        }

        private static void ProcessLayers(SimpleXmlNode node, NeuralNetwork neuralNetwork) {
            int layerIndex = 0;
            for (int i = 0; i < node.Children.Count; ++i) {
                SimpleXmlNode child = node.Children[i];
                if ("Layer".Equals(child.TagName)) {
                    ReadLayer(child, neuralNetwork.GetLayerAt(layerIndex));
                    ++layerIndex;
                }
            }
        }

        private static void ReadLayer(SimpleXmlNode node, NeuronLayer layer) {
            int neuronIndex = 0;
            for (int i = 0; i < node.Children.Count; ++i) {
                SimpleXmlNode child = node.Children[i];
                if("Neuron".Equals(child.TagName)) {
                    Gate neuron = layer.GetNeuronAt(neuronIndex);
                    ReadNeuron(child, neuron);
                    ++neuronIndex;
                }
            }
        }

        private static readonly SimpleList<float> WEIGHTS = new SimpleList<float>();

        private static void ReadNeuron(SimpleXmlNode node, Gate neuron) {
            // Collect weights
            WEIGHTS.Clear();

            for(int i = 0; i < node.Children.Count; ++i) {
                SimpleXmlNode child = node.Children[i];
                if("Weight".Equals(child.TagName)) {
                    WEIGHTS.Add(child.GetAttributeAsFloat("value"));
                }
            }

            neuron.Weights = WEIGHTS.ToArray();
        }
    }
}
