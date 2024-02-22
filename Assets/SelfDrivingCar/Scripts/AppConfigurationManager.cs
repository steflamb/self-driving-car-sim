using UnityEngine;
using SocketIO;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
public class AppConfigurationManager : MonoBehaviour
{

    public AppConfiguration conf = new AppConfiguration();
    private GameObject _app;
    private SocketIOComponent _socket;

    public void Awake() 
    {
        // TODO: change "__app" with a constant
        GameObject _app = GameObject.Find("__app");

        // TODO: read configuration from file
        // TODO: read configuration from command line
        conf.fps = 30;
        conf.port = 4567;
        Debug.Log("Application started with the following configuration: " + JsonUtility.ToJson(conf));

        // Set frame rate
        Application.targetFrameRate = conf.fps;

        // Move to somewhere else
        SceneManager.LoadScene(1);
    }

}

[System.Serializable]
public class AppConfiguration
{
    // Game information
     [SerializeField]
    public int fps = 30;

    // Socket IO configuration
     [SerializeField]
    public int port = 4567;

    // Minimum amount of time (in millis) to wait before sending car telemetry
    [SerializeField]
    public int telemetryMinInterval = 33; // ~33 = 1000 /30


}



// public class RemoteCommandHandler
// {
//     private SocketIOComponent _socket;
//     private EpisodeManager _episodeManager;
//     private CarRemoteControl _carRemoteControl;

//     public void Start() 
//     {

//         GameObject _app = GameObject.Find("_app");
//         _socket = _app.GetComponent<SocketIOComponent> ();
//         _episodeManager = _app.GetComponent<EpisodeManager> ();
//         // TODO: Separate commands for episode/scene and car
//         // Car action
//     }


// }