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
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("PlayerType");
        int amountToDestroy = playerObjects.Length;

        for (int x = 0; x < amountToDestroy; x++)
            Destroy(playerObjects[x]);

        Application.LoadLevel ("MainMenuScreen");
	}

	public void ExitGame()
	{
		Application.Quit ();
	}
}
