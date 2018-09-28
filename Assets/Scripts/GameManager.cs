using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    /* Create all the players
     * Keep track of each players score
     *  
     * 
     */
    public int numberOfPlayers;
    public int shotsPerRound;
    public static GameManager instance = null;
    public GameObject bot;
    public GameObject background;
    public GameObject enemy;
    public Vector3 startPositionBot;
    public Vector3 startPositionEnemy;


    private List<GameObject> botList = new List<GameObject>();

    void Awake () {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        initGame();
	}

    private void initGame()
    {
        createBoard();
    }

    private void createBoard()
    {
        GameObject newBG = Instantiate(background, Vector3.zero, Quaternion.identity);
        newBG.name = "Background";
        CreateEnemy();
        CreateBots();
        
    }
    private void CreateBots()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            GameObject inst = Instantiate(bot, startPositionBot, Quaternion.identity);
            inst.name = "Bot" + i.ToString();
            botList.Add(inst);
        }
    }

    public void BotDoneShooting(Bot sender) // Called through bots sending message that they are done shooting.
    {

    }

    private void CreateEnemy()
    {
        GameObject inst = Instantiate(enemy, startPositionEnemy, Quaternion.identity);
        inst.name = "Enemy";
    }
	// Update is called once per frame
	void Update () {
		
	}
}
