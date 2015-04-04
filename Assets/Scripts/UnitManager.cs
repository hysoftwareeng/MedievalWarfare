﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class UnitManager : MonoBehaviour {

	private VillageManager villageManager;
	private TileManager tileManager;
	private InGameGUI gameGUI;
	private readonly int TEN = 10;
	// Use this for initialization

	void Start () {
		villageManager = GameObject.Find ("VillageManager").GetComponent<VillageManager>();
		tileManager = GameObject.Find ("TileManager").GetComponent<TileManager> ();
		gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
	}

	[RPC]
	void moveUnitNet(NetworkViewID unitID, NetworkViewID tileID){
		Unit unitToMove = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		Tile dest = NetworkView.Find (tileID).gameObject.GetComponent<Tile>();
		moveUnit (unitToMove, dest);
	}

	// needs networking
	public void moveUnit(Unit unit, Tile dest)
	{
		//print ("----in move unit----");
		Village destVillage = dest.getVillage ();
		Village srcVillage = unit.getVillage ();
		
		Unit destUnit = dest.getOccupyingUnit ();
		UnitType srcUnitType = unit.getUnitType();
		
		bool unitPermitted = canUnitMove (srcUnitType, dest);
		
		//if the move is allowed to move onto the tile
		if (unitPermitted == true) 	
		{
			Tile originalLocation = unit.getLocation ();
			// moving within your region
			if (srcVillage == destVillage)
			{
				performMove(unit,dest);
				originalLocation.setOccupyingUnit(null);
			}
			else if (srcVillage != destVillage)
			{
				// taking over neutral tiles
				if (destVillage == null)
				{
					srcVillage.addTile(dest);
					performMove(unit,dest);
					villageManager.MergeAlliedRegions(dest);
					unit.setAction(UnitActionType.CapturingNeutral);
					originalLocation.setOccupyingUnit(null);
				}

				// TODO taking over enemy tiles and networking it
				else if (srcUnitType != UnitType.PEASANT)
				{
					// if there is any enemy unit
					//Unit destUnit = dest.getOccupyingUnit;
					if (destUnit!=null){
						if(srcUnitType>destUnit.getUnitType()){
							// kill enemy unit, remove it from tile, remove it from village
							// perform move? nope, perform move is shit
							destVillage.removeUnit(destUnit); //removes U from V's army AND sets U's v to null
							dest.setOccupyingUnit(null);
							Destroy (destUnit.gameObject);
						} else {
							print ("The enemy is too strong! I dont want to die!");
							return;
						}
					}
					// if the tile contains the enemy village
						// pillage, then move the hovel
					if (destVillage.getLocatedAt()==dest){
						if (srcUnitType > UnitType.INFANTRY){
							// plunder village will handle stealing resources
							villageManager.plunderVillage (srcVillage, destVillage, dest);
							// it also calls respawn hovel and creating a meadow
						} else {
							print ("This unit is too weak to plunder villages");
							return;
						}
					}
					// TODO knights destroying towers
					// You take over the tile and merge regions
					// Enemy removes tile and splits region
					// finally move onto tile and set action

					villageManager.takeoverTile(srcVillage,dest); //also splits region
					villageManager.MergeAlliedRegions(dest);
					//performMove(unit,dest); more complicated than what we need
					unit.setAction(UnitActionType.CapturingEnemy);
					originalLocation.setOccupyingUnit(null);

				} 
			}
		}

	}

	private void movePrefab(Unit u, Vector3 vector)
	{
		u.transform.localPosition = vector;
	}

	private void performMove(Unit unit, Tile dest)
	{
		dest.setOccupyingUnit(unit);
		unit.setLocation(dest);
		Village srcVillage = unit.getVillage ();
		UnitType srcUnitType = unit.getUnitType();
		LandType destLandType = dest.getLandType ();

		if (srcUnitType == UnitType.KNIGHT) 
		{
			bool destHasRoad = dest.checkRoad ();
			if (destLandType == LandType.Meadow && destHasRoad == false) 
			{
				dest.setLandType (LandType.Grass);
				Destroy (dest.prefab);
			}
			unit.setAction (UnitActionType.Moved);
		} 
		else
		{
			//Debug.LogError("HERREEE in else");
			if (destLandType == LandType.Trees)
			{
				//print ("entered cutting trees");
				unit.setAction(UnitActionType.ChoppingTree);
				//unit.animation.CrossFade("attack");
				Destroy (dest.prefab);
				dest.prefab = null;

				//unit.animation.CrossFade("idle");
				srcVillage.addWood(1);
				dest.setLandType(LandType.Grass);
			}
			else if (destLandType == LandType.Tombstone)
			{
				unit.setAction(UnitActionType.ClearingTombstone);
				dest.setLandType(LandType.Grass);
			}
		}
		movePrefab (unit, new Vector3 (dest.point.x, 0.15f,dest.point.y));
	}

	private bool canUnitMove(UnitType type, Tile dest)
	{
		if (dest.getStructure () == null && dest.getOccupyingUnit () == null && dest.getLandType () != LandType.Trees) 
		{
			return true;
		} 
		else if(dest.getLandType () == LandType.Trees && type != UnitType.KNIGHT)
		{
			return true;
		} 
		else if (dest.getStructure () != null) 
		{
			gameGUI.displayError (@"The tower doesn't want you to stand ontop of it. ¯\(°_o)/¯");
			return false;
		} 
		else if (type == UnitType.KNIGHT && dest.getLandType () == LandType.Trees) 
		{
			gameGUI.displayError (@"Your Knight is out of shape. It cannot cut down this tree. ¯\(°_o)/¯");
			return false;
		} 
		else if (dest.getOccupyingUnit () != null) 
		{
			gameGUI.displayError (@"There is a unit already standing there!!! ¯\(°_o)/¯");
			return false;
		}

		return false;
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
			gameGUI.displayError (@"The Knight is already your strongest warrior! ¯\(°_o)/¯");
		}
		else if((goldAvailable >= goldRequired)&&(newLevel > unitType)&&(unitAction == UnitActionType.ReadyForOrders || unitAction == UnitActionType.Moved))
		{
			if(newLevel == UnitType.SOLDIER && unitVillageLevel < VillageType.Town)
			{
				gameGUI.displayError (@"Please upgrade your village to a Town first. ¯\(°_o)/¯");
			}
			else if(newLevel == UnitType.KNIGHT && unitVillageLevel < VillageType.Fort)
			{
				gameGUI.displayError (@"Please upgrade your village to a Fort first. ¯\(°_o)/¯");
			}
			else
			{
				unitVillage.setGold (goldAvailable - goldRequired);
				u.upgrade(newLevel);
			}
		}
	}

	[RPC]
	void upgradeUnitNet(NetworkViewID unitID, int newlvl){
		Unit u = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		upgradeUnit (u, (UnitType)newlvl);
	}
}
