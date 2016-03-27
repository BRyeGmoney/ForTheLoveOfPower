using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class MenuBehaviour : MonoBehaviour {
    public static MenuBehaviour instance = null;
    public Text mpPlayerText;
    public InputField nameInput;
    public bool isMPGame;

    private Animator animator;

    public GameObject PlayerPrefab;
    public GameObject AIPlayerPrefab;
    public GameObject NetworkPlayerPrefab;

    public Sprite townHall;
    public Sprite cityHall;
    public Sprite capital;

    public Button playerColBtn;
    private Image playerColBtnCol;
    private int ColorChoice = 0;

    public Text ColorText;

    public string PlayerName = "PlayerName";
    public Color PlayerColor = new Color(0, 255, 255);
    private ulong matchID;


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


        instance = this;
	}

    public void ChangeToMultiMenu()
    {
        PhotonNetwork.ConnectUsingSettings("v4.2");
        PhotonNetwork.automaticallySyncScene = true;
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
        isMPGame = false;

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
        isMPGame = true;
        SetTextStatus(TextStatus.WaitingForPlayer);
        SetTextVisibility(true);
    }

    public void LookForMatch()
    {
        PhotonNetwork.JoinRandomRoom(null, 0, ExitGames.Client.Photon.MatchmakingMode.RandomMatching, TypedLobby.Default, "");
        isMPGame = true;
        SetTextStatus(TextStatus.SearchingForGame);
        SetTextVisibility(true);
    }

    public void OnJoinedRoom()
    {
        if (PhotonNetwork.playerList.Length < 2)
            SetTextStatus(TextStatus.WaitingForPlayer);
        else
        {
            SetTextStatus(TextStatus.PlayerFound);
        }
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        if (PhotonNetwork.playerList.Length > 1)
            StartMPGame();
    }

    public void WaitingForHost()
    {
        SetTextStatus(TextStatus.WaitingForHost);
    }

    private void StartMPGame()
    {
        //CreatePlayerObjects(true);
        PhotonNetwork.LoadLevel("MainScreen");
    }  

	private void StartGame() {
        //CreatePlayerObjects(false);

        SceneManager.LoadScene ("MainScreen");
	}

    public void CreatePlayerObjects(bool ismp)
    {
        GameObject PlayerOne;
        GameObject PlayerTwo;
        if (!ismp)
        {
            PlayerOne = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            PlayerTwo = Instantiate(AIPlayerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        }
        else
        {
            PlayerOne = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
            PlayerTwo = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
        }
        //DontDestroyOnLoad(PlayerOne);
        //DontDestroyOnLoad(PlayerTwo);
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
