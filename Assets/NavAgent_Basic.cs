using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAgent_Basic : MonoBehaviour
{
    public Transform goal;
    public bool calculatePath;

    void Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.destination = goal.position;

        if (calculatePath)
        {
            NavMeshPath path = new NavMeshPath(); //create new empty path
            NavMesh.CalculatePath(this.transform.position, goal.transform.position, NavMesh.AllAreas, path); //calculate path from agent to c
            float pathLength = 0.0f; //initialize to 0
            for (int i = 1; i < path.corners.Length; i++)//interate through all segments
            {
                pathLength += Vector3.Distance(path.corners[i - 1], path.corners[i]); 
            }
            Debug.Log("Path length: " + pathLength);
        }
    }
}
