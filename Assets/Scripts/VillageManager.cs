﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class VillageManager : MonoBehaviour {
	
	public readonly int ZERO = 0;
	public readonly int ONE = 1;
	public readonly int THREE = 3;
	public readonly int EIGHT = 8;
	public readonly int TEN = 10;
	public readonly int TWENTY = 20;
	public readonly int THIRTY = 30;
	public readonly int FOURTY = 40;
	private InGameGUI gameGUI;
	// Use this for initialization
	public GameObject meadowPrefab;
	public GameObject hovelPrefab;

	void Start () {
		gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
	}
	
	public void upgradeVillage(Village v)
	{
		int vWood = v.getWood ();
		VillageType vType = v.getMyType ();
		VillageActionType vAction = v.getAction ();
		if (vType == VillageType.Fort) 
		{
			gameGUI.displayError(@"The Fort is your strongest village! ¯\(°_o)/¯");
		}
		else if ((vType != VillageType.Fort) && (vWood >= 8) && (vAction == VillageActionType.ReadyForOrders)) 
		{
			v.upgrade ();
		} 
	}	

	[RPC]
	void upgradeVillageNet(NetworkViewID villageID){
		Village v = NetworkView.Find (villageID).gameObject.GetComponent<Village>();
		upgradeVillage (v);
	}
	
	public void MergeAlliedRegions(Tile newTile)
	{
		Village myVillage = newTile.getVillage ();
		List<Tile> neighbours = newTile.getNeighbours();
		int mySize = myVillage.getRegionSize ();
		Player myPlayer = myVillage.getPlayer ();
		List<Village> villagesToMerge = new List<Village>();
		villagesToMerge.Add (myVillage);
		Village biggestVillage = myVillage;
		//VillageType biggestType = biggestVillage.getMyType ();

		foreach (Tile neighbour in neighbours) 
		{
			Village neighbourVillage = neighbour.getVillage ();
			if( neighbourVillage != null )
			{
				Player neighbourPlayer = neighbourVillage.getPlayer ();
				if((myPlayer == neighbourPlayer) && !(villagesToMerge.Contains(neighbourVillage)))
				{
					villagesToMerge.Add(neighbourVillage);
					VillageType neighbourType = neighbourVillage.getMyType();
					int neighbourSize = neighbourVillage.getRegionSize();
					if (neighbourType>biggestVillage.getMyType())
					{
						biggestVillage = neighbourVillage;
					} 
					else if (neighbourType==biggestVillage.getMyType()&&neighbourSize>biggestVillage.getRegionSize())
					{
						biggestVillage = neighbourVillage;
					}
				}
			}
		}

		foreach (Village village in villagesToMerge) {
			if (village != biggestVillage) {
				biggestVillage.addGold (village.getGold ());
				biggestVillage.addWood (village.getWood ());
				biggestVillage.addRegion(village.getControlledRegion ());
				//foreach (Unit u in village.getControlledUnits ()){
				//	biggestVillage.addUnit (u);
				//}
				// remove prefab
				Tile villageLocation = village.getLocatedAt();
				Destroy (villageLocation.prefab);
				villageLocation.setLandType (LandType.Meadow);
				villageLocation.prefab = Instantiate (meadowPrefab, new Vector3 (villageLocation.point.x, 0.2f, villageLocation.point.y), meadowPrefab.transform.rotation) as GameObject;
			}
		}
	}

	/*
	 * Function adds village to invader. If the dest had a village prefab on it, then we take all resources
	 */ 
	public void takeoverTile(Village invader, Tile dest)
	{
		Village invadedVillage = dest.getVillage ();
		if(dest.checkVillagePrefab())
		{
			int pillagedWood = invadedVillage.getWood ();
			int pillagedGold = invadedVillage.getGold ();
			invader.addWood(pillagedWood);
			invader.addGold(pillagedGold);
			respawnHovel (invadedVillage);
		}
		invader.addTile(dest);
		splitRegion(dest, invadedVillage);
	}
	//TODO network component ?
	private List<Tile> getValidTilesForRespawn(List<Tile> region)
	{
		List<Tile> validTiles = new List<Tile> ();
		foreach (Tile t in region) 
		{
			if(t.getStructure() == null)
			{
				validTiles.Add(t);
			}
		}
		return validTiles;
	}

	//TODO needs networking component
	private void respawnHovel(Village v)
	{
		print ("made it to respawnhovel");
		List<Tile> validTiles = getValidTilesForRespawn (v.getControlledRegion ());
		System.Random rand = new System.Random();
		int randomTileIndex;
		Tile respawnLocation;
		if(validTiles.Count == 0)
		{
			randomTileIndex = rand.Next (0, v.getRegionSize());
			respawnLocation = validTiles[randomTileIndex];
			respawnLocation.replace (hovelPrefab); // TODO needs to use RPC replace
			v.setLocation(respawnLocation);
			// do we need to set tile's occupying structure? or does village not count?
		}
		else
		{
			randomTileIndex = rand.Next (0, validTiles.Count);
			respawnLocation = validTiles[randomTileIndex];
			respawnLocation.replace (hovelPrefab); // TODO needs to use RPC replace
			v.setLocation(respawnLocation);
		}
	}
	//TODO needs networking component
	private void splitRegion(Tile splitTile, Village villageToSplit)
	{
		Player villageToSplitPlayer = villageToSplit.getPlayer();
		List<Tile> neighbours = splitTile.getNeighbours();
		List<Tile> tilesToSplit = villageToSplit.getControlledRegion();
		int tilesToSplitSize = villageToSplit.getRegionSize();
		int villageToSplitTotalGold = villageToSplit.getGold();
		int villageToSplitTotalWood = villageToSplit.getWood();
		Dictionary<Tile,bool> visitedDictionary = new Dictionary<Tile,bool> ();

		foreach (Tile t in tilesToSplit) 
		{
			visitedDictionary.Add(t,false);
		}

		int splitGoldTracker, splitWoodTracker;
		List<Tile> tilesToReturn = new List<Tile>();
		Village tempVillage;
		Tile respawnLocation;
		int randomTileIndex;
		System.Random rand = new System.Random();
		bool isVisited;

		foreach(Tile n in neighbours)
		{	
			tilesToReturn.Clear();
			splitGoldTracker = ZERO;
			splitWoodTracker = ZERO;
			tempVillage = n.getVillage();
			visitedDictionary.TryGetValue(n,out isVisited);
			if(tempVillage == villageToSplit && isVisited == false)
			{
				print ("before bfs: "+tilesToReturn.Count);
				splitBFS(n,visitedDictionary,tilesToReturn);
				print ("after bfs: "+tilesToReturn.Count);
			}
			if(tilesToReturn.Count >= THREE)
			{
				print ("tilesToReturn >= 3");
				List<Tile> temp = getValidTilesForRespawn(tilesToSplit);
				randomTileIndex = rand.Next(0, villageToSplit.getRegionSize());
				respawnLocation = tilesToReturn[randomTileIndex];
				Vector3 hovelLocation = new Vector3(respawnLocation.point.x, 0, respawnLocation.point.y);
				GameObject villageObject = Network.Instantiate(hovelPrefab, hovelLocation, hovelPrefab.transform.rotation, 0) as GameObject;
				Village newVillage = villageObject.GetComponent<Village>();
				int newVillageGold = (int)(villageToSplitTotalGold *(tilesToReturn.Count/tilesToSplitSize));
				int newVillageWood = (int)(villageToSplitTotalWood *(tilesToReturn.Count/tilesToSplitSize));
				if((splitGoldTracker+newVillageGold)>villageToSplitTotalGold || 
				   (splitWoodTracker+newVillageWood)>villageToSplitTotalWood)
				{
					newVillage.setGold(villageToSplitTotalGold-splitGoldTracker);
					newVillage.setWood (villageToSplitTotalWood-splitWoodTracker);
				}
				else
				{
					splitGoldTracker = splitGoldTracker + newVillageGold;
					splitWoodTracker = splitWoodTracker + newVillageWood;
					newVillage.setGold(newVillageGold);
					newVillage.setWood(newVillageWood);
				}
			}
			//if the new size is <  3
			else if(tilesToReturn.Count < THREE) //&& tilesToReturn.Count > ZERO)
			{
				print ("tilesToReturn < 3");
				foreach(Tile t in tilesToReturn)
				{
					Unit u = t.getOccupyingUnit();
					Structure s = t.getStructure();
					if(u !=  null)
					{
						//TODO break relationship between tile and unit
						//destroy the unit gameobject
						//remove the tile owner
						//recolor the tile to neutral
					}
					if(s != null)
					{
						//TODO break relationship between tile and structure
						//destroy the structure gameobject
					}
				}
			}
		}
		//TODO destroy villagetoSplit and all of its relationships
		//TODO implement PlayerManager.checkLoss(villageToSplitPlayer)
		//TODO implement PlayerManager.checkWin();
	}

	private void splitBFS (Tile tiletoSearch, Dictionary<Tile, bool> visitedDictionary, List<Tile> tilesToReturn)
	{
		List<Tile> neighbours = tiletoSearch.getNeighbours();
		bool isVisited;
		foreach(Tile n in neighbours)
		{
			if (visitedDictionary.TryGetValue(n,out isVisited)){
				if(isVisited == false)
				{
					visitedDictionary[n] = true;
					tilesToReturn.Add(n);
					splitBFS(n,visitedDictionary,tilesToReturn);
				}
			}
		}
	}

	//TODO needs networking component
	public void removeUnitFromVillage(Village v,Unit u)
	{
		v.removeUnit(u);
	}

	//TODO needs networking component
	public void removeTileFromVillage(Village v, Tile t)
	{
		v.removeTile (t);
		t.setVillage (null);
	}

	public void hirePeasant(Village v,GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 10) 
		{
			//Unit p = Unit.CreateComponent (UnitType.PEASANT, tileAt, v, unitPrefab);
			GameObject newPeasant = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
			newPeasant.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.PEASANT, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);

			//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (true);
			//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);

			//new method that sets the mesh active:
			newPeasant.networkView.RPC("setActiveNet", RPCMode.All, "Peasant");
			//v.setGold (villageGold - TEN);
			v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -10);
			//v.addUnit (p);
			v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newPeasant.networkView.viewID);
		} else {
			gameGUI.displayError (@"Wow you're broke, can't even afford a peasant? ¯\(°_o)/¯");
		}

	}

	public void hireInfantry(Village v,GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 20) {
			//Unit p = Unit.CreateComponent (UnitType.INFANTRY, tileAt, v, unitPrefab);
			GameObject newInfantry = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
			newInfantry.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.INFANTRY, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);

			//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (true);
			//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);
			newInfantry.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Infantry");

			//v.setGold (villageGold - TWENTY);
			v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -20);

			//v.addUnit (p);
			v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newInfantry.networkView.viewID);
		} else {
			gameGUI.displayError (@"You do not have enough gold to train infantry. ¯\(°_o)/¯");
		}
	}

	public void hireSoldier(Village v, GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 30) {
			if(v.getMyType() >= VillageType.Town)
			{
				//Unit p = Unit.CreateComponent (UnitType.SOLDIER, tileAt, v, unitPrefab);
				GameObject newSoldier = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
				newSoldier.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.SOLDIER, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);

				//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (true);
				//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);

				newSoldier.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Soldier");
				//v.setGold (villageGold - THIRTY);
				v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -30);
				//v.addUnit (p);
				v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newSoldier.networkView.viewID);
			}
			else
			{
				gameGUI.displayError(@"Please upgrade your village to a Town first. ¯\(°_o)/¯");
			}
		} else {
			gameGUI.displayError(@"You can't afford a soldier. ¯\(°_o)/¯");
		}
	}
	public void hireKnight(Village v, GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 40) {
			if(v.getMyType() == VillageType.Fort)
			{
				//Unit p = Unit.CreateComponent (UnitType.KNIGHT, tileAt, v, unitPrefab);
				GameObject newKnight = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
				newKnight.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.KNIGHT, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);

				//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (true);
				newKnight.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Knight");

				//v.setGold (villageGold - FOURTY);
				v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -40);

				//v.addUnit (p);
				v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newKnight.networkView.viewID);
			}
			else
			{
				gameGUI.displayError (@"Please upgrade your village to a Fort first. ¯\(°_o)/¯");
			}
		} else {
			gameGUI.displayError(@"You don't have enough gold for a knight. ¯\(°_o)/¯");
		}

	}
}
