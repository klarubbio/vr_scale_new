﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum HideTargetOnStart
{
    Off,
    SetInactive,
    SetInvisible,
    SetProbeTrial
}

public class NavigationTask : ExperimentTask
{
    [Header("Task-specific Properties")]
    public ObjectList destinations;
	private GameObject current;

	private int score = 0;
	public int scoreIncrement = 50;
	public int penaltyRate = 2000;

    public static bool decreaseRepeat;

    private float penaltyTimer = 0;

	public bool showScoring;
	public TextAsset NavigationInstruction;

    public HideTargetOnStart hideTargetOnStart;
    [Range(0,60)] public float showTargetAfterSeconds;
    public bool hideNonTargets;

    // for compass assist
    public LM_Compass assistCompass;
    public bool usingCompass;
    [Min(-1)]
    public int SecondsUntilAssist = -1;
    public Vector3 compassPosOffset; // where is the compass relative to the active player snappoint
    public Vector3 compassRotOffset; // compass rotation relative to the active player snap point

    // For logging output
    private float startTime;
    private Vector3 playerLastPosition;
    private float playerDistance = 0;
    private Vector3 scaledPlayerLastPosition;
    private float scaledPlayerDistance = 0;
    private float optimalDistance;

    public override void startTask ()
	{
		TASK_START();
		avatarLog.navLog = true;
        if (isScaled) scaledAvatarLog.navLog = true;
    }

	public override void TASK_START()
	{
		if (!manager) Start();
        base.startTask();

        if (skip)
        {
            log.log("INFO    skip task    " + name, 1);
            return;
        }

        decreaseRepeat = false;

        hud.showEverything();
		hud.showScore = showScoring;
		current = destinations.currentObject();
        // Debug.Log ("Find " + destinations.currentObject().name);

        // if it's a target, open the door to show it's active
        if (current.GetComponentInChildren<LM_TargetStore>() != null)
        {
            current.GetComponentInChildren<LM_TargetStore>().OpenDoor();
        }

		if (NavigationInstruction)
		{
			string msg = NavigationInstruction.text;
			if (destinations != null) msg = string.Format(msg, current.name);
			hud.setMessage(msg);
   		}
		else
		{
            hud.SecondsToShow = 0;
            hud.setMessage("Please find the " + current.name);
		}

        // Handle if we're hiding all the non-targets
        if (hideNonTargets)
        {
            foreach (GameObject item in destinations.objects)
            {
                if (item.name != destinations.currentObject().name)
                {
                    item.SetActive(false);
                }
                else item.SetActive(true);
            }
        }


        // Handle if we're hiding the target object
        if (hideTargetOnStart != HideTargetOnStart.Off)
        {
            if (hideTargetOnStart == HideTargetOnStart.SetInactive)
            {
                destinations.currentObject().SetActive(false);
            }
            else if (hideTargetOnStart == HideTargetOnStart.SetInvisible)
            {
                destinations.currentObject().GetComponent<MeshRenderer>().enabled = false;
            }
            else if (hideTargetOnStart == HideTargetOnStart.SetProbeTrial)
            {
                destinations.currentObject().SetActive(false);
                destinations.currentObject().GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else
        {
            destinations.currentObject().SetActive(true); // make sure the target is visible unless the bool to hide was checked
            try
            {
                destinations.currentObject().GetComponent<MeshRenderer>().enabled = true;
            }
            catch (System.Exception ex)
            {

            }
        }

        // startTime = Current time in seconds
        startTime = Time.time;

        // Get the avatar start location (distance = 0)
        playerDistance = 0.0f;
        playerLastPosition = avatar.transform.position;
        if (isScaled)
        {
            scaledPlayerDistance = 0.0f;
            scaledPlayerLastPosition = scaledAvatar.transform.position;
        }

        // Calculate optimal distance to travel (straight line)
        if (isScaled)
        {
            optimalDistance = Vector3.Distance(scaledAvatar.transform.position, current.transform.position);
        }
        else optimalDistance = Vector3.Distance(avatar.transform.position, current.transform.position);


        // Grab our LM_Compass object and move it to the player snapPoint
        if (usingCompass)
        {
            assistCompass.transform.parent = avatar.GetComponentInChildren<LM_SnapPoint>().transform;
            assistCompass.transform.localPosition = compassPosOffset;
            assistCompass.transform.localEulerAngles = compassRotOffset;
            assistCompass.gameObject.SetActive(false);
        }

        //// MJS 2019 - Move HUD to top left corner
        //hud.hudPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
        //hud.hudPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.9f);
    }

    public override bool updateTask ()
	{
		base.updateTask();

        if (skip)
        {
            //log.log("INFO    skip task    " + name,1 );
            return true;
        }

        if (score > 0) penaltyTimer = penaltyTimer + (Time.deltaTime * 1000);


		if (penaltyTimer >= penaltyRate)
		{
			penaltyTimer = penaltyTimer - penaltyRate;
			if (score > 0)
			{
				score = score - 1;
				hud.setScore(score);
			}
		}

        //VR capability with showing target
        if (vrEnabled)
        {
            if (hideTargetOnStart != HideTargetOnStart.Off && hideTargetOnStart != HideTargetOnStart.SetProbeTrial && ((Time.time - startTime > (showTargetAfterSeconds) || vrInput.TouchpadButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.Any))))
            {
                destinations.currentObject().SetActive(true);
            }

            if (hideTargetOnStart == HideTargetOnStart.SetProbeTrial && vrInput.TouchpadButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.Any))
            {
                //get current location and then log it

                destinations.currentObject().SetActive(true);
                destinations.currentObject().GetComponent<MeshRenderer>().enabled = true;
            }
        }

        //show target on button click or after set time
        if (hideTargetOnStart != HideTargetOnStart.Off && hideTargetOnStart != HideTargetOnStart.SetProbeTrial && ((Time.time - startTime > (showTargetAfterSeconds) || Input.GetButtonDown("Return"))))
        {
            destinations.currentObject().SetActive(true);
        }

        if (hideTargetOnStart == HideTargetOnStart.SetProbeTrial && Input.GetButtonDown("Return"))
        {
            //get current location and then log it

            destinations.currentObject().SetActive(true);
            destinations.currentObject().GetComponent<MeshRenderer>().enabled = true;
        }

        // Keep updating the distance traveled
        playerDistance += Vector3.Distance(avatar.transform.position, playerLastPosition);
        playerLastPosition = avatar.transform.position;
        if (isScaled)
        {
            scaledPlayerDistance += Vector3.Distance(scaledAvatar.transform.position, scaledPlayerLastPosition);
            scaledPlayerLastPosition = scaledAvatar.transform.position;
        }

        if (usingCompass)
        {
            // Keep the assist compass pointing at the target (even if it isn't visible)
            var targetDirection = 2 * assistCompass.transform.position - destinations.currentObject().transform.position;
            targetDirection = new Vector3(targetDirection.x, assistCompass.pointer.transform.position.y, targetDirection.z);
            assistCompass.pointer.transform.LookAt(targetDirection, Vector3.up);
            // Show assist compass if and when it is needed
            if (assistCompass.gameObject.activeSelf == false & SecondsUntilAssist >= 0 & (Time.time - startTime > SecondsUntilAssist))
            {
                assistCompass.gameObject.SetActive(true);
            }

        }

		if (killCurrent == true)
		{
			return KillCurrent ();
		}

		return false;
	}

