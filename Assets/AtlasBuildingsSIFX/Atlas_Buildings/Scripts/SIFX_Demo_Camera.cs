using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SIFX_Demo_Camera : MonoBehaviour 
{
	public Transform cam_target; 
	public float cam_target_height= 1.0f; 
	public float cam_distance = 12.0f; 
	public float max_cam_distance= 12.0f; 
	public float min_cam_distance= 1.0f; 
	public float x_speed= 250.0f; 
	public float y_speed= 120.0f; 
	public float y_min_limit= 45.0f; 
	public float y_max_limit= 45.0f; 
	public float zoom_rate= 20.0f; 

	private float x= 0.0f; 
	private float y= 0.0f; 

	void  Start ()
	{ 
		Vector3 angles= transform.eulerAngles; 
		x = angles.y; 
		y = angles.x; 
	} 

	void  LateUpdate ()
	{ 
		
		if(!cam_target)
		{   
			return; 
		}

		if (Input.GetMouseButton(2)) 
	   { 
			x += Input.GetAxis("Mouse X") * x_speed * Time.deltaTime; 
			y -= Input.GetAxis("Mouse Y") * y_speed * Time.deltaTime; 
	   } 

		cam_distance -= (Input.GetAxis("Mouse ScrollWheel") *20* Time.deltaTime) * zoom_rate;
		cam_distance = Mathf.Clamp(cam_distance, min_cam_distance, max_cam_distance); 
		y = ClampAngle(y, y_min_limit, y_max_limit); 
		Quaternion qRot = Quaternion.Euler(y, x, 0);
		Vector3 vPos = cam_target.position - (qRot * Vector3.forward * cam_distance + new Vector3(0,-cam_target_height,0)); 
		transform.rotation = qRot; 
		transform.position = vPos; 
	} 

	float  ClampAngle ( float angle ,   float min ,   float max  )
	{ 
	   if(angle < -360.0f) 
	   {
			angle += 360.0f; 
		}
		
	   if(angle > 360.0f) 
	   {
			angle -= 360.0f; 
		}
		
	   return Mathf.Clamp (angle, min, max); 
	}

}
