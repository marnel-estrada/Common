using Common;

using UnityEngine;

namespace Delver {
    internal class XorTest : MonoBehaviour {

        private const float ZERO_CUTOFF = 1.0f;

        private readonly GenericNeuronLayer inputLayer = new GenericNeuronLayer(2);

        // We used this to select data indexes
        private readonly Roulette<int> roulette = new Roulette<int>();

        [SerializeField]
        private Vector3[] dataList = {
            new Vector3(0, 1, 1), new Vector3(1, 0, 1), new Vector3(0, 0, 0), new Vector3(1, 1, 0)
        };

        private bool finished;
        private OutputLayer hiddenLayer;

        [SerializeField]
        private int hiddenLayerNeuronCount = 5;

        [SerializeField]
        private int inputLayerNeuronCount = 4;

        private int iteration;
        private OutputLayer outputLayer;

        private int[] selectionWeights;

        [SerializeField]
        private float stepSize = 0.01f;

        private void Awake() {
            this.selectionWeights = new int[this.dataList.Length];
            ResetSelectionWeights();
            ResetRoulette();

            // Prepare the neural network

            // Add relu neurons to the input layer equal to the number of state features
            for (int i = 0; i < this.inputLayerNeuronCount; ++i) {
                Gate neuron = new ReluNeuron(2, this.stepSize, true);
                neuron.Prepare();
                this.inputLayer.AddNeuron(neuron);
            }

            // Prepare the hidden layer
            this.hiddenLayer = new OutputLayer(this.inputLayer, this.hiddenLayerNeuronCount, this.stepSize);
            this.hiddenLayer.Prepare();

            // Prepare the output neuron
            this.outputLayer = new OutputLayer(this.hiddenLayer, 1, this.stepSize);
            this.outputLayer.Prepare();
        }

        private void Update() {
            if (this.finished) {
                return;
            }

            ++this.iteration;

            // Learn 10x in each frame with random data
            for (int i = 0; i < 10; ++i) {
                int index = Random.Range(0, this.dataList.Length);
                Learn(index);
            }

            UpdateWeights();

            if (this.iteration % 10 == 0) {
                ResetRoulette();
            }

            if (this.iteration % 20 == 0) {
                float accuracy = ComputeTrainingAccuracy();
                Debug.LogFormat("Accuracy at {0}: {1}", this.iteration, accuracy);

                this.finished = Comparison.TolerantEquals(accuracy, 1.0f);
            }
        }

        private void UpdateWeights() {
            for (int i = 0; i < this.dataList.Length; ++i) {
                Forward(this.dataList[i]);
                float forwardValue = this.outputLayer.GetNeuronAt(0).ForwardUnit.Value;
                int predictedLabel = forwardValue > ZERO_CUTOFF ? 1 : 0;

                if (predictedLabel == (int) this.dataList[i].z) {
                    // Correct label
                    this.selectionWeights[i] = (int) (this.selectionWeights[i] * 0.9f);
                    if (this.selectionWeights[i] < 1) {
                        this.selectionWeights[i] = 1;
                    }
                } else {
                    // Wrong label
                    ++this.selectionWeights[i];
                }
            }
        }

        private void ResetRoulette() {
            this.roulette.Clear();

            int totalWeight = 0;
            for (int i = 0; i < this.selectionWeights.Length; ++i) {
                totalWeight += this.selectionWeights[i];
            }

            for (int i = 0; i < this.selectionWeights.Length; ++i) {
                this.roulette.Add(i, this.selectionWeights[i] / (float) totalWeight);
            }
        }

        private void ResetSelectionWeights() {
            for (int i = 0; i < this.selectionWeights.Length; ++i) {
                this.selectionWeights[i] = 1;
            }
        }

        private void Learn(int index) {
            Vector3 testData = this.dataList[index];

            // Single forward
            Forward(testData);

            // Single backward
            this.outputLayer.ResetGradients();
            this.hiddenLayer.ResetGradients();
            this.inputLayer.ResetGradients();

            float forwardValue = this.outputLayer.GetNeuronAt(0).ForwardUnit.Value;
            float pull = 0;
            int label = (int) testData.z;
            if (label == 1 && forwardValue <= ZERO_CUTOFF) {
                // Pull up
                //pull = (ZERO_CUTOFF - forwardValue) + 0.01f; // Add a little bit since we are comparing for > ZERO_CUTOFF to determine as 1
                pull = 1;
            }

            if (label == 0 && forwardValue > ZERO_CUTOFF) {
                // Pull down
                pull = -1;
            }

            this.outputLayer.GetNeuronAt(0).ForwardUnit.Gradient = pull;
            this.outputLayer.Backward();
            this.hiddenLayer.Backward();
            this.inputLayer.Backward();

            this.outputLayer.Regularize();
            this.hiddenLayer.Regularize();
            this.inputLayer.Regularize();

            this.outputLayer.UpdateParameters();
            this.hiddenLayer.UpdateParameters();
            this.inputLayer.UpdateParameters();
        }

        private void Forward(Vector3 testData) {
            this.inputLayer.SetInputValue(0, testData.x);
            this.inputLayer.SetInputValue(1, testData.y);

            this.inputLayer.Forward();
            this.hiddenLayer.Forward();
            this.outputLayer.Forward();
        }

        private float ComputeTrainingAccuracy() {
            int correctCount = 0;
            for (int i = 0; i < this.dataList.Length; ++i) {
                Forward(this.dataList[i]);
                float forwardValue = this.outputLayer.GetNeuronAt(0).ForwardUnit.Value;
                int predictedLabel = forwardValue > ZERO_CUTOFF ? 1 : 0;

                Debug.LogFormat("Data {0}: {1}", this.dataList[i], forwardValue);

                if (predictedLabel == (int) this.dataList[i].z) {
                    ++correctCount;
                }
            }

            return correctCount / (float) this.dataList.Length;
        }
    }
}