using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ZoomChangeScript : MonoBehaviour {

    public Camera actionView;
    public Camera tacticalView;
    public Camera statsView;
    public Canvas uiCanvas;
    public Animator zoomButtonAnim;
    public Image buttonImage;
    public Sprite[] viewSprites;

    private BloomPro actionBloom;
    private BloomPro tacticalBloom;

    public void Start()
    {
        actionBloom = actionView.GetComponent<BloomPro>();
        tacticalBloom = tacticalView.GetComponent<BloomPro>();
    }

	public void ChangeZoom()
    {
        if (Camera.main.name == "Action Camera")
        {
            GameGridBehaviour.instance.curCameraState = CameraState.Tactical;

            zoomButtonAnim.SetTrigger("ToTactical");
            //StartCoroutine(Wait());
            Wait();

            actionBloom.enabled = false;
            tacticalBloom.enabled = true;
        }
        else if (Camera.main.name == "Tactical Camera")
        {
            GameGridBehaviour.instance.curCameraState = CameraState.Action;

            actionView.transform.position = tacticalView.transform.position;
            actionView.gameObject.SetActive(true);
            tacticalView.gameObject.SetActive(false);
            uiCanvas.worldCamera = actionView;
            buttonImage.sprite = viewSprites[1];

            tacticalBloom.enabled = false;
            actionBloom.enabled = true;
        }
    }

    private void Wait()
    {
        //yield return new WaitForSeconds(0.16f);
        tacticalView.transform.position = actionView.transform.position;
        tacticalView.gameObject.SetActive(true);
        actionView.gameObject.SetActive(false);
        uiCanvas.worldCamera = tacticalView;
        buttonImage.sprite = viewSprites[0];
    }
}
