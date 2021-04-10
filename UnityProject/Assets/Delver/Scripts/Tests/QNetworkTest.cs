using System.Collections;

using Common;
using Common.Logger;

using UnityEngine;

namespace Delver {
    /// <summary>
    ///     Example is taken from http://mnemstudio.org/path-finding-q-learning-tutorial.htm
    /// </summary>
    internal class QNetworkTest : MonoBehaviour {

        private const int STATE_COUNT = 7;

        private const int BUFFER_COUNT = 100;

        private const int SAMPLE_COUNT = 10;

        // The indeces are the state
        private readonly SimpleList<SimpleList<int>> actionMap = new SimpleList<SimpleList<int>>();
        private int bufferIndex;

        [SerializeField]
        private float discount = 0.8f;

        private bool episodeDone = true;

        [SerializeField]
        private float exploitationProbability = 0.1f;

        private bool finished;

        private readonly NeuralInput input = new NeuralInput();

        [SerializeField]
        private int inputLayerNeuronCount = 2;

        private int iteration;

        [SerializeField]
        private float learningRate = 0.8f;

        [SerializeField]
        private int m = 10; // Number of training for each q value

        private float maxGradient = float.MinValue;

        private NeuralNetwork neuralNetwork;

        [SerializeField]
        private string neuralNetworkName = "QNetworkTest";

        private readonly SimpleList<QNetworkTestReplay> replayBuffer = new SimpleList<QNetworkTestReplay>();

        [SerializeField]
        private float stepSize = 0.0001f;

        [SerializeField]
        private string streamingAssetsDirectory = "Data/NeuralNetwork";

        [SerializeField]
        private int targetState = 5;

        [SerializeField]
        private int trainingCount = 2000;

        private void Awake() {
            // Populate action map
            for (int i = 0; i < STATE_COUNT; ++i) {
                this.actionMap.Add(new SimpleList<int>());
            }

            AddStateAction(0, 4);
            AddStateAction(1, 3);
            AddStateAction(1, 5);
            AddStateAction(2, 3);
            AddStateAction(2, 6);
            AddStateAction(3, 1);
            AddStateAction(3, 2);
            AddStateAction(3, 6);
            AddStateAction(3, 4);
            AddStateAction(4, 0);
            AddStateAction(4, 5);
            AddStateAction(4, 3);
            AddStateAction(5, 4);
            AddStateAction(5, 1);
            AddStateAction(6, 3);
            AddStateAction(6, 2);

            // Prepare the neural network
            this.neuralNetwork = new NeuralNetwork(1, this.inputLayerNeuronCount, this.stepSize, false);
            this.neuralNetwork.AddOutputLayer(this.inputLayerNeuronCount + 100);
            this.neuralNetwork.AddOutputLayer(this.inputLayerNeuronCount + 50);
            this.neuralNetwork.AddOutputLayer(STATE_COUNT);
        }

        // Note here that the action is also the next state
        private void AddStateAction(int state, int action) {
            Assertion.IsTrue(!this.actionMap[state].Contains(action));
            this.actionMap[state].Add(action);
        }

        private void Update() {
            if (this.finished) {
                return;
            }

            if (this.episodeDone) {
                StartCoroutine(DoEpisode());
            }

            this.finished = this.iteration >= this.trainingCount;
            if (this.finished) {
                // Print the Q Table
                Debug.Log("Before Save");
                for (int i = 0; i < this.actionMap.Count; ++i) {
                    PrintActions(i);
                }

                // Save 
                string fullPath = Application.streamingAssetsPath + "/" + this.streamingAssetsDirectory;
                NeuralNetworkXmlWriter writer = new NeuralNetworkXmlWriter(fullPath, this.neuralNetworkName);
                writer.Write(this.neuralNetwork);

                // Load
                this.neuralNetwork =
                    NeuralNetworkXmlReader.Read(fullPath + $"/{this.neuralNetworkName}.xml");
                Debug.Log("Loaded Network");
                for (int i = 0; i < this.actionMap.Count; ++i) {
                    PrintActions(i);
                }
            }
        }

        private void PrintActions(int state) {
            SimpleList<int> actions = this.actionMap[state];
            for (int i = 0; i < actions.Count; ++i) {
                int action = actions[i];
                float qValue = GetQValue(state, action);
                Debug.LogFormat("{0}; {1}; {2}", state, action, qValue);
            }
        }

