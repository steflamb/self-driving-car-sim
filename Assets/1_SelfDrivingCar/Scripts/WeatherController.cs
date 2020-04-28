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

	static private float emissionRate;
	static private float emissionRatePercentage;

    public WeatherController (){
		
	}

	void Start () {
		startTime = Time.time;
		//Debug.Log(weather);
		if (weather.Equals ("Rain")) {
			rain.gameObject.SetActive (true);
			snow.gameObject.SetActive (false);
			fog.gameObject.SetActive (false);
			//changeSkybox ("Overcast1 Skybox");
		} else if (weather.Equals ("Snow")) {
			snow.gameObject.SetActive (true);
			rain.gameObject.SetActive (false);
			fog.gameObject.SetActive (false);
			snowOffset = snow.transform.position - carController.transform.position;
			//changeSkybox ("Overcast2 Skybox");
		} else if (weather.Equals ("Fog")) {
			fog.gameObject.SetActive (true);
			rain.gameObject.SetActive (false);
			snow.gameObject.SetActive (false);
			//sun.color = new Vector4 (170.0f/255.0f, 170.0f/255.0f, 170.0f/255.0f, 170.0f/255.0f);
		} else {
			rain.gameObject.SetActive (false);
			snow.gameObject.SetActive (false);
			fog.gameObject.SetActive (false);
		}
	}
		
	public static void setWeather(String weatherCondiction){
		weather = weatherCondiction;
	}

	public static float getEmissionRate(){
		return emissionRate;
	}

	public static float getEmissionRatePercentage(){
		return emissionRatePercentage;
	}

	private void changeSkybox(string skyboxName){
		RenderSettings.skybox = (Material)Resources.Load ("Skyboxes/" + skyboxName);
		DynamicGI.UpdateEnvironment ();
		sun.color = new Vector4 (128.0f/255.0f, 128/255.0f, 128/255.0f, 255/255.0f);
	}

	private void changeSunColor(Vector4 v){
		sun.color = v;
	}
		
	void Update () {
		if (weather.Equals ("Sun")) {
			return;
		}

		timePassedInSeconds = Time.time - startTime;
		float timePassedInSecondsCycle = timePassedInSeconds % cycleTimeInSeconds;

		if (weather.Equals ("Rain")) {
			changeEmissionIntensity (rain, timePassedInSecondsCycle, minRainEmitionRate, maxRainEmitionRate);
		} else if (weather.Equals ("Fog")) {
			changeEmissionIntensity (fog, timePassedInSecondsCycle, minFogEmitionRate, maxFogEmitionRate);
		} else if (weather.Equals ("Snow")) {
			changeEmissionIntensity (snow, timePassedInSecondsCycle, minSnowEmitionRate, maxSnowEmitionRate);
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

	private void changeEmissionIntensity(ParticleSystem weatherEffect,  float time, float minEmitionRate, float maxEmitionRate){
		float half = cycleTimeInSeconds / 2;
		float rate = 0f;
		if (time < half) {
			// Increase effect
			float percent = time / half;
			rate = minEmitionRate + ((percent) * (maxEmitionRate - minEmitionRate));
			//changeSunColor (new Vector4 (248/255.0f, 226/255.0f, 180/255.0f, 255/255.0f));
		} else {
			float percent = (time - half) / (float)(cycleTimeInSeconds - half);
			rate = maxEmitionRate - ((percent) * System.Math.Abs (minEmitionRate - maxEmitionRate));
			//changeSunColor (new Vector4 (220.0f/255.0f, 200/255.0f, 140/255.0f, 255/255.0f));
		}
		var emission = weatherEffect.emission;
		emission.rateOverTime = rate;
	
		// set the object's field
		emissionRate = rate;
		emissionRatePercentage = rate / maxEmitionRate;
		//Debug.Log (emissionRatePercentage);

	}

}