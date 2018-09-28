using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork {

    // Use this for initialization

    public int nInput = 4;
    public int nOutput = 2;
    public int nHidden = 3;

    public Vector2 direction;

    private float[] input;
    private float[] hidden;
    private float[] output;

    public float[,] w; //Weights inputLayer rows - nInput, columns - hidden 
    public float[,] v; //Weights hidden layer



    public NeuralNetwork() {

        GenerateWeights(nInput, nHidden, out w);
        GenerateWeights(nHidden, nOutput, out v);

    }
    public NeuralNetwork(int nIn, int nOut)
    {

        GenerateWeights(nInput, nHidden, out w);
        GenerateWeights(nHidden, nOutput, out v);

        nInput = nIn;
        nOutput = nOut;

    }
    public NeuralNetwork(int nIn, int nOut, int nHid)
    {

        GenerateWeights(nInput, nHidden, out w);
        GenerateWeights(nHidden, nOutput, out v);

        nInput = nIn;
        nOutput = nOut;
        nHidden = nHid;

    }
    void GenerateWeights(int rows, int columns, out float[,] weights)
    {
        weights = new float[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                weights[i, j] = Random.Range(-1f, 1f);
            }
        }

    }
    public float[] CalculateOutput(float[] inp)
    {
        // multiply all inputs by w-weights for node 
        //-> pass it to node function (trying Relu first)
        //-> pass value on to output nodes, multiplying by v-weights
        input = inp;

        hidden = Vector.matVecMult(input, w); //To each hidden layer node we sum the values of all inputs*weight
        hidden = Relu(hidden); // Passing values through activation func.

        //TODO Add bias term;

        output = Vector.matVecMult(hidden, v); //To each output layer node we sum the values of all hidden node values*weights
        output = Softsign(output); // Maps values to between -1 and 1;
        
        return output;
    }

    public float[] Relu(float[] inp) // RELU(x) returns 0 if x is less than zero, otherwhise x.
    {
        for (int i = 0; i < inp.Length; i++)
        {
            if (inp[i] < 0)
                inp[i] = 0;
        }
        return inp;
    }
    public float[] Softsign(float[] inp)
    {
        for (int i = 0; i < inp.Length; i++)
        {
            inp[i] = inp[i] / (1 + Mathf.Abs(inp[i]));
        }
        return inp;
    }

}
