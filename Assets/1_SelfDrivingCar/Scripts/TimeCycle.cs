using UnityEngine;
using System.Collections;

public class TimeCycle : MonoBehaviour {

	public AnimationCurve sunIntensity;
	public Gradient sunLightColor;
	public Gradient cloudsBaseColor;
	public Gradient cloudsColor;
	public GameObject sun;
	public GameObject[] clouds;
	[Range(0.0f, 1.0f)]
	public float time = 0;
	public ReflectionProbe reflection;
	public Vector3 rotation;
	public float giUpdateTime = 0.5f;
	private float curTime = 0.0f;
	int probeReflection;

	Material backUpMaterial;
	public const float TIME_LIMIT = 10F;
	private float timer = 0F;

	// Store the default skybox at the beginning of the scene
	void Start () {
		backUpMaterial = makeSkyboxBackUp();
	}

	// Update is called once per frame
	void Update () {
		this.timer += Time.deltaTime;

		// check if it's time to switch scenes
		if (this.timer >= TIME_LIMIT) {
			int var = Random.Range (0, 2);
//			Debug.Log (var);
			if (var == 0) {
				RenderSettings.skybox = (Material)Resources.Load ("SkyboxProcedural");
				DynamicGI.UpdateEnvironment ();
				Debug.Log ("Changed skybox to SkyboxProcedural");
			} 
			else if (var == 1) {
				RenderSettings.skybox = backUpMaterial; //(Material)Resources.Load ("Default-Skybox");
				DynamicGI.UpdateEnvironment ();
				Debug.Log ("Changed skybox to Default-Skybox");
			}
			timer = 0F;
		}

//		rotation = new Vector3 (time * 360, rotation.y, rotation.z);
//		sun.transform.rotation = Quaternion.Euler (rotation);
//		sun.GetComponent<Light> ().color = sunLightColor.Evaluate (time);
//		sun.GetComponent<Light> ().intensity = sunIntensity.Evaluate (time);
//
//		if (Time.time - curTime > giUpdateTime) {
//			DynamicGI.UpdateEnvironment ();
//			curTime = Time.time;
//		}
	}

	Material makeSkyboxBackUp()
	{
		return new Material(RenderSettings.skybox);
	}

	//Restore skybox material back to the Default values
	void OnDisable()
	{
		RenderSettings.skybox = backUpMaterial;
		DynamicGI.UpdateEnvironment();
	}
}
