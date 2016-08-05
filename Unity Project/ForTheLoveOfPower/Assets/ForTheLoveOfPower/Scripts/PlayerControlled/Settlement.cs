using System;
using System.Linq;
using Gamelogic.Grids;
using UnityEngine;
using System.Collections.Generic;
using Vectrosity;

namespace AssemblyCSharp
{
	public class Settlement : StructureUnit, Assets.ForTheLoveOfPower.Scripts.PlayerControlled.IObjectPoolItem
    {
		public Boolean RefreshCachedBuildings { get; set; }

        public SpriteRenderer spriteRenderer;

		public List<StructureUnit> cachedBuildingList;
		public PointList<PointyHexPoint> tilesOwned;
		public float updateTimer;

		public float costOfUnitsInTown = 1.0f;
		public int numEconUnitsProducedInTown = 10;

        private int curStateOfSettlement = 0;
        public bool newBuildingAdded = false;
        public bool isDictatorInCity = false;

        public bool cityBeingConquered = false;
        private int indexOfBuildingBeingTaken = 0;
        private Color colorBeingConquered;

        private float halfWidthOfImage;

        public VectorLine border;
        private bool madeBorder = true;

		public Settlement ()
		{
			cachedBuildingList = new List<StructureUnit> ();
			tilesOwned = new PointList<PointyHexPoint> ();
            halfWidthOfImage = 128;
        }

        public void Start()
        {
            SpriteRenderer childSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (childSprite != null)
                childSprite.color = StructColor;

            border = new VectorLine("stlmntBorder", new List<Vector3>(251), null, 10.0f, LineType.Continuous);
            border.layer = LayerMask.NameToLayer("TacticalView");
            border.color = this.StructColor;
            madeBorder = false;
        }

        private Vector2 DetermineCoordinate(int side, StructureUnit unitToParse)
        {
            if (side == 0) //top-left
            {
                return new Vector2(unitToParse.transform.position.x - halfWidthOfImage, unitToParse.transform.position.y - halfWidthOfImage);
            }
            else if (side == 1) //top
            {
                return new Vector2(unitToParse.transform.position.x, unitToParse.transform.position.y - halfWidthOfImage);
            }
            else if (side == 2) //top right
            {
                return new Vector2(unitToParse.transform.position.x + halfWidthOfImage, unitToParse.transform.position.y - halfWidthOfImage);
            }
            else if (side == 3) //right
            {
                return new Vector2(unitToParse.transform.position.x + halfWidthOfImage, unitToParse.transform.position.y);
            }
            else if (side == 4) //bottom right
            {
                return new Vector2(unitToParse.transform.position.x + halfWidthOfImage, unitToParse.transform.position.y + halfWidthOfImage);
            }
            else if (side == 5) // bottom
            {
                return new Vector2(unitToParse.transform.position.x, unitToParse.transform.position.y + halfWidthOfImage);
            }
            else if (side == 6) //bottom left
            {
                return new Vector2(unitToParse.transform.position.x - halfWidthOfImage, unitToParse.transform.position.y + halfWidthOfImage);
            } else //left
            {
                return new Vector2(unitToParse.transform.position.x - halfWidthOfImage, unitToParse.transform.position.y);
            }
        }

		public void UpdateBuildingList(Player owningPlayer)
		{
            if (!madeBorder)
            {
                border.MakeSpline(RefreshBorder(), 250, true);//new Vector3[] { DetermineCoordinate(0, this), DetermineCoordinate(1, this), DetermineCoordinate(2, this), DetermineCoordinate(3, this) }, 250, true);
                border.Draw3D();
                madeBorder = true;
            }
            if (!cityBeingConquered)
            {
                //lets check if we can update the settlement based on whether it meets the criteria, the flags are set when a building is added, the dictator enters, or leaves
                if (newBuildingAdded || isDictatorInCity)
                {
                    if (curStateOfSettlement == 0 && cachedBuildingList.Count > 9)
                    {
                        curStateOfSettlement = 1;
                        spriteRenderer.sprite = GameGridBehaviour.instance.townHall;
                    }
                    else if ((curStateOfSettlement == 1 && cachedBuildingList.Count > 19) || (curStateOfSettlement == 3 && !isDictatorInCity))
                    {
                        curStateOfSettlement = 2;
                        spriteRenderer.sprite = GameGridBehaviour.instance.cityHall;
                    }
                    else if (curStateOfSettlement == 2 && isDictatorInCity)
                    {
                        curStateOfSettlement = 3;
                        spriteRenderer.sprite = GameGridBehaviour.instance.capital;
                    }

                    newBuildingAdded = false;
                }


                if (RefreshCachedBuildings)
                    RefreshBorders();

                cachedBuildingList.ForEach(build =>
                {
                    if (build.StructureType.Equals(StructureUnitType.Factory))
                    {
                        if (updateTimer > 10f)
                            owningPlayer.AddCash(200);
                        /*if (build.modifierAnim < 3)
                        {
                            build.AnimationController.SetTrigger("modifierAnim");
                            build.modifierAnim += 1;
                        }*/
                    }

                    build.UpdateBuilding();
                });

                if (updateTimer > 10f)
                {
                    owningPlayer.AddCash(200);

                    updateTimer = 0f;
                }

                UpdateBuilding();

                updateTimer += Time.deltaTime;
            }
            else
            {
                if (indexOfBuildingBeingTaken > -1) //if we're still iterating through 
                {
                    if (cachedBuildingList[indexOfBuildingBeingTaken].currentState == StructureState.Captured) //if the building being conquered is done, lets skip to the next one
                    {
                        indexOfBuildingBeingTaken -= 1;
                    }
                    else if (cachedBuildingList[indexOfBuildingBeingTaken].currentState != StructureState.BeingCaptured) //lets start conquering the next building in line
                    {
                        cachedBuildingList[indexOfBuildingBeingTaken].currentState = StructureState.BeingCaptured;
                        cachedBuildingList[indexOfBuildingBeingTaken].BeginCapturing(colorBeingConquered);
                    }
                    else //if it is being captured
                    {
                        //let's keep running the capture
                        cachedBuildingList[indexOfBuildingBeingTaken].UpdateBuilding();
                    }

                    
                }
                else //the settlement itself is being captured
                {
                    if (currentState.Equals(StructureState.Captured))
                    {
                        owningPlayer.playerCiv.AddToSettlements(this);
                        owningPlayer.playerCiv.AddToOwnedTiles(GameGridBehaviour.instance.Grid, tilesOwned);

                        owningPlayer.playerCiv.RemoveFromOwnedTiles(tilesOwned);
                        //owningPlayer.ownedTiles = owningPlayer.ownedTiles.Except(tilesOwned).ToList<PointyHexPoint>();
                        owningPlayer.playerCiv.RemoveFromSettlements(this);

                        FinishSettlementCapture();
                        RepaintOwnedTiles();
                    }
                    else if (!currentState.Equals(StructureState.BeingCaptured))
                    {
                        currentState = StructureState.BeingCaptured;
                        BeginCapturing(colorBeingConquered);
                    }
                    else
                    {
                        UpdateBuilding();
                    }
                }
            }
		}

