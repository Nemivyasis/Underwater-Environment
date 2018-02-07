using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockScript : MonoBehaviour {

    //the flockers
    public List<GameObject> flockers;
    private List<FlockerScript> flockerScripts;

    public Vector3 averageVelocity;
    public Vector3 center;

    public SpawnScript spawner;
    public int flockerNumber;

    public GameObject centerObject;

	// Use this for initialization
	void Start () {
        flockerScripts = new List<FlockerScript>();

        flockers = spawner.SpawnFlocker(flockerNumber);

        foreach (var item in flockers)
        {
            FlockerScript script = item.GetComponent<FlockerScript>();

            if(script != null)
            {
                flockerScripts.Add(script);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        center = FindCenter();
        centerObject.transform.position = center;
  
        averageVelocity = FindVelocity();
        centerObject.transform.forward = averageVelocity.normalized;
	}

    private Vector3 FindVelocity()
    {
        Vector3 sum = Vector3.zero;

        foreach (var item in flockerScripts)
        {
            sum += item.velocity;
        }

        return sum / flockerScripts.Count;
    }

    private Vector3 FindCenter()
    {
        Vector3 sum = Vector3.zero;

        foreach (var item in flockers)
        {
            sum += item.transform.position;
        }


        return sum / flockers.Count;
    }
}
