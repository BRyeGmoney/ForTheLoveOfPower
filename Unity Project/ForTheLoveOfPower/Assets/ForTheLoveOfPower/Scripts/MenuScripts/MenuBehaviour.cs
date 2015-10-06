using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuBehaviour : MonoBehaviour {

    public Text mpPlayerText;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LookForMatch()
    {
        if (mpPlayerText != null)
        {
            mpPlayerText.enabled = true;
        }
    }

	public void StartGame() {
		Application.LoadLevel ("MainScreen");
	}

	public void QuitGame() {
		Application.Quit ();
	}
}
