using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
/*ASM 2021
 * Specific NavTask for Navigation Learning Paradigm
 * Based on start-target pairings, calculates the optimal path distance for participant
 * Checks if participant performs better than (N x optimal value) with N being researcher specified
 * If participant performs better than padded optimum, gives specific found message & increments correctness counter
 * If participant performs worse than padded optimum, gives specific found message & does not increment correctness counter
 *
 *
 *
 */
public enum HideTarget //had to change for whatever reason
{
    Off,
    SetInactive,
    SetInvisible,
    SetProbeTrial
}

public enum CompassType
{
    Off,
    Distance,
    Time
}

public enum TargetIndicator
{
    Off,
    On,
    Distance
}
public class NavLearningTask : ExperimentTask
{
    [Header("NavLearning Properties")]
    public CompassType compassType;
    public float correctnessThresholdMultiplier;
    public float compassThresholdMultiplier;
    public float compassTimerSeconds;
    public int trialsToMoveOn;
    public int itucson; //incorrectTrialsUntilCompassStartsON;
    public bool usingAI;
    public bool visualizeOptimalPath;
    public GameObject prefab;
    private List<GameObject> pathComponents;
    private float correctnessThresholdDistance,compassThresholdDistance;
    public static int correctTrials, incorrectTrials;
    private bool compassOn;
    private float pathLength;
    private int y = 0;
    public bool showTargetInHud;
    public TargetIndicator targetIndicator;
    public float targetIndicatorDistance = 0;


    [Header("Task-specific Properties")]

    public ObjectList destinations;
	private GameObject current;

	private int score = 0;
	public int scoreIncrement = 50;
	public int penaltyRate = 2000;

    private float penaltyTimer = 0;

	public bool showScoring;
	public TextAsset NavigationInstruction;

    public HideTarget hideTargetOnStart;
    [Range(0,60)] public float showTargetAfterSeconds;
    public bool hideNonTargets;

    private Vector3[] efficientPath;

    //// for LM compass assist
    //public LM_Compass assistCompass;
    //public bool usingCompass;
    //[Min(-1)]
    //public int SecondsUntilAssist = -1;
    //public Vector3 compassPosOffset; // where is the compass relative to the active player snappoint
    //public Vector3 compassRotOffset; // compass rotation relative to the active player snap point

    // For logging output
    private float startTime;
    private Vector3 playerLastPosition;
    private float playerDistance = 0;
    private Vector3 scaledPlayerLastPosition;
    private float scaledPlayerDistance = 0;
    private float optimalEuclideanDistance;

    private string pathContainer, simpleContainer, efficientContainer;

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

        //Debugging Methods
        //VisualizeAllPaths();
        //VisualizeSinglePath(0,1);

        //if we want to visualize the path
        if (visualizeOptimalPath)
        {
            //initialize our private list
            pathComponents = new List<GameObject>();
        }

        hud.showEverything();
		hud.showScore = showScoring;
		current = destinations.objects[NavLearningSetup.currentTarget];
        // Debug.Log ("Find " + destinations.currentObject().name);

        // if it's a target, open the door to show it's active
        if (current.GetComponentInChildren<LM_TargetStore>() != null)
        {
            current.GetComponentInChildren<LM_TargetStore>().OpenDoor();
        }

        if (NavLearningSetup.generatingOutput)
        {
            pathContainer = "";
            simpleContainer = "";
            efficientContainer = "";
        }

        if (GameObject.FindGameObjectWithTag("Compass") != null)
        {
            GameObject.FindGameObjectWithTag("Compass").GetComponent<MeshRenderer>().enabled = false;
            NavLearningCompass.target = current;
            compassOn = false;
        }

        if (usingAI) //if we are using the nav AI
        {
            NavMeshAgent agent = GameObject.FindGameObjectWithTag("Player").GetComponent<NavMeshAgent>(); //get the agent
            agent.destination = current.transform.position; //set the agent's destination
        }

