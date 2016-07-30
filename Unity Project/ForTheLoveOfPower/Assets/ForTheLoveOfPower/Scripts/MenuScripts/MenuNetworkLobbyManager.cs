using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuNetworkLobbyManager {//: NetworkLobbyManager {

    public Text ConnectionStatusString;
    public GameObject GoButton;

    public int numPlayersConnected = 0;

	// Use this for initialization
	void Start () {
	
	}

    /*public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        ConnectionStatusString.text = "Waiting For Host";
    }

    public override void OnLobbyServerConnect(NetworkConnection conn)
    {
        base.OnLobbyServerConnect(conn);

        numPlayersConnected += 1;

        if (numPlayersConnected > 1 && ConnectionStatusString != null && GoButton != null)
        {
            ConnectionStatusString.text = "Player Found";
            GoButton.SetActive(true);
        }
    }*/
}
