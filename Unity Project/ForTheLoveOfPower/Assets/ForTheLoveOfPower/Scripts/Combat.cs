using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class Combat {
    private float combatTimer;
    private int attackerHitPoints = 0;
    private int defenderHitPoints = 0;
    public bool fightOver = false;
    private CombatIndicatorScript CombatIndicator {get;set;}

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

        SetCombatFlags(true);

        if (attacker.AnimationController != null)
		    attacker.AnimationController.SetTrigger ("inCombat");

        if (defender.AnimationController != null)
		    defender.AnimationController.SetTrigger ("inCombat");

        CombatIndicator = ObjectPool.instance.PullNewIndicator(attacker.gameObject.transform.position);
	}
	
	// Update is called once per frame
	public void Update () {
		if (Attacker != null && Defender != null) { //first lets make sure they exist
			if (Attacker.GetUnitAmount () > 0 && Defender.GetUnitAmount () < 1) { //attacker wins!
                WrapUpCombat(Attacker, Defender);
			} else if (Defender.GetUnitAmount () > 0 && Attacker.GetUnitAmount () < 1) { //defender wins!
                WrapUpCombat(Defender, Attacker);
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

    private void SetCombatFlags(bool inCombat)
    {
        Attacker.inCombat = inCombat;
        Defender.inCombat = inCombat;
    }

    private void WrapUpCombat(MilitaryUnit winner, MilitaryUnit loser)
    {
        winner.StopCombatAnimation();
        loser.StartDeathAnimation();
        fightOver = true;
        SetCombatFlags(false);
        ObjectPool.instance.DestroyOldIndicator(CombatIndicator);
    }
}