        //create new empty path
        NavMeshPath path = new NavMeshPath();
        //calculate the path from the player to the snap point (front of building)
        NavMesh.CalculatePath(destinations.objects[NavLearningSetup.currentStart].transform.Find("SnapPoint").position, current.transform.Find("SnapPoint").position, NavMesh.AllAreas, path); //calculate path from agent to c
        int x = 0;
        efficientPath = new Vector3[path.corners.Length];
        while(x < path.corners.Length){
          Debug.Log("Path: " + path.corners[x]);
          efficientPath[x] = path.corners[x];
          x++;
        }



         //initialize to 0
        pathLength = 0.0f;
        //calculate path
        for (int i = 1; i < path.corners.Length; ++i)
        {
            pathLength += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            //if we want to visualize the path
            if (visualizeOptimalPath)
            {
                //instantiate prefab spheres at the locations and store it in the private path components list
                pathComponents.Add(Instantiate(prefab, path.corners[i - 1], Quaternion.identity));
            }
        }
        //if we want to visualize the path
        if (visualizeOptimalPath)
        {
            //instantiate the last sphere that is skipped by the previous logic
            pathComponents.Add(Instantiate(prefab, path.corners[path.corners.Length - 1], Quaternion.identity));
            //render lines between each path sphere
            for (int j = 1; j < pathComponents.Count; j++)
            {
                LineRenderer line = pathComponents[j - 1].AddComponent<LineRenderer>();
                line.startWidth = 0.1f;
                line.endWidth = line.startWidth;
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.SetPosition(0, pathComponents[j - 1].transform.position);
                line.SetPosition(1, pathComponents[j].transform.position);
            }
        }

        //if our pathLength is invalid due to the nav mesh failing
        if (pathLength <= 0)
        {
            //get the euc distance x 1.25 and set that to the path length
            pathLength = 1.25f * Vector3.Distance(destinations.objects[NavLearningSetup.currentStart].transform.Find("SnapPoint").position, current.transform.Find("SnapPoint").position);
        }
        //if our path is valid, but manages to fail after the start
        else if (Vector3.Distance(path.corners[path.corners.Length-1], current.transform.Find("SnapPoint").position) > 0) 
        {
            //add 1.2x euc distance from end of path to target
            pathLength += 1.20f * Vector3.Distance(path.corners[path.corners.Length - 1], current.transform.Find("SnapPoint").position);
        }

        //add create threshold values
        correctnessThresholdDistance = pathLength * correctnessThresholdMultiplier;
        compassThresholdDistance = pathLength * compassThresholdMultiplier;


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

