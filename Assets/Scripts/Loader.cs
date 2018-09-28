using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

    public GameObject gameManager;

	void Start () {
        if (GameManager.instance == null)
        {
            GameObject gm = Instantiate(gameManager, Vector3.zero, Quaternion.identity);
            gm.name = "GameManager";
        }
    }
}
