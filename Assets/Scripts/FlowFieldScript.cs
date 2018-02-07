using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldScript : MonoBehaviour {

    public Vector3[,,] flowField;
    public int xNum;
    public int yNum;
    public int zNum;
    public float cubeSideSize;

    public float maxPointDistance;

    public CameraScript manager;
    public Material lineDebug;
    public Material directionColor;



    public bool cage;

    // Use this for initialization
    void Start () {
        //3d because why not
        flowField = new Vector3[xNum, yNum, zNum];

        //the starting values
        GenerateFlowField();


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnRenderObject()
    {
        if (manager.flowFieldDebug)
        {
            for (int i = 0; i < xNum; i++)
            {
                for (int j = 0; j < yNum; j++)
                {
                    for (int k = 0; k < zNum; k++)
                    {
                        if (flowField[i, j, k] != null)
                        {
                            Vector3 fieldCenter = new Vector3(i, j, k) * 5;
                            // Set the material to be used for the first line
                            lineDebug.SetPass(0);
                            // Draws one line
                            GL.Begin(GL.LINES);
                            // Begin to draw lines
                            GL.Vertex(fieldCenter);
                            // First endpoint of this line
                            GL.Vertex(fieldCenter + flowField[i, j, k] * 5);
                            // Second endpoint of this line
                            GL.End(); // Finish drawing the line

                            //red for the front of it
                            // Set the material to be used for the first line
                            directionColor.SetPass(0);
                            // Draws one line
                            GL.Begin(GL.LINES);
                            // Begin to draw lines
                            GL.Vertex(fieldCenter + flowField[i, j, k] * 4);
                            // First endpoint of this line
                            GL.Vertex(fieldCenter + flowField[i, j, k] * 5);
                            // Second endpoint of this line
                            GL.End(); // Finish drawing the line
                        }
                    }
                }
            }
        }
    }

    private void GenerateFlowField()
    {
        //make 4 vertices randomly
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(Random.Range(15f, 44f), Random.Range(15f, 30f), Random.Range(15f, 44f));
        vertices[1] = new Vector3(Random.Range(55f, 84f), Random.Range(15f, 30f), Random.Range(15f, 44f));
        vertices[2] = new Vector3(Random.Range(55f, 84f), Random.Range(15f, 30f), Random.Range(55f, 84f));
        vertices[3] = new Vector3(Random.Range(15f, 44f), Random.Range(15f, 30f), Random.Range(55f, 84f));

        //make 4 vectors from one vertex to the other
        Vector3[] pathVectors = new Vector3[4];
        for (int i = 0; i < vertices.Length; i++)
        {
            pathVectors[i] = vertices[(i + 1) % vertices.Length] - vertices[i];
        }

        Vector3 closestVector = Vector3.zero;
        float closestSqrMag = float.MaxValue;
        Vector3 closestToOrthogonal = Vector3.zero;

        List<Vector3> closeEnoughLerp = new List<Vector3>();
        List<float> closeEnoughSqDist = new List<float>();
        //for each section, find close enough lines and closest lines
        for (int i = 0; i < xNum; i++)
        {
            for (int j = 0; j < yNum; j++)
            {
                for (int k = 0; k < zNum; k++)
                {
                    //clear the stuff
                    closestSqrMag = float.MaxValue;
                    closestToOrthogonal = Vector3.zero;
                    closeEnoughLerp.Clear();
                    closeEnoughSqDist.Clear();
                    closestVector = Vector3.zero;

                    //set the center of the cube
                    Vector3 fieldCenter = new Vector3(i, j, k) * cubeSideSize + new Vector3(cubeSideSize / 2f, cubeSideSize / 2f, cubeSideSize / 2f);

                    Vector3 vectorToLine = Vector3.zero;
                    //find the distance to each line
                    for (int l = 0; l < vertices.Length; l++)
                    {
                        float sqrDist = FindSquaredDistance(vertices[l], vertices[(l + 1) % vertices.Length], fieldCenter, out vectorToLine);

                        //check it against minimum distance
                        if (sqrDist <= maxPointDistance * maxPointDistance)
                        {
                            //lerp it if close enough
                            closeEnoughLerp.Add(Vector3.Lerp(pathVectors[l], vectorToLine, sqrDist / (maxPointDistance * maxPointDistance) * .5f).normalized );
                            closeEnoughSqDist.Add(sqrDist);
                        }

                       //check it agains the closest
                        if (closeEnoughLerp.Count == 0 && sqrDist < closestSqrMag)
                        {
                            closestVector = pathVectors[l];
                            closestSqrMag = sqrDist;
                            closestToOrthogonal = vectorToLine;
                        }

                    }

                    //if there is none, the vector to the closest line is the value
                    if(closeEnoughLerp.Count == 0)
                    {
                        flowField[i, j, k] = Vector3.Lerp(closestVector, vectorToLine, .5f).normalized;
                    }
                    else if(closeEnoughLerp.Count == 1) //if there is one, the lerp is the value
                    {
                        flowField[i, j, k] = closeEnoughLerp[0].normalized;
                    }
                    else
                    {
                        //now its fun
                        //find a weighted average based on the magnitudes (not squared)
                        float magSum = 0;
                        for (int l = 0; l < closeEnoughSqDist.Count; l++)
                        {
                            closeEnoughSqDist[l] = Mathf.Sqrt(closeEnoughSqDist[l]);
                            magSum += closeEnoughSqDist[l];
                        }

                        Vector3 finalVector = Vector3.zero;
                        for (int l = 0; l < closeEnoughLerp.Count; l++)
                        {
                            finalVector += closeEnoughLerp[l].normalized * ((magSum - closeEnoughSqDist[l]) / (magSum * (closeEnoughSqDist.Count - 1)));   
                        }

                        //set it
                        flowField[i, j, k] = finalVector.normalized;
                    }

                }
            }
        }

        //if close enough then find the linear interpolation of the perpindicular vector and regular vector
        //
    }

    private float FindSquaredDistance(Vector3 vertex1, Vector3 vertex2, Vector3 point, out Vector3 vectorToLine)
    {
        //check to make sure it isn't behind vertex 2 (this makes the line not continue to infinity, but it goes from infinity)
        if (Vector3.Dot(vertex1 - vertex2, point - vertex2) <= -5)
        {
            vectorToLine = vertex2 - point;
            return vectorToLine.sqrMagnitude;
        }
        /*//check to make sure it isn't behind vertex 1 (this makes the line not continue to infinity, but it goes from infinity)
        if (Vector3.Dot(vertex2 - vertex1, point - vertex1) <= -5)
        {
            vectorToLine = vertex1 - point;
            return vectorToLine.sqrMagnitude;
        }*/

        //get vector of the line
        Vector3 vectorFrom1To2 = vertex2 - vertex1;
        //get projection of vector from vertex1 to point onto the line and use that to find the point orthogonal to the vector that goes through the line
        Vector3 orthogonalPoint = (Vector3.Dot(vectorFrom1To2, point - vertex1) / Vector3.Dot(vectorFrom1To2, vectorFrom1To2)) * vectorFrom1To2 + vertex1;

        //get the vector from point to the orthogonal point
        vectorToLine= orthogonalPoint - point;
        //print(vertex1 + " " + vertex2 + " " + point + " " + vectorToLine);
        //return squared magnitude
        return vectorToLine.sqrMagnitude;
        
    }

    public Vector3 GetForce(Vector3 position)
    {
        return flowField[(int)Mathf.Clamp(Mathf.Floor(position.x / 5), 0, xNum - 1), (int)Mathf.Clamp(Mathf.Floor(position.y / 5), 0, yNum - 1), (int)Mathf.Clamp(Mathf.Floor(position.z / 5), 0, zNum - 1)];
    }
}
