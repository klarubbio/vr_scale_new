using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

public class Move_Look_VR : MonoBehaviour {

    public static Move_Look_VR instance;

    public string participantID;
    public Transform Reference_Frame;
    public GameObject game_object;
    public Transform omniRing_transform;

    //for direction person's waist is facing in Omni
    public float newOmniDirection;
    public float omniYaw1;
    public float omniYaw2;
    float ringAngleInGameWorld;

    public float interval = .1F; // save positions each 1.0 second
    public float tSample = 1.0F; // sampling starts after 10 seconds
    private string fileName = "log_" + DateTime.Now.ToString("dd-MM-yyyy_hhmmss") + ".txt";
    private string debugfileName = "debuglog_" + DateTime.Now.ToString("dd-MM-yyyy_hhmmss") + ".txt";

    private void Start () {
      InvokeRepeating("RecordPosition", tSample, interval);
      // Create file to store pointing data
      string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
      string pathString = System.IO.Path.Combine(path, "SilctonVRData");
      System.IO.Directory.CreateDirectory(pathString);
      string fullFileName = System.IO.Path.Combine(pathString,fileName);

      // Get Scene Name from Unity
      string sceneName = SceneManager.GetActiveScene().name;
      string sceneNameWrite = "Scene Name is " + sceneName + "\n";

      // Assemble participantID AND quit the application if none is entered.
      
      string participantIDWrite = "Participant ID is " + participantID + "\n";
      if (string.IsNullOrEmpty(participantID)) {
        Debug.Log("You forgot a participant ID");
        Application.Quit();
        // UnityEditor.EditorApplication.isPlaying = false;
      }


      string header = "x,y,z,walking_angle,head_angle\n";
      string fileNameWrite = fileName + "\n";
      using (System.IO.StreamWriter sw = System.IO.File.CreateText(fullFileName))
      {
        sw.WriteLine(fileNameWrite);
        sw.WriteLine(sceneNameWrite);
        sw.WriteLine(participantIDWrite);
        sw.WriteLine(header);
      }

      int randNum = UnityEngine.Random.Range(0,360);
      game_object.transform.eulerAngles = new Vector3(0,randNum,0);

      // omniRing_transform.transform.eulerAngles = new Vector3(0,randNum,0);
      // omniYaw1 = Reference_Frame.transform.rotation.eulerAngles.y;
      Debug.Log("The random number that was chosen for the starting global rotation is: " + randNum);
      //debug purposes only
      //string debugfullFileName = System.IO.Path.Combine(pathString,debugfileName);
      //string debugheader = "angle1,angle2\n";
      //using (System.IO.StreamWriter sw = System.IO.File.CreateText(debugfileName))
      //{
      //  sw.WriteLine(debugheader);
      //}
      // omniRing_transform.transform.eulerAngles.y = randNum;

      // newOmniDirection = Reference_Frame.transform.rotation.eulerAngles.y;
    }

    private void Stop () {
    CancelInvoke("RecordPosition");
    }


    private string FormatString(Vector3 position,float BodyRotation, float HeadRotation)
    {
        return System.String.Format("{0,3:f2};{1,3:f2};{2,3:f2};{3,3:f2};{4,3:f2}\r\n", position.x, position.y, position.z, BodyRotation, HeadRotation);
    }


    private void RecordPosition () {

        // get formatted string
        // GET CHILD: forwardIndicator
        // GameObject forwardInd = FIND(forwardIndicator)
        // forwardInd.transform.rotation.eulerAngles.y

        //Trying something new because Unity will not read the rotation from gloabl transform.

        //This will be the forward vector from the VR camera pointing 10 meters in front of the character controller
        // Vector3 forward = cameraReference.transform.TransformDirection(Vector3.forward)*1000;
        // Vector3 origin_forward = origin.transform.TransformDirection(Vector3.forward)*1000;
        // //Ray test_ray = new Ray(origin.transform.)
        // Debug.DrawRay(game_object.transform.position, forward, Color.green);
        // Debug.DrawRay(origin.transform.position, origin_forward, Color.blue);

        //Setting up new Omni rotation to be equal to amount of rotation that OmniYaw does


        //Try somethign new, from Zach:

        // if (omniYaw1 != OmniMovementComponent.instance.currentOmniYaw){
        //   omniYaw1 = omniYaw1 + (OmniMovementComponent.instance.currentOmniYaw - omniYaw1);
        // }
        // //To ensure the angles are between 0 and 360 degrees
        // if (omniYaw1 > 360f) omniYaw1 -= 360f;
        // if (omniYaw1 < 0f) omniYaw1 += 360f;
        // omniYaw2 = OmniMovementComponent.instance.currentOmniYaw;
        // if (omniYaw2 > 360f) omniYaw2 -= 360f;
        // if (omniYaw2 < 0f) omniYaw2 += 360f;
        // newAdjustedOmniYaw = newOmniDirection + (omniYaw2 - omniYaw1);
        ringAngleInGameWorld = PlayerPrefs.GetFloat("ringAngleInGameWorld");
        string positionString = FormatString(game_object.transform.position, ringAngleInGameWorld, Reference_Frame.transform.rotation.eulerAngles.y);
        // Debug.Log("head angle: " + Reference_Frame.transform.rotation.eulerAngles.y);

        // Debug.Log("This is the angle of rotation for the Camera Reference globally: " + Reference_Frame.transform.rotation.eulerAngles.y);

        // Debug.Log("This is the angle of rotation from the Omni Ring to camera");
        // Debug.Log(OmniMovementComponent.instance.angleBetweenControllerAndCamera);

        // Debug.Log("this is the first log");
        // // Debug.Log(transform.rotation.eulerAngles.y);
        // Debug.Log(cameraReference.transform.rotation.eulerAngles.y);
        // Debug.Log("this is the formatted string");
        // Debug.Log(FormatString(game_object.transform.position, Reference_Frame.transform.rotation.eulerAngles.y));

        // send pointing angle to the text file
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string pathString = System.IO.Path.Combine(path, "SilctonVRData");
        System.IO.Directory.CreateDirectory(pathString);
        string fullFileName = System.IO.Path.Combine(pathString,fileName);

        using (System.IO.StreamWriter sw = System.IO.File.AppendText(fullFileName))
        {
          sw.WriteLine(positionString);
        }

        // void Update(){
        //   RecordPosition();
        // }
        //string debugfullFileName = System.IO.Path.Combine(pathString,debugfileName);
        //using (System.IO.StreamWriter sw = System.IO.File.AppendText(debugfullFileName))
        //{
        //  sw.WriteLine(System.String.Format("{0,3:f3};{1,3:f4}\r\n", transform.rotation.eulerAngles.y, game_object.transform.rotation.y));
        //}

        }




}
