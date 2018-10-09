using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Population {

    public List<GameObject> enemyList;
    public List<Bot> botList;


    private List<Bot> parentPool;
    private GameManager gm;
    private int nInput;
    private int nOutput;
    private int nHidden;
    private float mutationRate = 0.02f;
    private int savedPerGen = 2;
    private int specialsPerGen = 10; //Specials are heavily mutated children to introduce more variation in the gene samples.
    private int parentPoolSize;

    public Population(List<GameObject> bl)
    {
        enemyList = bl;
        botList = new List<Bot>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        foreach (GameObject go in enemyList)
        {
            botList.Add(go.GetComponent<Bot>());
        }
        nInput = gm.nInputs;
        nOutput = gm.nOutputs;
        nHidden = gm.nHidden;
    }

    public List<NeuralNetwork> Generate()
    {

        //Save 2 highest scoring bots
        //Generate NN weights based on score
        /*Initial Approach
         * Each score will give every bot a probability to be selected: selectionProb = score/totalScore;
         * 
         */

        List<NeuralNetwork> nextGeneration = new List<NeuralNetwork>();

        CalculateProbabilities();

        botList.Sort((x, y) => y.score.CompareTo(x.score));


        parentPool = new List<Bot>();
        for (int j = 0; j < parentPoolSize; j++)
        {
            parentPool.Add(botList[j]);
        }
        parentPool = CalculateProbabilities(parentPool);

       // Debug.Log(botList[0].score + ", " + botList[1].score + ", " + botList[2].score);

        int i;
        for (i = 0; i < savedPerGen; i++)
        {
            nextGeneration.Add(botList[i].nn);
        }
        for (i = savedPerGen; i< botList.Count - specialsPerGen; i++)
        {
            NeuralNetwork childBrain = GenerateChild();
            nextGeneration.Add(childBrain);
        }
        for (i = botList.Count - specialsPerGen; i<botList.Count; i++)
        {
            NeuralNetwork specialBrain = GenerateSpecial();
            nextGeneration.Add(specialBrain);
        }

        return nextGeneration;
        
    }
    /*
     * This version uses genetic material from two parents to create a new child. 
     */
    
     
     NeuralNetwork GenerateChild()
    {
        NeuralNetwork parentA = SelectParent();
        NeuralNetwork parentB = SelectParent();
        while (parentA == parentB)
        {
            parentB = SelectParent();
        }
        
        NeuralNetwork child = new NeuralNetwork(nInput,nOutput,nHidden);
        float[] wA = parentA.GetWeights();
        float[] wB = parentB.GetWeights();
        float[] wChild = new float[wA.Length];
        float keepPercentage = 0.5f;
        for (int i = 0; i < wA.Length; i++)
        {
            if(Random.Range(0f,1f) < keepPercentage)
            {
                wChild[i] = wA[i];
            }
            else
            {
                wChild[i] = wB[i];
            }
        }
        wChild = Mutate(wChild);
        child.setWeights(wChild);
        return child;

    }
    // FOR DEBUGGING
    /*NeuralNetwork GenerateChild() //DEBUGGING 
    {
        NeuralNetwork parentA = SelectParent();
        NeuralNetwork child = new NeuralNetwork(nInput, nOutput, nHidden);
        child.setWeights(botList[0].nn.GetWeights()); 
        return child;

    }*/

    /*
    NeuralNetwork GenerateChild()
    {
        NeuralNetwork child = new NeuralNetwork(nInput, nOutput, nHidden);
        float[] wChild = child.GetWeights();
        for (int i = 0; i < wChild.Length; i++)
        {
            NeuralNetwork nn = SelectParent();
            float[] wA = nn.GetWeights();
            wChild[i] = wA[i];
        }
        wChild = Mutate(wChild);
        child.setWeights(wChild);
        return child;

    }
    */
    NeuralNetwork SelectParent()
    {
        float r = Random.Range(0f, 1f);
        NeuralNetwork parent = new NeuralNetwork(nInput,nOutput,nHidden);
        foreach (GameObject go in enemyList)
        {
            Bot curBot = go.GetComponent<Bot>();
            if (r-curBot.selectionProb < 0)
            {
                parent = curBot.nn;
                break;
            }
            else
            {
                r -= curBot.selectionProb;
            }
        }
        
        return parent;
    }

    NeuralNetwork GenerateSpecial()
    {
        NeuralNetwork parent = new NeuralNetwork(nInput, nOutput, nHidden);
        parent.setWeights(botList[0].nn.GetWeights());
        parent.setWeights(Mutate(parent.GetWeights(),mutationRate+0.1f));
        return parent;
    }
    void CalculateProbabilities()
    {
        float totalScore = 0;
        foreach (Bot bot in botList)
        {
            totalScore += bot.score;
        }
        for(int i = 0; i <botList.Count; i++)
        {
            botList[i].selectionProb = botList[i].score / totalScore;
            enemyList[i].GetComponent<Bot>().selectionProb = botList[i].selectionProb;
        }
    }
    List<Bot> CalculateProbabilities(List<Bot> inList)
    {
        List<Bot> ret = inList;
        float totalScore = 0;
        foreach (Bot bot in inList)
        {
            totalScore += bot.score;
        }
        for (int i = 0; i < inList.Count; i++)
        {
            inList[i].selectionProb = inList[i].score / totalScore;
            enemyList[i].GetComponent<Bot>().selectionProb = botList[i].selectionProb;
        }
        return ret;
    }

    float[] Mutate(float[] inp)
    {
        float[] mutatedInp = new float[inp.Length]; 

        for (int i = 0; i<inp.Length; i++)
        {
            if (Random.Range(0f, 1f) < mutationRate)
            {
                mutatedInp[i] += Random.Range(botList[0].nn.lowerWeightLimit, botList[0].nn.higherWeightLimit)/2;
            }
            else
                mutatedInp[i] = inp[i];
        }

        return mutatedInp;
    }
    float[] Mutate(float[] inp, float mutRate)
    {
        float[] mutatedInp = new float[inp.Length];

        for (int i = 0; i < inp.Length; i++)
        {
            if (Random.Range(0f, 1f) < mutRate)
            {
                mutatedInp[i] += Random.Range(botList[0].nn.lowerWeightLimit, botList[0].nn.higherWeightLimit) / 2;
            }
            else
                mutatedInp[i] = inp[i];
        }

        return mutatedInp;
    }


}
