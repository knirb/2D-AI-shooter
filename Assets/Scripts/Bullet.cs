using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float movementSpeed;
    public Rigidbody2D rb;
    public GameObject shooter;

    public float minDist;
    public float curDist;
    private GameObject enemy;
    private bool hitSomething;


    // Use this for initialization
    void Start () {
        hitSomething = false;
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * movementSpeed;
        enemy = GameObject.Find("Enemy");
        minDist = (enemy.transform.position - transform.position).sqrMagnitude;
    }
	
	// Update is called once per frame
	void FixedUpdate() {
        curDist = (enemy.transform.position - transform.position).sqrMagnitude;
        if (curDist < minDist)
            minDist = curDist;
        //transform.Translate(transform.right * movementSpeed * Time.deltaTime);
	}

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {           
            
        if (hitInfo.name == "Background" && !hitSomething)
        {
            Destroy(gameObject);
            shooter.GetComponent<Bot>().MissedTarget(minDist);
            hitSomething = true;
        }

        if (hitInfo.name == "Enemy" && !hitSomething)
        {
            Destroy(gameObject);
            shooter.GetComponent<Bot>().HitTarget();
            hitSomething = true;
            
        }
    }
}