        //functionality to enable/disable visual target indicators
        foreach (GameObject item in destinations.objects)
        {
            if (item.name != current.name)
            {
                item.transform.Find("SnapPoint").GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                if (targetIndicator == TargetIndicator.On)
                {
                    item.transform.Find("SnapPoint").GetComponent<MeshRenderer>().enabled = true;
                }
                else
                {
                    item.transform.Find("SnapPoint").GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }


        // Handle if we're hiding the target object
        if (hideTargetOnStart != HideTarget.Off)
        {
            if (hideTargetOnStart == HideTarget.SetInactive)
            {
                destinations.currentObject().SetActive(false);
            }
            else if (hideTargetOnStart == HideTarget.SetInvisible)
            {
                destinations.currentObject().GetComponent<MeshRenderer>().enabled = false;
            }
            else if (hideTargetOnStart == HideTarget.SetProbeTrial)
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
            optimalEuclideanDistance = Vector3.Distance(scaledAvatar.transform.position, current.transform.position);
        }
        else optimalEuclideanDistance = Vector3.Distance(avatar.transform.position, current.transform.position);


        if (showTargetInHud)
        {
            hud.SecondsToShow = 3600; //hud visible for 1 hour (overkill but just in case)
            hud.setMessage(current.name); //set UI message reminder for target
            GameObject.Find("[HudPanel]").GetComponent<RectTransform>().localPosition = new Vector3(0, -Screen.height / 2 + 75, 0); //set hud to 75px from bottom of center screen
        }

        //// Grab our LM_Compass object and move it to the player snapPoint
        //if (usingCompass)
        //{
        //    assistCompass.transform.parent = avatar.GetComponentInChildren<LM_SnapPoint>().transform;
        //    assistCompass.transform.localPosition = compassPosOffset;
        //    assistCompass.transform.localEulerAngles = compassRotOffset;
        //    assistCompass.gameObject.SetActive(false);
        //}



        //// MJS 2019 - Move HUD to top left corner
        //hud.hudPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
        //hud.hudPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.9f);

        //VisualizeAllPaths();
        
    }

    public override bool updateTask ()
	{
		base.updateTask();

        if (skip)
        {
            //log.log("INFO    skip task    " + name,1 );
            return true;
        }

        if (NavLearningSetup.generatingOutput)
        {
            //Environment \tTrial \tStart \tStartLocationX \tStartLocationZ \tTarget  \tTargetLocationX \tTargetLocationZ \tPlayerX \tPlayerZ \tPlayerRotation");
            pathContainer += NavLearningSetup.currentEnvironment + "\t" + NavLearningSetup.currentTrial + "\t" + Time.time*1000 + "\t" + destinations.objects[NavLearningSetup.currentStart].name + "\t" + destinations.objects[NavLearningSetup.currentStart].transform.Find("SnapPoint").position.x
             + "\t" + destinations.objects[NavLearningSetup.currentStart].transform.Find("SnapPoint").position.z + "\t" + current.name + "\t" + current.transform.Find("SnapPoint").position.x + "\t" + current.transform.Find("SnapPoint").position.z
              + "\t" + avatar.transform.position.x + "\t" + avatar.transform.position.z + "\t" + avatar.transform.localEulerAngles.y + "\n";
        }

        //check if compass is off and if the player has either walked far enough or for enough time
        if (!compassOn && ((compassType == CompassType.Time && Time.time - startTime > compassTimerSeconds) || (compassType == CompassType.Distance && playerDistance > compassThresholdDistance) || incorrectTrials > itucson))
        {
            //enable compass if so
            if (GameObject.FindGameObjectWithTag("Compass") != null)
            {
                GameObject.FindGameObjectWithTag("Compass").GetComponent<MeshRenderer>().enabled = true;
                compassOn = true;
            }
        }


        if (targetIndicator == TargetIndicator.Distance && Vector3.Distance(avatar.transform.position, current.transform.Find("SnapPoint").transform.position) <= targetIndicatorDistance)
        {
            current.transform.Find("SnapPoint").GetComponent<MeshRenderer>().enabled = true;
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
            if (hideTargetOnStart != HideTarget.Off && hideTargetOnStart != HideTarget.SetProbeTrial && ((Time.time - startTime > (showTargetAfterSeconds) || vrInput.TouchpadButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.Any))))
            {
                destinations.currentObject().SetActive(true);
            }

            if (hideTargetOnStart == HideTarget.SetProbeTrial && vrInput.TouchpadButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.Any))
            {
                //get current location and then log it

                destinations.currentObject().SetActive(true);
                destinations.currentObject().GetComponent<MeshRenderer>().enabled = true;
            }

        }

        //show target on button click or after set time
        if (hideTargetOnStart != HideTarget.Off && hideTargetOnStart != HideTarget.SetProbeTrial && ((Time.time - startTime > (showTargetAfterSeconds) || Input.GetButtonDown("Return"))))
        {
            destinations.currentObject().SetActive(true);
        }

        if (hideTargetOnStart == HideTarget.SetProbeTrial && Input.GetButtonDown("Return"))
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

