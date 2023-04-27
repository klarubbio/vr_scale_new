using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavLearningCompass : MonoBehaviour
{
    public bool disappearWhenClose;
    public float disappearDistance;
    private bool visible;
    public static GameObject target;
    private float xRot, zRot;
    // Start is called before the first frame update
    void Start()
    {
        xRot = this.transform.eulerAngles.x;
        zRot = this.transform.eulerAngles.z;
        visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //get visibility of compass needle
        visible = this.GetComponentInChildren<MeshRenderer>().enabled;
        //check visibility
        if (visible)
        {
            //if we want the compass to disappear when it is within a certain distance to the target
            if (disappearWhenClose && Vector3.Distance(this.transform.position, target.transform.Find("SnapPoint").position) <= disappearDistance)
            {
                //make it invisible
                this.GetComponentInChildren<MeshRenderer>().enabled = false;
            }

            try
            {
                this.transform.LookAt(target.transform.Find("SnapPoint").position, Vector3.up);
                this.transform.eulerAngles = new Vector3(xRot, this.transform.eulerAngles.y, zRot);
            }
            catch (Exception e)
            {
                //target will probably be null at some point
            }

        }

    }
}
