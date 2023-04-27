using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
/*
    NavLearningSetup handles the reading of participant and environment files
    for the purpose of knowing which targets to present and in which order.
    All relevant values are stored within static variables to allow for easy access w/out
    overwriting the files themselves (until the end where we update the participant file).
        Participant files are arranged as follows:
            Participant#.txt
                Environment#\t    Line#\t    Trial#\t
        Environment files are arranged as follows:
            Environment#.txt
                0 1\n
                0 2\n
                ...\n
                X Y\n
     */

public class NavLearningSetup : MonoBehaviour
{
    public string participantNumber;
    private string participantFile, environmentFile;
    [Range(0, 59)]
    public int timeLimitHours = 2, timeLimitMinutes = 0, timeLimitSeconds = 0;
    public bool outputting;
    public static int currentEnvironment, currentLine, currentTrial, lineCount, currentStart, currentTarget;
    public static string participant, outputPath, pathOutputPath, efficientOutputPath, accessibleParticipantPath;
    public static string[] participantArray, environmentArray;
    public static List<int> environmentList;
    public static bool done, generatingOutput;
    private int totalAllottedSeconds;
    private bool doublecheck;
    private string participantFilePath = "Assets/Landmarks/Scenes/NavLearningData/ParticipantFiles/";
    private string environmentFilePath = "Assets/Landmarks/Scenes/NavLearningData/EnvironmentFiles/";
    private string dataFilePath = "C:/NavLearningData/Data/";
    // Start is called before the first frame update
    void Start()
    {
        participantNumber = experimentFlow.participant;
        //Display Dialog to have researcher double-check that we are running the correct participant
        doublecheck = EditorUtility.DisplayDialog("Please verify that participant # is correct","Is this participant: " + participantNumber +"?", "Yes", "No");
        //if for whatever reason it isn't correct, quit before we do anything
        if (!doublecheck)
        {
            Quit();
        }
        else
        {
            if (SceneManager.GetActiveScene().name.Contains("Silcton"))
            {
                //participantFilePath += "/Silcton/";
                environmentFilePath += "/Silcton/";
                //dataFilePath += "/Silcton/";
            }
            //if it is correct then get the appropriate file info
            participantFile = participantFilePath + "p_" + participantNumber.ToString() + ".txt";
            //if this is a new participant
            if (!File.Exists(participantFile))
            {
                if (participantNumber.ToString().Contains("_T2") || participantNumber.ToString().Contains("_t2"))
                {
                    File.WriteAllText(participantFile, "2\t0\t1");
                    participantArray = new string[3] { "2", "0", "1" };
                }
                else
                {
                    //create new blank participant file
                    File.WriteAllText(participantFile, "1\t0\t1");
                    participantArray = new string[3] { "1", "0", "1" };
                }
            }
            else
            {
                //create an accessible array from the participant file using streamreader
                using (StreamReader pf = new StreamReader(participantFile))
                {
                    participantArray = Regex.Split(pf.ReadLine(),"\t");
                }
            }
            
        }
        //initialize done to false
        done = false;

        //generate output
        generatingOutput = outputting;

        //Get a total runtime in seconds we want
        totalAllottedSeconds = (timeLimitHours * 60 * 60) + (timeLimitMinutes * 60) + timeLimitSeconds;

        //set current values
        currentEnvironment = int.Parse(participantArray[0].ToString());

        //set the appropriate environment file path
        environmentFile = environmentFilePath + "Environment" + currentEnvironment.ToString() + ".txt";

        if (!File.Exists(environmentFile))
        {
            //create new blank participant file
            Debug.LogError("We do not have the appropriate environment file!");
            Quit();
        }

        //get current line
        currentLine = int.Parse(participantArray[1].ToString());

        //get current trial
        currentTrial = int.Parse(participantArray[2].ToString());

        //get number of lines in the file
        lineCount = File.ReadLines(environmentFile).Count();

        //create an accessible array from the environment file using File.ReadAllLines
        environmentArray = File.ReadAllLines(environmentFile);
        //remove tabs from the array
        for (int i = 0; i < lineCount; i++)
        {
            environmentArray[i] = Regex.Replace(environmentArray[i], "\t", "");
            Debug.LogError(environmentArray[i]);
        }

        //initialize accessible list
        environmentList = new List<int>();

        //add all target numbers to list
        for (int i = 0; i < lineCount; i++)//this is where we should add
        {
            
            int first = int.Parse((environmentArray[i][0].ToString() + environmentArray[i][1].ToString()).ToString());
            int second = int.Parse((environmentArray[i][2].ToString() + environmentArray[i][3].ToString()).ToString());

            environmentList.Add(first);
            environmentList.Add(second);
        }

        //remove duplicates
        environmentList = environmentList.Distinct().ToList();

        /* Iterate through list
        for (int i = 0; i < environmentList.Count; i++)
        {
            Debug.LogError(environmentList[i]);
        }
        */

        //get current start and current target based on environment array
        currentStart = int.Parse(environmentArray[currentLine][0].ToString() + environmentArray[currentLine][1].ToString());
        currentTarget = int.Parse(environmentArray[currentLine][2].ToString() + environmentArray[currentLine][3].ToString());

        Debug.Log("Initialized start#: " + currentStart);
        Debug.Log("Initialized target#: " + currentTarget);

        participant = participantNumber.ToString();

        bool checkOutput = outputting;

        if (!outputting)
        {
            checkOutput = EditorUtility.DisplayDialog("We are not outputting data!", "Do we want to be outputting data?", "Yes", "No");
            generatingOutput = checkOutput;
        }
        
        if (outputting || checkOutput)
        {
            //check if the file path exists for the dataFilePath
            if(!Directory.Exists(dataFilePath))
            {
                //if it doesn't then we create it
                Directory.CreateDirectory(dataFilePath);
            }
            string currentDT = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            outputPath = dataFilePath + participant + "_" + currentDT +".xls";
            File.WriteAllText(outputPath, participant + currentDT);
            File.AppendAllText(outputPath, "\nEnvironment\tTrial\tStart\tStartLocationX\tStartLocationZ\tTarget\tTargetLocationX\tTargetLocationZ\tOptimalDistance\tDistanceThreshold\tWalkedDistance\tDistanceError\tTotalTime\n");

            pathOutputPath = dataFilePath + participant + "_" + currentDT + "_PATHFILE.xls";
            File.WriteAllText(pathOutputPath, participant + currentDT + "_PATHFILE");
            File.AppendAllText(pathOutputPath, "\nEnvironment\tTrial\tTime(ms)\tStart\tStartLocationX\tStartLocationZ\tTarget\tTargetLocationX\tTargetLocationZ\tPlayerX\tPlayerZ\tPlayerRotation\n");

            efficientOutputPath = dataFilePath + participant + "_" + currentDT + "_efficientPATHFILE.xls";
            File.WriteAllText(efficientOutputPath, participant + currentDT + "_efficientPATHFILE");
            File.AppendAllText(efficientOutputPath, "\nEnvironment\tTrial\tTime(ms)\tStart\tStartLocationX\tStartLocationZ\tTarget\tTargetLocationX\tTargetLocationZ\tOptimalPathX\tOptimalPathZ\n");
        }

        accessibleParticipantPath = participantFile;

    }