        private IEnumerator DoEpisode() {
            ++this.iteration;
            Debug.Log("Episode: " + this.iteration);
            this.episodeDone = false;

            if (this.iteration == this.trainingCount - 3) {
                Debug.Log("Breakpoint");
            }

            // pick a starting state
            int startState = -1;
            do {
                startState = Random.Range(0, STATE_COUNT);
            } while (startState == this.targetState);

            int currentState = startState;

            while (currentState != this.targetState) {
                int action = ResolveAction(currentState);
                int nextState = action; // Note here that the action is also the next state

                float reward = 0;
                if (nextState == this.targetState) {
                    reward = 1;
                }

                // Store to replay buffer
                QNetworkTestReplay currentReplay = new QNetworkTestReplay(currentState, action, reward);
                if (this.replayBuffer.Count < BUFFER_COUNT) {
                    this.replayBuffer.Add(currentReplay);
                } else {
                    // Buffer is already full
                    // Replace oldest entry
                    this.replayBuffer[this.bufferIndex] = currentReplay;
                    this.bufferIndex = (this.bufferIndex + 1) % BUFFER_COUNT;
                }

                if (this.replayBuffer.Count >= BUFFER_COUNT) {
                    // Update neural network by samples
                    for (int i = 0; i < SAMPLE_COUNT; ++i) {
                        QNetworkTestReplay sample = this.replayBuffer[Random.Range(0, this.replayBuffer.Count)];
                        Learn(sample.State, sample.Action, sample.Reward);
                    }

                    // Learn the latest action
                    Learn(currentReplay.State, currentReplay.Action, currentReplay.Reward);
                }

                currentState = nextState;
            }

            yield return null;

            // update the exploitation probability
            //this.exploitationProbability = (float)this.iteration / (float)this.trainingCount;
            this.episodeDone = true;
        }

        private void Learn(int state, int action, float reward) {
            // Compute new qValue
            // Computation is based from here https://en.wikipedia.org/wiki/Q-learning
            float oldValue = GetQValue(state, action);
            int nextState = action;

            float nextMaxQValue = GetMaxQValue(nextState);
            float targetValue = (1 - this.learningRate) * oldValue +
                this.learningRate * (reward + this.discount * nextMaxQValue);
            UpdateQValue(state, action, targetValue);
        }

        private int ResolveAction(int currentState) {
            // exploitation or exploration?
            bool exploitation = Comparison.TolerantLesserThanOrEquals(Random.value, this.exploitationProbability);
            if (exploitation) {
                return ResolveBestAction(currentState);
            }

            // Exploration
            // Return a random action
            SimpleList<int> actions = this.actionMap[currentState];

            return actions[Random.Range(0, actions.Count)];
        }

        private int ResolveBestAction(int currentState) {
            this.input.Clear();
            this.input.Add(currentState);

            this.neuralNetwork.Forward(this.input);

            float maxQValue = float.MinValue;
            int bestAction = -1;

            // Note that each output neuron corresponds to an action
            SimpleList<int> actions = this.actionMap[currentState];

            // Note here that we are evaluating only the possible actions from the current state
            // Not all the actions
            for (int i = 0; i < actions.Count; ++i) {
                float qValue = this.neuralNetwork.LastOutputLayer.GetNeuronAt(actions[i]).ForwardUnit.Value;
                if (qValue > maxQValue) {
                    maxQValue = qValue;
                    bestAction = actions[i];
                }
            }

            return bestAction;
        }

        private float GetMaxQValue(int state) {
            this.input.Clear();
            this.input.Add(state);

            this.neuralNetwork.Forward(this.input);

            float maxQValue = float.MinValue;

            SimpleList<int> actions = this.actionMap[state];

            // Note that each output neuron corresponds to an action
            // Note here that we are evaluating only the possible actions from the current state
            // Not all the actions
            for (int i = 0; i < actions.Count; ++i) {
                float qValue = this.neuralNetwork.LastOutputLayer.GetNeuronAt(actions[i]).ForwardUnit.Value;
                if (qValue > maxQValue) {
                    maxQValue = qValue;
                }
            }

            return maxQValue;
        }

        private float GetQValue(int state, int action) {
            this.input.Clear();
            this.input.Add(state);

            this.neuralNetwork.Forward(this.input);

            return this.neuralNetwork.LastOutputLayer.GetNeuronAt(action).ForwardUnit.Value;
        }

        private void UpdateQValue(int state, int action, float targetQValue) {
            // Train M times
            for (int i = 0; i < this.m; ++i) {
                this.neuralNetwork.ResetGradients();

                // Set the gradient
                // Set only to the action
                float currentValue = GetQValue(state, action);
                float gradient = targetQValue - currentValue;
                this.neuralNetwork.LastOutputLayer.GetNeuronAt(action).ForwardUnit.Gradient = gradient;

                if (Mathf.Abs(gradient) > this.maxGradient) {
                    this.maxGradient = Mathf.Abs(gradient);
                    QuickLogger.Log("Max Gradient: " + this.maxGradient);
                }

                this.neuralNetwork.Learn();
            }
        }

        private void SetGradient(int action, float gradient) {
            for (int i = 0; i < STATE_COUNT; ++i) {
                Gate neuron = this.neuralNetwork.LastOutputLayer.GetNeuronAt(i);
                if (i == action) {
                    neuron.ForwardUnit.Gradient = gradient;
                } else {
                    neuron.ForwardUnit.Gradient = 0;
                }
            }
        }
    }
}