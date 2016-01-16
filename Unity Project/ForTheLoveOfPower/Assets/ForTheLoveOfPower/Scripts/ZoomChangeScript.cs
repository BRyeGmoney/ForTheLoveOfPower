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

	public void ChangeZoom()
    {
        if (Camera.main.name == "Action Camera")
        {
            zoomButtonAnim.SetTrigger("ToTactical");
            StartCoroutine(Wait());
        }
        else if (Camera.main.name == "Tactical Camera")
        {
            actionView.transform.position = actionView.transform.position;
            actionView.gameObject.SetActive(true);
            tacticalView.gameObject.SetActive(false);
            uiCanvas.worldCamera = actionView;
            buttonImage.sprite = viewSprites[1];
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.16f);
        tacticalView.transform.position = actionView.transform.position;
        tacticalView.gameObject.SetActive(true);
        actionView.gameObject.SetActive(false);
        uiCanvas.worldCamera = tacticalView;
        buttonImage.sprite = viewSprites[0];
    }
}
