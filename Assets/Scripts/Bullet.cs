using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float movementSpeed;
    public Rigidbody2D rb;
    public GameObject shooter;

    private float minDist;
    private float curDist;
    private GameObject enemy;


    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * movementSpeed;
        enemy = GameObject.Find("Enemy");
        minDist = (enemy.transform.position - transform.position).sqrMagnitude;
    }
	
	// Update is called once per frame
	void Update () {
        curDist = (enemy.transform.position - transform.position).sqrMagnitude;
        if (curDist < minDist)
            minDist = curDist;
        //transform.Translate(transform.right * movementSpeed * Time.deltaTime);
	}

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.name == "Background")
        {
            shooter.GetComponent<Bot>().missedTarget(minDist);
            Destroy(gameObject);
        }
            ;
        if (hitInfo.name == "Enemy")
        {
            shooter.GetComponent<Bot>().hitTarget();
            Destroy(gameObject);
        }
    }
}
