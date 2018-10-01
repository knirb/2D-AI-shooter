using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Population {

    private List<GameObject> enemyList;
    private List<Bot> botList;
    private float mutationRate = 0.05f;
    private int savedPerGen = 2;
    private int specialsPerGen = 0; //Specials are heavily mutated children to introduce more variation in the gene samples.

    public Population(List<GameObject> bl)
    {
        enemyList = bl;
        botList = new List<Bot>();
        foreach (GameObject go in enemyList)
        {
            botList.Add(go.GetComponent<Bot>());
        }
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

    NeuralNetwork GenerateChild()
    {
        NeuralNetwork parentA = SelectParent();
        NeuralNetwork parentB = SelectParent();
        NeuralNetwork child = new NeuralNetwork();
        float[] wA = parentA.GetWeights();
        float[] wB = parentB.GetWeights();
        float[] wChild = new float[wA.Length];
        for (int i = 0; i < wA.Length; i++)
        {
            if(Random.Range(0f,1f) < 0.5)
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

    NeuralNetwork SelectParent()
    {
        float r = Random.Range(0f, 1f);
        NeuralNetwork parent = new NeuralNetwork();
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
        return new NeuralNetwork();
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


    float[] Mutate(float[] inp)
    {
        float[] mutatedInp = new float[inp.Length]; 

        for (int i = 0; i<inp.Length; i++)
        {
            if (Random.Range(0f, 1f) < mutationRate)
            {
                mutatedInp[i] = Random.Range(0f, 1f);
            }
            else
                mutatedInp[i] = inp[i];
        }

        return mutatedInp;
    }


}
