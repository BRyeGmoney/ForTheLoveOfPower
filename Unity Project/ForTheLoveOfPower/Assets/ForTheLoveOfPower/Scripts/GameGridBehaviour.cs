using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Gamelogic;
using Gamelogic.Grids;
using AssemblyCSharp;
using Vectrosity;
using DG.Tweening;

public class GameGridBehaviour : GridBehaviour<PointyHexPoint> {
    public static GameGridBehaviour instance = null;

    public Rect Viewport { get; set; }

    public static bool didWin;
	public static Color baseFloorColor;
    public static int localPlayer = 0;
    public static bool isMP = false;

	BuildMenuBehaviour buildScreenSettings;

	public Player[] listOfPlayers;
	public Text moneyText;
    public Text PlaceDictText;
    public RectTransform fadePanel;
	public GameObject buildScreen;
    public Text NotEnoughMoneyText;
    public Texture lineTex;

    public Sprite townHall;
    public Sprite cityHall;
    public Sprite capital;

    public Material tileMat;
    public Material finalMat;
    public GameObject tileSprite;
    private bool inited = false;

    float timer = 0f;
    float timer2 = 0f;

    public CameraState curCameraState;

	public List<Combat> listofCurrentCombats;

    public Canvas optionsCanvas;
    private GameState curGameState;

    // Use this for initialization
    void Start () {
		listofCurrentCombats = new List<Combat> ();
        Viewport = new Rect();

        buildScreenSettings = buildScreen.GetComponent<BuildMenuBehaviour>();
        baseFloorColor = GridBuilder.Colors[0];

        curGameState = GameState.LoadState;
        curCameraState = CameraState.Action;
        instance = this;

        if (isMP)
            PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0).GetComponent<Player>().netPlayer = false;
        else
            CreatePlayerObjects(false);

