using System.Collections.Generic;
using UnityEngine;
using System;

public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers;
    private float[][] neurons;
    private float[][][] weights;

    private float fitness;
    public float Fitness
    {
        get { return fitness; }
        set { fitness = value; }
    }

    private float value;

    /// <summary>
    /// Parent constructor 
    /// </summary>
    /// <param name="layers"></param>
    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        InitialiseNeurons();
        InitialiseWeights();
    }

    /// <summary>
    /// Deep copy constructor
    /// </summary>
    /// <param name="duplicate"></param>
    public NeuralNetwork(NeuralNetwork duplicate)
    {
        // Copy layers
        layers = new int[duplicate.layers.Length];
        for (int i = 0; i < duplicate.layers.Length; i++)
        {
            layers[i] = duplicate.layers[i];
        }

        // Copy neurons
        InitialiseNeurons();

        // Copy weights
        InitialiseWeights();
        DuplicateWeights(duplicate.weights);
    }

    /// <summary>
    /// Create the array of neurons
    /// </summary>
    private void InitialiseNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();

        // Loop through all layers
        for (int i = 0; i < layers.Length; i++)
        {
            // Add layer to the list of neurons
            neuronsList.Add(new float[layers[i]]);
        }

        neurons = neuronsList.ToArray();
    }

    /// <summary>
    /// Create the array of weights
    /// </summary>
    private void InitialiseWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();

        for (int i = 0; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int prevNeurons = layers[i - 1];

            // Loop through all neurons
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[prevNeurons];

                // Randomise the weight values between -1 and 1
                for (int k = 0; k < prevNeurons; k++)
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }

                layerWeightsList.Add(neuronWeights);
            }

            // Convert to 2D array
            weightsList.Add(layerWeightsList.ToArray());
        }

        // Convert to 3D array
        weights = weightsList.ToArray();
    }

    /// <summary>
    /// Feed forward the neural network using the parsed inputs
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    public float[] FeedForward(float[] inputs)
    {
        // Assign input layer neurons
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        // Loop through layers with neurons that have weights
        for (int i = 1; i < layers.Length; i++)
        {
            // Loop through neurons
            for (int j = 0; j < neurons[i].Length; j++)
            {
                value = 0.25f;

                // Loop through all neurons in the previous layer
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    float currentWeight = weights[i - 1][j][k];
                    float currentNeuron = neurons[i - 1][k];

                    // Calculate weight value for current neuron
                    value += currentWeight * currentNeuron;
                }

                // Convert weight value to between -1 and 1 with hyperbolic tangent activation
                neurons[i][j] = (float)Math.Tanh(value);
            }
        }

        // Returns the output layer
        return neurons[neurons.Length - 1];
    }

    public void SGD()
    {

    }

    /// <summary>
    /// Alters the weights of the neural network
    /// </summary>
    public void IterateNetwork()
    {
        float weight;

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    // Access each weight value in the network
                    weight = weights[i][j][k];

                    // Mutate each weight
                    weights[i][j][k] = Mutate(weight);
                }
            }
        }
    }

    /// <summary>
    /// Mutates a specific weight randomly according to 4 different types of mutation
    /// </summary>
    /// <param name="weight"></param>
    /// <returns></returns>
    private float Mutate(float weight)
    {
        float returnNum;
        float rndNum = UnityEngine.Random.Range(-0.5f, 0.5f) * 1000f;

        // Apply 4 different types of mutation (with the final statement being no mutation)
        if (rndNum <= 6f)
        {
            // Increase by 0% - 100%
            returnNum = weight * UnityEngine.Random.Range(0f, 1f) + 1f;
        }
        else if (rndNum <= 8f)
        {
            // Decrease by 0% - 100%
            returnNum = weight * UnityEngine.Random.Range(0f, 1f);
        }
        else if (rndNum <= 2f)
        {
            // Flip sign
            returnNum = weight * -1f;
        }
        else if (rndNum <= 4f)
        {
            // Assign a random number between -1 and 1
            returnNum = UnityEngine.Random.Range(-0.5f, 0.5f);
        }
        else
        {
            // No mutation
            returnNum = weight;
        }

        return returnNum;
    }

    /// <summary>
    /// Creates a duplicate of the weights from the duplicate network
    /// </summary>
    /// <param name="duplicateWeights"></param>
    private void DuplicateWeights(float[][][] duplicateWeights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    // Loop through each weight in the duplicate network
                    weights[i][j][k] = duplicateWeights[i][j][k];
                }
            }
        }
    }

    public void AddFitness(float fitness)
    {
        this.fitness += fitness;
    }

    /// <summary>
    /// Compare two neural networks based on their fitness
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(NeuralNetwork other)
    {
        if (other == null)
        {
            return 1;
        }

        if (fitness > other.fitness)
        {
            return 1;
        }
        else if (fitness < other.fitness)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}