        //if (usingCompass)
        //{
        //    // Keep the assist compass pointing at the target (even if it isn't visible)
        //    var targetDirection = 2 * assistCompass.transform.position - destinations.currentObject().transform.position;
        //    targetDirection = new Vector3(targetDirection.x, assistCompass.pointer.transform.position.y, targetDirection.z);
        //    assistCompass.pointer.transform.LookAt(targetDirection, Vector3.up);
        //    // Show assist compass if and when it is needed
        //    if (assistCompass.gameObject.activeSelf == false & SecondsUntilAssist >= 0 & (Time.time - startTime > SecondsUntilAssist))
        //    {
        //        assistCompass.gameObject.SetActive(true);
        //    }

        //}

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

        if (showTargetInHud)
        {
            GameObject.Find("[HudPanel]").GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0); //set hud to prev values (0,0,0)
        }

        //if we visualized the path
        if (visualizeOptimalPath)
        {
            //destroy all the path spheres
            for (int k = 0; k < pathComponents.Count; k++)
            {
                Destroy(pathComponents[k]);
            }
        }

        if (GameObject.FindGameObjectWithTag("Compass") != null)
        {
            GameObject.FindGameObjectWithTag("Compass").GetComponent<MeshRenderer>().enabled = false;
        }

        if (NavLearningSetup.generatingOutput)
        {
            //"Environment \tTrial \tStart \tStartLocationX \tStartLocationZ \tTarget  \tTargetLocationX \tTargetLocationZ \tOptimalDistance \tDistanceThreshold \tWalkedDistance \tDistanceError \tTotalTime");
            simpleContainer = NavLearningSetup.currentEnvironment + "\t" + NavLearningSetup.currentTrial + "\t" + destinations.objects[NavLearningSetup.currentStart].name + "\t" + destinations.objects[NavLearningSetup.currentStart].transform.Find("SnapPoint").position.x + "\t" + destinations.objects[NavLearningSetup.currentStart].transform.Find("SnapPoint").position.z + "\t" +
               current.name + "\t" + current.transform.Find("SnapPoint").position.x + "\t" + current.transform.Find("SnapPoint").position.z + "\t" + pathLength + "\t" + correctnessThresholdDistance + "\t" + playerDistance + "\t" + (playerDistance-pathLength) + "\t" + ((Time.time*1000) - (startTime*1000)) + "\n";
            //Add in efficient path for file to calculate the most efficient pathway
            while(y < efficientPath.Length){
              efficientContainer += NavLearningSetup.currentEnvironment + "\t" + NavLearningSetup.currentTrial + "\t" + Time.time*1000 + "\t" + destinations.objects[NavLearningSetup.currentStart].name + "\t" + destinations.objects[NavLearningSetup.currentStart].transform.Find("SnapPoint").position.x
                + "\t" + destinations.objects[NavLearningSetup.currentStart].transform.Find("SnapPoint").position.z + "\t" + current.name + "\t" + current.transform.Find("SnapPoint").position.x + "\t" + current.transform.Find("SnapPoint").position.z
                 + "\t" + efficientPath[y].x + "\t" + efficientPath[y].z + "\n";
              y++;
            }

            System.IO.File.AppendAllText(NavLearningSetup.outputPath, simpleContainer);
            System.IO.File.AppendAllText(NavLearningSetup.pathOutputPath,pathContainer);
            System.IO.File.AppendAllText(NavLearningSetup.efficientOutputPath,efficientContainer);
            pathContainer = "";
            simpleContainer = "";
            efficientContainer = "";
            y = 0;
        }

        //Nav Learning Functionality
        NavLearningSetup.currentTrial++;
        //create temp storage variable
        int tempStorage = NavLearningSetup.currentStart;
        //flip the start and target values
        NavLearningSetup.currentStart = NavLearningSetup.currentTarget;
        NavLearningSetup.currentTarget = tempStorage;
        //check correctness
        if (playerDistance > correctnessThresholdDistance)
        {
            //if not correct
            NavLearningFoundInstructions.conditionalMessage = "Good, now try to be faster while following the shortest pathway";
            incorrectTrials += 1;//increment incorrect trials
        }
        else
        {
            correctTrials += 1;
            incorrectTrials = 0;//set incorrect trials to 0
            if (correctTrials < trialsToMoveOn)
            {
                NavLearningFoundInstructions.conditionalMessage = "Great, you found the shortest pathway!\n Please do that again.";
            }
            else
            {
                correctTrials = 0;
                NavLearningFoundInstructions.conditionalMessage = "Excellent, you successfully navigated between the " + destinations.objects[NavLearningSetup.currentTarget].name + " and the " + destinations.objects[NavLearningSetup.currentStart].name;
                //increment the line we are on
                if (!NavLearningSetup.done && NavLearningSetup.currentLine + 1 < NavLearningSetup.lineCount)
                {
                    NavLearningSetup.currentLine++;
                }
                else
                {
                    //set done to true
                    NavLearningSetup.done = true;
                    //make trials to move on 1
                    trialsToMoveOn = 1;
                    //set current line to a random line
                    NavLearningSetup.currentLine = Random.Range(0, NavLearningSetup.lineCount);
                }
                //change the start and target to be the appropriate ones
                NavLearningSetup.currentStart = int.Parse((NavLearningSetup.environmentArray[NavLearningSetup.currentLine][0].ToString() + NavLearningSetup.environmentArray[NavLearningSetup.currentLine][1].ToString()));
                NavLearningSetup.currentTarget = int.Parse((NavLearningSetup.environmentArray[NavLearningSetup.currentLine][2].ToString() + NavLearningSetup.environmentArray[NavLearningSetup.currentLine][3].ToString()));
            }
        }

        //Update participant file if we aren't done with the experiment

        //Get path
        string path = NavLearningSetup.accessibleParticipantPath;
            string content = "";

            //write the updated information to the participant file (in the event of a crash or early termination this will be useful)
            if (NavLearningSetup.done)
            {
                //if we are done with the experiment, set up the participant file to move on to the next environment
               content = (NavLearningSetup.currentEnvironment + 1).ToString() + "\t" + "0" + "\t" + "1";
            }
            else
            {
                content = NavLearningSetup.currentEnvironment.ToString() + "\t" + NavLearningSetup.currentLine.ToString() + "\t" + NavLearningSetup.currentTrial.ToString();
            }
            //(over)write the file
            System.IO.File.WriteAllText(path, content);


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

        //if (usingCompass)
        //{
        //    // Hide the assist compass
        //    assistCompass.gameObject.SetActive(false);
        //}
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
        var excessPath = perfDistance - optimalEuclideanDistance;

        var navTime = Time.time - startTime;

        // set impossible values if the nav task was skipped
        if (skip)
        {
            navTime = float.NaN;
            perfDistance = float.NaN;
            optimalEuclideanDistance = float.NaN;
            excessPath = float.NaN;
        }


        log.log("LM_OUTPUT\tNavigationTask.cs\t" + masterTask + "\t" + this.name + "\n" +
        	"Task\tBlock\tTrial\tTargetName\tOptimalPath\tActualPath\tExcessPath\tRouteDuration\n" +
        	masterTask.name + "\t" + masterTask.repeatCount + "\t" + parent.repeatCount + "\t" + destinations.currentObject().name + "\t" + optimalEuclideanDistance + "\t"+ perfDistance + "\t" + excessPath + "\t" + navTime
            , 1);


        // More concise LM_TrialLog logging
        if (trialLog.active)
        {
            trialLog.AddData(transform.name + "_target", destinations.currentObject().name);
            trialLog.AddData(transform.name + "_actualPath", perfDistance.ToString());
            trialLog.AddData(transform.name + "_optimalPath", optimalEuclideanDistance.ToString());
            trialLog.AddData(transform.name + "_excessPath", excessPath.ToString());
            trialLog.AddData(transform.name + "_duration", navTime.ToString());
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Silcton") && NavLearningSetup.done)
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            else
            {
                Application.Quit();
            }
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

    private void VisualizeSinglePath(int firstIndex, int secondIndex) 
    {
        //initialize our private list
        List<GameObject> pathComponents = new List<GameObject>();
        float singlePathLength = 0.0f;
        //create new empty path
        NavMeshPath path = new NavMeshPath();
        //calculate the path from the player to the snap point (front of building)
        NavMesh.CalculatePath(destinations.objects[firstIndex].transform.Find("SnapPoint").position, destinations.objects[secondIndex].transform.Find("SnapPoint").position, NavMesh.AllAreas, path); //calculate path from agent to c
        for (int i = 1; i < path.corners.Length; ++i)
        {
            pathLength += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            //instantiate prefab spheres at the locations and store it in the private path components list
            pathComponents.Add(Instantiate(prefab, path.corners[i - 1], Quaternion.identity));
        }
        Debug.LogError("Distance between " + destinations.objects[firstIndex].name + " and " + destinations.objects[secondIndex].name + " is " + pathLength);
        //instantiate the last sphere that is skipped by the previous logic
        pathComponents.Add(Instantiate(prefab, path.corners[path.corners.Length - 1], Quaternion.identity));
        //render lines between each path sphere
        for (int j = 1; j < pathComponents.Count; j++)
        {
            LineRenderer line = pathComponents[j - 1].AddComponent<LineRenderer>();
            line.startWidth = 0.1f;
            line.endWidth = line.startWidth;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.SetPosition(0, pathComponents[j - 1].transform.position);
            line.SetPosition(1, pathComponents[j].transform.position);
        }


    }

    /*
     Visualize all paths visualizes all of the nav mesh paths between all objects underneath TargetObjects
     */
    private void VisualizeAllPaths()
    {
        //get the parent object we want
        GameObject buildings = GameObject.Find("TargetObjects");
        //Debug.LogError("to count " + buildings.transform.childCount);
        //iterate through first loop
        for (int i = 0; i < buildings.transform.childCount; i++)
        {
            //iterate through second loop
            for (int j = 0; j < buildings.transform.childCount; j++)
            {
                //Debug.LogError("i= " + i + " j= " + j);
                //only do stuff if it wont cause an error (cant have an optimal path to and from the same building)
                if (j != i)
                {
                    //create empty paths components list
                    List<GameObject> pathsComponents = new List<GameObject>();
                    //create empty path
                    NavMeshPath paths = new NavMeshPath();
                    //calculate the path from I to J
                    NavMesh.CalculatePath(buildings.transform.GetChild(i).gameObject.transform.Find("SnapPoint").position, buildings.transform.GetChild(j).gameObject.transform.Find("SnapPoint").position, NavMesh.AllAreas, paths);
                    //as long as the optimal path isn't a straight line
                    if (paths.corners.Length > 1)
                    {
                        //iterate through all parts of the path
                        for (int k = 1; k < paths.corners.Length; ++k)
                        {
                            //Debug.LogError("k= " + k + " " + paths.corners.Length);
                            //add path component
                            pathsComponents.Add(Instantiate(prefab, paths.corners[k - 1], Quaternion.identity));
                        }
                        //add last path component
                        pathsComponents.Add(Instantiate(prefab, paths.corners[paths.corners.Length - 1], Quaternion.identity));
                        //iterate through all the components and draw lines between them
                        for (int l = 1; l < pathsComponents.Count; l++)
                        {
                            //Debug.LogError("l= " + l + " " + pathsComponents.Count);
                            LineRenderer line = pathsComponents[l - 1].AddComponent<LineRenderer>();
                            line.startWidth = 0.1f;
                            line.endWidth = line.startWidth;
                            line.positionCount = 2;
                            line.useWorldSpace = true;
                            line.SetPosition(0, pathsComponents[l - 1].transform.position);
                            line.SetPosition(1, pathsComponents[l].transform.position);
                        }
                    }
                }
            }

        }
    }
}
