using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollowerScript : Vehicle3d {

    //where to seek
    public GameObject[] pathVertices;
    public int currentSoughtVertexInd;
    public bool waiting;
    public float maxWaitTime;
    public float currentWaitTime;

    //weights
    public float pathFollowWeight;
    public float frictionWeight;
    public float resistanceWeight;


    public CameraScript manager;

    //for debug lines
    public Material pathDebug;
    public Material resistanceDebug;

    // Use this for initialization
    protected override void Start () {
        currentWaitTime = 0;
        currentSoughtVertexInd = 0;
        waiting = false;
        base.Start();
	}

    public override void calcSteeringForces()
    {
        Vector3 ultimateForce = Vector3.zero;

        bool arrived = false;


        //check if he is waiting to move (eating)
        if (waiting)
        {
            //make him stop with friction!
            ultimateForce += Friction() * frictionWeight;

            //update the wait time
            currentWaitTime += Time.deltaTime;

            //see if it is time to move
            if(currentWaitTime >= maxWaitTime)
            {
                waiting = false;
                currentWaitTime = 0;
                currentSoughtVertexInd = (currentSoughtVertexInd + 1) % pathVertices.Length;
            }

        }
        else //otherwise move to the next point
        {
            ultimateForce += SeekArrival2D(pathVertices[currentSoughtVertexInd].transform.position, out arrived) * pathFollowWeight;
        }

        //if they are not waiting but have gotten close enough to the point, make them wait
        if (!waiting && arrived)
        {
            waiting = true;
            currentWaitTime = 0;
        }

        //the resistance field
        ultimateForce += Resistance() * resistanceWeight;

        ApplyForce(ultimateForce);
    }

    private void OnRenderObject()
    {
        if (manager.pathDebug)
        {
            for (int i = 0; i < pathVertices.Length; i++)
            {
                // Set the material to be used for the first line
                pathDebug.SetPass(0);
                // Draws one line
                GL.Begin(GL.LINES);
                // Begin to draw lines
                GL.Vertex(new Vector3(pathVertices[i].transform.position.x, 2, pathVertices[i].transform.position.z));
                // First endpoint of this line
                GL.Vertex(new Vector3(pathVertices[(i + 1) % pathVertices.Length].transform.position.x, 2, pathVertices[(i + 1) % pathVertices.Length].transform.position.z));
                // Second endpoint of this line
                GL.End(); // Finish drawing the line
            }
        }

        if (manager.resistDebug)
        {
            // Set the material to be used for the first line
            resistanceDebug.SetPass(0);
            // Draws one line
            GL.Begin(GL.QUADS);
            // Begin to draw lines
            GL.Vertex3(resistanceBounds.xMax, 3, resistanceBounds.yMax);
            // First endpoint of this line
            GL.Vertex3(resistanceBounds.xMax, 2, resistanceBounds.yMin);
            GL.Vertex3(resistanceBounds.xMin, 2, resistanceBounds.yMin);
            GL.Vertex3(resistanceBounds.xMin, 2, resistanceBounds.yMax);
            // Second endpoint of this line
            GL.End(); // Finish drawing the line
        }
    }
}
