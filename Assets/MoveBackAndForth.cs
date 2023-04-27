using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBackAndForth : MonoBehaviour
{

    public float lower, upper;
    private int sign;

    // Start is called before the first frame update
    void Start()
    {
        sign = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.position.x >= lower && sign < 0)
        {
            this.transform.position -= new Vector3(0.01f, 0.0f, 0.0f);
        }
        else if (this.transform.position.x <= upper && sign > 0)
        {
            this.transform.position += new Vector3(0.01f, 0.0f, 0.0f);
        }

        if (this.transform.position.x > upper || this.transform.position.x < lower)
        {
            sign = -sign;
        }
            
    }
}
