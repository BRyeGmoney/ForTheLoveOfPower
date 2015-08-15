using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {
	Vector3 prevMousePos;
	Vector3 currMousePos;
	Vector3 cameraMovement;
	
	// Use this for initialization
	void Start () {
		prevMousePos = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * 200f;
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);
		
		currMousePos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);
		
		if (Input.GetMouseButtonDown (1)) {
			cameraMovement += new Vector3 (prevMousePos.x - currMousePos.x, prevMousePos.y - currMousePos.y, 0);
		}
		
		
		if (Mathf.Abs (cameraMovement.x) > 0.1f || Mathf.Abs (cameraMovement.y) > 0.1f) {
			Camera.main.transform.position += cameraMovement;
			cameraMovement *= 0.9f;
		}
		
		prevMousePos = currMousePos;
	}
}
