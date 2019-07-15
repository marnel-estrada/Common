using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Delver {
    class LinearNeuronTest : MonoBehaviour {

        private LinearNeuron neuron = new LinearNeuron(2, 0.01f);

        // Note here that Z is the label
        private Vector3[] dataList = new Vector3[] {
            new Vector3(1.2f, 0.7f, 1),
            new Vector3(-0.3f, -0.5f, -1),
            new Vector3(3.0f, 0.1f, 1),
            new Vector3(-0.1f, -1.0f, -1),
            new Vector3(-1.0f, 1.1f, -1),
            new Vector3(2.1f, -3.0f, 1),
            //new Vector3(-5, 4, 1)
        };

        void Awake() {
            this.neuron.Prepare();
            TestZeroBackProp();
        }

        [SerializeField]
        private Vector3 quickTestData = new Vector3(1, 1, 2);

        [SerializeField]
        private float quickTestStepSize = 0.001f;

        private void QuickTest() {
            LinearNeuron quickNeuron = new LinearNeuron(2, this.quickTestStepSize);
            quickNeuron.Prepare();

            // single forward
            quickNeuron.SetInputValue(0, this.quickTestData.x);
            quickNeuron.SetInputValue(1, this.quickTestData.y);
            quickNeuron.Forward();
            Debug.Log("Value before backprop: " + quickNeuron.ForwardUnit.Value);

            // single backward
            quickNeuron.ResetGradients();

            // Compute the pull
            float pull = this.quickTestData.z - quickNeuron.ForwardUnit.Value;
            quickNeuron.ForwardUnit.Gradient = pull;

            quickNeuron.Backward();
            quickNeuron.Regularize();
            quickNeuron.UpdateWeights();

            // forward after backprop
            quickNeuron.SetInputValue(0, this.quickTestData.x);
            quickNeuron.SetInputValue(1, this.quickTestData.y);
            quickNeuron.Forward();
            Debug.Log("Value after backprop: " + quickNeuron.ForwardUnit.Value);
        }

        private void TestZeroBackProp() {
            LinearNeuron testNeuron = new LinearNeuron(2, this.quickTestStepSize);
            testNeuron.Prepare();

            Learn(testNeuron, this.quickTestData, 0);
            Learn(testNeuron, this.quickTestData, 1);
            Learn(testNeuron, this.quickTestData, 0);
            Learn(testNeuron, this.quickTestData, 0);
        }

        private static void Learn(LinearNeuron neuron, Vector3 testData, int label) {
            Debug.Log("Learning Label: " + label);

            // single forward
            neuron.SetInputValue(0, testData.x);
            neuron.SetInputValue(1, testData.y);
            neuron.Forward();
            Debug.Log("Value before backprop: " + neuron.ForwardUnit.Value);

            // single backward
            neuron.ResetGradients();
            neuron.ForwardUnit.Gradient = label;
            neuron.Backward();
            neuron.Regularize();
            neuron.UpdateWeights();

            neuron.SetInputValue(0, testData.x);
            neuron.SetInputValue(1, testData.y);
            neuron.Forward();
            Debug.Log("Value after backprop: " + neuron.ForwardUnit.Value);
        }

        private int iteration = 0;
        private int dataIndex = 0;
        private bool finished = false;

        /// <summary>
        /// Run until it learns
        /// </summary>
        void Update() {
            //if(this.finished) {
            //    return;
            //}

            ++this.iteration;
            
            Learn(this.dataList[this.dataIndex]);
            dataIndex = (dataIndex + 1) % this.dataList.Length;
            float accuracy = ComputeTrainingAccuracy();
            Debug.Log("Accuracy: " + accuracy + " at " + this.iteration);

            this.finished = Comparison.TolerantGreaterThanOrEquals(accuracy, 1.0f);
            if(this.finished) {
                //this.neuron.PrintParameters();
            }
        }

        private void Learn(Vector3 data) {
            Forward(data);
            Backward((int)data.z);
            this.neuron.UpdateWeights();
        }

        private void Forward(Vector3 data) {
            // Set the parameters
            this.neuron.SetInputValue(0, data.x);
            this.neuron.SetInputValue(1, data.y);

            this.neuron.Forward();
        }

        private void Backward(int label) {
            this.neuron.ResetGradients();

            // Compute the pull
            float pull = 0;
            if(label == 1 && this.neuron.ForwardUnit.Value < 1) {
                pull = 1; // Score was low, pull up
            }

            if(label == -1 && this.neuron.ForwardUnit.Value > -1) {
                pull = -1; // Score was too high for a negative example, pull down
            }

            this.neuron.ForwardUnit.Gradient = pull;
            this.neuron.Backward();
            this.neuron.Regularize();
        }

        private float ComputeTrainingAccuracy() {
            int correctCount = 0;
            for(int i = 0; i < this.dataList.Length; ++i) {
                Forward(this.dataList[i]);
                int predictedLabel = this.neuron.ForwardUnit.Value > 0 ? 1 : -1;

                if(predictedLabel == (int)this.dataList[i].z) {
                    ++correctCount;
                }
            }

            return (float)correctCount / (float)this.dataList.Length;
        }

    }
}
