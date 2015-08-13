//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public enum StructureUnitType
	{
		Settlement,
		TownHall,
		CityHall,
		Capitol,
		Exporter,
		Factory,
		Market,
		Contractor,
		LabourCamp,
		Propaganda,
		Airport,
		Barracks,
		TankDepot
	}

	public enum StructureAnimationIndex
	{
		Capitol = 0,
		CityHall = 1,
		Settlement = 2,
		TownHall = 3,
		Exporter = 4,
		Factory = 5,
		Market = 6,
		Contractor = 7,
		LabourCamp = 8,
		Propaganda = 9,
		Airport = 10,
		Barracks = 11,
		TankDepot = 12,
	}

	public class StructureUnit
	{
		//Properties
		public Int16 StructureSpriteIndex { get; set; }
		public Color StructColor { get; set; }

		public StructureUnitType StructureType
		{
			get { return structureType; }
			set { structureType = value; }
		}
		private StructureUnitType structureType;

		public Settlement owningSettlement { get; set; }

		public StructureUnit ()
		{
		}

		public void UpdateBuilding(Player owningPlayer)
		{
		}
	}

	public static class CreateStructureUnit
	{
		public static StructureUnit CreateFromType(StructureUnitType structType, Color structColor)
		{
			if (structType.Equals (StructureUnitType.Settlement))
				return CreateSettlement (structColor);
			else if (structType.Equals (StructureUnitType.Factory))
				return CreateFactory (structColor);
			else if (structType.Equals (StructureUnitType.Exporter))
				return CreateExporter (structColor);
			else if (structType.Equals (StructureUnitType.Market))
				return CreateMarket (structColor);
			else if (structType.Equals (StructureUnitType.Barracks))
				return CreateBarracks (structColor);
			else if (structType.Equals (StructureUnitType.TankDepot))
				return CreateTankDepot (structColor);
			else if (structType.Equals (StructureUnitType.Airport))
				return CreateAirport (structColor);
			else if (structType.Equals (StructureUnitType.Contractor))
				return CreateContractor (structColor);
			else if (structType.Equals (StructureUnitType.LabourCamp))
				return CreateLabourCamp (structColor);
			else if (structType.Equals (StructureUnitType.Propaganda))
				return CreatePropaganda (structColor);
			else
				return null;
		}

		public static Settlement CreateSettlement(Color structColor)
		{
			return new Settlement () { StructureType = StructureUnitType.Settlement, StructureSpriteIndex = (short)StructureAnimationIndex.Settlement, StructColor = structColor };
		}

		public static StructureUnit CreateFactory(Color structColor)
		{
			return new StructureUnit () { StructureType = StructureUnitType.Factory, StructureSpriteIndex = (short)StructureAnimationIndex.Factory, StructColor = structColor };
		}

		public static StructureUnit CreateExporter(Color structColor)
		{
			return new StructureUnit () { StructureType = StructureUnitType.Exporter, StructureSpriteIndex = (short)StructureAnimationIndex.Exporter, StructColor = structColor };
		}

		public static StructureUnit CreateMarket(Color structColor)
		{
			return new StructureUnit () { StructureType = StructureUnitType.Market, StructureSpriteIndex = (short)StructureAnimationIndex.Market, StructColor = structColor };
		}

		public static StructureUnit CreateContractor(Color structColor)
		{
			return new StructureUnit () { StructureType = StructureUnitType.Contractor, StructureSpriteIndex = (short)StructureAnimationIndex.Contractor, StructColor = structColor };
		}

		public static StructureUnit CreateLabourCamp(Color structColor)
		{
			return new StructureUnit () { StructureType = StructureUnitType.LabourCamp, StructureSpriteIndex = (short)StructureAnimationIndex.LabourCamp, StructColor = structColor };
		}

		public static StructureUnit CreatePropaganda(Color structColor)
		{
			return new StructureUnit () { StructureType = StructureUnitType.Propaganda, StructureSpriteIndex = (short)StructureAnimationIndex.Propaganda, StructColor = structColor };
		}

		public static StructureUnit CreateBarracks(Color structColor)
		{
			return new StructureUnit () { StructureType = StructureUnitType.Barracks, StructureSpriteIndex = (short)StructureAnimationIndex.Barracks, StructColor = structColor };
		}

		public static StructureUnit CreateTankDepot(Color structColor)
		{
			return new StructureUnit () { StructureType = StructureUnitType.TankDepot, StructureSpriteIndex = (short)StructureAnimationIndex.TankDepot, StructColor = structColor };
		}

		public static StructureUnit CreateAirport(Color structColor)
		{
			return new StructureUnit () { StructureType = StructureUnitType.Airport, StructureSpriteIndex = (short)StructureAnimationIndex.Airport, StructColor = structColor };
		}
	}
}

