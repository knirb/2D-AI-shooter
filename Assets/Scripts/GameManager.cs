using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


/* Create all the players
 * Keep track of each players score
 * Creating new rounds
 * Manage heritage of Genetic Algorithms*/
public class GameManager : MonoBehaviour
{

    public static GameManager instance = null;
    public int numberOfPlayers;
    public int shotsPerRound;
    public float timeScale;
    public float timeBetweenRounds;
    public float timePerRound;
    public float roundTimer;
    public float enemyMoveSpeed;
    public float botMoveSpeed;
    public float bulletSpeed;
    public float fireRate;
    public float hitScore;
    [HideInInspector] public float maxScore;
    public int nInputs;
    public int nOutputs;
    public int nLayers;
    public int[] nHidden;
    public BotType botType;
    public HeritageMethod heritageMethod; //Defined in Algorithms.cs
    public ParentPool useParentPool;
    public SoftSign softSign;
    public float mutationRate;
    public float specialsMutationRate; //CURRENTLY NOT USED; INSTEAD USING SCORE BASED MUTATION
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
    public bool playing;
    public bool paused;

    private int botsDone;
    private bool boardExists;
    private float timeNextRound;
    private List<GameObject> botList = new List<GameObject>();
    private int curGen;
    private GameObject enemy;

    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        boardExists = false;
        intermissionImage = GameObject.Find("IntermissionImage");
        curGen = 1;
        maxScore = shotsPerRound * hitScore;
        Time.timeScale = timeScale;
        initGame();
        paused = false;
    }
    void Update()
    {
        if (Input.anyKeyDown)
            KeyDown();
        roundTimer += Time.deltaTime;
        if (roundTimer >= timePerRound)
            EndRound();
        if (Time.timeScale != timeScale && !paused)
            Time.timeScale = timeScale;
    }
    void KeyDown()
    {
        if (Input.GetKeyDown("space"))
        {
            ToggleShownBots();
        }
        else if (Input.GetKeyDown("p"))
        {
            botList[0].GetComponent<Bot>().PrintWeights();
        }
        else if (Input.GetKeyDown("up"))
        {
            timeScale += 0.5f;
        }
        else if (Input.GetKeyDown("down"))
        {
            timeScale -= 0.5f;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
                Pause();
            else
                Unpause();
        }
    }
    void FixedUpdate()
    {

        if (!playing && Time.time >= timeNextRound && boardExists)
        {
            NewRound();
        }



    }
    private void initGame()
    {
        playing = true;
        intermissionImage.SetActive(false);
        createBoard();
        roundTimer = 0f;
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

    private void CreateEnemy()
    {
        enemy = Instantiate(enemyPrefab, startPositionEnemy, Quaternion.identity);
        enemy.name = "Enemy";
        enemy.GetComponent<Movement_UpDown>().setSpeed(enemyMoveSpeed);
    }

    public void BotDoneShooting(Bot sender) // Called through bots sending message that they are done shooting.
    {
        botsDone++;

        if (botsDone == numberOfPlayers) // "|" - alt + 0124;
        {
            Debug.DebugBreak();
            EndRound();
        }
    }
    public void Pause()
    {
        Time.timeScale = 0;
        paused = !paused;
    }
    public void Unpause()
    {
        Time.timeScale = timeScale;
        paused = !paused;
    }

    private void PauseMovement()
    {
        foreach (GameObject bot in botList)
        {
            bot.GetComponent<Bot>().Pause();
        }
        enemy.GetComponent<Movement_UpDown>().setVelocity(0);
    }
    private void UnpauseMovement()
    {
        foreach (GameObject bot in botList)
        {
            bot.GetComponent<Bot>().Unpause();
        }

        enemy.GetComponent<Movement_UpDown>().setVelocity(enemyMoveSpeed);
    }

    private void ResetPositions()
    {
        foreach (GameObject b in botList)
        {
            b.GetComponent<Bot>().RoundReset();
        }
        enemy.transform.position = startPositionEnemy;
        enemy.GetComponent<Movement_UpDown>().setVelocity(new Vector3(0, 1, 0) * enemyMoveSpeed);
    }

    private void RemoveRemains()
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        for (int i = 0; i < bullets.Length; i++)
        {
            DestroyImmediate(bullets[i]);
        }

    }

    private void MakeNewBots()
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
    }
    private void EndRound()
    {
        

        ResetPositions();
        PauseMovement();
        RemoveRemains();
        intermissionImage.SetActive(true);
        MakeNewBots();
        roundTimer = 0f;
        playing = false;

    }

    private void ToggleShownBots()
    {
        for (int i = 1; i < botList.Count; i++)
        {
            botList[i].GetComponent<Bot>().ToggleColor();
        }
    }

    private void NewRound()
    {
        botList[0].GetComponent<Bot>().SetColor(ColorScheme.green);
        UnpauseMovement();
        playing = true;
        botsDone = 0;
        intermissionImage.SetActive(false);

    }
}
