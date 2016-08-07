using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class MenuBehaviour : MonoBehaviour {
    public Text mpPlayerText;
    public InputField nameInput;

    private Animator animator;

    public Button playerColBtn;
    private Image playerColBtnCol;
    private int ColorChoice = 0;

    public Text ColorText;

    public string PlayerName = "PlayerName";
    public Color PlayerColor = new Color(0, 255, 255);

    public Text VersionTxt;
    public Text OpenRoomsTxt;
    public Text NumPlayersTxt;

    public RectTransform LobbyPanel;

    private ulong matchID;

    public GameObject LobbyPlayerObj;

    private List<LobbyPlayer> lobbyPlayers;

	// Use this for initialization
	void Start () {
        PlayerName = PlayerPrefs.GetString(SaveData.PlayerName.ToString(), PlayerName);
        PlayerColor = PlayerPrefsX.GetColor(SaveData.PlayerColor.ToString(), PlayerColor);
        playerColBtnCol = playerColBtn.GetComponent<Image>();

        animator = gameObject.GetComponent<Animator>();

        ColorChoice = ReturnIndexFromColorChoice(PlayerColor);
        playerColBtnCol.color = PlayerColor;
        ColorBlock newBlock = playerColBtn.colors;
        newBlock.normalColor = ReturnColorChoice((ColorChooser)ColorChoice);
        playerColBtn.colors = newBlock;

        lobbyPlayers = new List<LobbyPlayer>();
	}

    void Awake()
    {
        if (VersionTxt != null)
        {
            VersionTxt.text = string.Format("Version: {0}", Application.version);
        }
    }

    public void UpdateRoomsText()
    {
        if (OpenRoomsTxt != null)
        {
            OpenRoomsTxt.text = string.Format("Open Rooms: {0}", PhotonNetwork.countOfRooms);
            NumPlayersTxt.text = string.Format("# of Players: {0}", PhotonNetwork.countOfPlayers);
        }
    }

    public void ChangeToMultiMenu()
    {
        PhotonNetwork.ConnectUsingSettings("v4.2");
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.playerName = PlayerPrefs.GetString(SaveData.PlayerName.ToString(), PlayerName);
        Color pColor = PlayerPrefsX.GetColor(SaveData.PlayerColor.ToString(), PlayerColor);

        ExitGames.Client.Photon.Hashtable propTable = new ExitGames.Client.Photon.Hashtable();
        propTable.Add("PlayerColor", new Vector3(pColor.r, pColor.g, pColor.b));

        PhotonNetwork.SetPlayerCustomProperties(propTable);

        InvokeRepeating("UpdateRoomsText", 0f, 5f);
    }

    public void OnConnectedToMaster()
    {
        SetTextVisibility(false);
        animator.SetTrigger("MultiplayerPressed");
    }

    public void ChangeToMainMenuFromMulti()
    {
        PhotonNetwork.Disconnect();
    }

    public void OnDisconnectedFromPhoton()
    {
        GameGridBehaviour.isMP = false;
        lobbyPlayers.Clear();

        SetTextVisibility(false);
        animator.SetTrigger("BackPressed");
    }

    public void ChangeToMainMenuFromOpt()
    {
        PlayerPrefs.Save();
        animator.SetTrigger("BackPressed");
    }

    public void ChangeToOptionsMenu()
    {
        nameInput.text = PlayerName;
        animator.SetTrigger("OptionsPressed");
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom("PvPMatch", new RoomOptions() { isOpen = true, isVisible = true, maxPlayers = 2 }, TypedLobby.Default);
        GameGridBehaviour.isMP = true;

        ExitGames.Client.Photon.Hashtable propTable = new ExitGames.Client.Photon.Hashtable();
        propTable.Add("PlayerOrder", 0);

        PhotonNetwork.SetPlayerCustomProperties(propTable);

        SetTextStatus(TextStatus.WaitingForPlayer);
        SetTextVisibility(true);
    }

    public void LookForMatch()
    {
        PhotonNetwork.JoinRandomRoom(null, 0, ExitGames.Client.Photon.MatchmakingMode.RandomMatching, TypedLobby.Default, "");
        GameGridBehaviour.isMP = true;

        ExitGames.Client.Photon.Hashtable propTable = new ExitGames.Client.Photon.Hashtable();
        propTable.Add("PlayerOrder", 1);

        PhotonNetwork.SetPlayerCustomProperties(propTable);

        SetTextStatus(TextStatus.SearchingForGame);
        SetTextVisibility(true);
    }

    [PunRPC]
    void SwitchReadyState(int lpindex, int photonViewId)
    {
        lobbyPlayers[lpindex].ChangeReady();

        //set the photonview
        PhotonView[] nViews = gameObject.GetComponentsInChildren<PhotonView>();
        nViews[0].viewID = photonViewId;

        if (PhotonNetwork.player == lobbyPlayers[0].player)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("CheckIfBothReady", PhotonTargets.AllBuffered, photonViewId);
        }
    }

    [PunRPC]
    void CheckIfBothReady(int photonViewId)
    {
        if (lobbyPlayers.Count > 1 && lobbyPlayers.All(lP => lP.isReady))
            StartMPGame();

        //set the photonview
        PhotonView[] nViews = gameObject.GetComponentsInChildren<PhotonView>();
        nViews[0].viewID = photonViewId;
    }

    [PunRPC]
    void CreateLobbyPlayer(PhotonPlayer player, int photonViewId)
    {
        //Instantiate the object over the network
        GameObject lobbyPlayer = Instantiate(Resources.Load("LobbyPlayer"), Vector3.zero, Quaternion.identity) as GameObject;

        //add the lobby player to the panel, set the position, and add the click handler for the ready state
        LobbyPlayer lP = lobbyPlayer.GetComponent<LobbyPlayer>();
        lobbyPlayer.transform.SetParent(LobbyPanel);
        lP.player = player;

        lobbyPlayer.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0f, -40f - (100f  * lobbyPlayers.Count), 0f);

        if (player == PhotonNetwork.player)
        {
            lobbyPlayer.GetComponentInChildren<Button>().onClick.AddListener(() => PlayerChangeState(lP));
            //Debug.Log("Child count: " + lobbyPlayer.transform.childCount);
            //lobbyPlayer.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => PlayerChangeColor(lP));
        }

        lobbyPlayers.Add(lP);
        lP.playerNameText.text = player.name;
        Vector3 pCol = (Vector3)player.customProperties["PlayerColor"];
        lP.playerColorBtn.GetComponent<Image>().color = new Color(pCol.x, pCol.y, pCol.z);

        //set the photonview
        PhotonView[] nViews = gameObject.GetComponentsInChildren<PhotonView>();
        nViews[0].viewID = photonViewId;

        lobbyPlayers = lobbyPlayers.OrderBy(x => (int)x.player.customProperties["PlayerOrder"]).ToList();
    }

    public void OnJoinedRoom()
    {
        if (PhotonNetwork.playerList.Length < 2)
        {
            SetTextStatus(TextStatus.WaitingForPlayer);
        }
        else
        {
            SetTextStatus(TextStatus.PlayerFound);
        }
    }

    public void OnCreatedRoom()
    {
        int id1 = PhotonNetwork.AllocateViewID();
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("CreateLobbyPlayer", PhotonTargets.AllBuffered, PhotonNetwork.player, id1);
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        int id1 = PhotonNetwork.AllocateViewID();
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("CreateLobbyPlayer", PhotonTargets.AllBuffered, player, id1);
    }

    public void WaitingForHost()
    {
        SetTextStatus(TextStatus.WaitingForHost);
    }

    private void StartMPGame()
    {
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.LoadLevel("MainScreen");
    }  

	private void StartGame() {
        GameGridBehaviour.isMP = false;
        SceneManager.LoadScene ("MainScreen");
	}

    public void PlayerChangeState(LobbyPlayer player)
    {
        int id1 = PhotonNetwork.AllocateViewID();
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SwitchReadyState", PhotonTargets.AllBuffered, lobbyPlayers.IndexOf(player), id1);

        
    }

    public void PlayerChangeColor(LobbyPlayer player)
    {
        int id1 = PhotonNetwork.AllocateViewID();
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SwitchReadyState", PhotonTargets.AllBuffered, player, id1);
    }

    public void QuitGame() {
		Application.Quit ();
	}

    public void SetPlayerName()
    {
        PlayerName = nameInput.text;
        PlayerPrefs.SetString(SaveData.PlayerName.ToString(), PlayerName);
    }

    public void PlayerColorPressed()
    {
        ColorChoice += 1;

        if (ColorChoice > 8)
            ColorChoice = 0;

        playerColBtnCol.color = ReturnColorChoice((ColorChooser)ColorChoice);
        ColorBlock newBlock = playerColBtn.colors;
        newBlock.normalColor = ReturnColorChoice((ColorChooser)ColorChoice);
        playerColBtn.colors = newBlock;
    }

    public void SetPlayerColor()
    {
        PlayerColor = playerColBtn.colors.normalColor;
        PlayerPrefsX.SetColor(SaveData.PlayerColor.ToString(), PlayerColor);
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
        else if (mpStatus == TextStatus.HostFound)
            mpPlayerText.text = "Match Found. Connecting...";
        else if (mpStatus == TextStatus.WaitingForHost) //player connected - client end
            mpPlayerText.text = "Waiting for Host to Start";
        else
            mpPlayerText.text = "Error: function not defined";
    }

    private Color ReturnColorChoice(ColorChooser choice)
    {
        if (choice.Equals(ColorChooser.Aqua))
            return new Color32(0, 230, 230, 255);
        else if (choice.Equals(ColorChooser.Pink))
            return new Color32(230, 0, 230, 255);
        else if (choice.Equals(ColorChooser.Green))
            return new Color32(0, 230, 0, 255);
        else if (choice.Equals(ColorChooser.Yellow))
            return new Color32(230, 230, 0, 255);
        else if (choice.Equals(ColorChooser.Red))
            return new Color32(230, 0, 0, 255);
        else if (choice.Equals(ColorChooser.DarkBlue))
            return new Color32(0, 0, 230, 255);
        else if (choice.Equals(ColorChooser.Purple))
            return new Color32(121, 32, 230, 255);
        else if (choice.Equals(ColorChooser.Magenta))
            return new Color32(233, 9, 135, 255);
        else if (choice.Equals(ColorChooser.Orange))
            return new Color32(230, 51, 0, 255);
        else
            return new Color32(0, 255, 255, 255);
    }

    private int ReturnIndexFromColorChoice(Color choice)
    {
        if (choice.Equals(new Color32(0, 255, 255, 255)))
            return (int)ColorChooser.Aqua;
        else if (choice.Equals(new Color32(0, 255, 255, 255)))
            return (int)ColorChooser.Pink;
        else if (choice.Equals(new Color32(0, 255, 0, 255)))
            return (int)ColorChooser.Green;
        else if (choice.Equals(new Color32(255, 255, 0, 255)))
            return (int)ColorChooser.Yellow;
        else if (choice.Equals(new Color32(255, 0, 0, 255)))
            return (int)ColorChooser.Red;
        else if (choice.Equals(new Color32(0, 0, 255, 255)))
            return (int)ColorChooser.DarkBlue;
        else if (choice.Equals(new Color32(121, 32, 255, 255)))
            return (int)ColorChooser.Purple;
        else if (choice.Equals(new Color32(253, 9, 135, 255)))
            return (int)ColorChooser.Magenta;
        else if (choice.Equals(new Color32(255, 51, 0, 255)))
            return (int)ColorChooser.Orange;
        else
            return (int)ColorChooser.Aqua;
    }
}
public enum TextStatus
{
    SearchingForGame = 0,
    WaitingForPlayer = 1,
    PlayerFound = 2,
    HostFound = 3,
    WaitingForHost = 4,
}

public enum SaveData
{
    PlayerName,
    PlayerColor,
}

public enum ColorChooser
{
    Aqua,
    Pink,
    Green,
    Yellow,
    Red,
    DarkBlue,
    Purple,
    Magenta,
    Orange,
}
