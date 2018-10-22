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
    private HeritageMethod heritageMethod;
    private ParentPool pp;

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
        heritageMethod = gm.heritageMethod;
        pp = gm.useParentPool;
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
        if (pp == ParentPool.yes)
        {
            parentPool = new List<GameObject>();
            for (int j = 0; j < parentPoolSize; j++)
            {
                parentPool.Add(enemyList[j]);
            }
            parentPool = CalculateProbabilities(parentPool);
        }
        else
        {
            CalculateProbabilities();
        }
        
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
        switch (heritageMethod)
        {
            case HeritageMethod.weightProbability:
                child = WeightProbability();
                break;
            case HeritageMethod.twoParents:
                child = twoParents();
                break;
            case HeritageMethod.scoreBasedMutation:
                child = ScoreBasedMutation();
                break;
        }
        return child;

    }

    #region Heritage Methods

    private NeuralNetwork twoParents() //Crossover Alternative
    {
        Bot parentA = SelectParent();
        Bot parentB = SelectParent();
        while (parentA.nn == parentB.nn)
        {
            parentB = SelectParent();
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
        NeuralNetwork child = new NeuralNetwork(nInput, nOutput, nHidden, nLayers);
        Bot parent = SelectParent();
        float[] wParent = parent.nn.GetWeights();
        float score = parent.score;
        float mutRate = ((gm.maxScore - score)/gm.maxScore > 0.05f) ? ((gm.maxScore - score) / gm.maxScore) : 0.05f; //EXPERIMENTATION TO INCREASE LEARNING RATE AT HIGH SCORES
        //float mutScale = ((gm.maxScore - score)/gm.maxScore > 0.01f) ? ((gm.maxScore - score) / gm.maxScore) : 0.01f; //EXPERIMENTATION TO INCREASE LEARNING RATE AT HIGH SCORES
        //float mutRate = (gm.maxScore - score) / gm.maxScore;    //NORMAL MUTRATE
        float mutScale = mutRate;                               //NORMAL MUTSCALE
        child.SetWeights(Mutate(wParent, mutRate, mutScale));
        return child;
    }
    #endregion
    //SELECTING PARENT WITHOUT POOL;
    private Bot SelectParent()
    {
        switch (pp)
        {
            case ParentPool.no:
                return NoPool();
            case ParentPool.yes:
                return Pool();
        }
        return null;
    }

    private Bot NoPool()
    {
        float r = Random.Range(0f, 1f);
        foreach (Bot curBot in botList)
        {
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

    //PARENT FROM POOL
    private Bot Pool()
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
        NeuralNetwork newNet = new NeuralNetwork(nInput, nOutput, nHidden, nLayers);
        Bot parent = SelectParent();
        float mutRate = ((gm.maxScore - parent.score) / gm.maxScore > 0.05f) ? ((gm.maxScore - parent.score) / gm.maxScore) : 0.05f;
        float mutScale = mutRate;
        //newNet.SetWeights(Mutate(parent.nn.GetWeights(), mutRate,mutScale));
        newNet.SetWeights(Mutate(parent.nn.GetWeights(), specialsMutationRate, 0.1f));
        return newNet;
    }

    void CalculateProbabilities()
    {
        float totalScore = 0;
        float probSum = 0; //ONLY DEBUG;
        foreach (Bot bot in botList)
        {
            totalScore += bot.score;
        }
        for(int i = 0; i <botList.Count; i++)
        {
            botList[i].selectionProb = botList[i].score / totalScore;
            probSum += enemyList[i].GetComponent<Bot>().selectionProb;
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
        for (int i = 0; i < botList.Count; i++)
        {
            if (i < inList.Count)
            {
                inList[i].GetComponent<Bot>().selectionProb = inList[i].GetComponent<Bot>().score / totalScore;
                enemyList[i].GetComponent<Bot>().selectionProb = inList[i].GetComponent<Bot>().selectionProb;
            }
            else
            {
                enemyList[i].GetComponent<Bot>().selectionProb = 0;
            }
            
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
        for (int i = 0; i < inp.Length; i++)
        {
            if (Random.Range(0f, 1f) < mutRate)
            {
                inp[i] += Random.Range(botList[0].nn.lowerWeightLimit, botList[0].nn.higherWeightLimit)*mutScale;
            }
        }
        return inp;
    }


}
