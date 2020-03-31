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
    private WaypointTracker_pid _wpt; // for the cte

    // Use this for initialization
    void Start()
    {
        _socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        _socket.On("open", OnOpen);
        _socket.On("steer", OnSteer);
        _socket.On("manual", onManual);
        _carController = CarRemoteControl.GetComponent<CarController>();
        _wayPointUpdate = CarRemoteControl.GetComponent<WayPointUpdate>();
        _wpt = new WaypointTracker_pid();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnOpen(SocketIOEvent obj)
    {
        Debug.Log("Connection Open");
        EmitTelemetry(obj);
    }

    // 
    void onManual(SocketIOEvent obj)
    {
        EmitTelemetry(obj);
    }

    void OnSteer(SocketIOEvent obj)
    {
        JSONObject jsonObject = obj.data;
        CarRemoteControl.SteeringAngle = float.Parse(jsonObject.GetField("steering_angle").str);
        CarRemoteControl.Acceleration = float.Parse(jsonObject.GetField("throttle").str);
        CarRemoteControl.Confidence = int.Parse(jsonObject.GetField("confidence").str);
        CarRemoteControl.Loss = float.Parse(jsonObject.GetField("loss").str);
        CarRemoteControl.MaxLaps = int.Parse(jsonObject.GetField("max_laps").str);
        EmitTelemetry(obj);
    }

    void EmitTelemetry(SocketIOEvent obj)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // send only if it's not being manually driven
            if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S)))
            {
                _socket.Emit("telemetry", new JSONObject());

                // Collect Data from the Car
                //Dictionary<string, string> data = new Dictionary<string, string>();

                //var cte = _wpt.CrossTrackError(_carController);
                //Debug.Log("CTE: " + cte);

                //data["cte"] = cte.ToString("N4");
                //_socket.Emit("telemetry", new JSONObject(data));

            }
            // stop the simulation if max number of laps is reached
            else if (_wayPointUpdate.getLapNumber() != 1 && _wayPointUpdate.getLapNumber() > CarRemoteControl.MaxLaps)
            {
                SceneManager.LoadScene("MenuScene");
            }
            else
            {
                // Collect Data from the Car
                Dictionary<string, string> data = new Dictionary<string, string>();

                data["steering_angle"] = _carController.CurrentSteerAngle.ToString("N4");
                data["throttle"] = _carController.AccelInput.ToString("N4");
                data["speed"] = _carController.CurrentSpeed.ToString("N4");

                if (_wayPointUpdate != null)
                {
                    data["lapNumber"] = _wayPointUpdate.getLapNumber().ToString();
                    data["currentWayPoint"] = _wayPointUpdate.getCurrentWayPointNmber().ToString();
                    data["crash"] = _wayPointUpdate.isCrashInTheLastSecond() ? "1" : "0";
                    data["tot_obes"] = _wayPointUpdate.getOBENumber().ToString();
                    data["tot_crashes"] = _wayPointUpdate.getCrashNumber().ToString();
                }

                if (_wpt != null)
                {
                    var cte = _wpt.CrossTrackError(_carController);
                    //Debug.Log("CTE: " + cte);
                    data["cte"] = cte.ToString("N4");
                }

                data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
                _socket.Emit("telemetry", new JSONObject(data));
            }
        });

    }
}