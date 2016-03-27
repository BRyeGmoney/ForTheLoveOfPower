using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class LobbyPlayer : NetworkLobbyPlayer {

    private TextMesh playerNameText;

    [SyncVar(hook = "OnPlayerName")]
    public string playerName;
    [SyncVar(hook = "OnPlayerColor")]
    public Color playerColor;

    public void Awake()
    {
        playerNameText = gameObject.GetComponentInChildren<TextMesh>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        OnPlayerName(playerName);
        OnPlayerColor(playerColor);
        //playerNameText.text = playerName;
        //transform.SetParent(MenuBehaviour.instance.LobbyPlayersPanel, false);
        //int index = Array.IndexOf(MenuBehaviour.instance.lobbyManager.lobbySlots, this);
        //transform.position = new Vector3(0, -350 + (index * 35), 0);// * Random.Range(1, 4), 0);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        
        //CmdNameChanged(MenuBehaviour.instance.PlayerName);
        //OnPlayerName(playerName);
        //CmdColorChanged(MenuBehaviour.instance.PlayerColor);
        //OnPlayerColor(playerColor);

        //int index = Array.IndexOf(MenuBehaviour.instance.lobbyManager.lobbySlots, this);
        //transform.position = new Vector3(0, -350 + (index * 35), 0);

        //if (index > 1)
        //{
         //   readyToBegin = true;
         //   SendReadyToBeginMessage();
       // }
    }

    public void OnPlayerName(string newPlayerName)
    {
        playerName = newPlayerName;
        playerNameText.text = playerName;
    }

    public void OnPlayerColor(Color newPlayerColor)
    {
        playerColor = newPlayerColor;
        playerNameText.color = playerColor;
    }

    [Command]
    public void CmdNameChanged(string name)
    {
        playerName = name;
    }

    [Command]
    public void CmdColorChanged(Color pColor)
    {
        playerColor = pColor;
    }
}
