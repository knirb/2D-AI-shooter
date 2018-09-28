using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_UpDown : MonoBehaviour {


    public float movementSpeed;
    private Rigidbody2D rb;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector3(0, 1, 0) * movementSpeed;
	}

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.name == "Background")
        {
            rb.velocity = -rb.velocity;
        }
    }
}
