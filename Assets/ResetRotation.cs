using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetRotation : MonoBehaviour
{
    private Quaternion rot;
    // Start is called before the first frame update
    void Start()
    {
        rot = this.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = rot;
    }
}
