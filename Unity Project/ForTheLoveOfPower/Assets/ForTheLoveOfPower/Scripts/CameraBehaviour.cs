using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {
	Vector3 prevMousePos;
	Vector3 currMousePos;
	Vector3 cameraMovement;
	byte prevState = 0;

	public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
	public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
	//pinch
	/*public int speed = 4;
	public float MINSCALE = 2.0F;
	public float MAXSCALE = 5.0F;
	public float minPinchSpeed = 5.0F;
	public float varianceInDistances = 5.0F;
	private float touchDelta = 0.0F;
	private Vector2 prevDist = new Vector2(0,0);
	private Vector2 curDist = new Vector2(0,0);
	private float speedTouch0 = 0.0F;
	private float speedTouch1 = 0.0F;*/
	
	// Use this for initialization
	void Start () {
		prevMousePos = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {

		Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * 200f;
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);
		
		/*if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved) { //we're zooming
			curDist = Input.GetTouch(0).position - Input.GetTouch(1).position; //current distance between finger touches
			prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition)); //difference in previous locations using delta positions
			touchDelta = curDist.magnitude - prevDist.magnitude;
			speedTouch0 = Input.GetTouch(0).deltaPosition.magnitude / Input.GetTouch(0).deltaTime;
			speedTouch1 = Input.GetTouch(1).deltaPosition.magnitude / Input.GetTouch(1).deltaTime;
			
			
			if ((touchDelta + varianceInDistances <= 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
			{
				
				Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView + (1 * speed),15,90);
			}
			
			if ((touchDelta +varianceInDistances > 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
			{
				
				Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - (1 * speed),15,90);
			}

			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)), 5 * Time.deltaTime);
			currMousePos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);
			prevState = 1;
		}*/

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
			
			// If the camera is orthographic...
			if (Camera.main.orthographic)
			{
				// ... change the orthographic size based on the change in distance between the touches.
				Camera.main.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
				
				// Make sure the orthographic size never drops below zero.
				//Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize, 0.1f);
				Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);
			}
			else
			{
				// Otherwise change the field of view based on the change in distance between the touches.
				Camera.main.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;
				
				// Clamp the field of view to make sure it's between 0 and 180.
				Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 0.1f, 179.9f);
			}
		}

		if (Input.GetMouseButton (1)) { // we're moving
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)), 5 * Time.deltaTime);
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
