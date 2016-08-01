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

    Vector2 touchStartPosition;
    Vector2 lastPosition;
    Vector2 deltaVector;
    Vector2 cameraVelocity;
    float cameraSpeed = 0.1807f;
    float cameraFriction = 0.0201f;

    // Use this for initialization
    void Start () {
		prevMousePos = Vector3.zero;

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS)
        cameraSpeed = 0.5f;
        Debug.Log("Camera Speed:" + cameraSpeed);
#else
        cameraSpeed = 0.8f;
#endif
    }

    void MobileActionCamera()
    {
        
    }

    void MobileTacticalCamera()
    {

    }

    void MobileInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touchOne = Input.GetTouch(0);

            if (Input.touchCount == 2 && GameGridBehaviour.instance.curCameraState == CameraState.Action)
            {
                Touch touchTwo = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchOne.position - touchOne.deltaPosition;
                Vector2 touchOnePrevPos = touchTwo.position - touchTwo.deltaPosition;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchOne.position - touchTwo.position).magnitude;

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                // ... change the orthographic size based on the change in distance between the touches.
                Camera.main.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Make sure the orthographic size never drops below zero.
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 800, 1350); //Mathf.Max(Camera.main.orthographicSize, 0.1f);
            }
            else
            {
                if (touchOne.phase == TouchPhase.Began)
                {
                    touchStartPosition = lastPosition = touchOne.position;
                }
                else if (touchOne.phase == TouchPhase.Stationary)
                {

                }
                else if (touchOne.phase == TouchPhase.Moved)
                {
                    deltaVector = touchOne.position - lastPosition;
                    cameraVelocity = deltaVector * cameraSpeed;
                    lastPosition = touchOne.position;
                }
            }
        }

        Camera.main.transform.position += (Vector3)cameraVelocity;
        cameraVelocity *= Mathf.Pow(cameraFriction, Time.deltaTime);
    }

    void MouseInput()
    {
        if (GameGridBehaviour.instance.curCameraState == CameraState.Action)
        {
            Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * 600f;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 800, 1350);
        }

        if (Input.GetMouseButtonDown(1))
        {
            lastPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1))
        { // we're moving
            /*float drag = dragMultiply * (Camera.main.orthographicSize / 800);
            cameraMovement = new Vector3(-Input.GetAxis("Mouse X") * drag, -Input.GetAxis("Mouse Y") * drag, 0);
            Camera.main.transform.position += cameraMovement;*/

            deltaVector = (Vector2)Input.mousePosition - lastPosition;
            cameraVelocity = deltaVector * cameraSpeed;
            Debug.Log(cameraVelocity);

            lastPosition = Input.mousePosition;
        }

        Camera.main.transform.position += (Vector3)cameraVelocity;
        cameraVelocity *= Mathf.Pow(0.2f, Time.deltaTime);

        if (Input.GetMouseButton(2))
        {
            float percX = Input.mousePosition.x / Screen.width;
            float percY = Input.mousePosition.y / Screen.height;
            float drag = dragMultiply * (Camera.main.orthographicSize / 800);

            if (percX > 0.8f)
            {
                cameraMovement.x = (percX - 0.8f) * drag;
            }
            else if (percX < 0.2f)
            {
                cameraMovement.x = (-0.2f + percX) * drag;
            }
            else
                cameraMovement.x = 0f;

            if (percY > 0.8f)
            {
                cameraMovement.y = (percY - 0.8f) * drag;
            }
            else if (percY < 0.2f)
            {
                cameraMovement.y = (-0.2f + percY) * drag;
            }
            else
                cameraMovement.y = 0;

            Camera.main.transform.position += cameraMovement;
        }

        prevMousePos = currMousePos;
    }
	
	// Update is called once per frame
	void Update () {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS)
        MobileInput();
#else
        MouseInput();
#endif
	}
}
