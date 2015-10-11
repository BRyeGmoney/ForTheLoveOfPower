using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class MenuNetworkLobbyManager : NetworkLobbyManager {

    public Text ConnectionStatusString;
    public Button GoButton;

	// Use this for initialization
	void Start () {
	
	}

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        
        if (numPlayers > 1 && ConnectionStatusString != null && GoButton != null)
        {
            ConnectionStatusString.text = "Player Found";
            GoButton.enabled = true;
        }
    }
}
