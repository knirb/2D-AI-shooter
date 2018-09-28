using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public GameObject bullet;

    private Vector2 clickPosition; 
    private Vector2 playerPosition; 
    private Vector2 shotDirection; 


	void Start () {
		
	}
	

	void Update () {
        
        if (Input.GetButtonDown("Fire1"))
        {
            clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Clicked position in world space instead of screen space
            shotDirection = (clickPosition - new Vector2(transform.position.x, transform.position.y)).normalized; // normalized vector player-clicked position
             // In radians
            shoot(shotDirection);
        }
            

	}

    void shoot(Vector2 Direction)
    {
        playerPosition = transform.position;
        float shotAngle = Mathf.Atan2(Direction.y, Direction.x);
        Vector2 instatiatePosition = playerPosition + new Vector2(0.24f * Direction.x,  0.24f * Direction.y);
        Instantiate(bullet, instatiatePosition, Quaternion.AngleAxis((180/Mathf.PI) * shotAngle, new Vector3(0,0,1)));        
    }

    
}
