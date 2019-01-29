using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour {

    private float lifetime = 2f;
	// Use this for initialization
	void Awake () {
        Destroy(gameObject, lifetime);
	}

    
	
	// Update is called once per frame
	void Update () {
	
	}

    private void OnCollisionEnter(Collision collision)
    {
       // Destroy(gameObject);

    }
}
