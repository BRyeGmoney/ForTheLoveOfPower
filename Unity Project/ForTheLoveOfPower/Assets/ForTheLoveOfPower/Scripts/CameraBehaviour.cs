using UnityEngine;
using UnityEngine.UI;
using System;

public class CameraBehaviour : MonoBehaviour {
	Vector3 prevMousePos;
	Vector3 currMousePos;
	Vector3 cameraMovement;
    //byte prevState = 0;

    //Scrolling
    public int dragMultiply = 20;
    public float moveSensitivity = 1.0f;
    public bool invertMoveX = false;
    public bool invertMoveY = false;
    public float inertiaDuration = 1.0f;
    private float scrollVelocity = 0.0f;
    private float timeTouchPhaseEnded;
    //private bool timeTouchPhaseEndRecorded = false;
    private Vector2 scrollDirection = Vector2.zero;

    public float orthoZoomSpeed = 2f;        // The rate of change of the orthographic size in orthographic mode.
	
	// Use this for initialization
	void Start () {
		prevMousePos = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        if (Camera.main.name == "Action Camera")
        {
            // If there are two touches on the device...
            /*if (Input.touchCount == 2)
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

                if (touchDeltaMag > 300)
                {
                    // ... change the orthographic size based on the change in distance between the touches.
                    Camera.main.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                    // Make sure the orthographic size never drops below zero.
                    //Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize, 0.1f);
                    Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 200, 1000);

                    moveSensitivity = Camera.main.orthographicSize / 5.0f;
                }
                else
                {
                    //Camera.main.transform.parent.transform.Translate(-touchZero.deltaPosition.x * 5f, -touchZero.deltaPosition.y * 5f, 0);
                    //touch scrolololol
                    if (touchZero.phase == TouchPhase.Began)
                    {
                        scrollVelocity = 0.0f;
                        //timeTouchPhaseEndRecorded = false;
                    }
                    else if (touchZero.phase == TouchPhase.Moved && touchOne.phase == TouchPhase.Moved)
                    {
                        Vector2 delta = touchZero.deltaPosition;

                        float positionX = delta.x * moveSensitivity * Time.deltaTime;
                        positionX = invertMoveX ? positionX : positionX * -1;

                        float positionY = delta.y * moveSensitivity * Time.deltaTime;
                        positionY = invertMoveY ? positionY : positionY * -1;

                        Camera.main.transform.position += new Vector3(positionX, positionY, 0);

                        scrollDirection = touchZero.deltaPosition.normalized;
                        scrollVelocity = touchZero.deltaPosition.magnitude / touchZero.deltaTime;


                        if (scrollVelocity <= 100)
                            scrollVelocity = 0;
                    }
                }
                if (touchZero.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Ended)
                {
                    timeTouchPhaseEnded = Time.time;
                }
            }
            else
            {
                //if the camera is currently scrolling
                if (scrollVelocity != 0.0f)
                {
                    if (!timeTouchPhaseEndRecorded)
                    {
                        timeTouchPhaseEnded = Time.time;
                        timeTouchPhaseEndRecorded = true;
                    }

                    //slow down over time
                    float t = (Time.time - timeTouchPhaseEnded) / inertiaDuration;
                    float frameVelocity = Mathf.Lerp(scrollVelocity, 0.0f, t);
                    Camera.main.transform.position += -(Vector3)scrollDirection.normalized * (frameVelocity * 0.05f) * Time.deltaTime;
                    if (t >= 1.0f)
                        scrollVelocity = 0.0f;
                }
            }*/

            if (Input.touchSupported)
            {
                if (Input.touchCount == 2)
                {
                    Touch touchOne = Input.GetTouch(0);

                    float drag = dragMultiply * (Camera.main.orthographicSize / 800);
                    cameraMovement = new Vector3(-(touchOne.deltaPosition.x), -(touchOne.deltaPosition.y), 0);
                    Camera.main.transform.position += cameraMovement;
                }
                else //if none or even one
                {

                }
            }
            else if (!Input.touchSupported)
            {
                Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * 200f;
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 200, 1000);

                if (Input.GetMouseButton(1))
                { // we're moving
                    float drag = dragMultiply * (Camera.main.orthographicSize / 800);
                    cameraMovement = new Vector3(-Input.GetAxis("Mouse X") * drag, -Input.GetAxis("Mouse Y") * drag, 0);
                    Camera.main.transform.position += cameraMovement;
                    //Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)), 5 * Time.deltaTime);
                    //currMousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                    //prevState = 1;
                }
                else
                {
                    //if (prevState == 1)
                    //    cameraMovement = new Vector3(prevMousePos.x - currMousePos.x, prevMousePos.y - currMousePos.y, 0);

                    //if (Mathf.Abs(cameraMovement.x) > 0.1f || Mathf.Abs(cameraMovement.y) > 0.1f)
                    //{
                    //    Camera.main.transform.position += cameraMovement;
                    //    cameraMovement *= 0.9f;
                    //}

                    //prevState = 0;
                }

                prevMousePos = currMousePos;
            }
        }
	}
}
