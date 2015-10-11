using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class MenuBehaviour : MonoBehaviour {
    public static MenuBehaviour instance = null;
    public Text mpPlayerText;
    public InputField nameInput;

    private Animator animator;

    public GameObject PlayerPrefab;
    public GameObject AIPlayerPrefab;
    public MenuNetworkLobbyManager lobbyManager;

    //public RectTransform LobbyPlayersPanel;
    VerticalLayoutGroup layout;

    public string PlayerName = "PlayerName";
    private ulong matchID;

	// Use this for initialization
	void Start () {
        animator = gameObject.GetComponent<Animator>();
        //layout = LobbyPlayersPanel.GetComponent<VerticalLayoutGroup>();
        instance = this;
	}

    void Update()
    {
        //this dirty the layout to force it to recompute evryframe (a sync problem between client/server
        //sometime to child being assigned before layout was enabled/init, leading to broken layouting)
        //if (layout)
         //   layout.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
    }

    public void LookForMatch()
    {
        SetTextStatus(TextStatus.SearchingForGame);
        SetTextVisibility(true);

        lobbyManager.StartMatchMaker();
        lobbyManager.matchMaker.ListMatches(0, 10, "", ConnectToMatchOne);
    }

    private void ConnectToMatchOne(UnityEngine.Networking.Match.ListMatchResponse matchResponses)
    {
        if (matchResponses.matches.Count > 0 && System.Convert.ToUInt64(matchResponses.matches[0].networkId) != matchID)
        {
            UnityEngine.Networking.Match.MatchDesc match = matchResponses.matches[0];
            
            lobbyManager.StartClient();
            lobbyManager.matchMaker.JoinMatch(match.networkId, "", OnJoinedMatch);
        }
    }

    private void OnJoinedMatch(UnityEngine.Networking.Match.JoinMatchResponse joinedMatchResp)
    {
        SetTextStatus(TextStatus.HostFound);
    }

    public void ChangeToMultiMenu()
    {
        SetTextVisibility(false);
        animator.SetTrigger("MultiplayerPressed");
    }

    public void ChangeToMainMenuFromMulti()
    {
        if (lobbyManager.matchMaker != null)
            lobbyManager.matchMaker.DestroyMatch((UnityEngine.Networking.Types.NetworkID)matchID, OnMatchDestroyed);

        SetTextVisibility(false);
        animator.SetTrigger("BackPressed");
    }

    public void ChangeToMainMenuFromOpt()
    {
        animator.SetTrigger("BackPressed");
    }

    public void ChangeToOptionsMenu()
    {
        nameInput.text = PlayerName;
        animator.SetTrigger("OptionsPressed");
    }

    public void CreateRoom()
    {
        SetTextStatus(TextStatus.WaitingForPlayer);
        SetTextVisibility(true);

        lobbyManager.serverBindToIP = true;
        lobbyManager.serverBindAddress = Network.player.externalIP;
        lobbyManager.StartMatchMaker();
        lobbyManager.matchMaker.CreateMatch(
            PlayerName,
            (uint)lobbyManager.maxPlayers,
            true,
            "",
            OnMatchCreate);
    }

    public void OnMatchCreate(UnityEngine.Networking.Match.CreateMatchResponse matchInfo)
    {
        lobbyManager.OnMatchCreate(matchInfo);
        
        matchID = (System.UInt64)matchInfo.networkId;
    }

    private void OnMatchDestroyed(UnityEngine.Networking.Match.BasicResponse baseResponse)
    {
        matchID = (System.UInt64)UnityEngine.Networking.Types.NetworkID.Invalid;
        lobbyManager.StopMatchMaker();
        lobbyManager.StopHost();
    }

    public void StartMPGame()
    {
        lobbyManager.CheckReadyToBegin();
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

    public void SetPlayerName()
    {
        PlayerName = nameInput.text;
    }
}
public enum TextStatus
{
    SearchingForGame = 0,
    WaitingForPlayer = 1,
    PlayerFound = 2,
    HostFound = 3,
}
