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
    private bool hitSomething; //Used because it could hit multiple things causing counts in game manager to break.

    void Start () {
        hitSomething = false;
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * movementSpeed;
        enemy = GameObject.Find("Enemy");
        minDist = (enemy.transform.position - transform.position).sqrMagnitude;
    }
	
	void FixedUpdate() {
        curDist = (enemy.transform.position - transform.position).sqrMagnitude;
        if (curDist < minDist)
            minDist = curDist;
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
