using UnityEngine;
using System.Collections;

public class MenuBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StartGame() {
		Application.LoadLevel ("MainScreen");
	}

	public void ConnectToIP() {

	}

	public void QuitGame() {
		Application.Quit ();
	}
}
