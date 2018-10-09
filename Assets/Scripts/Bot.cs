using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour {

    public GameObject bullet;
    public NeuralNetwork nn;

    
    public float score; //Total score for this bot.
    public float scoreHitTarget;
    private float timeSinceShot = 0; //Also used in fireRate;
    public bool done;
    [HideInInspector] public string ID;
    public float selectionProb;

    private int ammo;
    private int numberOfInputs; //For NeuralNetwork
    private int numberOfOutputs; // NN outputs;
    private int numberOfLayers;
    private int[] numberOfHidden;
    private Vector2 shotPosition; //NN will target a position
    private Vector2 playerPosition;
    private Vector2 shotDirection; //Normalized direction of shot.
    private bool canShoot = true; //Used to create a fireRate - not being able to fire each frame. 
    public int bulletsInAir = 0;
    private float fireRate; // Bullets per Second
    private float bulletSpeed;
    private float[] nnOutput; //saveSpace for outPut of NN
    private float[] nnInput; //SaveSpace for inputs going in to nn
    private GameObject enemy;
    private GameManager gm;
    private Rigidbody2D rb;
    private Movement_UpDown mud;
    



    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        numberOfInputs = gm.nInputs;
        numberOfOutputs = gm.nOutputs;
        numberOfHidden = gm.nHidden;
        numberOfLayers = gm.nLayers;
        ammo = gm.shotsPerRound;
        bulletSpeed = gm.bulletSpeed;
        fireRate = gm.fireRate;
        rb = GetComponent<Rigidbody2D>();
        bullet.GetComponent<Bullet>().movementSpeed = bulletSpeed;
        mud = GetComponent<Movement_UpDown>();
        nn = new NeuralNetwork(numberOfInputs, numberOfOutputs, numberOfHidden, numberOfLayers); //Currently using non parametrized constructor.
         
        
        ID = gameObject.name;
        enemy = GameObject.Find("Enemy"); 
    }


    void FixedUpdate()
    {
        if (!done)
        {
            TryShooting(); // TryShooting does what it says
            CheckIfDone(); 
        }
        
    }

    public void RoundReset()
    {
        ammo = gm.shotsPerRound;
        done = false;
        timeSinceShot = 0;
        rb.position = gm.startPositionBot;
        mud.setVelocity(new Vector3(0 ,1 ,0 ) * gm.botMoveSpeed);

        
    }
    //Checks if canShoot and acts accordingly.
    void TryShooting()
    {
        if (canShoot == true)
        {
            playerPosition = transform.position;
            GenerateInputs(); // Normalizes inputs to scale from 0-1;
            nnOutput = nn.CalculateOutput(nnInput);
            ScaleOutputs();
            shotPosition = new Vector2(nnOutput[0], nnOutput[1]);

            /*Debug.Log(ID + " I: " + nnInput[2] + ", " + nnInput[3]);
            string ret = "";
            for (int i = 0; i < nn.GetWeights().Length; i++)
                ret += nn.GetWeights()[i] + ", ";
            Debug.Log(ID + " O: " + nnOutput[0] + ", " + nnOutput[1]);
            Debug.Log(ID + " W:" + ret);*/

            /* shotDirection = (shotPosition - playerPosition).normalized; // used if targeting groundposition instead of shotangle;
             * if (shotDirection == Vector2.zero)
             */

            shotDirection = shotPosition.normalized;
            if (shotDirection == Vector2.zero)
            {
                canShoot = false;
                timeSinceShot = 0;
                ammo--;
                return;
            }
            shoot(shotDirection);

            timeSinceShot = 0;
        }
        else
        {
            timeSinceShot += Time.deltaTime;
            if (timeSinceShot >= 1 / fireRate && ammo > 0)
                canShoot = true;
        }
    }

    //Called if TryShoot deems it should.
    void shoot(Vector2 Direction) // Direction must be normalized;
    {

        float shotAngle = Mathf.Atan2(Direction.y, Direction.x);
        Vector2 instatiatePosition = playerPosition + new Vector2(0.24f * Direction.x, 0.24f * Direction.y);

        GameObject inst = Instantiate(bullet, instatiatePosition, Quaternion.AngleAxis((180 / Mathf.PI) * shotAngle, new Vector3(0, 0, 1)));
        inst.GetComponent<Bullet>().shooter = gameObject;
        inst.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
        canShoot = false;
        ammo--;
        bulletsInAir++;
        
    }


    //HitTarget and MissedTarget are used for scoring, called from Bullet.cs 
    public void HitTarget()
    {
        score += scoreHitTarget;
        bulletsInAir--;
        if (bulletsInAir < 0)
            Debug.Break();

    }



    public void MissedTarget(float sqrdDist)
    {
        score += (1 - sqrdDist/Mathf.Pow(5f-0.24f,2))*scoreHitTarget*0.02f;
        bulletsInAir--;
        if (bulletsInAir < 0)
            Debug.Break();

    }

    void CheckIfDone()
    {
        //Debug.Log("CheckedIfDone, amm: " + ammo + ", bulletsInAir: " + bulletsInAir);
        if (ammo <= 0 && bulletsInAir == 0)
        {
            done = true;
            gm.BotDoneShooting(this);
        }
    }

    //Chosing and scaling inputs for the NN.
    void GenerateInputs()
    {
        nnInput = new float[numberOfInputs];
        for (int i = 0; i < numberOfInputs; i++)

        nnInput[0] = transform.position.x / 2.35f;
        nnInput[1] = transform.position.y / 2.35f;
        nnInput[2] = enemy.transform.position.x / 2.35f;
        nnInput[3] = enemy.transform.position.y / 2.35f;
        nnInput[4] = enemy.GetComponent<Rigidbody2D>().velocity.x/10;
        nnInput[5] = enemy.GetComponent<Rigidbody2D>().velocity.y/10;

        //Debug.Log(enemy.GetComponent<Rigidbody2D>().velocity.x + " , " + enemy.GetComponent<Rigidbody2D>().velocity.y);
    }

    void ScaleOutputs()
    {
        //nnOutput[0] *= 2.35f;
        //nnOutput[1] *= 2.35f;
    }
}
