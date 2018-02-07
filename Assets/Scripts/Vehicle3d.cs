using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Vehicle3d : MonoBehaviour
{

    //Vectors a vehicle needs for force based movement
    public Vector3 vehiclePosition;
    public Vector3 direction;
    public Vector3 velocity;
    public Vector3 acceleration;

    //floats a vehicle needs for force based movement
    public float mass;
    //public float friction;
    public float maxSpeed;
    public float maxForce;

    //for collision
    public float radius;

    //for separation
    public float separationRadius;

    //for arrival
    public float arrivalRadius;

    //fo wandering
    public float wanderRadius;
    public float wanderStep;
    public float maxWanderAngle;
    private float wanderPerlinValueXZ;
    private float wanderPerlinValueY;

    //for obstacle avoidance
    public List<GameObject> obstacles;
    public float maxObstacleRadius;

    //if it wants to stay on ground
    public bool grounded;
    public Terrain terrain;

    //for resistance fields
    public Rect resistanceBounds;

    // Use this for initialization
    protected virtual void Start()
    {
        //grab the transforms position
        vehiclePosition = transform.position;
        wanderPerlinValueXZ = UnityEngine.Random.Range(0f, 10f);
        wanderPerlinValueY = UnityEngine.Random.Range(0f, 10f);
    }

    // Update is called once per frame
    void Update()
    {

        calcSteeringForces();

        calcPosition();

        //make it point the right way
        SetTransform();
    }

    //calculate forces
    public abstract void calcSteeringForces();

    //finds the position from acceleration and velocity
    private void calcPosition()
    {
        //for testing
        vehiclePosition = transform.position;

        //clamp acceleration
        acceleration = Vector3.ClampMagnitude(acceleration, maxForce);

        //add acceleration to velocity
        velocity += acceleration * Time.deltaTime;

        //make the velocity's y = 0
        velocity = new Vector3(velocity.x, velocity.y , velocity.z);

        //clamp velocity
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        //add current velocity to position
        vehiclePosition += velocity * Time.deltaTime;

        //if grounded, set y
        if (grounded)
        {
            vehiclePosition.y = terrain.SampleHeight(vehiclePosition);
            velocity.y = 0;
        }

        //Normalize the direction vector
        direction = velocity.normalized;

        //zero acceleration
        acceleration = Vector3.zero;
    }

    //changes direction to match velocity
    private void SetTransform()
    {

        //make the object there
        transform.position = vehiclePosition;

        transform.forward = direction;

    }

    //steering forces
    //fleeing
    //flee from an object
    protected Vector3 FleeForce(GameObject targetObject)
    {
        if (targetObject.transform != null && targetObject.transform.position != null)
        {
            return FleeForce(targetObject.transform.position);
        }
        else
        {
            return new Vector3(0, 0, 0);
        }
    }
    //flee from a position
    protected Vector3 FleeForce(Vector3 targetPosition)
    {
        //get desired velocity
        Vector3 desiredVelocity = transform.position - targetPosition;

        //scale it
        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        //get the force
        Vector3 steeringForce = desiredVelocity - velocity;

        return steeringForce;
    }

    //separation - keep distance
    protected Vector3 Separation(List<GameObject> others)
    {
        Vector3 ultimateForce = Vector3.zero;

        foreach (var item in others)
        {
            if (item != gameObject)
            {
                if ((item.transform.position - transform.position).sqrMagnitude < separationRadius * separationRadius && (item.transform.position - transform.position).sqrMagnitude != 0)
                {
                    ultimateForce += FleeForce(item) * ((separationRadius * separationRadius) / (item.transform.position - transform.position).sqrMagnitude);
                }
            }
        }

        return ultimateForce;
    }

    //void ApplyForce()
    //Applies an incoming force to the accel vector
    public void ApplyForce(Vector3 force)
    {
        //a = f/m
        //add the force divided by mass to current acceleration
        acceleration += force / mass;
    }

    //seeking
    //seek a position
    protected Vector3 SeekForce(Vector3 targetPosition)
    {
        //get the desired velocity
        Vector3 desiredVelocity = targetPosition - transform.position;
        //clamp it to the max speed
        //desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);

        //other approach
        desiredVelocity = maxSpeed * desiredVelocity.normalized;

        //get the steering force
        Vector3 steeringForce = desiredVelocity - velocity;
        return steeringForce;
    }

    //seek an object
    protected Vector3 SeekForce(GameObject targetObject)
    {
        if (targetObject.transform != null && targetObject.transform.position != null)
        {
            return SeekForce(targetObject.transform.position);
        }
        else
        {
            return new Vector3(0, 0, 0);
        }
    }

    //seek an object but slow upon getting closer and say true when get there place thing yo
    protected Vector3 SeekArrival2D(Vector3 targetPosition, out bool Arrived)
    {
        Vector3 desiredVelocity = targetPosition - transform.position;

        //clamp it to the max speed
        desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);

        //get the steering force
        Vector3 steeringForce = desiredVelocity - velocity;

        //check if it arrived
        if((targetPosition - transform.position).sqrMagnitude <= arrivalRadius * arrivalRadius)
        {
            Arrived = true;
        }
        else{
            Arrived = false;
        }

        return steeringForce.normalized;
    }

    //wander
    //get a circle in front and then rotate the forward radius around center
    protected Vector3 Wander()
    {
        //set the circle center
        Vector3 circleCenter = transform.position + 2 * transform.forward;

        //find a random angle with perlin noise
        float angleXZ = Mathf.PerlinNoise(wanderPerlinValueXZ, 1);
        wanderPerlinValueXZ += wanderStep;

        float angleY = Mathf.PerlinNoise(wanderPerlinValueY, 1);
        wanderPerlinValueY += wanderStep;

        //make the angle within bounds
        angleXZ -= .5f;
        angleXZ *= 2;
        angleXZ *= maxWanderAngle;

        angleY -= .5f;
        angleY *= 2;
        angleY *= maxWanderAngle;

        //rotate the forward standardized vector by that much
        Vector3 desiredPointVector = Rotate2d(transform.forward.normalized * wanderRadius, angleXZ);
        desiredPointVector = RotateY(desiredPointVector, angleY);

        return SeekForce(circleCenter + desiredPointVector);
    }

    //rotate a 3d vector around the y axis
    private Vector3 Rotate2d(Vector3 original, float angle)
    {
        float newX = original.x * Mathf.Cos(angle) - original.z * Mathf.Sin(angle);
        float newZ = original.x * Mathf.Sin(angle) + original.z * Mathf.Cos(angle);

        return new Vector3(newX, original.y, newZ);
    }

    private Vector3 RotateY(Vector3 original, float angle)
    {
        original.y = original.y * Mathf.Cos(angle);

        return original;
    }

    //check if the vehicle is outside of a rectangular prism and push them back in
    protected Vector3 CheckBounds(float[] xBounds, float[] yBounds, float[] zBounds)
    {
        Vector3 ultimateForce = Vector3.zero;

        //check x
        if(transform.position.x < xBounds[0])
        {
            ultimateForce += new Vector3(1, 0, 0);
        }
        else if(transform.position.x > xBounds[1])
        {
            ultimateForce += new Vector3(-1, 0, 0);
        }

        //y
        if (transform.position.y < yBounds[0])
        {
            ultimateForce += new Vector3(0, 1, 0);
        }
        else if (transform.position.y > yBounds[1])
        {
            ultimateForce += new Vector3(0, -1, 0);
        }

        //z
        if (transform.position.z < zBounds[0])
        {
            ultimateForce += new Vector3(0, 0, 1);
        }
        else if (transform.position.z > zBounds[1])
        {
            ultimateForce += new Vector3(0, 0, -1);
        }

        return ultimateForce.normalized;
    }

    //obstacle avoidance
    public Vector3 AvoidObstacle(List<GameObject> avoidThese)
    {
        Vector3 ultimateForce = Vector3.zero;

        foreach (var obst in avoidThese)
        {
            float dotForward = Vector3.Dot(obst.transform.position - transform.position, transform.forward);

            ObstacleScript obScript = obst.GetComponent<ObstacleScript>();

            if(obScript != null)
            {
                //is it in front and close enough
                if (dotForward > 0 && dotForward < maxObstacleRadius)
                {
                    //will I collide on the left-right plane
                    float dotRight = Vector3.Dot(obst.transform.position - transform.position, transform.right);

                    if (dotRight < radius + obScript.radius)
                    {
                        //will it collide on up-down plane
                        float dotUp = Vector3.Dot(obst.transform.position - transform.position, transform.up);

                        if(dotUp < radius + obScript.radius)
                        {
                            if(dotUp > 0)
                            {
                                ultimateForce += -transform.up * (maxObstacleRadius - dotForward) * dotUp;
                            }
                            if (dotUp > 0)
                            {
                                ultimateForce += transform.up * (maxObstacleRadius - dotForward) * -dotUp;
                            }

                            if (dotRight > 0)
                            {
                                ultimateForce += -transform.right * (maxObstacleRadius - dotForward) * dotRight;
                            }
                            if (dotRight > 0)
                            {
                                ultimateForce += transform.right * (maxObstacleRadius - dotForward) * -dotRight;
                            }

                            //odd edge case
                            if(dotRight == 0 && dotUp == 0)
                            {
                                ultimateForce += (transform.right * Random.Range(.001f, 1f) + transform.up * Random.Range(.001f, 1f)).normalized * (maxObstacleRadius - dotForward);
                            }
                        }
                    }
                }
            }
        }

        return ultimateForce.normalized;
    }

    //apply a reverse force
    protected Vector3 Friction()
    {
        //get the dot productes of the future old velocity and the new future velocity if just the friction is applied 
        //this dot is with the velocity vector normalized
        float oldVelDot = Vector3.Dot(velocity.normalized, velocity.normalized);
        float newVelDot = Vector3.Dot(velocity.normalized, velocity.normalized * -1 * maxForce + velocity);

        //if they are on separate sides of the agent, then make it so that the force is only the required force to be 0, not the max
        if (oldVelDot / Mathf.Abs(oldVelDot) != newVelDot / Mathf.Abs(newVelDot))
        {
            return -1 * velocity * mass;
        }

        //otherwise its max
        return velocity.normalized * -1 * maxForce;
    }

    //check if the thing is inside a 2D resistance field and slow them
    protected Vector3 Resistance()
    {
        //check if in bounds of resistance
        if(transform.position.x >= resistanceBounds.x && transform.position.x <= resistanceBounds.x + resistanceBounds.width)
        {
            if(transform.position.z >= resistanceBounds.y && transform.position.z <= resistanceBounds.y + resistanceBounds.height)
            {
                return -velocity;
            }
        }

        return Vector3.zero;

    }


}