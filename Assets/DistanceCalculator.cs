using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceCalculator : MonoBehaviour
{
  public GameObject object1;
  public GameObject object2;


  // Update is called once per frame
  void Update()
  {
      float distance = Vector3.Distance(object1.transform.position, object2.transform.position);
      Debug.Log("This distance between " + object1 + " and " + object2 + " is " + distance);
  }
}
