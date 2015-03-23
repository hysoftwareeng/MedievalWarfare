﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class MapGenerator : MonoBehaviour {
	
	public GameObject GrassPrefab;
	public GameObject MeadowPrefab;
	public GameObject TreePrefab;
	public GameObject HovelPrefab;

	public bool minimap;

	public int numTiles;
	public int removeTiles;
	public bool isMap1;

	private Graph map;
	private List<Tile> unvisited_vertices;
	private System.Random rand = new System.Random();

	public Graph getMap()
	{
		return this.map;
	}

	// Use this for initialization
	//Changed to public functon
	public void initMap () 
	{
		// add tag for selection
		TreePrefab.tag = "Trees";
		MeadowPrefab.tag = "Meadow";
		GrassPrefab.tag = "Grass";

		GameObject firstPref = Network.Instantiate(GrassPrefab, new Vector3(0, 0, 0), GrassPrefab.transform.rotation, 0) as GameObject;
		firstPref.networkView.RPC("changeMapLayer", RPCMode.AllBuffered, isMap1);
		//no longer static
		//Tile firstTile = Tile.CreateComponent(new Vector2 (0, 0), firstPref);
		Tile firstTile = firstPref.GetComponent<Tile> ();

		map = new Graph (firstTile, null);
		unvisited_vertices = new List<Tile>();
		unvisited_vertices.Add(firstTile);

		int maxNumberTile = rand.Next (Mathf.FloorToInt(numTiles * .8f), Mathf.FloorToInt(numTiles * 1.2f));
		
		while(map.getVertices().Count < maxNumberTile)
		{
			Tile curr = unvisited_vertices[0];

			GameObject upPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x+1, 0, curr.point.y), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile up = Tile.CreateComponent(new Vector2(curr.point.x+1, curr.point.y), upPref);
			Tile up = upPref.GetComponent<Tile>();
			//Vector2 uPos = new Vector2(curr.point.x+1, curr.point.y);
			upPref.networkView.RPC("setPointN", RPCMode.AllBuffered, new Vector3(curr.point.x+1, 0, curr.point.y));
			upPref.networkView.RPC("changeMapLayer", RPCMode.AllBuffered, isMap1);
			
			GameObject downPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x-1, 0, curr.point.y), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile down =Tile.CreateComponent(new Vector2(curr.point.x-1, curr.point.y), downPref);
			Tile down = downPref.GetComponent<Tile>();
			//Vector2 dPos = new Vector2(curr.point.x-1, curr.point.y);
			downPref.networkView.RPC("setPointN", RPCMode.AllBuffered,new Vector3(curr.point.x-1, 0, curr.point.y));
			downPref.networkView.RPC("changeMapLayer", RPCMode.AllBuffered, isMap1);

			GameObject leftupPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x + 0.5f, 0, curr.point.y + 0.75f), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile leftup = Tile.CreateComponent(new Vector2(curr.point.x + 0.5f, curr.point.y + 0.75f), leftupPref);
			Tile leftup = leftupPref.GetComponent<Tile>();
			//Vector2 luPos =new Vector2(curr.point.x + 0.5f, curr.point.y + 0.75f);
			leftupPref.networkView.RPC("setPointN", RPCMode.AllBuffered,new Vector3(curr.point.x + 0.5f, 0, curr.point.y + 0.75f));
			leftupPref.networkView.RPC("changeMapLayer", RPCMode.AllBuffered, isMap1);

			GameObject rightupPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x + 0.5f, 0, curr.point.y - 0.75f), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile rightup = Tile.CreateComponent(new Vector2(curr.point.x + 0.5f, curr.point.y - 0.75f), rightupPref);
			Tile rightup = rightupPref.GetComponent<Tile>();
			//Vector2 ruPos =new Vector2(curr.point.x + 0.5f, curr.point.y - 0.75f);
			rightupPref.networkView.RPC("setPointN", RPCMode.AllBuffered,new Vector3(curr.point.x + 0.5f, 0, curr.point.y - 0.75f));
			rightupPref.networkView.RPC("changeMapLayer", RPCMode.AllBuffered, isMap1);


			GameObject leftdownPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x - 0.5f, 0, curr.point.y + 0.75f), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile leftdown = Tile.CreateComponent(new Vector2(curr.point.x - 0.5f, curr.point.y + 0.75f), leftdownPref);
			Tile leftdown = leftdownPref.GetComponent<Tile>();
			//Vector2 ldPos =new Vector2(curr.point.x - 0.5f, curr.point.y + 0.75f);
			leftdownPref.networkView.RPC("setPointN", RPCMode.AllBuffered,new Vector3(curr.point.x - 0.5f, 0, curr.point.y + 0.75f));
			leftdownPref.networkView.RPC("changeMapLayer", RPCMode.AllBuffered, isMap1);


			GameObject rightdownPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x - 0.5f, 0, curr.point.y - 0.75f), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile rightdown = Tile.CreateComponent(new Vector2(curr.point.x - 0.5f, curr.point.y - 0.75f), rightdownPref);
			Tile rightdown = rightdownPref.GetComponent<Tile>();
			//Vector2 rdPos =new Vector2(curr.point.x - 0.5f, curr.point.y - 0.75f);
			rightdownPref.networkView.RPC("setPointN", RPCMode.AllBuffered,new Vector3(curr.point.x - 0.5f, 0, curr.point.y - 0.75f));
			rightdownPref.networkView.RPC("changeMapLayer", RPCMode.AllBuffered, isMap1);
			unvisited_vertices.RemoveAt(0);
			
			insertTile(curr, up);
			insertTile(curr, down);
			insertTile(curr, leftup);
			insertTile(curr, rightup);
			insertTile(curr, leftdown);
			insertTile(curr, rightdown);
		}

		List<Tile> outerTiles = map.getVertices().Where (t => t.getNeighbours().Count< 6).ToList();

		// the outer tilees were generated by thier neighbors. we need to link them back.
		foreach(Tile curr in outerTiles)
		{
			Tile tmp = map.GetTile(curr.point.x + 1, curr.point.y);
			
			if(tmp != null)
			{
				//curr.addNeighbour(tmp);
				curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
			}
			
			tmp = map.GetTile(curr.point.x-1, curr.point.y);
			
			if(tmp != null)
			{
				//curr.addNeighbour(tmp);
				curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
			}
			
			tmp = map.GetTile(curr.point.x + 0.5f, curr.point.y + 0.75f);
			
			if(tmp != null)
			{
				//curr.addNeighbour(tmp);
				curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
			}
			
			tmp = map.GetTile(curr.point.x + 0.5f, curr.point.y - 0.75f);
			
			if(tmp != null)
			{
				//curr.addNeighbour(tmp);
				curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
			}
			
			tmp = map.GetTile(curr.point.x - 0.5f, curr.point.y + 0.75f);
			
			if(tmp != null)
			{
				//curr.addNeighbour(tmp);
				curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
			}
			
			tmp = map.GetTile(curr.point.x - 0.5f, curr.point.y - 0.75f);
			
			if(tmp != null)
			{
				//curr.addNeighbour(tmp);
				curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
			}
			
		}
		int tileRemoved = 0;
		int index = 0, count = 0;

		int tilesToRemove = rand.Next (Mathf.FloorToInt(removeTiles*.8f),Mathf.FloorToInt(removeTiles*1.2f));
		while(tileRemoved < tilesToRemove)
		{
			if(count > tilesToRemove)
			{
				// avoid infinite loop
				break;
			}

			//its a little tricky here. we want to remove tiles that are on the outer outer, so <4 is more outer
			//than <6.
			List<Tile> sideTiles = map.getVertices().Where (t =>t.getNeighbours().Count<4 ).ToList();

			if(sideTiles.Count == 0)
			{
				count++;
				continue;
			}
			
			index = rand.Next(sideTiles.Count);
			Tile curr = sideTiles[index];
			
			foreach(Tile neighboor in curr.getNeighbours())
			{

				neighboor.getNeighbours().Remove(curr);


			}

			map.getVertices().Remove(curr);

			//Destroy (curr.gameObject);
			curr.gameObject.networkView.RPC("destroyTile", RPCMode.AllBuffered, curr.gameObject.networkView.viewID);
			tileRemoved++;
			
			count++;
		}
		/* there are now a few lone tiles around the edges, but they are no longer considered part of map
		 * thus, they should not affect further map generation...
		// the lone tiles are no longer considered part of map?
		foreach(Tile n in map.getVertices()){
			n.gameObject.renderer.material.color = Color.red;
			//if (n.getNeighbours().Count<=0){
				//map.getVertices().Remove(n);
				//n.gameObject.networkView.RPC("destroyTile", RPCMode.AllBuffered, n.gameObject.networkView.viewID);
				//Destroy (this.gameObject);
			//}
		} */

		//stop here for minimaps
		if (minimap){
			return;
		}

		foreach(Tile n in map.getVertices())
		{
			//TODO: hardcoded to 2 players color
			int color = rand.Next(0,3);
			int probability = rand.Next(0,101);
			if( probability > 0 && probability <= 20)
			{
				//n.InstantiateTree(TreePrefab);
				GameObject tpref = Network.Instantiate(TreePrefab, new Vector3(n.point.x, 0.2f, n.point.y), TreePrefab.transform.rotation, 0) as GameObject;
				n.networkView.RPC ("setPrefab", RPCMode.AllBuffered, tpref.networkView.viewID);
				n.networkView.RPC ("setLandTypeNet", RPCMode.AllBuffered, (int)LandType.Trees);
	
			}
			else if( probability > 20 && probability <=30)
			{
				//n.InstantiateMeadow(MeadowPrefab);
				GameObject mpref = Network.Instantiate(MeadowPrefab, new Vector3(n.point.x, 0.2f, n.point.y), MeadowPrefab.transform.rotation, 0) as GameObject;
				n.networkView.RPC ("setPrefab", RPCMode.AllBuffered, mpref.networkView.viewID);
				n.networkView.RPC ("setLandTypeNet", RPCMode.AllBuffered, (int)LandType.Meadow);

			}
			//n.setColor(color);
			//n.colorTile();
			n.networkView.RPC ("setAndColor", RPCMode.AllBuffered, color);
		}
		
	}


	public void initializeVillagesOnMap(List<Player> players)
	{
		//stop here for minimaps
		if (minimap){
			return;
		}
		foreach ( Tile t in this.map.getVertices() )
		{
			// player.count is the neutral color.
			if ( t.getVisited() == false  && t.getColor() != players.Count )
			{
				List<Tile> TilesToReturn = new List<Tile>();
				t.setVisited(true);
				int color = t.getColor();
			
				searchVillages( t, TilesToReturn, color );
				TilesToReturn.Add (t);

				if( TilesToReturn.Count >= 3 )
				{
					Player p = players[color];
			
					Tile location = TilesToReturn[0];
					//location.setLandType (LandType.Grass);
					location.networkView.RPC("setLandTypeNet", RPCMode.AllBuffered, (int) LandType.Grass);
					Vector3 hovelLocation = new Vector3(location.point.x, 0, location.point.y);
					GameObject hovel = Network.Instantiate(HovelPrefab, hovelLocation, HovelPrefab.transform.rotation, 0) as GameObject;
					//Village newVillage = Village.CreateComponent(p, TilesToReturn, location, hovel );
					Village newVillage = hovel.GetComponent<Village>();

					//need to be set over network: controlledRegion, locatedAt.Replace(), locatedAt, locatedAt.setVillage(), updateControlledRegionNet()
					//ControlledRegion: loop to add tiles over network
					foreach (Tile cTile in TilesToReturn){
						NetworkViewID tileID = cTile.gameObject.networkView.viewID;
						hovel.networkView.RPC ("addTileNet", RPCMode.AllBuffered, tileID);
					}
					//replace the prefab at the location
					location.networkView.RPC ("replaceTilePrefabNet", RPCMode.AllBuffered, hovel.networkView.viewID);

					//set locatedAt for the new village:
					hovel.networkView.RPC("setLocatedAtNet", RPCMode.AllBuffered, location.networkView.viewID);

					//set the village for the LocatedAt tile:
					location.networkView.RPC("setVillageNet", RPCMode.AllBuffered, hovel.networkView.viewID);

					//update the tile regions: Not done over a network, The client at this point should have all the necessary info from controlledRegion
					hovel.networkView.RPC ("updateControlledRegionNet", RPCMode.AllBuffered);

					//TODO: Set Player (Controlled by), Currently NOT set over network
					//newVillage.setControlledBy(p);
					hovel.networkView.RPC ("setControlledByNet", RPCMode.AllBuffered, gameObject.networkView.viewID, color);

					//newVillage.addGold( 200 );
					hovel.networkView.RPC("addGoldNet", RPCMode.AllBuffered, 200);
					//newVillage.addWood ( 200);
					hovel.networkView.RPC("addWoodNet", RPCMode.AllBuffered, 200);

					//TODO: add village to player over network
					//p.addVillage( newVillage );
					p.gameObject.networkView.RPC ("addVillageNet", RPCMode.AllBuffered, newVillage.networkView.viewID);
				}
			}
			if (t.getVillage() == null && t.getColor() != players.Count)
			{
				//t.setColor(players.Count);
				//t.gameObject.renderer.material.color = Color.white;
				t.gameObject.networkView.RPC("setAndColor", RPCMode.AllBuffered, 2);
			}
		}

		cleanVisitedTiles();
	}

	public void cleanVisitedTiles()
	{
		foreach(Tile t in map.getVertices())
		{
			t.setVisited(false);
		}
	}

	public void searchVillages(Tile toSearch, List<Tile> TilesToReturn, int color )
	{
		foreach( Tile n in toSearch.getNeighbours() )
		{
			if(n.getVisited() == false && n.getColor() == color)
			{
				n.setVisited( true );
				TilesToReturn.Add(n);
				searchVillages(n, TilesToReturn, color);
			}
		}
	}

	private void insertTile(Tile curr, Tile t)
	{
		if(map.addTileUnique(t))
		{
			unvisited_vertices.Add(t);
			//curr.addNeighbour(t);
			curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, t.gameObject.networkView.viewID);
		}
		else
		{
			Tile tmpTile = map.GetTile(t.point.x, t.point.y);
			//curr.addNeighbour(tmpTile);
			curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmpTile.gameObject.networkView.viewID);
			//Destroy(t.gameObject);
			t.gameObject.networkView.RPC("destroyTile", RPCMode.AllBuffered, t.gameObject.networkView.viewID);

		}
	}

	[RPC]
	void logMsg(string text){
		Debug.Log (text);
	}
	
}