using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class MenuBehaviour : MonoBehaviour {

    public Text mpPlayerText;

    private Animator animator;

    public GameObject PlayerPrefab;
    public GameObject AIPlayerPrefab;

	// Use this for initialization
	void Start () {
        animator = gameObject.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LookForMatch()
    {
        SetTextStatus(TextStatus.SearchingForGame);
        SetTextVisibility(true);
    }

    public void ChangeToMultiMenu()
    {
        //SetTextVisibility(false);
        //animator.SetTrigger("MultiplayerPressed");

    }

    public void ChangeToMainMenu()
    {
        SetTextVisibility(false);
        animator.SetTrigger("BackPressed");
    }

    public void CreateRoom()
    {
        SetTextStatus(TextStatus.WaitingForPlayer);
        SetTextVisibility(true);
        
    }

    private void SetTextVisibility(bool isVisible)
    {
        if (isVisible)
        {
            mpPlayerText.color = new Color(mpPlayerText.color.r, mpPlayerText.color.g, mpPlayerText.color.b, 1f);
            animator.SetBool("MPSearchStatusFluctuate", true);
        }
        else
        {
            mpPlayerText.color = new Color(mpPlayerText.color.r, mpPlayerText.color.g, mpPlayerText.color.b, 0f);
            animator.SetBool("MPSearchStatusFluctuate", false);
        }
    }

    private void SetTextStatus(TextStatus mpStatus)
    {
        if (mpStatus == TextStatus.SearchingForGame)
            mpPlayerText.text = "Searching For Game";
        else if (mpStatus == TextStatus.WaitingForPlayer)
            mpPlayerText.text = "Waiting For Player...";
        else if (mpStatus == TextStatus.PlayerFound) //player connected - host end
            mpPlayerText.text = "Player found! Ready";
        else if (mpStatus == TextStatus.HostFound) //player connected - client end
            mpPlayerText.text = "Waiting for Host to Start";
        else
            mpPlayerText.text = "Error: function not defined";
    }

	public void StartGame() {
        GameObject mainPlayer = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        GameObject aiPlayer = Instantiate(AIPlayerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        DontDestroyOnLoad(mainPlayer);
        DontDestroyOnLoad(aiPlayer);

		Application.LoadLevel ("MainScreen");
	}

	public void QuitGame() {
		Application.Quit ();
	}
}
public enum TextStatus
{
    SearchingForGame = 0,
    WaitingForPlayer = 1,
    PlayerFound = 2,
    HostFound = 3,
}
