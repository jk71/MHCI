using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienBehaviour : MonoBehaviour {

    public int health = 3;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter()
    {
        health--;
        Debug.Log("Alien Health is: " + health);
    
        if(health == 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        health--;
        Debug.Log("Alien Health is: " + health);

        if (health == 0)
        {
            Destroy(gameObject);
        }
    }
}

