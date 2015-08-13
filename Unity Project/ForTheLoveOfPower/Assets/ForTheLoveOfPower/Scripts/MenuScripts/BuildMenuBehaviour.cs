using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuildMenuBehaviour : MonoBehaviour {
	public Color BgColor;
	public bool IsSettlementMenu;

	public event BuildingChosenEventHandler structChosen;

	public void DoSettlementMenu(bool isSettlementMenu)
	{
		IsSettlementMenu = isSettlementMenu;
	}

	public void ClickSettlement()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Settlement, IsSettlement = true });
	}

	public void ClickBarracks()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Barracks, IsSettlement = false });
	}

	public void ClickTankDepot()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.TankDepot, IsSettlement = false });
	}

	public void ClickAirport()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Airport, IsSettlement = false });
	}

	public void ClickFactory()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Factory, IsSettlement = false });
	}

	public void ClickExporter()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Exporter, IsSettlement = false });
	}

	public void ClickMarket()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Market, IsSettlement = false });
	}

	public void ClickPropaganda()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Propaganda, IsSettlement = false });
	}

	public void CreateLabourCamp()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.LabourCamp, IsSettlement = false });
	}

	public void CreateContractor()
	{
		structChosen (this, new BuildingChosenArgs () { toBuild = AssemblyCSharp.StructureUnitType.Contractor, IsSettlement = false });
	}
}

public delegate void BuildingChosenEventHandler(object sender, BuildingChosenArgs e);

public class BuildingChosenArgs : EventArgs
{
	public AssemblyCSharp.StructureUnitType toBuild {get;set;}
	public Boolean IsSettlement {get;set;}
}
