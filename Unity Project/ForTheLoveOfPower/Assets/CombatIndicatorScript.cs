using UnityEngine;
using System.Collections;
using Assets.ForTheLoveOfPower.Scripts.PlayerControlled;
using System;

public class CombatIndicatorScript : MonoBehaviour, IObjectPoolItem
{

    public Vector2 CombatPosition { get; set; }

    public bool PoolObjInUse { get; set; }


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate() {
	    if (PoolObjInUse && !CombatPosition.Equals(Vector2.zero))
        {
            this.gameObject.transform.position = new Vector3(Mathf.Clamp(CombatPosition.x, GameGridBehaviour.instance.Viewport.xMin + (GameGridBehaviour.instance.Viewport.width * 0.05f), GameGridBehaviour.instance.Viewport.xMax - (GameGridBehaviour.instance.Viewport.width * 0.05f)), 
                                                             Mathf.Clamp(CombatPosition.y, GameGridBehaviour.instance.Viewport.yMin + (GameGridBehaviour.instance.Viewport.height * 0.05f), GameGridBehaviour.instance.Viewport.yMax - (GameGridBehaviour.instance.Viewport.height * 0.05f)), 
                                                             0f);
        }
	}

    void OnMouseDown()
    {
        Camera.main.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, Camera.main.transform.position.z);
    }
}
