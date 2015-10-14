using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class LobbyPlayer : NetworkLobbyPlayer {

    private TextMesh playerNameText;

    [SyncVar(hook = "OnPlayerName")]
    public string playerName;

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
        //playerNameText.text = playerName;
        //transform.SetParent(MenuBehaviour.instance.LobbyPlayersPanel, false);
        int index = Array.IndexOf(MenuBehaviour.instance.lobbyManager.lobbySlots, this);
        transform.position = new Vector3(0, -350 + (index * 35), 0);// * Random.Range(1, 4), 0);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        CmdNameChanged(MenuBehaviour.instance.PlayerName);
        OnPlayerName(playerName);
        int index = Array.IndexOf(MenuBehaviour.instance.lobbyManager.lobbySlots, this);
        transform.position = new Vector3(0, -350 + (index * 35), 0);
    }

    public void OnPlayerName(string newPlayerName)
    {
        playerName = newPlayerName;
        playerNameText.text = playerName;
    }

    [Command]
    public void CmdNameChanged(string name)
    {
        playerName = name;
    }
}