        public void AddToBuildingList(StructureUnit structToAdd)
        {
            cachedBuildingList.Add(structToAdd);
            newBuildingAdded = true;
            madeBorder = false;
        }

        private Vector3[] RefreshBorder()
        {
            List<PointyHexPoint> points = new List<PointyHexPoint>();
            Vector3[] toReturn;
            PointyHexPoint[] toSort;

            points.AddRange(GameGridBehaviour.instance.GetSurroundingTiles(TilePoint));
            cachedBuildingList.ForEach(s =>
            {
                points.AddRange(GameGridBehaviour.instance.GetSurroundingTiles(s.TilePoint));
            });

            points = points.Distinct<PointyHexPoint>().ToList<PointyHexPoint>();

            points.RemoveAll(point => cachedBuildingList.Any(cB => cB.TilePoint == point));

            points.Remove(TilePoint); //remove the settlement
            toSort = points.ToArray();

            //order the remainder
            Array.Sort(toSort, new ClockwisePointyHexComparer(TilePoint));
            points = toSort.ToList();

            //if (points.Count > 0)
            //{
            toReturn = new Vector3[points.Count];
            for (int a = 0; a < points.Count; a++)
            {
                toReturn[a] = GameGridBehaviour.instance.Grid[points[a]].transform.position;
            }
            return toReturn;
            //}
        }

		public void RefreshBorders() 
		{
            List<PointyHexPoint> points = new List<PointyHexPoint>();
            PointyHexPoint[] toSort;

            //add the settlement tiles too
            points.AddRange(GameGridBehaviour.instance.GetSurroundingTiles(TilePoint));
            cachedBuildingList.ForEach(s => 
            {
                points.AddRange(GameGridBehaviour.instance.GetSurroundingTiles(s.TilePoint));
            });

            points = points.Distinct<PointyHexPoint>().ToList<PointyHexPoint>();

            points.RemoveAll(point => cachedBuildingList.Any(cB => cB.TilePoint == point));

            points.Remove(TilePoint); //remove the settlement
            toSort = points.ToArray();

            //order the remainder
            Array.Sort(toSort, new ClockwisePointyHexComparer(TilePoint));
            points = toSort.ToList();
            if (points.Count > 0)
            {
                //splineDrawer.spline.Reset();
                //points.ForEach(point =>
                //{
                //    splineDrawer.spline.AddPoint(GameGridBehaviour.instance.Grid[point].transform.position - gameObject.transform.position);//get a local relative position to sit from
                //});
                //splineDrawer.Generate();
                //points.ForEach(p => lineRenderer.SetPosition(points.IndexOf(p), new Vector3(p.X, p.Y, 0)));
            }
		}

		public bool TileBelongsToSettlement(PointyHexPoint tileToCheck) 
		{
			return tilesOwned.Contains (tileToCheck);
		}

        public void BeginSettlementCapture(Color colorToChangeTo)
        {
            colorBeingConquered = colorToChangeTo;
            indexOfBuildingBeingTaken = cachedBuildingList.Count - 1;
            cityBeingConquered= true;
        }

        public void FinishSettlementCapture()
        {
            cachedBuildingList.ForEach(structure =>
            {
                structure.StructColor = colorBeingConquered;
                structure.spriteRender.color = colorBeingConquered;
                structure.percentageConquered = 0;
                structure.currentState = StructureState.Owned;
            });

            StructColor = colorBeingConquered;
            spriteRender.color = colorBeingConquered;
            percentageConquered = 0;
            currentState = StructureState.Owned;
            cityBeingConquered = false;
        }

        public void DissolveFinished()
        {

        }

        private void RepaintOwnedTiles()
        {
            foreach (PointyHexPoint point in tilesOwned)
            {
                (GameGridBehaviour.instance.Grid[point] as UnitCell).SetTileColorStructure(StructColor);
            }
        }
	}
}

