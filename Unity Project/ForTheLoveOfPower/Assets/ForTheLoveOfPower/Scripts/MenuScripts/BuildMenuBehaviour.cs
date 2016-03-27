using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuildMenuBehaviour : MonoBehaviour {
	public bool IsSettlementMenu;
	public List<Button> buttonList;
	private Color btnColor;

	public event BuildingChosenEventHandler structChosen;

	public void ShowBuildMenu(bool isSettlementMenu)
	{
		IsSettlementMenu = isSettlementMenu;

		if (isSettlementMenu) {
            SetActiveSettlementMenu(true);
		}
		else {
            SetActiveMainMenu(true);
        }
	}

    private void SetActiveSettlementMenu(bool setting)
    {
        gameObject.SetActive(setting);
        gameObject.transform.GetChild(1).gameObject.SetActive(setting);
    }

    private void SetActiveMainMenu(bool setting)
    {
        gameObject.SetActive(setting);
        gameObject.transform.GetChild(2).gameObject.SetActive(setting);
    }

    public void ReEnableAll()
	{
		foreach (Button btn in buttonList) {
			btn.enabled = true;
			btn.image.color = btnColor;
		}
	}

	public void SetBGColor(Color bgColor)
	{
		btnColor = bgColor;

		foreach (Button btn in buttonList) {
			btn.image.color = btnColor;//new Color(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		}
	}

	public void OnClick() 
	{
		structChosen (this, new BuildingChosenArgs() {toBuild = AssemblyCSharp.StructureUnitType.None, IsSettlement = false });
        SetActiveSettlementMenu(false);
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }

	public void ClickSettlement()
	{
		if (IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Settlement, IsSettlement = true });

        SetActiveSettlementMenu(false);
		//ReEnableAll ();
	}

	public void ClickBarracks()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Barracks, IsSettlement = false });
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }

	public void ClickTankDepot()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.TankDepot, IsSettlement = false });
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }

	public void ClickAirport()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Airport, IsSettlement = false });
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }

	public void ClickFactory()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Factory, IsSettlement = false });
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }

	public void ClickExporter()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Exporter, IsSettlement = false });
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }

	public void ClickMarket()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Market, IsSettlement = false });
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }

	public void ClickPropaganda()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Propaganda, IsSettlement = false });
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }

	public void CreateLabourCamp()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.LabourCamp, IsSettlement = false });
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }

	public void CreateContractor()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Contractor, IsSettlement = false });
        SetActiveMainMenu(false);
        //ReEnableAll ();
    }
}

public delegate void BuildingChosenEventHandler(object sender, BuildingChosenArgs e);

public class BuildingChosenArgs : EventArgs
{
	public AssemblyCSharp.StructureUnitType toBuild {get;set;}
	public Boolean IsSettlement {get;set;}
}
