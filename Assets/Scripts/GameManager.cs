using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


    /* Create all the players
     * Keep track of each players score
     * Creating new rounds
     * Manage heritage of Genetic Algorithms*/
public class GameManager : MonoBehaviour {

    public static GameManager instance = null;
    public int numberOfPlayers;
    public int shotsPerRound;
    public float timeScale;
    public float timeBetweenRounds;
    public float playerMoveSpeed;
    public float botMoveSpeed;
    public float bulletSpeed;
    public float fireRate;
    public float hitScore;
    [HideInInspector] public float maxScore;
    public int nInputs;
    public int nOutputs;
    public int nLayers;
    public int[] nHidden;
    public HeritageMethod hm; //Defined in Algorithms.cs
    public ParentPool pp;
    public float mutationRate;
    public float specialsMutationRate;
    public int savedPerGen;
    public int specialsPerGen; //Specials are heavily mutated children to introduce more variation in the gene samples.
    public int parentPoolSize;
    public GameObject bot;
    public GameObject background;
    public GameObject enemyPrefab;
    public GameObject intermissionImage;
    public Vector3 startPositionBot;
    public Vector3 startPositionEnemy;
    public Population population;

    private int botsDone;
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
        playerMoveSpeed *= timeScale;
        botMoveSpeed *= timeScale;
        bulletSpeed *= timeScale;
        fireRate *= timeScale;
        maxScore = shotsPerRound * hitScore;

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
            inst.GetComponent<Movement_UpDown>().setSpeed(botMoveSpeed);
            botList.Add(inst);
        }
        population = new Population(botList);
    }

    private void CreateEnemy()
    {
        enemy = Instantiate(enemyPrefab, startPositionEnemy, Quaternion.identity);
        enemy.name = "Enemy";
        enemy.GetComponent<Movement_UpDown>().setSpeed(playerMoveSpeed);
    }

    public void BotDoneShooting(Bot sender) // Called through bots sending message that they are done shooting.
    {
        botsDone++;

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
        botList[0].GetComponent<SpriteRenderer>().color = Color.green;
        botList[1].GetComponent<SpriteRenderer>().color = Color.blue;



        foreach (GameObject b in botList)
        {
            b.GetComponent<Bot>().RoundReset();
        }
        enemy.transform.position = startPositionEnemy;
        enemy.GetComponent<Movement_UpDown>().setVelocity(new Vector3(0, 1, 0) * playerMoveSpeed);
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