        InformDoneLoading();
    }

    public void CreatePlayerObjects(bool ismp)
    {
        GameObject P1;
        GameObject P2;

        P1 = Instantiate(Resources.Load("Player") as GameObject, Vector3.zero, Quaternion.identity) as GameObject;
        P2 = Instantiate(Resources.Load("AIPlayer") as GameObject, Vector3.zero, Quaternion.identity) as GameObject;

        P1.GetComponent<Player>().netPlayer = false;
        P2.GetComponent<Player>().netPlayer = false;
    }

    public void InformDoneLoading()
    {
        //tileMat.SetFloat(BeautifulDissolves.DissolveHelper.dissolveAmountID, 1f);
        curGameState = GameState.ComeFromLoad;
    }

    private void InitializePlayers(GameObject[] playerObjects)
    {
        listOfPlayers = new Player[playerObjects.Length];

        if (listOfPlayers.Length > 1)
        {
            inited = true;
            int c = 0;
            foreach (GameObject playerObj in playerObjects)
            {
                if (isMP)
                {
                    Player netPlayer = playerObj.GetComponent<Player>();
                    listOfPlayers[c] = netPlayer;

                    if (netPlayer.netPlayer)
                        netPlayer.SetPlayerColor();

                    //if (listOfPlayers[c])
                        localPlayer = 0;
                }
                else
                {
                    AIPlayer aiPlayer = playerObj.GetComponent<AIPlayer>();
                    if (aiPlayer != null)
                    {
                        listOfPlayers[c] = aiPlayer;
                    }
                    else
                        listOfPlayers[c] = playerObj.GetComponent<Player>();
                }

                c++;
            }

            buildScreenSettings.SetBGColor(listOfPlayers[localPlayer].PlayerColor);

            moneyText.color = listOfPlayers[localPlayer].PlayerColor;
            curGameState = GameState.PlayerSetupState;

            Animator anim = PlaceDictText.GetComponent<Animator>();
            anim.SetTrigger("PlaceDictTrigger");
        }
    }

    

	// Update is called once per frame
	void Update () {
        if (curGameState.Equals(GameState.RegGameState))
            UpdatePlayGameState();
        else if (curGameState.Equals(GameState.InitState))
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerType");
            if (players.Length >= 2 && !inited)
                InitializePlayers(players);
        }
        else if (curGameState.Equals(GameState.ComeFromLoad))
        {
            curGameState = GameState.CreateBoardState;
            ChangeToInitState();
            //DOTween.To(() => timer2, x => timer2 = x, 1, 1).OnComplete(ChangeToBoardState);
            //DOTween.To(UpdateTileTween, 1f, 0f, 3f).SetDelay(1f).OnComplete(ChangeToInitState);
        }
        else if (curGameState.Equals(GameState.PlayerSetupState))
            PlayerSetupState();
        else if (curGameState.Equals(GameState.EndGameState))
            EndGameState();

	}

    void ChangeToBoardState()
    {
        DOTween.To(UpdateTileTween, 1f, 0f, 3f).SetDelay(1f).OnComplete(ChangeToInitState);
    }

    void UpdateTileTween(float amount)
    {
        tileMat.SetFloat(BeautifulDissolves.DissolveHelper.dissolveAmountID, amount);
    }

    void ChangeToInitState()
    {
        curGameState = GameState.InitState;
        SwitchTileMaterial();
    }

    void FixedUpdate()
    {
        float height = 2f * Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;

        Viewport = new Rect(new Vector2(Camera.main.transform.position.x - width / 2, Camera.main.transform.position.y - height / 2), new Vector2(width, height));
    }

    private void PlayerSetupState()
    {
        if (listOfPlayers[0].playerArmy.GetUnitCount() > 0 && listOfPlayers[1].playerArmy.GetUnitCount() > 0)
            curGameState = GameState.RegGameState;
    }

    private void UpdatePlayGameState()
    {
        CheckEndGame();

        UpdateGame();

        //update our timer
        timer += Time.deltaTime;
    }

    private void UpdateGame()
    {
        //Update All the Fights Going On
        listofCurrentCombats.ForEach(fight => {
            fight.Update();

            if (fight.fightOver)
            {
                listofCurrentCombats.Remove(fight);
                fight = null;
            }
        });
    }

	private void CheckEndGame ()
	{
		if (!listOfPlayers[0].DictatorAlive || !listOfPlayers[1].DictatorAlive) {

			if (!listOfPlayers[0].DictatorAlive)
				GameGridBehaviour.didWin = false;
			else
				GameGridBehaviour.didWin = true;

            timer = 0f;
            fadePanel.gameObject.SetActive(true);
            curGameState = GameState.EndGameState;
		}
	}

    private void EndGameState()
    {
        fadePanel.GetComponent<Animator>().SetTrigger("FadeToBlack");
        timer += Time.deltaTime;

        if (timer > 1.2f)
            SceneManager.LoadScene("StatsScreen");
    }

	void SwitchTileMaterial()
    {
        tileSprite.GetComponent<Renderer>().material = finalMat;
    }

	/// <summary>
	/// We need to know which settlement a building belongs to. and since buildings HAVE to be connected by
	/// atleast a single other building, and from inception be told what their owning settlement is, we can
	/// pull the data from any neighbor of the same colour and not just a settlement
	/// </summary>
	/// <param name="pointToSearchFrom">Point to search from.</param>
	/// <param name="owningPlayerColor">Owning player color.</param>
	public Settlement FindOwningSettlement(PointyHexPoint pointToSearchFrom, PointList<PointyHexPoint> surroundingTiles, Color owningPlayerColor)
	{
		foreach (PointyHexPoint point in surroundingTiles) {
			UnitCell currCell = (Grid[point] as UnitCell);
			if (currCell.buildingOnTile && currCell.structureOnTile.StructColor.Equals (owningPlayerColor)) {
				if (currCell.structureOnTile.StructureType.Equals (StructureUnitType.Settlement))
					return currCell.structureOnTile as Settlement;
				else
					return currCell.structureOnTile.OwningSettlement;
			}
		}

		return null;
	}

	public bool IsBuildingInArea(PointyHexPoint pointToSearchFrom, PointList<PointyHexPoint> surroundingTiles, Color owningPlayerColor)
	{
		foreach (PointyHexPoint point in surroundingTiles) {
			UnitCell currCell = (Grid[point] as UnitCell);

			if (currCell.buildingOnTile)
				return true;
		}

		return false;
	}

	public IEnumerable<PointyHexPoint> GetGridPath(PointyHexPoint startPoint, PointyHexPoint endPoint)
	{
		//We use the original grid here, and not the 
		//copy, to preserve neighbor relationships. Therefore, we
		//have to cast the cell in the lambda expression below.
		var path = Algorithms.AStar(
			Grid,
			startPoint,
			endPoint,
			(p, q) => p.DistanceFrom(q),
			c => true,
			(p, q) => 1);
		
		return path;
	}

	public PointList<PointyHexPoint> GetSurroundingTiles(PointyHexPoint clickedPoint)
	{
		var path = Algorithms.GetPointsInRangeCost (Grid,
		                                           clickedPoint, 
		                                           tile => true,
		                                           (p,q) => 1,
		                                           1);

		return path.Keys.ToPointList<PointyHexPoint>();
	}

    public int GetIndexOfOppositePlayer(Player player)
    {
        if (Array.IndexOf(listOfPlayers, player) == 0)
            return 1;
        else
            return 0;
    }

    public bool IsBuildScreenBlocking()
    {
        return buildScreenSettings.isActiveAndEnabled;
    }

    public void SetupBuildMenu(bool isSettlementMenu, BuildingChosenEventHandler HandleStructChosen)
    {
        buildScreenSettings.structChosen += HandleStructChosen;
        buildScreenSettings.ShowBuildMenu(isSettlementMenu);
    }

    public void RemoveBuildScreenHandler(BuildingChosenEventHandler HandleStructChosen)
    {
        buildScreenSettings.structChosen -= HandleStructChosen;
    }

    public GameState GetCurrentGameState()
    {
        return curGameState;
    }

    public void OpenOptionsMenu()
    {
        optionsCanvas.gameObject.SetActive(true);
    }

    public void CloseOptionsMenu()
    {
        optionsCanvas.gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        if (PhotonNetwork.connected)
            PhotonNetwork.Disconnect();

        SceneManager.LoadScene(0);
    }
}

public enum CameraState
{
    Action,
    Tactical,
    Statistical
}

public enum GameState
{
    None,
    ComeFromLoad,
    LoadState,
    InitState,
    PlayerSetupState,
    RegGameState,
    EndGameState,
    CreateBoardState,
}
