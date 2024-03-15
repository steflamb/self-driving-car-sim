# Documentation

**This document is in Working Progress State.**

This document is a comprehensive reference guide for the Application Programming Interface (API) used within this Unity
project, specifically for the Udacity Self-Driving Car Simulator.

# Network API

This section details the functionalities related to networking used within the project. Here, we primarily focus on the
interaction with the Socket.IO library for communication.
The networking protocol adopted by this library is compatible with JavaScript Socket.IO version 1.x and 2.x, and it is
compatible with python-socketio version 4.x.

The functionalities of the simulator are accessible through the network API.
The API is fully asynchronous. Acknowledgements are sent in case anyone needs a synchronous API.

### Application API

The class managing the application API can be found at `Assets/SelfDrivingCar/AppConfigurationManager.cs`.
The class permits to manage the following properties either using a config file or as command line arguments:

- `fps`: frame per second of the application. (Default value: 30)
- `port`: port of Socket.IO server to which the application should connect. (Default value: 4567)

It is possible to edit the parameters using the following command:

```bash
./unity_binary.x86_64 --fps 60 --port 8080
```

or using a JSON configuration file like:

```json
{
  "fps": 60,
  "port": 8080
}
```

and running the binary with:

```bash
./unity_binary.x86_64 --config {PATH_CONFIG_FILE}
```

Command line settings override config file settings.

### Episode API

The class managing the Episode API can be found at `Assets/SelfDrivingCar/EpisodeManager.cs`.
This section will detail functionalities related to managing episodes within the Udacity Self-Driving Car Simulator.
Each `Episode` can be seen as an experimental run. It is possible to manage the lifecycle of the `Episode` by sending a
network message with namespace:

- `start_episode`: starts a new episode.
    - it requires a parameter that describe the track of the new episode (default `lake`).
    - it requires a parameter describing the weather (default `sunny`).
    - it requires a parameter describing if day or day-night cycle (default `day`).
- `end_episode`: ends an episode.
- `pause_sim`: pauses the simulation (frames are still rendered).
- `resume_sim`: resumes the simulation.

After each command, the API will provide an acknowledgement.

During the episode some messages can be emitted by this component. In particular:

- `episode_event`: describes an event that happened in the episode. An event is described with the following JSON
  object: `{timestamp: "unix_time", key: "oot", value: {position: (0, 1, 2)}}`. Currently, only two types of events are
  generated: `hit` (collision) and `oot` (out of track).
- `episode_events`: describes all events that happened in the episode. This message is sent when an episode concludes.
- `episode_metrics`: describes the metrics collected in the episode. This message is sent when an episode concludes.

## Car API

The class managing the Episode API can be found at `Assets/SelfDrivingCar/CarManager.cs`.
The car API provides a way to manage the Car.

- `enable segmentation`: enables the segmentation camera. Currently, only three labels are supported `road`, `terrain` and `sky`.
- `action`: provides driving commands to the car in the simulator. Driving commands should be in this format: `{"steering_angle":0.1, "throttle":0.1}`.

- `car_telemetry`: the car returns telemetry at each frame. Telemetry has the following format:
```json
{
    "timestamp": "unix_time",
    "image": "BASE64_encoded_image",
    "semantic_segmentation": "BASE64_encoded segmentation",
    "pos_x": 0.5,
    "pos_y": 0.5,
    "pos_z": 0.0,
    "steering_angle": 0.3,
    "throttle": 0.25,
    "speed": 12.3,
    "cte": 1.4
}
```
