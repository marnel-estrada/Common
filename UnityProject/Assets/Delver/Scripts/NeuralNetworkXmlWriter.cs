using System;
using System.Xml;

using Common;

namespace Delver {
    public class NeuralNetworkXmlWriter {
        private readonly string directoryPath;
        private readonly string name; // Name of the neural net

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="name"></param>
        public NeuralNetworkXmlWriter(string directoryPath, string name) {
            this.directoryPath = directoryPath;
            this.name = name;
        }

        /// <summary>
        /// Writes the neural network as an XML file
        /// </summary>
        /// <param name="neuralNetwork"></param>
        public void Write(NeuralNetwork neuralNetwork) {
            string filename = string.Format("{0}/{1}.xml", this.directoryPath, this.name);
            Write(neuralNetwork, filename);
        }

        /// <summary>
        /// Writes the neural network to the specified filename (can be absolute path)
        /// </summary>
        /// <param name="neuralNetwork"></param>
        /// <param name="filename"></param>
        public static void Write(NeuralNetwork neuralNetwork, string filename) {
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true, NewLineChars = "\r\n" };
            using (XmlWriter writer = XmlWriter.Create(filename, settings)) {
                writer.WriteStartDocument();
                writer.WriteStartElement("NeuralNetwork");

                // Neural network attributes
                writer.WriteAttributeString("inputCount", neuralNetwork.InputCount.ToString());
                writer.WriteAttributeString("inputLayerNeuronCount", neuralNetwork.InputLayerNeuronCount.ToString());
                writer.WriteAttributeString("stepSize", neuralNetwork.StepSize.ToString());
                writer.WriteAttributeString("rectified", neuralNetwork.Rectified.ToString());

                DateTime now = DateTime.Now;
                string timestamp = string.Format("{0:MMMM dd, yyyy (HH mm ss)}", now);
                writer.WriteAttributeString("timestamp", timestamp);

                WriteOutputLayers(writer, neuralNetwork);

                // Write each layer
                for(int i = 0; i < neuralNetwork.LayerCount; ++i) {
                    WriteLayer(writer, neuralNetwork.GetLayerAt(i));
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private static void WriteOutputLayers(XmlWriter writer, NeuralNetwork neuralNetwork) {
            // Note that every layer beyond the first are output layers
            for(int i = 1; i < neuralNetwork.LayerCount; ++i) {
                NeuronLayer layer = neuralNetwork.GetLayerAt(i);

                writer.WriteStartElement("OutputLayer");
                writer.WriteAttributeString("neuronCount", layer.NeuronCount.ToString());
                writer.WriteEndElement();
            }
        }

        private static void WriteLayer(XmlWriter writer, NeuronLayer layer) {
            writer.WriteStartElement("Layer");

            // Write each neuron
            for(int i = 0; i < layer.NeuronCount; ++i) {
                Gate neuron = layer.GetNeuronAt(i);
                WriteNeuron(writer, neuron);
            }

            writer.WriteEndElement();
        }

        private static void WriteNeuron(XmlWriter writer, Gate neuron) {
            writer.WriteStartElement("Neuron");

            float[] weights = neuron.Weights;
            Assertion.AssertNotNull(weights); // Neuron should have weights

            // Write weights
            for(int i = 0; i < weights.Length; ++i) {
                writer.WriteStartElement("Weight");
                writer.WriteAttributeString("value", weights[i].ToString("R")); // R format here is for maximum precision
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
