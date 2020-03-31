using System;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.SceneManagement;


public class WeatherController : MonoBehaviour
{
	static private string weather = "Sun";

	public CarController carController;
	public ParticleSystem rain;
	public ParticleSystem snow;
	public ParticleSystem fog;
	public Light sun;
	private float cycleTimeInSeconds = 60;		// CHANGE THE TOTAL TIME CYCLE OF THE EFFECT
	public int maxRainEmitionRate = 10000;
	public int minRainEmitionRate = 100;
	public int maxFogEmitionRate = 5000;
	public int minFogEmitionRate = 100;
	public int maxSnowEmitionRate = 800;
	public int minSnowEmitionRate = 100;
	private float startTime;
	private float timePassedInSeconds;
	private Vector3 snowOffset;
	private Vector3 rainOffset;

	// minRait = 100

	public WeatherController (){
		
	}

	void Start () {
		startTime = Time.time;
		Debug.Log(weather);
		if (weather.Equals ("Rain")) {
			snow.gameObject.SetActive (false);
			fog.gameObject.SetActive (false);
			changeSkybox ("Overcast1 Skybox");
		} else if (weather.Equals ("Snow")) {
			rain.gameObject.SetActive (false);
			fog.gameObject.SetActive (false);
			snowOffset = snow.transform.position - carController.transform.position;
			changeSkybox ("Overcast2 Skybox");
		} else if (weather.Equals ("Fog")) {
			rain.gameObject.SetActive (false);
			snow.gameObject.SetActive (false);
			sun.color = new Vector4 (170.0f/255.0f, 170.0f/255.0f, 170.0f/255.0f, 170.0f/255.0f);
		} else {
			rain.gameObject.SetActive (false);
			snow.gameObject.SetActive (false);
			fog.gameObject.SetActive (false);
		}
	}
		
	public static void setWeather(String weatherCondiction){
		weather = weatherCondiction;
	}

	private void changeSkybox(string skyboxName){
		RenderSettings.skybox = (Material)Resources.Load ("Skyboxes/" + skyboxName);
		DynamicGI.UpdateEnvironment ();
		sun.color = new Vector4 (128.0f/255.0f, 128/255.0f, 128/255.0f, 255/255.0f);
	}

	// Every 60 seconds move from shallowRain 

	void Update () {
		if (weather.Equals ("Sun")) {
			return;
		}

		timePassedInSeconds = Time.time - startTime;
		float timePassedInSecondsCycle = timePassedInSeconds % cycleTimeInSeconds;
		if (weather.Equals ("Rain")) {
			changeEmissionStrenght (rain, timePassedInSecondsCycle, minRainEmitionRate, maxRainEmitionRate);
		} else if (weather.Equals ("Fog")) {
			changeEmissionStrenght (fog, timePassedInSecondsCycle, minFogEmitionRate, maxFogEmitionRate);
		} else if (weather.Equals ("Snow")) {
			changeEmissionStrenght (snow, timePassedInSecondsCycle, minSnowEmitionRate, maxSnowEmitionRate);
			moveEffect (snow.gameObject, carController.gameObject, snowOffset);

		}
	}

	private void moveEffect(GameObject effect, GameObject target, Vector3 offset){
		effect.gameObject.transform.position = target.transform.position +  offset;
		if (SceneManager.GetActiveScene ().name.Contains ("Lake")) {
			Vector3 dir = effect.transform.position - target.transform.position; // get point direction relative to pivot
			dir = Quaternion.Euler (target.transform.eulerAngles) * dir; // rotate it
			Vector3 point = dir + target.transform.position; // calculate rotated point
			effect.gameObject.transform.position = point;
		}
	}

	private void changeEmissionStrenght(ParticleSystem weatherEffect,  float time, float minEmitionRate, float maxEmitionRate){
		float half = cycleTimeInSeconds / 2;
		float rate = 0f;
		if (time < half) {
			// Increase effect
			float percent = time / half;
			rate = minEmitionRate + ((percent) * (maxEmitionRate - minEmitionRate));
		} else {
			float percent = (time - half) / (float)(cycleTimeInSeconds - half);
			rate = maxEmitionRate - ((percent) * System.Math.Abs (minEmitionRate - maxEmitionRate));
		}
		var emission = weatherEffect.emission;
		emission.rateOverTime = rate;
	}
}