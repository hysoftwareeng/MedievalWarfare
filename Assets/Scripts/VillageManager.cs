﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VillageManager : MonoBehaviour {

	protected VillageManager() {
		GameObject go = new GameObject("_villageManager");
		go.AddComponent(typeof(NetworkView));
		NetworkViewID viewID = Network.AllocateViewID();
		go.networkView.viewID = viewID;
	}
	
	private static VillageManager _instance = null;	

	public readonly int ZERO = 0;
	public readonly int ONE = 1;
	public readonly int EIGHT = 8;
	
	// Use this for initialization
	void Start () {
		
	}

	public static VillageManager instance
	{
		get
		{
			if (VillageManager._instance == null)
			{
				VillageManager._instance = new VillageManager();
			}
			return VillageManager._instance;
		}
	}
	
	public void upgradeVillage(Village v)
	{
		int vWood = v.getWood ();
		VillageType vType = v.getMyType ();
		VillageActionType vAction = v.getAction ();
		if ((vType != VillageType.Fort) && (vWood >= EIGHT) && (vAction == VillageActionType.ReadyForOrders)) 
		{
			v.upgrade ();
		}
	}	
	
	public void MergeAlliedRegions(Tile newTile)
	{
		Village myVillage = newTile.getVillage ();
		List<Tile> neighbours = newTile.getNeighbours();
		Player myPlayer = myVillage.getPlayer ();
		VillageType myVillageType = myVillage.getMyType ();
		List<Village> villagesToMerge = new List<Village>();
		int size = ZERO;
		Village biggestVillage = null;
		VillageType biggestVillageType = VillageType.Hovel;
		foreach (Tile neighbour in neighbours) 
		{
			Village neighbourVillage = neighbour.getVillage ();
			Player neighbourPlayer = neighbourVillage.getPlayer ();
			if((myPlayer == neighbourPlayer) && !(villagesToMerge.Contains(neighbourVillage)))
			{
				List<Tile> neighbourControlledRegion = neighbourVillage.getControlledRegion();
				int neighbourSize = neighbourControlledRegion.Count();
				VillageType neighbourVillageType = neighbourVillage.getMyType();
				if(((size < neighbourSize) && (biggestVillageType == neighbourVillageType)) || biggestVillageType < neighbourVillageType)
				{
					size = neighbourSize;
					biggestVillage = neighbourVillage;
					biggestVillageType = neighbourVillageType;
				}
				villagesToMerge.Add(neighbourVillage);
			}
		}
		int totalGold = ZERO;
		int totalWood = ZERO;
		List<Tile> totalRegion = new List<Tile> ();
		foreach (Village village in villagesToMerge) 
		{
			if(village != biggestVillage)
			{
				totalGold += village.getGold ();
				totalWood += village.getWood ();
				List<Tile> villageRegion = village.getControlledRegion();
				Tile villageLocation = village.getLocatedAt();
				List<Unit> villageUnits = village.getControlledUnits();
				totalRegion.AddRange(villageRegion);
				foreach(Unit u in villageUnits)
				{
					u.setVillage(biggestVillage);
					biggestVillage.addUnit(u);
				}
				//destroying a village from the game
				//Destroy (//prefab for village);
				villageLocation.setLandType (LandType.Meadow);
			}
			biggestVillage.addGold(totalGold);
			biggestVillage.addWood(totalWood);
			biggestVillage.addRegion(totalRegion);
		}
	}
	
	public void takeoverTile(Tile Destination)
	{
		
	}
}
