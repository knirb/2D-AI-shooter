﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


    /* Create all the players
     * Keep track of each players score
     * Creating new rounds
     * Manage heritage of Genetic Algorithms*/
public class GameManager : MonoBehaviour {

    public int numberOfPlayers;
    public int shotsPerRound;
    public int botsDone = 0;
    public int nInputs;
    public int nOutputs;
    public int nHidden;
    public static GameManager instance = null;
    public GameObject bot;
    public GameObject background;
    public GameObject enemyPrefab;
    public GameObject intermissionImage;
    public Vector3 startPositionBot;
    public Vector3 startPositionEnemy;
    public Population population;
    public float timeBetweenRounds;

    private bool boardExists;
    private bool playing;
    private float timeNextRound;
    private List<GameObject> botList = new List<GameObject>();
    private int curGen;
    private GameObject enemy;

    void Awake () {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        boardExists = false;
        intermissionImage = GameObject.Find("IntermissionImage");
        curGen = 1;
    }

    private void initGame()
    {
        playing = true;
        intermissionImage.SetActive(false);
        createBoard();
        
    }

    private void createBoard()
    {
        GameObject newBG = Instantiate(background, Vector3.zero, Quaternion.identity);
        newBG.name = "Background";
        CreateEnemy();
        CreateBots();
        boardExists = true;
    }
    private void CreateBots()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            GameObject inst = Instantiate(bot, startPositionBot, Quaternion.identity);
            inst.name = "Bot" + i.ToString(); //For Inspector
            botList.Add(inst);
        }
        population = new Population(botList);
    }

    public void BotDoneShooting(Bot sender) // Called through bots sending message that they are done shooting.
    {
        botsDone++;
        Debug.Log("Bot done!");

        if (botsDone == numberOfPlayers)
        {
            EndRound();
        }
    }

    private void EndRound()
    {
        List<NeuralNetwork> newNNs = population.Generate();
        intermissionImage.GetComponentInChildren<Text>().text = "Generation " + curGen + "\n" + "Highest score: " + population.botList[0].score;
        for (int i = 0; i < botList.Count; i++)
        {
            botList[i].GetComponent<Bot>().nn = newNNs[i];
            botList[i].GetComponent<Bot>().score = 0;
        }
 
        timeNextRound = Time.time + timeBetweenRounds;
        curGen++;
        intermissionImage.SetActive(true);
        playing = false;
    }

    private void NewRound()
    {
        playing = true;
        botsDone = 0;
        intermissionImage.SetActive(false);
        foreach (GameObject b in botList)
        {
            b.GetComponent<Bot>().ammo = shotsPerRound;
            b.GetComponent<Bot>().done = false;
        }
        enemy.transform.position = startPositionEnemy;
        enemy.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 1, 0) * enemy.GetComponent<Movement_UpDown>().movementSpeed;
    }

    private void CreateEnemy()
    {
        enemy = Instantiate(enemyPrefab, startPositionEnemy, Quaternion.identity);
        enemy.name = "Enemy";
    }
	// Update is called once per frame
	void FixedUpdate () {

        if (!boardExists && Time.time >= timeBetweenRounds)
            initGame();

        else if (!playing && Time.time >= timeNextRound && boardExists)
        {
            NewRound();
        }


    }
}
