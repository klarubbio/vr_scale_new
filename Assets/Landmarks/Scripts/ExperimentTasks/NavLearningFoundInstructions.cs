/*
    Copyright (C) 2010  Jason Laczko

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;
using TMPro;

public class NavLearningFoundInstructions : ExperimentTask {

    public static string conditionalMessage;

    [Header("Task-specific Properties")]

    public TextAsset instruction;
    public TextAsset message;

    public ObjectList objects;
    public ObjectList[] multiObjects; // if you want the same subset from several lists
    private GameObject currentObject;
    private string[] currentMultiObjects;

    public TextList texts;
    private string currentText;

    public bool blackout = true;
    public Color text_color = Color.white;
    public Font instructionFont;
    public int instructionSize = 12;

    public bool actionButtonOn = true;
    public string customButtonText = "";

    private Text gui;

    public bool restrictMovement = true; // MJS do we want to keep them still during this?
    public bool selfPaced = true; // can they press return to end the task?

    void OnDisable ()
    {
        if (gui)
            DestroyImmediate (gui.gameObject);
    }

    public override void startTask () {
        TASK_START();
        Debug.Log ("Starting an Instructions Task");
        ResetHud();
    }

    public override void TASK_START()
    {
        if (!manager) Start();
        base.startTask();

        if (skip) {
            log.log("INFO    skip task    " + name,1 );
            return;
        }

        GameObject sgo = new GameObject("Instruction Display");

        GameObject avatar = manager.player.GetComponent<HUD>().Canvas as GameObject;
        Text canvas = avatar.GetComponent<Text>();
        hud.SecondsToShow = hud.InstructionDuration;

        sgo.AddComponent<Text>();
        sgo.hideFlags = HideFlags.HideAndDontSave;
        sgo.transform.position = new Vector3(0,0,0);
        gui = sgo.GetComponent<Text>();
        // DEPRICATED IN UNITY 2019 // gui.pixelOffset = new Vector2( 20, Screen.height - 20);
        gui.font = instructionFont;
        gui.fontSize = instructionSize;
        gui.material.color = text_color;

        

        if (blackout) hud.showOnlyHUD();
        else hud.showEverything();

            hud.setMessage(conditionalMessage);
       

        hud.flashStatus("");

        if (restrictMovement)
        {
            manager.player.GetComponent<CharacterController>().enabled = false;
            manager.scaledPlayer.GetComponent<ThirdPersonCharacter>().immobilized = true;
        }

        // Change text and turn on the map action button if we're using it
        if (actionButtonOn)
        {

            // Use custom text for button (if provided)
            if (customButtonText != "") actionButton.GetComponentInChildren<Text>().text = customButtonText;
            // Otherwise, use default text attached to the button (component)
            else actionButton.GetComponentInChildren<Text>().text = actionButton.GetComponent<DefaultText>().defaultText;


            // activate the button
            hud.actionButton.SetActive(true);
            hud.actionButton.GetComponent<Button>().onClick.AddListener(hud.OnActionClick);

            // we'll need the mouse, as well
            // make the cursor functional and visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    // Update is called once per frame
    public override bool updateTask () {

        if (skip) {
            //log.log("INFO    skip task    " + name,1 );
            return true;
        }
        if ( interval > 0 && Experiment.Now() - task_start >= interval)  {
            return true;
        }


        //------------------------------------------
        // Handle buttons to advance the task - MJS
        //------------------------------------------
        if (selfPaced)  
        {
            if (Input.GetButtonDown("Return") | Input.GetKeyDown(KeyCode.Return))
            {
                log.log("INPUT_EVENT    PlayerPressedReturn    1", 1);
                return true;
            }

            else if (actionButtonOn && hud.actionButtonClicked == true)
            {
                hud.actionButtonClicked = false;
                log.log("INPUT_EVENT    PlayerPressedActionButton    1", 1);
                return true;
            }
            else return false;
        }


        if (killCurrent == true)
        {
            return KillCurrent ();
        }



        return false;
    }

    public override void endTask() {
        // Debug.Log ("Ending an instructions task");
        TASK_END();
    }

    public override void TASK_END() {
        base.endTask ();

        hud.setMessage ("");
        hud.SecondsToShow = hud.GeneralDuration;

        if (canIncrementLists) {

            if (objects) {
                objects.incrementCurrent();
                currentObject = objects.currentObject();
            }
            if (texts) {
                texts.incrementCurrent();
                currentText = texts.currentString();
            }

        }

        GameObject avatar = manager.player.GetComponent<HUD>().Canvas as GameObject;
        Text canvas = avatar.GetComponent<Text>();
        string nullstring = null;
        canvas.text = nullstring;
//            StartCoroutine(storesInactive());

        if (actionButtonOn)
        {
            // Reset and deactivate action button
            hud.actionButton.GetComponentInChildren<Text>().text = hud.actionButton.GetComponent<DefaultText>().defaultText;
            hud.actionButton.GetComponent<Button>().onClick.RemoveListener(hud.OnActionClick);
            hud.actionButton.SetActive(false);

            // make the cursor invisible
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }

        // If we turned movement off; turn it back on
        if (restrictMovement)
        {
            manager.player.GetComponent<CharacterController>().enabled = true;
            manager.scaledPlayer.GetComponent<ThirdPersonCharacter>().immobilized = false;
        }
    }

}
