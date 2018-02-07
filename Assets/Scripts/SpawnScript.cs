using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScript : MonoBehaviour {

    //the prefab
    public GameObject prefab;
    public FlockScript flockScript;

    //spawn bounds
    public float[] xSpawnBounds;
    public float[] ySpawnBounds;
    public float[] zSpawnBounds;

    //obstacles
    public List<GameObject> obstacles;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //only spawns something at a random location
    public GameObject SpawnFlocker(GameObject pref, float[] xBounds, float[] yBounds, float[] zBounds)
    {
        //instantiate it
        GameObject newObject = Instantiate(pref);

        //get a random location in the bounds
        float randomX = Random.Range(xBounds[0], xBounds[1]);
        float randomY = Random.Range(yBounds[0], yBounds[1]);
        float randomZ = Random.Range(zBounds[0], zBounds[1]);

        newObject.transform.position = new Vector3(randomX, randomY, randomZ);

        return newObject;
    }

    public List<GameObject> SpawnFlocker(int number)
    {
        List<GameObject> flockers = new List<GameObject>();

        //spawn it
        for (int i = 0; i < number; i++)
        {
            flockers.Add(SpawnFlocker(prefab, xSpawnBounds, ySpawnBounds, zSpawnBounds));

            //set the script
            FlockerScript script = flockers[i].GetComponent<FlockerScript>();
            script.flock = flockScript;
            script.obstacles = obstacles;
        }

        return flockers;
    }
}
