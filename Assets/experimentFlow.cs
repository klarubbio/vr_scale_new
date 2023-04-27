using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class experimentFlow : MonoBehaviour
{
    string nextScene = "";
    public static string participant = "def";

    void Awake()
    {
        //ensures game info is available in next scene
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (nextScene != "" && Input.GetKeyDown(KeyCode.Return))
        {
            LoadNextScene();
        }
    }

    public void SetNextScene(string name)
    {
        nextScene = name;
    }

    public void SetParticipant(string id)
    {
        participant = id;
    }

    void LoadNextScene()
    {
        Debug.Log(nextScene+ " "+ participant);
        //single scene mode loads this scene instead of the current one
        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }
}
