using System;
using UnityEngine;
using UnityEngine.UI;
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
    public MenuNetworkLobbyManager lobbyManager;

    public Sprite townHall;
    public Sprite cityHall;
    public Sprite capital;

    //private Sprite colorPicker;
    //public GameObject ThePicker;
    public Button playerColBtn;
    private Image playerColBtnCol;
    private int ColorChoice = 0;
    //private bool showColorPicker;
    //private int ImageWidth = 100;
    //private int ImageHeight = 100;
    //private Vector2 ColorPickerPos;

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
        //colorPicker = ThePicker.GetComponent<SpriteRenderer>().sprite;
        //ImageWidth = (int)colorPicker.bounds.extents.x;
        //ImageHeight = (int)colorPicker.bounds.extents.y;
        //ColorPickerPos = new Vector2(ThePicker.transform.position.x, ThePicker.transform.position.y);


        instance = this;
	}

    void Update()
    {
        /*if (showColorPicker && Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            Vector2 pickpos = Vector2.zero;
            int a, b;

            if (Input.touchCount > 0)
            {
                pickpos = Input.GetTouch(0).position;
            }
            else if (Input.GetMouseButton(0))
            {
                pickpos = Input.mousePosition;
            }
            a = Convert.ToInt32(pickpos.x + 60);
            b = Convert.ToInt32(pickpos.y - 270);

            ColorBlock newBlock = playerColBtn.colors;
            newBlock.normalColor = colorPicker.texture.GetPixel(a, b);
            playerColBtn.colors = newBlock;
            PlayerColor = newBlock.normalColor;
            ColorText.text = String.Format("Color: {0}, {1}, {2}", PlayerColor.r, PlayerColor.g, PlayerColor.b);
        }*/
    }

    public void LookForMatch()
    {
        isMPGame = true;
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
            lobbyManager.matchName = match.name;
            lobbyManager.matchSize = (uint)match.maxSize;
            lobbyManager.matchMaker.JoinMatch(match.networkId, "", OnJoinedMatch);
        }
    }

    private void OnJoinedMatch(UnityEngine.Networking.Match.JoinMatchResponse joinedMatchResp)
    {
        lobbyManager.OnMatchJoined(joinedMatchResp);
        SetTextStatus(TextStatus.HostFound);
    }

    public void WaitingForHost()
    {
        SetTextStatus(TextStatus.WaitingForHost);
    }

    public void ChangeToMultiMenu()
    {
        SetTextVisibility(false);
        animator.SetTrigger("MultiplayerPressed");
    }

    public void ChangeToMainMenuFromMulti()
    {
        isMPGame = false;

        if (lobbyManager.matchMaker != null)
            lobbyManager.matchMaker.DestroyMatch((UnityEngine.Networking.Types.NetworkID)matchID, OnMatchDestroyed);

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
        isMPGame = true;
        SetTextStatus(TextStatus.WaitingForPlayer);
        SetTextVisibility(true);

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
        foreach (LobbyPlayer lP in lobbyManager.lobbySlots)
        {
            lP.readyToBegin = true;
            lP.SendReadyToBeginMessage();
        }
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
        //showColorPicker = true;
        //ThePicker.SetActive(true);
    }

    public void SetPlayerColor()
    {
        PlayerColor = playerColBtn.colors.normalColor;
        PlayerPrefsX.SetColor(SaveData.PlayerColor.ToString(), PlayerColor);
        //showColorPicker = false;
        //ThePicker.SetActive(false);
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
