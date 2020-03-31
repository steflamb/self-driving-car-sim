using UnityEngine;

public class OnCarCollision : MonoBehaviour {
	void onCollisionEnter (Collision collision) {
		Debug.Log ("Enter Collision");
	}

	void onCollisionStay (Collision collision) {
		Debug.Log ("Collision Going On");
	}

	void onCollisionExit (Collision collision) {
		Debug.Log ("Exit Collision");
	}
}


