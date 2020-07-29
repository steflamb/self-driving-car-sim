using UnityEngine;
using System.Collections.Generic;
using SocketIO;
using UnityStandardAssets.Vehicles.Car;
using System;
using UnityEngine.SceneManagement;

public class CommandServer : MonoBehaviour
{
	public CarRemoteControl CarRemoteControl;
	public Camera FrontFacingCamera;
	private SocketIOComponent _socket;
	public CarController _carController;
	public WayPointUpdate _wayPointUpdate;
	// for the cte
	private WaypointTracker_pid _wpt;
	int startingTime;

	public static int Current()
	{
		DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		int currentEpochTime = (int)(DateTime.UtcNow - epochStart).TotalSeconds;

		return currentEpochTime;
	}

	public static int SecondsElapsed(int t1)
	{
		int difference = Current() - t1;

		return Mathf.Abs(difference);
	}

	// Use this for initialization
	void Start ()
	{
		_socket = GameObject.Find ("SocketIO").GetComponent<SocketIOComponent> ();
		_socket.On ("open", OnOpen);
		_socket.On ("steer", OnSteer);
		_socket.On ("manual", onManual);
		_carController = CarRemoteControl.GetComponent<CarController> ();
		_wayPointUpdate = CarRemoteControl.GetComponent<WayPointUpdate> ();
		_wpt = new WaypointTracker_pid ();
		startingTime = Current ();
	}

	// Update is called once per frame
	void Update ()
	{
	}

	void OnOpen (SocketIOEvent obj)
	{
		// Debug.Log ("Connection Open");
		EmitTelemetry (obj);
	}

	//
	void onManual (SocketIOEvent obj)
	{
		EmitTelemetry (obj);
	}

	void OnSteer (SocketIOEvent obj)
	{
		JSONObject jsonObject = obj.data;
		CarRemoteControl.SteeringAngle = float.Parse (jsonObject.GetField ("steering_angle").str);
		CarRemoteControl.Acceleration = float.Parse (jsonObject.GetField ("throttle").str);

		// new fields for ICSE '20
		CarRemoteControl.Confidence = int.Parse (jsonObject.GetField ("confidence").str);
		CarRemoteControl.Loss = float.Parse (jsonObject.GetField ("loss").str);
		CarRemoteControl.MaxLaps = int.Parse (jsonObject.GetField ("max_laps").str);

		// new fields
		//CarRemoteControl.Brake = float.Parse (jsonObject.GetField ("brake").str);
		//CarRemoteControl.Intensity = int.Parse (jsonObject.GetField ("intensity").str);

		EmitTelemetry (obj);
	}

	void EmitTelemetry (SocketIOEvent obj)
	{
		DateTime actualTime;

		UnityMainThreadDispatcher.Instance ().Enqueue (() => {
			// send only if it's not being manually driven
			if ((Input.GetKey (KeyCode.W)) || (Input.GetKey (KeyCode.S))) {
				_socket.Emit ("telemetry", new JSONObject ());
			}
            // stop the simulation if max number of laps is reached
            else if (_wayPointUpdate.getLapNumber () != 1 && _wayPointUpdate.getLapNumber () > CarRemoteControl.MaxLaps) {
				SceneManager.LoadScene ("MenuScene");
			} else {
				// Collect Data from the Car
				Dictionary<string, string> data = new Dictionary<string, string> ();

				data ["steering_angle"] = _carController.CurrentSteerAngle.ToString ("N4");
				data ["throttle"] = _carController.AccelInput.ToString ("N4");
				data ["speed"] = _carController.CurrentSpeed.ToString ("N4");
				data ["brake"] = _carController.BrakeInput.ToString ("N4");
				//data ["intensity"] = WeatherController.getEmissionRatePercentage ().ToString ("N2");

				if (_wayPointUpdate != null) {
					data ["lapNumber"] = _wayPointUpdate.getLapNumber ().ToString ();
					data ["currentWayPoint"] = _wayPointUpdate.getCurrentWayPointNmber ().ToString ();
					data ["crash"] = _wayPointUpdate.isCrashInTheLastSecond () ? "1" : "0";
					data ["tot_obes"] = _wayPointUpdate.getOBENumber ().ToString ();
					data ["tot_crashes"] = _wayPointUpdate.getCrashNumber ().ToString ();
					data ["distance"] = _wayPointUpdate.getDrivenDistance().ToString();

					int difference = Mathf.Abs(Current() - startingTime);
					// Debug.Log("simulationTime (s): " + difference);
					data ["sim_time"] = difference.ToString();

					float ang_diff = _wayPointUpdate.getAngularDifference();
					// Debug.Log("angular difference: " + ang_diff);
					data ["ang_diff"] = ang_diff.ToString();
				}

				if (_wpt != null) {
					var cte = _wpt.CrossTrackError (_carController);
					//Debug.Log("CTE: " + cte);
					data ["cte"] = cte.ToString ("N4");
				}

				data ["image"] = Convert.ToBase64String (CameraHelper.CaptureFrame (FrontFacingCamera));
				_socket.Emit ("telemetry", new JSONObject (data));
			}
		});

	}
}