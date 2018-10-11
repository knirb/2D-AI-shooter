using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Population {

    public List<GameObject> enemyList;
    public List<Bot> botList;

    private List<GameObject> parentPool;
    private GameManager gm;
    private int nInput;
    private int nOutput;
    private int[] nHidden;
    private int nLayers;
    private float mutationRate;
    private float specialsMutationRate;
    private int savedPerGen;
    private int specialsPerGen; //Specials are heavily mutated children to introduce more variation in the gene samples.
    private int parentPoolSize;
    private HeritageMethod hm;

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
        nLayers = gm.nLayers;
        parentPoolSize = (gm.parentPoolSize < gm.numberOfPlayers) ? gm.parentPoolSize : gm.numberOfPlayers;
        hm = gm.hm;
        mutationRate = gm.mutationRate;
        specialsMutationRate = gm.specialsMutationRate;
        savedPerGen = gm.savedPerGen;
        specialsPerGen = gm.specialsPerGen; //Specials are heavily mutated children to introduce more variation in the gene samples.

}

    public List<NeuralNetwork> Generate()
    {

        CalculateFitness();
        GenerateGenePool();
        List<NeuralNetwork> nextGeneration = GenerateNewGeneration();

        return nextGeneration;
        
    }

    private void CalculateFitness() //Sorts bots in order of fitness/score
    {
        botList.Sort((x, y) => y.score.CompareTo(x.score));
    }

    private void GenerateGenePool()
    {
        parentPool = new List<GameObject>();
        for (int j = 0; j < parentPoolSize; j++)
        {
            parentPool.Add(enemyList[j]);
        }
        parentPool = CalculateProbabilities(parentPool);
    }
    private List<NeuralNetwork> GenerateNewGeneration()
    {
        List<NeuralNetwork> newGen = new List<NeuralNetwork>();
        int i;
        for (i = 0; i < savedPerGen; i++)
        {
            newGen.Add(botList[i].nn);
        }
        for (i = savedPerGen; i < botList.Count - specialsPerGen; i++)
        {
            NeuralNetwork childBrain = GenerateChildNN();
            newGen.Add(childBrain);
        }
        for (i = botList.Count - specialsPerGen; i < botList.Count; i++)
        {
            NeuralNetwork specialBrain = GenerateSpecial();
            newGen.Add(specialBrain);
        }
        return newGen;
    }

    private NeuralNetwork GenerateChildNN()
    {
        NeuralNetwork child = new NeuralNetwork(nInput, nOutput, nHidden, nLayers);
        switch (hm)
        {
            case HeritageMethod.weightProbability:
                child = WeightProbability();
                break;
            case HeritageMethod.twoParents:
                child = twoParents();
                break;
            case HeritageMethod.scoreBasedMutation:
                child = scoreBasedMutation();
                break;
        }
        return child;

    }

    #region Heritage Methods

    private NeuralNetwork twoParents() //Crossover Alternative
    {
        Bot parentA = SelectPoolParent();
        Bot parentB = SelectPoolParent();
        while (parentA == parentB)
        {
            parentB = SelectPoolParent();
        }
        
        NeuralNetwork child = new NeuralNetwork(nInput, nOutput, nHidden, nLayers);
        float[] wA = parentA.nn.GetWeights();
        float[] wB = parentB.nn.GetWeights();
        float[] wChild = new float[wA.Length];
        int cutPoint = Random.Range(0, wA.Length - 1);
        for (int i = 0; i < wA.Length; i++)
        {
            if (i < cutPoint)
            {
                wChild[i] = wA[i];
            }
            else
            {
                wChild[i] = wB[i];
            }
        }
        Mutate(ref wChild);
        child.SetWeights(wChild);
        return child;
    }
    private NeuralNetwork WeightProbability() //Crossover alternative
    {
        NeuralNetwork child = new NeuralNetwork(nInput, nOutput, nHidden, nLayers);
        float[] wChild = child.GetWeights();
        for (int i = 0; i < wChild.Length; i++)
        {
            NeuralNetwork nn = SelectParent().nn;
            float[] wA = nn.GetWeights();
            wChild[i] = wA[i];
        }
        Mutate(ref wChild);
        child.SetWeights(wChild);
        return child;

    }
    private NeuralNetwork ScoreBasedMutation()
    {
        
        
    }
    #endregion
    //SELECTING PARENT WITHOUT POOL;
    private Bot SelectParent()
    {
        float r = Random.Range(0f, 1f);
        foreach (GameObject go in enemyList)
        {
            Bot curBot = go.GetComponent<Bot>();
            if (r-curBot.selectionProb < 0)
            {
                return curBot;
                break;
            }
            else
            {
                r -= curBot.selectionProb;
            }
        }
        
        return null;
    }

    //PARENT FROM POOL
    private Bot SelectPoolParent()
    {
        float r = Random.Range(0f, 1f);
        foreach (GameObject go in parentPool)
        {
            Bot curBot = go.GetComponent<Bot>();
            if (r - curBot.selectionProb < 0)
            {
                return curBot;
            }
            else
            {
                r -= curBot.selectionProb;
            }
        }

        return null;
    }

    private NeuralNetwork GenerateSpecial()
    {
        NeuralNetwork parent = new NeuralNetwork(nInput, nOutput, nHidden, nLayers);
        parent.SetWeights(botList[0].nn.GetWeights());
        parent.SetWeights(Mutate(parent.GetWeights(),specialsMutationRate,0.1f));
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


    private List<GameObject> CalculateProbabilities(List<GameObject> inList)
    {
        List<GameObject> ret = inList;
        float totalScore = 0;
        foreach (GameObject bot in inList)
        {
            totalScore += bot.GetComponent<Bot>().score;
        }
        for (int i = 0; i < inList.Count; i++)
        {
            inList[i].GetComponent<Bot>().selectionProb = inList[i].GetComponent<Bot>().score / totalScore;
            enemyList[i].GetComponent<Bot>().selectionProb = botList[i].selectionProb;
        }
        return ret;
    }

    private void Mutate(ref float[] inp)
    {

        for (int i = 0; i<inp.Length; i++)
        {
            if (Random.Range(0f, 1f) < mutationRate)
            {
                inp[i] = Random.Range(botList[0].nn.lowerWeightLimit, botList[0].nn.higherWeightLimit);
            }
        }
        
    }
    private float[] Mutate(float[] inp, float mutRate, float mutScale)
    {
        float[] mutatedInp = new float[inp.Length];

        for (int i = 0; i < inp.Length; i++)
        {
            if (Random.Range(0f, 1f) < mutRate)
            {
                mutatedInp[i] += Random.Range(botList[0].nn.lowerWeightLimit, botList[0].nn.higherWeightLimit)*mutScale;
            }
            else
                mutatedInp[i] = inp[i];
        }

        return mutatedInp;
    }


}
