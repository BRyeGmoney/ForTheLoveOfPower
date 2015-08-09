using UnityEngine;
using System;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public List<AssemblyCSharp.MilitaryUnit> milUnits;
	public List<AssemblyCSharp.StructureUnit> structUnits;

	// Use this for initialization
	void Start () {
		milUnits = new List<AssemblyCSharp.MilitaryUnit> ();
		structUnits = new List<AssemblyCSharp.StructureUnit> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
