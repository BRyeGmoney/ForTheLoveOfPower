using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	public float speed = 1f;
	public Text scoreText;

	private Rigidbody playerBall;
	private int score = 0;

	void Start()
	{
		playerBall = GetComponent<Rigidbody> ();
		SetScoreText ();
	}
	
	// FixedUpdate called before any physics calculations
	void FixedUpdate () {
		float moveHorX = Input.GetAxis ("Horizontal");
		float moveHorZ = Input.GetAxis ("Vertical");
		float moveVert = Input.GetKeyDown ("space") ? 20 : 0;

		playerBall.AddForce(new Vector3(moveHorX, moveVert, moveHorZ) * speed);
	}

	void OnTriggerEnter(Collider other) 
	{
		if (other.gameObject.CompareTag ("Pickup")) {
			other.gameObject.SetActive (false);
			score += 1;
			SetScoreText();
		}
	}

	void SetScoreText()
	{
		scoreText.text = "Score: " + score.ToString ();
	}
}
