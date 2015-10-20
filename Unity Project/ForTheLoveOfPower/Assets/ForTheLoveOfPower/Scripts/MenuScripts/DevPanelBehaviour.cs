using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DevPanelBehaviour : MonoBehaviour {

    public GameObject PlayerPrefab;
    public GameObject AIPlayerPrefab;

    public void CreatePlayersPressed()
    {
        GameObject mainPlayer = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        GameObject aiPlayer = Instantiate(AIPlayerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
    }
}
