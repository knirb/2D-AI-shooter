using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork {

    public int nInput;
    public int nOutput;

    //Used for multiLayered networks;
    public int[] hiddenSizes;
    public List<float[,]> wm;
    public int nLayers;
    private int totalNoWeights;

    public float lowerWeightLimit = -1f;
    public float higherWeightLimit = 1f;

    //Constructors for varying levels of specificity.   
    //Use public NeuralNetwork(int nIn, int nOut, int nHid) for your own network.

    #region CONSTRUCTORS
    public NeuralNetwork() {

        nInput = 6;
        nOutput = 2;
        hiddenSizes = new int[1];
        hiddenSizes[0] = 5;
        nLayers = 1;

        GenerateWeights(out wm);

    }

    public NeuralNetwork(int nIn, int nOut, int nHid)
    {
        nInput = nIn;
        nOutput = nOut;
        hiddenSizes = new int[1];
        hiddenSizes[0] = nHid;
        nLayers = 1;

        GenerateWeights(out wm);
    }

    public NeuralNetwork(int nIn, int nOut, int[] nHid, int nLay)
    {
        nInput = nIn;
        nOutput = nOut;
        hiddenSizes = nHid;
        nLayers = nLay;
        totalNoWeights = 0;

        GenerateWeights(out wm);
    }

    #endregion

    void GenerateWeights(out List<float[,]> wList)
    {
        wList = new List<float[,]>();


        wList.Add(GenerateWeights(nInput, hiddenSizes[0]));
        for (int i = 1; i < nLayers; i++)
        {
            wList.Add(GenerateWeights(hiddenSizes[i - 1], hiddenSizes[i]));
        }

        wList.Add(GenerateWeights(hiddenSizes[nLayers - 1], nOutput));
    }

    private float[,] GenerateWeights(int rows, int columns)
    {
        float[,] weights = new float[rows + 1, columns];
        for (int i = 0; i < rows + 1; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                weights[i, j] = Random.Range(lowerWeightLimit, higherWeightLimit);
                totalNoWeights++;
            }
        }
        return weights;
    }
    

    public float[] CalculateOutput(float[] inp)
    {
        float[] nextOutVal = new float[0];
        int count=0;
        foreach (float[,] wi in wm)
        {
            count++;
            int sizeIn = inp.Length;
            float[] input = new float[sizeIn + 1];
            //Debug.Log(sizeIn);
            for (int i = 0; i < sizeIn - 1; i++)
            {
                input[i] = inp[i];
            }
            input[sizeIn] = 1; //bias term
            nextOutVal = Vector.matVecMult(input, wi);
            if (count != wm.Count)
            {
                nextOutVal = Relu(nextOutVal);
            }
            inp = nextOutVal;
            
        }
        return Softsign(nextOutVal); //NORMAL SOFTSIGN MODE
        //return nextOutVal; //FOR TESTING WITHOUT SOFTSIGN;
    }

    public float[] GetWeights()
    {
        float[] ret = new float[totalNoWeights];
        int count = 0;
        int rows;
        int columns;
        foreach(float[,] wi in wm)
        {
            rows = wi.GetLength(0);
            columns = wi.GetLength(1);
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    ret[count] = wi[j, i];
                    count++;
                }
            }
        }
        return ret;
    }

    public void SetWeights(float[] inp)
    {
        int count = 0;
        int rows;
        int columns;
        foreach (float[,] wi in wm)
        {
            rows = wi.GetLength(0);
            columns = wi.GetLength(1);
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    wi[j, i] = inp[count];
                    count++;
                }
            }
        }
    }

    #region Activation Functions

    public float[] Relu(float[] inp) // RELU(x) returns close to zero if x is less than zero, otherwhise x.
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
