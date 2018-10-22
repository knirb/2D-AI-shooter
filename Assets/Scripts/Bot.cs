using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour {

    public GameObject bullet;
    public NeuralNetwork nn;

    
    public float score; //Total score for this bot.
    
    private float timeSinceShot = 0; //Also used in fireRate;
    public bool done;
    public bool paused;
    [HideInInspector] public string ID;
    public float selectionProb;

    private ColorScheme cs;
    private SoftSign ss;
    private BotType botType;
    private float scoreHitTarget;
    private int ammo;
    private float movementSpeed;
    private int numberOfInputs; //For NeuralNetwork
    private int numberOfOutputs; // NN outputs;
    private int numberOfLayers;
    private int[] numberOfHidden;
    private Vector2 shotPosition; //NN will target a position
    private Vector2 shotDirection; //Normalized direction of shot.
    private bool canShoot = false; //Used to create a fireRate - not being able to fire each frame. 
    public int bulletsInAir = 0;
    private float fireRate; // Bullets per Second
    private float bulletSpeed;
    private float[] nnOutput; //saveSpace for outPut of NN
    private float[] nnInput; //SaveSpace for inputs going in to nn
    private GameObject enemy;
    private GameManager gm;
    private Rigidbody2D rb;
    private Movement_UpDown mud;
    private SpriteRenderer sr;
    private Color baseColor;

    void Start()
    {
        
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        cs = ColorScheme.baseColor;
        botType = gm.botType;
        
        ss = gm.softSign;
        numberOfInputs = gm.nInputs;
        numberOfOutputs = gm.nOutputs;
        numberOfHidden = gm.nHidden;
        numberOfLayers = gm.nLayers;
        ammo = gm.shotsPerRound;
        bulletSpeed = gm.bulletSpeed;
        fireRate = gm.fireRate;
        movementSpeed = gm.botMoveSpeed;
        rb = GetComponent<Rigidbody2D>();
        bullet.GetComponent<Bullet>().movementSpeed = bulletSpeed;
        CheckType();

        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        
        nn = new NeuralNetwork(numberOfInputs, numberOfOutputs, numberOfHidden, numberOfLayers); //Currently using non parametrized constructor.
        ID = gameObject.name;
        enemy = GameObject.Find("Enemy");
        scoreHitTarget = gm.hitScore;
        paused = false;
    }
    void CheckType()
    {
        switch (botType)
        {
            case BotType.mud:
                mud = GetComponent<Movement_UpDown>();
                mud.setSpeed(-movementSpeed);
                break;
            case BotType.selfMoving:
                Destroy(GetComponent<Movement_UpDown>());
                break;

        }
    }
    
    void FixedUpdate()
    {
        if (!done && !paused)
        {
            if (!canShoot)
                ReloadWeapon();
            TryActing(); // TryShooting does what it says
            CheckIfDone(); 
        }
        
    }
    public void ReloadWeapon()
    {
        timeSinceShot += Time.deltaTime;
        if (timeSinceShot >= 1 / fireRate && ammo > 0)
            canShoot = true;
    }
    public void Pause()
    {
        paused = true;
        if (botType == BotType.mud) { mud.setVelocity(0); }       
    }
    public void Unpause()
    {
        if (botType == BotType.mud) { mud.setVelocity(movementSpeed); }
        paused = false;

    }
    public void RoundReset()
    {
        ammo = gm.shotsPerRound;
        done = false;
        timeSinceShot = 0;
        rb.position = gm.startPositionBot;
        BotSpecificReset();
        bullet.GetComponent<Bullet>().movementSpeed = gm.bulletSpeed;
        fireRate = gm.fireRate;
        //bulletsInAir = 0;


    }
    private void BotSpecificReset()
    {
        switch (botType)
        {
            case BotType.mud:
                mud.setVelocity(Vector2.down*movementSpeed);
                break;

            case BotType.selfMoving:
                break;
        }
    }
    //Checks if canShoot and acts accordingly.
    void TryActing()
    {
        GenerateInputs();
        nnOutput = nn.CalculateOutput(nnInput, ss);

        if (numberOfOutputs == 2 && canShoot)
            Shoot();
        else if (numberOfOutputs >= 3)
        {
            if (canShoot && WantToShoot())
            {
                Shoot();
            }
        }
        if (numberOfOutputs >= 5)
            TryMoving();
    }
    private void TryMoving()
    {
        float moveX = (nnOutput[3] > 0.5f) ? 1f : (nnOutput[3] < -0.5f) ? -1 : 0f;
        float moveY = (nnOutput[4] > 0.5f) ? 1f : (nnOutput[4] < -0.5f) ? -1 : 0f;
        rb.velocity = Vector2.up*moveY*movementSpeed + Vector2.right*moveX*movementSpeed;
    }
    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        
    }

        private bool WantToShoot()
    {
        if (nnOutput[2] > 0)
            return true;
        else
            return false;
    }
    private void Shoot()
    {
        shotPosition = new Vector2(nnOutput[0], nnOutput[1]);
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

    //Called if TryShoot deems it should.
    void shoot(Vector2 Direction) // Direction must be normalized;
    {

        float shotAngle = Mathf.Atan2(Direction.y, Direction.x);
        Vector2 instatiatePosition = (Vector2)transform.position + new Vector2(0.24f * Direction.x, 0.24f * Direction.y);

        GameObject inst = Instantiate(bullet, instatiatePosition, Quaternion.AngleAxis((180 / Mathf.PI) * shotAngle, new Vector3(0, 0, 1)));
        inst.GetComponent<Bullet>().shooter = gameObject;
        inst.GetComponent<SpriteRenderer>().color = sr.color;
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
        score += (1 - Mathf.Sqrt(sqrdDist)/Mathf.Pow(5f-0.24f,1))*scoreHitTarget;
        bulletsInAir--;
        if (bulletsInAir < 0)
            Debug.Break();

    }

    private void CheckIfDone()
    {
        //Debug.Log("CheckedIfDone, amm: " + ammo + ", bulletsInAir: " + bulletsInAir);
        if (ammo <= 0 && bulletsInAir == 0)
        {
            done = true;
            gm.BotDoneShooting(this);
        }
    }

    public void ToggleColor()
    {
        switch (cs)
        {
            case ColorScheme.baseColor:
                sr.color = Color.clear;
                cs = ColorScheme.clear;
                break;
            case ColorScheme.clear:
                sr.color = baseColor;
                cs = ColorScheme.baseColor;
                break;
        }
    }

    public void PrintWeights()
    {
        nn.PrintWeights();
    }

    public void SetColor(ColorScheme inC)
    {
        switch (inC)
        {
            case ColorScheme.green:
                sr.color = Color.green;
                cs = ColorScheme.green;
                break;
            case ColorScheme.clear:
                sr.color = Color.clear;
                cs = ColorScheme.clear;
                break;
            case ColorScheme.baseColor:
                sr.color = baseColor;
                cs = ColorScheme.baseColor;
                break;
        }
    }

    //Chosing and scaling inputs for the NN.
    void GenerateInputs()
    {
        nnInput = new float[numberOfInputs];
        for (int i = 0; i < numberOfInputs; i++)
        nnInput[0] = transform.position.x / (2.35f);
        nnInput[1] = transform.position.y / (2.35f);
        nnInput[2] = enemy.transform.position.x / (2.35f);
        nnInput[3] = enemy.transform.position.y / (2.35f);
        switch (numberOfInputs)
            {
                case 6:
                    nnInput[4] = enemy.GetComponent<Rigidbody2D>().velocity.x / 10;
                    nnInput[5] = enemy.GetComponent<Rigidbody2D>().velocity.y / 10;
                    break;
                case 7:
                    nnInput[4] = enemy.GetComponent<Rigidbody2D>().velocity.x / 10;
                    nnInput[5] = enemy.GetComponent<Rigidbody2D>().velocity.y / 10;
                    nnInput[6] = (2.35f - enemy.GetComponent<Rigidbody2D>().transform.position.y)/4.7f;
                    //nnInput[6] = enemy.GetComponent<Movement_UpDown>().timeSinceBounce/2;
                    break;
                case 8:
                    nnInput[4] = enemy.GetComponent<Rigidbody2D>().velocity.x / 10;
                    nnInput[5] = enemy.GetComponent<Rigidbody2D>().velocity.y / 10;
                    nnInput[6] = (2.35f - enemy.GetComponent<Rigidbody2D>().transform.position.y) / 4.7f;
                    nnInput[7] = (-2.35f - enemy.GetComponent<Rigidbody2D>().transform.position.y) / 4.7f;
                    //nnInput[6] = transform.position.x / (2.35f) - enemy.transform.position.x / (2.35f);
                    //nnInput[7] = transform.position.y / (2.35f) - enemy.transform.position.y / (2.35f);
                    break;
                case 9:
                    nnInput[4] = enemy.GetComponent<Rigidbody2D>().velocity.x / 10;
                    nnInput[5] = enemy.GetComponent<Rigidbody2D>().velocity.y / 10;
                    nnInput[6] = transform.position.x / (2.35f) - enemy.transform.position.x / (2.35f);
                    nnInput[7] = transform.position.y / (2.35f) - enemy.transform.position.y / (2.35f);
                    nnInput[8] = enemy.GetComponent<Movement_UpDown>().timeSinceBounce / 2;
                    break;

        }
    }
}
