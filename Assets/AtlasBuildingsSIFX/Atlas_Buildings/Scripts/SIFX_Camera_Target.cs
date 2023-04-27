using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SIFX_Camera_Target : MonoBehaviour
{
	float fMoveSpeed = 2.0f;
	// Update is called once per frame
	void Update () 
	{
		transform.rotation = Quaternion.Euler(0,Camera.main.transform.eulerAngles.y,0); 	
		Vector3 vTranslate = new Vector3(Input.GetAxis ("Horizontal"),0.0f,Input.GetAxis("Vertical")) * fMoveSpeed * Time.deltaTime;
		transform.Translate(vTranslate, transform);
	}
}
