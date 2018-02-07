using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockerScript : Vehicle3d {

    //the script for the full flock
    public FlockScript flock;

    //weights
    public float cohesionWeight;
    public float alignmentWeight;
    public float separationWeight;
    public float wanderWeight;
    public float checkBoundsWeight;
    public float avoidObstacleWeight;

    //bounds things
    public float[] xBounds;
    public float[] yBounds;
    public float[] zBounds;

    // Use this for initialization
    protected override void Start () {
        base.Start();
	}
	

    public override void calcSteeringForces()
    {
        Vector3 ultimateForce = Vector3.zero;
        //cohesion
        ultimateForce += Cohesion() * cohesionWeight;

        //alignment
        ultimateForce += Alignment() * alignmentWeight;

        //separation
        ultimateForce += Separation(flock.flockers) * separationWeight;

        //wander
        ultimateForce += Wander() * wanderWeight;

        //bounds
        ultimateForce += CheckBounds(xBounds, yBounds, zBounds) * checkBoundsWeight;

        //obstacle avoidance
        ultimateForce += AvoidObstacle(obstacles) * avoidObstacleWeight;

        ApplyForce(ultimateForce);
    }

    //find center and velocity
    //only returns something from flock script with a one flock system
    //useful if I ever want individual and separate flocks
    public Vector3 FindFlockCenter()
    {
        return flock.center;
    }

    public Vector3 FindFlockAverageVelocity()
    {
        return flock.averageVelocity;
    }

    //flocker specific forces
    //cohesion
    protected Vector3 Cohesion()
    {
        return SeekForce(FindFlockCenter());
    }

    //alignment
    protected Vector3 Alignment()
    {
        return (FindFlockAverageVelocity() - velocity).normalized;
    }
}
