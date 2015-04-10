﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TileManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
			
	}
	// TODO add check for tower
	public bool checkNeighboursForGuards(Tile center, Unit unit)
	{
		List<Tile> neighbours = center.getNeighbours ();
		UnitType unitType = unit.getUnitType();
		Village unitVillage = unit.getVillage ();
		Player unitVillagePlayer = unitVillage.getPlayer ();

		foreach (Tile n in neighbours) 
		{
			Unit neighbouringUnit = n.getOccupyingUnit();
			if(neighbouringUnit != null)
			{
				UnitType neighbourUnitType = neighbouringUnit.getUnitType();
				Village neighbourVillage = neighbouringUnit.getVillage ();
				Player neighbourPlayer = neighbourVillage.getPlayer ();
				if(unitType <= neighbourUnitType && unitVillagePlayer != neighbourPlayer)
				{
					return true;
				}
			}
			Structure tower = n.getStructure();
			if (tower != null && unitType > UnitType.INFANTRY){
				return true;
			}
		}
		return false;
	}

	[RPC]
	void destroyTile(NetworkViewID tileid){
		Destroy (NetworkView.Find (tileid).gameObject);
	}

	/* DEPRECATED
	public void removeUnitFromTile(Tile t, Unit u)
	{
		t.setOccupyingUnit (null);
		u.setLocation (null);
		t.setLandType(LandType.Tombstone);
	}*/
}
