using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuildMenuBehaviour : MonoBehaviour {
	public bool IsSettlementMenu;
	public List<Button> buttonList;

	public event BuildingChosenEventHandler structChosen;

	public void DoSettlementMenu(bool isSettlementMenu)
	{
		IsSettlementMenu = isSettlementMenu;
	}

	public void SetBGColor(Color bgColor)
	{
		foreach (Button btn in buttonList) {
			btn.image.color = bgColor;//new Color(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		}
	}

	public void OnClick() 
	{
		structChosen (this, new BuildingChosenArgs() {toBuild = AssemblyCSharp.StructureUnitType.None, IsSettlement = false });
	}

	public void ClickSettlement()
	{
		if (IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Settlement, IsSettlement = true });
	}

	public void ClickBarracks()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Barracks, IsSettlement = false });
	}

	public void ClickTankDepot()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.TankDepot, IsSettlement = false });
	}

	public void ClickAirport()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Airport, IsSettlement = false });
	}

	public void ClickFactory()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Factory, IsSettlement = false });
	}

	public void ClickExporter()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Exporter, IsSettlement = false });
	}

	public void ClickMarket()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Market, IsSettlement = false });
	}

	public void ClickPropaganda()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Propaganda, IsSettlement = false });
	}

	public void CreateLabourCamp()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.LabourCamp, IsSettlement = false });
	}

	public void CreateContractor()
	{
		if (!IsSettlementMenu)
			structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Contractor, IsSettlement = false });
	}
}

public delegate void BuildingChosenEventHandler(object sender, BuildingChosenArgs e);

public class BuildingChosenArgs : EventArgs
{
	public AssemblyCSharp.StructureUnitType toBuild {get;set;}
	public Boolean IsSettlement {get;set;}
}
