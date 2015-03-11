﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UnitManager : MonoBehaviour {

	public VillageManager villageManager;
	public readonly int TEN = 10;
	void Start()
	{
		villageManager = GameObject.Find ("VillageManager").GetComponent<VillageManager>();
	}


	public void moveUnit(Unit unit, Tile dest)
	{
		Village destVillage = dest.getVillage ();
		Village srcVillage = unit.getVillage ();
		
		Unit destUnit = dest.getOccupyingUnit ();
		LandType destLandType = dest.getLandType ();
		UnitType srcUnitType = unit.getUnitType();
		
		bool unitPermitted = dest.canUnitMove (srcUnitType); //need to implement canUnitMove in tile
		
		//if the move is allowed to move onto the tile
		if (unitPermitted == true) 	
		{
			if (srcVillage == destVillage)
			{
				dest.setOccupyingUnit(unit); //reassign unit/tile to each other
				unit.setLocation(dest);
				this.performMove(unit,dest); //need to be implemented in UnitManager
			}
			else if (srcVillage != destVillage)
			{
				if (destVillage == null)
				{
					dest.setOccupyingUnit(unit);
					unit.setLocation(dest);
					unit.setAction(UnitActionType.CapturingNeutral);
					srcVillage.addTile(dest);
					villageManager.MergeAlliedRegions(dest);
				}
				
				
				//USED FOR INVADING
				/*else if (srcUnitType != UnitType.PEASANT)
				{
					dest.getNeighbours();
					bool isGuardSurrounding = checkNeighboursForGuards(dest);
					if (isGuardSurrounding == false)
					{
						if (destUnit != null)
						{
							UnitType destUnitType = destUnit.getUnitType();
							if (srcUnitType > destUnitType)
							{
								destVillage.removeUnit(destUnit);
								destUnit.setVillage (null);
								//Destroy () //destroy prefab
								unit.setLocation(dest);
								dest.setOccupyingUnit(unit);
								unit.setAction(UnitActionType.CapturingNeutral);
								villageManager.takeOverTile(dest);
								villageManager.MergeAlliedRegions((dest);
							}

							else if (destUnit == null)
							{
								unit.setLocation(dest);
								dest.setOccupyingUnit(unit);
								unit.setAction(UnitActionType.CapturingEnemy);
								villageManager.takeOverTile(dest);
								villageManager.MergeAlliedRegions((dest);
							}
						}
					}
				}*/
			}
		}
	}
	
	public void performMove(Unit unit, Tile dest)
	{
		Village srcVillage = unit.getVillage ();
		
		UnitType srcUnitType = unit.getUnitType();
		LandType destLandType = dest.getLandType ();
		
		if (srcUnitType == UnitType.KNIGHT) {
			bool destHasRoad = dest.checkRoad ();
			if (destLandType == LandType.Meadow && destHasRoad == false) {
				dest.setLandType (LandType.Grass);
				unit.setAction (UnitActionType.Moved);
			}		
		} 
		else if (srcUnitType != UnitType.KNIGHT) 
		{
			if (destLandType == LandType.Trees)
			{
				unit.setAction(UnitActionType.ChoppingTree);
				srcVillage.addWood(1);
				dest.setLandType(LandType.Grass);
			}
			else if (destLandType == LandType.TombStone)
			{
				unit.setAction(UnitActionType.ClearingTombstone);
				dest.setLandType(LandType.Grass);
			}
			unit.setAction(UnitActionType.Moved);
		}
	}

/*	public void upgradeUnit(Unit u, UnitType newLevel)
	{
		UnitType currentType = u.getUnitType();

		else 
		{
			UnitActionType currentAction = u.getAction ();
			int goldAvailable = u.getVillage ().getGold ();
			int goldRequired = 
		}

	}*/
}
