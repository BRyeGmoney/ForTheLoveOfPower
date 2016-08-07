using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LobbyPlayer : MonoBehaviour {

    public Text playerNameText;
    public Button playerColorBtn;
    public Button playerReadyBtn;

    public string playerName;
    public Color playerColor;
    public bool isReady;

    public void Awake()
    {
        
    }

    public void ChangeReady()
    {
        isReady = !isReady;

        /*ColorBlock cb = new ColorBlock();

        if (!isReady)
            cb.normalColor = new Color(0.655f, 0.114f, 0.114f);
        else
            cb.normalColor = new Color(0.114f, 1f, 0.114f);
        cb.pressedColor = cb.normalColor;
        cb.highlightedColor = cb.normalColor;*/

        if (!isReady)
            playerReadyBtn.GetComponent<Image>().color = new Color(0.655f, 0.114f, 0.114f);
        else
            playerReadyBtn.GetComponent<Image>().color = new Color(0.114f, 1f, 0.114f);
    }
}
