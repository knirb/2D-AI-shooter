using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour {

    public GameObject bullet;
    public NeuralNetwork nn;

    public int ammo;
    public int numberOfInputs; //For NeuralNetwork
    public int numberOfOutputs; // NN outputs;
    public float Score; //Total score for this bot.
    public float scoreHitTarget;
    public float fireRate; // Bullets per Second

    private Vector2 shotPosition;
    private Vector2 playerPosition;
    private Vector2 shotDirection;
    private bool canShoot = true;
    private float timeSinceShot = 0;
    private float[] nnOutput;
    private float[] nnInput;
    private GameObject enemy;
    private GameManager gm;



    void Start()
    {

        nn = new NeuralNetwork();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        numberOfInputs = nn.nInput;
        numberOfOutputs = nn.nOutput;

        enemy = GameObject.Find("Enemy");
    }


    void Update()
    {
        GenerateInputs();
        if (canShoot == true)
        {
            playerPosition = transform.position;
            nnOutput = nn.CalculateOutput(nnInput);
            ScaleOutputs();
            shotPosition = new Vector2(nnOutput[0], nnOutput[1]);
            if (shotPosition == Vector2.zero)
            {
                canShoot = false;
                timeSinceShot = 0;
                ammo--;
                return;
            }
            shotDirection = shotPosition.normalized;
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

    void shoot(Vector2 Direction) // Direction must be normalized;
    {
        

        float shotAngle = Mathf.Atan2(Direction.y, Direction.x);
        Vector2 instatiatePosition = playerPosition + new Vector2(0.24f * Direction.x, 0.24f * Direction.y);

        GameObject inst = Instantiate(bullet, instatiatePosition, Quaternion.AngleAxis((180 / Mathf.PI) * shotAngle, new Vector3(0, 0, 1)));
        inst.GetComponent<Bullet>().shooter = gameObject;
        canShoot = false;
        ammo--;
        if (ammo <= 0)
        {
            gm.BotDoneShooting(this);
        }
    }

    public void hitTarget()
    {
        Score += scoreHitTarget;
    }

    public void missedTarget(float sqrdDist)
    {
        Score += scoreHitTarget - Mathf.Sqrt(sqrdDist);
    }
    void GenerateInputs()
    {
        nnInput = new float[numberOfInputs];
        nnInput[0] = transform.position.x / 2.35f;
        nnInput[1] = transform.position.y / 2.35f;
        nnInput[2] = enemy.transform.position.x / 2.35f;
        nnInput[3] = enemy.transform.position.y / 2.35f;
    }

    void ScaleOutputs()
    {
       
    }
}
