using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.SceneManagement;


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
	}

	// Update is called once per frame, update the waypoint 5 times per second.
	void Update ()
	{
		detectCrash ();
		if (this.isCrash ()) {
			SceneManager.LoadScene ("MenuScene");

			if (carController.CurrentSpeed > 0.5) {
				stopCar ();
			} else {
				this.carRemoteControl.Acceleration = 0;
				moveCarToNextWayPoint ();
			}
			return;
		}

		if (Time.fixedTime >= timeToGo) {
			foreach (GameObject wayPoint in this.waypoints) {
				if (getWayPointNumber (wayPoint) == (getWayPointNumber (currentWayPoint) + 1) % numberOfWayPoints) {
					float dist = Vector3.Distance (this.transform.position, wayPoint.transform.position);
					if (dist < wayPointActivationDistance) {
						this.currentWayPoint = wayPoint;
						if (this.currentWayPoint == this.firstWayPoint) {
							this.laps += 1;
						}
						//  Debug.Log ("Waypoint: " + this.currentWayPoint.name + " distance = " + dist + " lap number " + this.laps);
					}
				}
			}
			this.timeToGo = Time.fixedTime + updateDelay;
		}
	}

	private void detectCrash ()
	{
		solveLastCrash ();
		if (carController.CurrentSpeed < 1) {

			stuckTimer += 1;
			if (stuckTimer > 180) { // 5 seconds
				registerCrash (carCollider.getLastOBENumber ());
				stuckTimer = 0;
			}

		} else {
			stuckTimer = 0;
		}
	}

	private void stopCar ()
	{
		float currentSpeed = this.carController.CurrentSpeed;
		//  float acc = this.carRemoteControl.Acceleration;

		if (currentSpeed > prevSpeed) {
			decelerationSign = decelerationSign * -1;
		}
		this.carRemoteControl.Acceleration = 100 * decelerationSign;
		prevSpeed = currentSpeed;
	}

	// this is actually OBE
	public void registerCrash (int obe_num)
	{
		this.isCarCrashed = true;
		this.isCrashedInTheLastSecond = true;
		this.lastCrash = System.DateTime.Now.ToFileTime ();
		this.tot_obes = obe_num;
	}

	public void registerCollision (int collision_num)
	{
		this.isCarCrashed = true;
		this.isCrashedInTheLastSecond = true;
		this.lastCrash = System.DateTime.Now.ToFileTime ();
		this.tot_crashes = collision_num;
	}

	public void solveCrash ()
	{
		this.isCarCrashed = false;
		solveLastCrash ();
	}

	public void solveLastCrash ()
	{
		if (isCarCrashed == false && (System.DateTime.Now.ToFileTime () - this.lastCrash) > 10000000) {
			this.isCrashedInTheLastSecond = false;
		}
	}

	public bool isCrash ()
	{
		return this.isCarCrashed;
	}

	public bool isCrashInTheLastSecond ()
	{
		return this.isCrashedInTheLastSecond;
	}

	public GameObject getNextWaypoint ()
	{
		int currentWayPointNumber = getWayPointNumber (this.currentWayPoint);
		int nextWayPointNumber = (currentWayPointNumber + 1) % this.numberOfWayPoints;
		GameObject nextWayPoint = findWayPointByNumber (nextWayPointNumber);
		return nextWayPoint;
	}

	public GameObject getPreviousWaypoint ()
	{
		int currentWayPointNumber = getWayPointNumber (this.currentWayPoint);
		int previousWayPointNumber = (currentWayPointNumber - 1) % this.numberOfWayPoints;
		GameObject previousWayPoint = findWayPointByNumber (previousWayPointNumber);
		return previousWayPoint;
	}

	public void moveCarToNextWayPoint ()
	{
		// 1. update the wayPoint.
		GameObject nextWayPoint = getNextWaypoint ();

		// 2. move the cat to the wayPoint
		Vector3 newRotation = nextWayPoint.transform.rotation.eulerAngles;
		this.transform.position = nextWayPoint.transform.position;
		this.transform.eulerAngles = newRotation;
		solveCrash ();
	}

	public int getCurrentWayPointNmber ()
	{
		return getWayPointNumber (this.currentWayPoint);
	}

	public int getTotalWayPointNmber ()
	{
		return numberOfWayPoints;
	}

	public int getLapNumber ()
	{
		return this.laps;
	}

	public int getOBENumber ()
	{
		return this.tot_obes;
	}

	public int getCrashNumber ()
	{
		return this.tot_crashes;
	}

	private int getWayPointNumber (GameObject wayPoint)
	{
		string name = wayPoint.name;
		// stringNumb shape XXX or 001.
		string stringNumb = name.Substring (name.Length - 3);
		return int.Parse (stringNumb);
	}

	private GameObject findWayPointByNumber (int number)
	{
		string suffix = "";
		if (number < 10) {
			suffix = "00" + number;
		} else if (number < 100) {
			suffix = "0" + number;
		} else {
			suffix += number;
		}
		string wayPointName = "Waypoint " + suffix;
		return GameObject.Find (wayPointName);
	}
}