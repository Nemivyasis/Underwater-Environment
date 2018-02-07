using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFielderScript : Vehicle3d {

    public FlowFieldScript flowFieldSc;

    public float flowWeight;


    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }


    public override void calcSteeringForces()
    {
        Vector3 ultimateForce = Vector3.zero;

        ultimateForce += FlowForce();

        ApplyForce(ultimateForce);
    }

    private Vector3 FlowForce()
    {
        //future position
        return (flowFieldSc.GetForce(transform.position + velocity) * maxSpeed - velocity).normalized;
    }
}