    // Update is called once per frame
    void Update()
    {
        //if we have reached our time, or gone over, update the file
        if (Time.time >= totalAllottedSeconds)
        {

            //make sure we establish the path for where the participant files are stored
            string path = participantFile;
            //write the updated information to the participant file
            string content = "";
            if (done)
            {
                //if we are done with the experiment, set up the participant file to move on to the next environment
                content = (currentEnvironment + 1) + "\t" + 0 + "\t" + 1;
            }
            else
            {
                content = currentEnvironment + "\t" + currentLine + "\t" + currentTrial;
            }

            File.WriteAllText(path, content);
            Debug.LogError("we should be writing " + content + " to " + path);
            Quit();
        }
    }

    /*
     * ValdiateEnvironment performs a comparison of the environment number listed in the participant file to that
     * in the environment file name to determine if we are loading the appropriate environment file for the participant
     * If we are then nothing happens
     * If we are loading the wrong file, then we end the experiment and throw an error.
     
    void ValidateEnvironment()
    {
        //get the number of the environment from the file name (e.g. Environment2.txt = 2)
        string environmentNum = Regex.Replace(environmentFile.name, "[^0-9.]", "");
        //compare to ensure we are on the appropriate number
        if (string.Compare(participantArray[0], environmentNum) != 0)
        {
            Debug.LogError("The environment# in the participant file does not match that of the environment file given!");
            Debug.LogError("Please ensure you are loading the correct environment file for the participant.");
            Quit();
        }
    }
    */
    void Quit()
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
