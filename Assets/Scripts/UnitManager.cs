﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UnitManager : MonoBehaviour {

	private VillageManager villageManager;
	private InGameGUI gameGUI;
	public readonly int TEN = 10;
	// Use this for initialization
	void Start () {
		villageManager = GameObject.Find ("VillageManager").GetComponent<VillageManager>();
		gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
	}

	[RPC]
	void moveUnitNet(NetworkViewID unitID, NetworkViewID tileID){
		Unit unitToMove = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		Tile dest = NetworkView.Find (tileID).gameObject.GetComponent<Tile>();
		moveUnit (unitToMove, dest);
	}
	
	public bool moveUnit(Unit unit, Tile dest)
	{
		print ("----in move unit----");
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
				unit.movePrefab (new Vector3 (dest.point.x, 0.15f,dest.point.y));
				return true;
			}
			else if (srcVillage != destVillage)
			{
				if (destVillage == null)
				{
					dest.setOccupyingUnit(unit);
					unit.setLocation(dest);
					srcVillage.addTile(dest);
					villageManager.MergeAlliedRegions(dest);
					this.performMove(unit,dest); 
					unit.movePrefab (new Vector3 (dest.point.x, 0.15f,dest.point.y));
					return true;
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
		return false;
	}
	
	public void performMove(Unit unit, Tile dest)
	{

		Village srcVillage = unit.getVillage ();
		
		UnitType srcUnitType = unit.getUnitType();
		LandType destLandType = dest.getLandType ();
		print ("--------------landtype of the tile below------------------");		
		print (destLandType);
		if (srcUnitType == UnitType.KNIGHT) {
			bool destHasRoad = dest.checkRoad ();
			if (destLandType == LandType.Meadow && destHasRoad == false) {
				dest.setLandType (LandType.Grass);
				unit.setAction (UnitActionType.Moved);
			}		
		} 
		else
		{
			Debug.LogError("HERREEE in else");
			if (destLandType == LandType.Trees)
			{
				print ("entered cutting trees");
				unit.setAction(UnitActionType.ChoppingTree);
				//unit.animation.CrossFade("attack");
	
				Destroy (dest.prefab);
				dest.prefab = null;

				//unit.animation.CrossFade("idle");
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

	public void upgradeUnit(Unit u, UnitType newLevel)
	{
		Village unitVillage = u.getVillage();
		VillageType unitVillageLevel = unitVillage.getMyType();
		UnitType unitType = u.getUnitType();
		UnitActionType unitAction = u.getAction();
		int goldAvailable = unitVillage.getGold();
		int goldRequired = (newLevel - unitType) * TEN;
		if (unitType == UnitType.KNIGHT) {
			gameGUI.displayError ("The Knight is already your strongest warrior!");
		} 
		else if((goldAvailable >= goldRequired)&&(newLevel > unitType)&&(unitAction == UnitActionType.ReadyForOrders || unitAction == UnitActionType.Moved))
		{
			if(newLevel == UnitType.SOLDIER && unitVillageLevel < VillageType.Town)
			{
				gameGUI.displayError ("Please upgrade your village to a Town first.");
			}
			else if(newLevel == UnitType.KNIGHT && unitVillageLevel < VillageType.Fort)
			{
				gameGUI.displayError ("Please upgrade your village to a Fort first.");
			}
			else
			{
				unitVillage.setGold (goldAvailable - goldRequired);
				u.upgrade(newLevel);
			}
		}
	}
}
