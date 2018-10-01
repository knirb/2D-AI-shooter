using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork {


    //Sizes
    public int nInput = 4;
    public int nOutput = 2;
    public int nHidden = 3;

    //Arrays for node values;
    private float[] input;
    private float[] hidden;
    private float[] output;

    public float[,] w; //Weights inputLayer rows - nInput, columns - hidden 
    public float[,] v; //Weights hidden layer

    public float lowerWeightLimit = -1f;
    public float higherWeightLimit = 1f;

    //Constructors for varying levels of specificity. 
    //Use public NeuralNetwork(int nIn, int nOut, int nHid) for your own network.

    #region CONSTRUCTORS
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
    #endregion

    //Generating random weights
    void GenerateWeights(int rows, int columns, out float[,] weights)
    {
        weights = new float[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                weights[i, j] = Random.Range(lowerWeightLimit, higherWeightLimit);
            }
        }

    }
    public void setWeightW(float[] wIn)
    {
        int count = 0;
        for (int i = 0; i < nInput; i++)
        {
            for (int j = 0; j < nHidden; j++)
            {
                w[j, i] = wIn[count];
                count++;
            }
        }
    }
    public void setWeightV(float[] vIn)
    {
        int count = 0;
        for (int i = 0; i < nHidden; i++)
        {
            for (int j = 0; j < nOutput; j++)
            {
                v[j, i] = vIn[count];
                count++;
            }
        }
    }
    public void setWeights(float[] wIn)
    {
        int count = 0;
        for (int i = 0; i < nHidden; i++)
        {
            for (int j = 0; j < nInput; j++)
            {
                w[j, i] = wIn[count];
                count++;
            }
        }
        for (int i = 0; i < nOutput; i++)
        {
            for (int j = 0; j < nHidden; j++)
            {
                wIn[count] = v[j, i];
                count++;
            }
        }
    }
   public void setWeights(float[] wIn, float[] vIn)
    {
        int count = 0;
        for (int i = 0; i < nInput; i++)
        {
            for (int j = 0; j < nHidden; j++)
            {
                w[j, i] = wIn[count];
                count++;
            }
        }

        count = 0;

        for (int i = 0; i < nHidden; i++)
        {
            for (int j = 0; j < nOutput; j++)
            {
                v[j, i] = vIn[count];
                count++;
            }
        }
    }

    //Feeds input to the network, returns the output. 
    public float[] CalculateOutput(float[] inp)
    {
        // multiply all inputs by w-weights for node 
        //-> pass it to node function (trying Relu first)
        //-> pass value on to output nodes, multiplying by v-weights
        //-> pass through node function
        //return output.

        input = inp;

        hidden = Vector.matVecMult(input, w); //To each hidden layer node we sum the values of all inputs*weight
        hidden = Relu(hidden); // Passing values through activation func.

        //TODO Add bias term;

        output = Vector.matVecMult(hidden, v); //To each output layer node we sum the values of all hidden node values*weights
        output = Softsign(output); // Maps values to between -1 and 1;
        
        return output;
    }

    public float[] GetWeights()
    {
        int count = 0;
        float[] ret = new float[nInput*nHidden + nHidden*nOutput];
        for (int i = 0; i < nHidden; i++)
        {
            for (int j = 0; j < nInput; j++)
            {
                ret[count] = w[j, i];
                count++;
            }
        }
        for (int i = 0; i < nOutput; i++)
        {
            for (int j = 0; j < nHidden; j++)
            {
                ret[count] = v[j, i];
                count++;
            }
        }
        return ret;
    }
    
    #region Activation Functions

    public float[] Relu(float[] inp) // RELU(x) returns 0 if x is less than zero, otherwhise x.
    {
        for (int i = 0; i < inp.Length; i++)
        {
            if (inp[i] < 0)
                inp[i] = 0;
        }
        return inp;
    }
    public float[] Softsign(float[] inp) //Returns a value scaled to be between -1 and 1;
    {
        for (int i = 0; i < inp.Length; i++)
        {
            inp[i] = inp[i] / (1 + Mathf.Abs(inp[i]));
        }
        return inp;
    }
    #endregion


}
