using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class WayPointUpdate : MonoBehaviour
{

	public CarRemoteControl carRemoteControl;
	public GameObject[] waypoints;
	public int laps;
	public GameObject currentWayPoint;
	public GameObject firstWayPoint;
	public int numberOfWayPoints = 0;
	private float wayPointActivationDistance = 5.00f;
	private float timeToGo;
	private float updateDelay = 0.1f;
	private CarController carController;
	private bool isCarCrashed;
	private int stuckTimer;
	private bool isCrashedInTheLastSecond = false;
	private long lastCrash;
	private float prevSpeed = 0;
	private int decelerationSign = 1;
	private int tot_obes = 0;
	private int tot_crashes = 0;

	public CarCollider carCollider;
	private float timeLeft = 120.0f;
	private float total_driven_distance = 0.0f;
	private float angular_difference = 0.0f;

	public WaypointTracker_pid waypointTracker_pid;

	// Use this for initialization
	void Start ()
	{
		// find all view points. set up object fields
		this.waypoints = GameObject.FindGameObjectsWithTag ("Waypoint");
		this.numberOfWayPoints = this.waypoints.Length;
		this.firstWayPoint = findWayPointByNumber (0);
		this.laps = 1;
		this.currentWayPoint = firstWayPoint;
		carRemoteControl = GetComponent<CarRemoteControl> ();
		carController = GetComponent<CarController> ();
		carCollider = GetComponent<CarCollider> ();
		this.timeToGo = Time.fixedTime + updateDelay;

		waypointTracker_pid = new WaypointTracker_pid();
	}

	// Update is called once per frame, update the waypoint 5 times per second.
	void Update ()
	{
		bool stuck = false;
		bool outs = false;

		if (getWayPointNumber(this.currentWayPoint) > 1)
		{
			// TODO: when the car gets stuck, crash is not registered
			stuck = detectCarIsStuck();

			// seems to work, needs more testing
			outs = detectCarIsOutOfTrack();
		}

		//if (this.isCrash ()) {
		if (stuck || outs) {
            if (carController.CurrentSpeed > 1)
            {
                stopCar();
            }
            else
            {
                this.carRemoteControl.Acceleration = 0;
                moveCarToNextWayPoint();
            }
            return;
        }

		if (Time.fixedTime >= timeToGo) {
			foreach (GameObject wayPoint in this.waypoints) {
				if (getWayPointNumber (wayPoint) == (getWayPointNumber (currentWayPoint) + 1) % numberOfWayPoints) {

					// distance between can and next waypoint
					float dist = Vector3.Distance (this.transform.position, wayPoint.transform.position);

					// angular difference
					Vector3 targetDir = wayPoint.transform.position - this.transform.position;
					this.angular_difference = Vector3.Angle(targetDir, transform.forward);

					if (dist < wayPointActivationDistance) {
						this.currentWayPoint = wayPoint;
						if (this.currentWayPoint == this.firstWayPoint)
						{
							this.laps += 1;
						}
						//  Debug.Log ("Waypoint: " + this.currentWayPoint.name + " distance = " + dist + " lap number " + this.laps);
					}
				}
			}
			this.timeToGo = Time.fixedTime + updateDelay;
		}
	}

	private bool detectCarIsOutOfTrack()
    {
		float cte = waypointTracker_pid.CrossTrackError(carController);
        solveLastCrash();

		if (Mathf.Abs(cte) > 7)
        {
			// add the obe manually cause we are outside the carCollider
			registerCrash(carCollider.getLastOBENumber() + 1);
			this.isCarCrashed = true;
			return true;
		}
		else
        {
			this.isCarCrashed = false;
			return false;
		}
	}

	private bool detectCarIsStuck()
	{
		solveLastCrash();
		if (carController.CurrentSpeed < 1)
		{
			stuckTimer += 1;
			if (stuckTimer > 120) // 5 seconds  // 50 for testing
			{
				//this.isCarCrashed = true;
				//this.isCrashedInTheLastSecond = true;
				registerCrash(carCollider.getLastOBENumber() + 1);
				stuckTimer = 0;
			}
			return true;
		}
		else
		{
			//this.isCarCrashed = false;
			stuckTimer = 0;
			return false;
		}
	}

	private void stopCar()
	{
		float currentSpeed = this.carController.CurrentSpeed;

		if (currentSpeed > prevSpeed)
		{
			decelerationSign = decelerationSign * -1;
		}
		this.carRemoteControl.Acceleration = 100 * decelerationSign;
		prevSpeed = currentSpeed;
	}

	// this is actually OBE
	public void registerCrash (int obe_num)
	{
        // TODO: fixes erroneous OBE when the simulation starts. Find a better way.
        if (getWayPointNumber (this.currentWayPoint) > 1) {
			this.isCarCrashed = true;
			this.isCrashedInTheLastSecond = true;
			this.lastCrash = System.DateTime.Now.ToFileTime ();
			this.tot_obes = obe_num;
		}
	}

	public void registerCollision(int collision_num)
	{
		this.isCarCrashed = true;
		this.isCrashedInTheLastSecond = true;
		this.lastCrash = System.DateTime.Now.ToFileTime();
		this.tot_crashes = collision_num;
	}

	public void solveCrash()
	{
		this.isCarCrashed = false;
		solveLastCrash();
	}

	public void solveLastCrash()
	{
		if (isCarCrashed == false && (System.DateTime.Now.ToFileTime() - this.lastCrash) > 10000000)
		{
			this.isCrashedInTheLastSecond = false;
		}
	}

	public bool isCrash()
	{
		return this.isCarCrashed;
	}

	public bool isCrashInTheLastSecond()
	{
		return this.isCrashedInTheLastSecond;
	}

	public void moveCarToNextWayPoint()
	{
		int currentWayPointNumber = getWayPointNumber(this.currentWayPoint);
		int nextWayPointNumber = (currentWayPointNumber + 1) % this.numberOfWayPoints;

		// 1. update the wayPoint.
		GameObject nextWayPoint = findWayPointByNumber(nextWayPointNumber);

		// 2. move the cat to the wayPoint
		Vector3 newRotation = nextWayPoint.transform.rotation.eulerAngles;
		this.transform.position = nextWayPoint.transform.position;
		this.transform.eulerAngles = newRotation;
		solveCrash();
	}

	public int getCurrentWayPointNumber()
	{
		return getWayPointNumber(this.currentWayPoint);
	}

	public int getTotalWayPointNmber()
	{
		return numberOfWayPoints;
	}

	public int getLapNumber()
	{
		return this.laps;
	}

	public int getOBENumber()
	{
		return this.tot_obes;
	}

	public float getDrivenDistance ()
	{
		return this.total_driven_distance;
	}

	public float getAngularDifference ()
	{
		return this.angular_difference;
	}

	public int getCrashNumber ()
	{
		return this.tot_crashes;
	}

	private int getWayPointNumber(GameObject wayPoint)
	{
		string name = wayPoint.name;
		// stringNumb shape XXX or 001.
		string stringNumb = name.Substring(name.Length - 3);
		return int.Parse(stringNumb);
	}

	private GameObject findWayPointByNumber(int number)
	{
		string suffix = "";
		if (number < 10)
		{
			suffix = "00" + number;
		}
		else if (number < 100)
		{
			suffix = "0" + number;
		}
		else
		{
			suffix += number;
		}
		string wayPointName = "Waypoint " + suffix;
		return GameObject.Find(wayPointName);
	}
}