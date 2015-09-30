using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {
	Vector3 prevMousePos;
	Vector3 currMousePos;
	Vector3 cameraMovement;
	byte prevState = 0;

	public float orthoZoomSpeed = 2f;        // The rate of change of the orthographic size in orthographic mode.
	
	// Use this for initialization
	void Start () {
		prevMousePos = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {

		// If there are two touches on the device...
		if (Input.touchCount == 2)
		{
			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			
			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
			
			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
			
			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

			if (touchDeltaMag > 120) {
				// ... change the orthographic size based on the change in distance between the touches.
				Camera.main.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
				
				// Make sure the orthographic size never drops below zero.
				//Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize, 0.1f);
				Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);
			} else {
				Camera.main.transform.parent.transform.Translate(-touchZero.deltaPosition.x * 5f, -touchZero.deltaPosition.y * 5f, 0);
			}
		} 

		if (!Input.touchSupported) {
			Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * 200f;
			Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);

			if (Input.GetMouseButton (2)) { // we're moving
				Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y)), 5 * Time.deltaTime);
				currMousePos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);
				prevState = 1;
			} else {
				if (prevState == 1) 
					cameraMovement = new Vector3 (prevMousePos.x - currMousePos.x, prevMousePos.y - currMousePos.y, 0);

				if (Mathf.Abs (cameraMovement.x) > 0.1f || Mathf.Abs (cameraMovement.y) > 0.1f) {
					Camera.main.transform.position += cameraMovement;
					cameraMovement *= 0.9f;
				}

				prevState = 0;
			}

			prevMousePos = currMousePos;
		}
	}
}
