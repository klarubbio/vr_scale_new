using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FCG;

public class Tf01 : MonoBehaviour {

    [HideInInspector]
    public Vector3[] tf01;
    [HideInInspector]
    public Transform[] tsParent;
    [HideInInspector]
    public bool[] tsOneway;
    [HideInInspector]
    public int[] tsSide;

    private bool tested = false;

    public void getTF01()
    {

        if (!tested)
        {

            FCGWaypointsContainer[] ts = FindObjectsOfType<FCGWaypointsContainer>();

            tf01 = new Vector3[ts.Length * 2];
            tsParent = new Transform[ts.Length * 2];
            tsSide = new int[ts.Length * 2];
            tsOneway = new bool[ts.Length * 2];


            int t = -1;
            for (int i = 0; i < ts.Length; i++)
            {
                t++;
                tf01[t] = ts[i].Node(0, 0);
                tsParent[t] = ts[i].transform;
                tsSide[t] = 0;
                tsOneway[t] = ts[i].oneway;
                t++;
                tf01[t] = ts[i].Node(1, 0);
                tsParent[t] = ts[i].transform;
                tsSide[t] = 1;
                tsOneway[t] = ts[i].oneway;
            }

        }
        
        tested = true;
  
    }


}
