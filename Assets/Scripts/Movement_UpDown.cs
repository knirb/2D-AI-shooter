using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_UpDown : MonoBehaviour {

    private GameManager gm;
    private float movementSpeed;
    private Vector3 direction;
    private Rigidbody2D rb;
    public float timeSinceBounce;

	void Start () {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector3(0, 1, 0) * movementSpeed;
        direction = rb.velocity.normalized;
        timeSinceBounce = 0;
	}
    private void Update()
    {
        timeSinceBounce += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        timeSinceBounce = 0;
        if (hitInfo.name == "Background")
        {
            rb.velocity = -rb.velocity;
            direction = rb.velocity.normalized;
        }
    }
    public void setSpeed(float inp)
    {
        movementSpeed = inp;
    }

    public void setVelocity(float inp)
    {
        rb.velocity = direction * inp;
    }

    public void setVelocity(Vector3 inp)
    {
        rb.velocity = inp;
        direction = inp.normalized;
    }

    public float getSpeed()
    {
        return movementSpeed;
    }
}
