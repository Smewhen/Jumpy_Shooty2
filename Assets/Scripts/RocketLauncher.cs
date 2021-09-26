using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour {
    private World world;

    public Rigidbody rocket;
    public int rocketSpeed = 15;

    public float checkIncrement = 0.01f;
    
    void Start () {
        world = GameObject.Find ("World").GetComponent<World>();
    
    }

    void Update () {
        if (Input.GetKeyDown ("p")) {
            LaunchRocket ();
        }
    }

    private void LaunchRocket () {
        Rigidbody clonedRocket;
        clonedRocket = Instantiate (rocket, transform.position, transform.rotation);

        clonedRocket.velocity = transform.TransformDirection (Vector3.up * rocketSpeed);
    }
}
