using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    //cameras
    public GameObject[] cameras;
    public string[] cameraNames;
    private int currentCamera;

    public bool flowFieldDebug;
    public bool pathDebug;
    public bool resistDebug;

	// Use this for initialization
	void Start () {
        currentCamera = 0;
    }
	
	// Update is called once per frame
	void Update () {
        //switch cameras with c
        if (Input.GetKeyDown(KeyCode.C))
        {
            cameras[currentCamera].SetActive(false);
            
            if(currentCamera == cameras.Length - 1)
            {
                currentCamera = 0;
            }
            else
            {
                currentCamera++;
            }

            cameras[currentCamera].SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            flowFieldDebug = !flowFieldDebug;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            pathDebug = !pathDebug;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            resistDebug = !resistDebug;
        }
    }

    //the game stuff
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 250, 100), "Press C to Change Cameras.\nPress F to show flow field.\nPress P to show the path.\nPress R to show the resistance field.\n\n Current Camera: " + cameraNames[currentCamera]);
        //skills
    }
}
