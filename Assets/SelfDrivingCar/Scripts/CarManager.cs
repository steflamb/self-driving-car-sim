using UnityEngine;
using SocketIO;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
// TODO: It is sending telemetry even if it is not needed
// we should check if _socket is not null
// we should check if _the telemetry is reasonable

// TODO: should be removed once RemoteCommandHelper is moved from this file
using UnityStandardAssets.Vehicles.Car;


public class CarManager : MonoBehaviour
{

    public CarTelemetry telemetry;

    private GameObject _app;
    private SocketIOComponent _socket;
    private AppConfigurationManager _confManager;
    private long _lastTimeTelemetryUpdated;
    // TODO: the remote control should be assigned externally
    private CarRemoteControl _carRemoteControl;

    public CarRemoteControl CarRemoteControl
    {
        set => _carRemoteControl = value;
    }

    public void Awake()
    {
        _app = GameObject.Find("__app");
        _socket = _app.GetComponent<SocketIOComponent> ();
        _confManager = _app.GetComponent<AppConfigurationManager> ();
        _lastTimeTelemetryUpdated = -1;
        _socket.On("action", CarAction);
    }

    public void Update()
    {
        // TODO: update telemetry here
        connectRemoteControl();
        long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (_carRemoteControl != null) 
        {
            if (currentTime - _lastTimeTelemetryUpdated > _confManager.conf.telemetryMinInterval) {
                telemetry.timestamp = currentTime.ToString();
                _socket.Emit("car_telemetry", JsonUtility.ToJson(telemetry));
                _lastTimeTelemetryUpdated = currentTime;
            }
        }
    }

    private void connectRemoteControl()
    {
        var car = GameObject.Find("Car");
        if (car != null)
        {
            _carRemoteControl = car.GetComponent<CarRemoteControl> ();
        }
    }

    private void CarAction (SocketIOEvent obj)
	{
        if (_carRemoteControl != null) 
        {
            JSONObject jsonObject = obj.data;
            _carRemoteControl.SteeringAngle = float.Parse (jsonObject.GetField ("steering_angle").str);
            _carRemoteControl.Acceleration = float.Parse (jsonObject.GetField ("throttle").str);
        }
	}
}

// TODO: allow for dynamic telemetry. Send maybe more images (according to the cameras the car is equipped)
[System.Serializable]
public class CarTelemetry
{
    public string timestamp;
    public string image;
    public float pos_x;
    public float pos_y;
    public float pos_z;
    public float steering_angle;
    public float throttle;
    public float speed;
    public float cte;
}