	public override void endTask()
	{
		TASK_END();
		//avatarController.handleInput = false;
	}

	public override void TASK_PAUSE()
	{
		avatarLog.navLog = false;
        if (isScaled) scaledAvatarLog.navLog = false;
		//base.endTask();
		log.log("TASK_PAUSE\t" + name + "\t" + this.GetType().Name + "\t" ,1 );
		//avatarController.stop();

		hud.setMessage("");
		hud.showScore = false;

	}

	public override void TASK_END()
	{
		base.endTask();
		//avatarController.stop();
		avatarLog.navLog = false;
        if (isScaled) scaledAvatarLog.navLog = false;

        // close the door if the target was a store and it is open
        // if it's a target, open the door to show it's active
        if (current.GetComponentInChildren<LM_TargetStore>() != null)
        {
            current.GetComponentInChildren<LM_TargetStore>().CloseDoor();
        }

        if (canIncrementLists)
		{
			destinations.incrementCurrent();
		}
		current = destinations.currentObject();
		hud.setMessage("");
		hud.showScore = false;

        hud.SecondsToShow = hud.GeneralDuration;

        if (usingCompass)
        {
            // Hide the assist compass
            assistCompass.gameObject.SetActive(false);
        }
        // Move hud back to center and reset
        hud.hudPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        hud.hudPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

        float perfDistance;
        if (isScaled)
        {
            perfDistance = scaledPlayerDistance;
        }
        else perfDistance = playerDistance;


        var parent = this.parentTask;
        var masterTask = parent;
        while (!masterTask.gameObject.CompareTag("Task")) masterTask = masterTask.parentTask;
        // This will log all final trial info in tab delimited format
        var excessPath = perfDistance - optimalDistance;

        var navTime = Time.time - startTime;

        // set impossible values if the nav task was skipped
        if (skip)
        {
            navTime = float.NaN;
            perfDistance = float.NaN;
            optimalDistance = float.NaN;
            excessPath = float.NaN;
        }
        

        log.log("LM_OUTPUT\tNavigationTask.cs\t" + masterTask + "\t" + this.name + "\n" +
        	"Task\tBlock\tTrial\tTargetName\tOptimalPath\tActualPath\tExcessPath\tRouteDuration\n" +
        	masterTask.name + "\t" + masterTask.repeatCount + "\t" + parent.repeatCount + "\t" + destinations.currentObject().name + "\t" + optimalDistance + "\t"+ perfDistance + "\t" + excessPath + "\t" + navTime
            , 1);


        // More concise LM_TrialLog logging
        if (trialLog.active)
        {
            trialLog.AddData(transform.name + "_target", destinations.currentObject().name);
            trialLog.AddData(transform.name + "_actualPath", perfDistance.ToString());
            trialLog.AddData(transform.name + "_optimalPath", optimalDistance.ToString());
            trialLog.AddData(transform.name + "_excessPath", excessPath.ToString());
            trialLog.AddData(transform.name + "_duration", navTime.ToString());
        }
    }

	public override bool OnControllerColliderHit(GameObject hit)
	{
		if (hit == current)
		{
			if (showScoring)
			{
				score = score + scoreIncrement;
				hud.setScore(score);
			}
			return true;
		}

		//		Debug.Log (hit.transform.parent.name + " = " + current.name);
		if (hit.transform.parent == current.transform)
		{
			if (showScoring)
			{
				score = score + scoreIncrement;
				hud.setScore(score);
			}
			return true;
		}
		return false;
	}
}
