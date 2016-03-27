using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class Combat {
	private float combatTimer;
	private int attackerHitPoints = 0;
	private int defenderHitPoints = 0;
	public bool fightOver = false;

	public MilitaryUnit Attacker {
		get;
		set;
	}
	public MilitaryUnit Defender {
		get;
		set;
	}

	// Use this for initialization
	void Start () {
	}

	public void Setup(MilitaryUnit attacker, MilitaryUnit defender)
	{
		Attacker = attacker;
		Defender = defender;

        if (attacker.AnimationController != null)
		    attacker.AnimationController.SetTrigger ("inCombat");

        if (defender.AnimationController != null)
		    defender.AnimationController.SetTrigger ("inCombat");
	}
	
	// Update is called once per frame
	public void Update () {
		if (Attacker != null && Defender != null) { //first lets make sure they exist
			if (Attacker.GetUnitAmount () > 0 && Defender.GetUnitAmount () < 1) { //attacker wins!
				Defender.StartDeathAnimation ();
				Attacker.StopCombatAnimation ();
				fightOver = true;
			} else if (Defender.GetUnitAmount () > 0 && Attacker.GetUnitAmount () < 1) { //defender wins!
				Attacker.StartDeathAnimation ();
				Defender.StopCombatAnimation ();
				fightOver = true;
			}

			if (!fightOver && combatTimer > 2f) {
				//Collect attacker hit points
				attackerHitPoints += Random.Range (0, 5);
				defenderHitPoints += Random.Range (0, 5);

                if (attackerHitPoints > defenderHitPoints)
                {
                    Defender.DamageUnit();
                    Attacker.ShootAnimation(Defender.transform.position);
                }
                else if (defenderHitPoints > attackerHitPoints)
                {
                    Attacker.DamageUnit();
                    Defender.ShootAnimation(Attacker.transform.position);
                }
                else
                {
                    Attacker.ShootAnimation(Defender.transform.position);
                    Defender.ShootAnimation(Attacker.transform.position);
                }
                    

                attackerHitPoints = 0;
                defenderHitPoints = 0;
				combatTimer = 0f; //Let's reset this mofucka
			}
		}

		combatTimer += Time.deltaTime;
	}
}
