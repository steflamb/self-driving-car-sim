using UnityEngine;
using SocketIO;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.Sensors.Channels;
using UnityEngine.UI;
// TODO: It is sending telemetry even if it is not needed
// we should check if socket is not null
// we should check if the telemetry is reasonable

// TODO: should be removed once RemoteCommandHelper is moved from this file
using UnityStandardAssets.Vehicles.Car;

// TODO: we should divide the script in two submodule:
// 1) driving commands
// 2) sensors (car telemetry)

public class CarManager : MonoBehaviour
{

    public CarTelemetry CurrentTelemetry;

    private GameObject app;
    private SocketIOComponent socket;
    private AppConfigurationManager confManager;
    private long lastTimeTelemetryUpdated;
    // TODO: the remote control should be assigned externally
    // TODO: this is very important because otherwise the Update loop is not very performing
    private CarRemoteControl carRemoteController;
    private CarController carController;
    private WaypointTracker_pid waypointController;
    private Camera frontFacingCamera;
    private PerceptionCamera perceptionCamera;

    public void Awake()
    {
        app = GameObject.Find("__app");
        socket = app.GetComponent<SocketIOComponent> ();
        confManager = app.GetComponent<AppConfigurationManager> ();
        lastTimeTelemetryUpdated = -1;
		SceneManager.sceneLoaded += OnSceneLoaded;
        // waypointController = new WaypointTracker_pid();
        socket.On("action", carAction);
    }

	public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		Debug.Log(scene);
        // waypointController = new WaypointTracker_pid();
	}

    public void Update()
    {
        // TODO: update telemetry here
        updateTelemetry();
        // TODO: remove this part from Update in future implementation
        // TODO: this is very important for performance reasons
        connectCarController();
        // connectWaypointController();
    }
    
    private void connectCarController()
    {
	    var car = GameObject.Find("Car");
	    if (car != null)
	    {
		    carController = car.GetComponent<CarController> ();
		    carRemoteController = car.GetComponent<CarRemoteControl> ();
			var wu = car.GetComponent<WayPointUpdate>();
			waypointController = wu.waypointTracker_pid;
	    }
		var ffc = GameObject.Find("Front Facing Camera");
		if (ffc != null) {
			frontFacingCamera = ffc.GetComponent<Camera>();
		}
		var pc = GameObject.Find("Perception Camera");
		if (pc != null) {
			perceptionCamera = pc.GetComponent<PerceptionCamera>();
		}
    }

    private void connectWaypointController()
    {
	    var car = GameObject.Find("Car");
	    if (car != null)
	    {
		    waypointController= car.GetComponent<WaypointTracker_pid> ();
	    }
    }

    private void carAction (SocketIOEvent obj)
	{
        if (carRemoteController != null) 
        {
            JSONObject jsonObject = obj.data;
			Debug.Log("Steering: " + jsonObject.GetField ("steering_angle").str + ", CTE: " + CurrentTelemetry.cte);
            carRemoteController.SteeringAngle = float.Parse (jsonObject.GetField ("steering_angle").str);
            carRemoteController.Acceleration = float.Parse (jsonObject.GetField ("throttle").str);
        }
	}

	private void updateTelemetry()
	{
		UnityMainThreadDispatcher.Instance().Enqueue(() =>
			{
				CarTelemetry telemetry = new CarTelemetry();
				// If the car controller is available, collect car position
				if (carController != null)
				{
					telemetry.timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

					telemetry.image = Convert.ToBase64String (CameraHelper.CaptureFrame (frontFacingCamera));

					telemetry.pos_x = carController.transform.position[0];
					telemetry.pos_y = carController.transform.position[1];
					telemetry.pos_z = carController.transform.position[2];
					telemetry.steering_angle = carController.CurrentSteerAngle;
					telemetry.throttle = carController.AccelInput;
					telemetry.speed = carController.CurrentSpeed;
					this.CurrentTelemetry = telemetry;
				}

				if (perceptionCamera != null)
				{
					perceptionCamera.RequestCapture();
					if (GameObject.Find("SegmentTexture") != null)
					{
						var targetTexture = GameObject.Find("SegmentTexture").GetComponent<RawImage>().texture;
						RenderTexture.active = (RenderTexture) targetTexture;
						Texture2D texture2D = new Texture2D (targetTexture.width, targetTexture.height, TextureFormat.RGB24, false, false);
						texture2D.ReadPixels (new Rect (0, 0, targetTexture.width, targetTexture.height), 0, 0);
						texture2D.Apply ();
						byte[] image = texture2D.EncodeToPNG ();
						UnityEngine.Object.DestroyImmediate (texture2D);
						telemetry.semantic_segmentation = Convert.ToBase64String(image);
						this.CurrentTelemetry = telemetry;
					} else {
						Debug.Log("SegmentTexture not found");
					}
				}

				// TODO: manage the part for Road Generator

				// If there's a way point tracking system
				if (waypointController != null)
				{
					telemetry.cte = waypointController.CrossTrackError(carController, absolute: false);
					this.CurrentTelemetry = telemetry;
				} else {
					Debug.Log("Waypoint controller not found");
				}
				this.CurrentTelemetry = telemetry;

        		long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        		if (carRemoteController != null)
        		{
            		if (currentTime - lastTimeTelemetryUpdated > confManager.conf.telemetryMinInterval)
					{
	            		CurrentTelemetry.timestamp = currentTime.ToString();
                		socket.Emit("car_telemetry", JsonUtility.ToJson(CurrentTelemetry));
                		lastTimeTelemetryUpdated = currentTime;
            		}
        		}
			}
		);
	}
}

// TODO: allow for dynamic telemetry. Send maybe more images (according to the cameras the car is equipped)
[System.Serializable]
public class CarTelemetry
{
    public string timestamp;
    public string image;
    public string semantic_segmentation;
    public float pos_x;
    public float pos_y;
    public float pos_z;
    public float steering_angle;
    public float throttle;
    public float speed;
    public float cte;
}
