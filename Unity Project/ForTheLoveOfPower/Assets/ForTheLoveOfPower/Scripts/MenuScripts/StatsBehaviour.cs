using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatsBehaviour : MonoBehaviour {

	public Text resultText;

	// Use this for initialization
	void Start () {
		if (GameGridBehaviour.didWin) 
			resultText.text = "You Win!";
		else
			resultText.text = "You Lose";

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void BackToMainMenu() 
	{
		Application.LoadLevel ("MainMenuScreen");
	}

	public void ExitGame()
	{
		Application.Quit ();
	}
}
