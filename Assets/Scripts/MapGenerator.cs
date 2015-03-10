﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class MapGenerator : MonoBehaviour {
	
	public GameObject GrassPrefab;
	public GameObject MeadowPrefab;
	public GameObject TreePrefab;
	public GameObject villages;

	public GameObject HovelPrefab;
	
	private Graph map;
	private List<Tile> unvisited_vertices;
	private System.Random rand = new System.Random();

	public Graph getMap()
	{
		return this.map;
	}
	// Use this for initialization
	public void initMap () 
	{	
		Debug.Log ("Mapgenerator: initMap()");
		// add tag for selection
		TreePrefab.tag = "Trees";
		MeadowPrefab.tag = "Meadow";
		GrassPrefab.tag = "Grass";

		GameObject firstPref = Network.Instantiate(GrassPrefab, new Vector3(0, 0, 0), GrassPrefab.transform.rotation, 0) as GameObject;
		//no longer static
		//Tile firstTile = Tile.CreateComponent(new Vector2 (0, 0), firstPref);
		Tile firstTile = firstPref.GetComponent<Tile> ();

		map = new Graph (firstTile, null);
		unvisited_vertices = new List<Tile>();
		unvisited_vertices.Add(firstTile);

		int maxNumberTile = rand.Next (400, 450);
		
		while(map.vertices.Count < maxNumberTile)
		{
			Tile curr = unvisited_vertices[0];

			GameObject upPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x+1, 0, curr.point.y), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile up = Tile.CreateComponent(new Vector2(curr.point.x+1, curr.point.y), upPref);
			Tile up = upPref.GetComponent<Tile>();
			up.point = new Vector2(curr.point.x+1, curr.point.y);

			GameObject downPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x-1, 0, curr.point.y), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile down =Tile.CreateComponent(new Vector2(curr.point.x-1, curr.point.y), downPref);
			Tile down = downPref.GetComponent<Tile>();
			down.point = new Vector2(curr.point.x-1, curr.point.y);

			GameObject leftupPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x + 0.5f, 0, curr.point.y + 0.75f), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile leftup = Tile.CreateComponent(new Vector2(curr.point.x + 0.5f, curr.point.y + 0.75f), leftupPref);
			Tile leftup = leftupPref.GetComponent<Tile>();
			leftup.point =new Vector2(curr.point.x + 0.5f, curr.point.y + 0.75f);

			GameObject rightupPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x + 0.5f, 0, curr.point.y - 0.75f), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile rightup = Tile.CreateComponent(new Vector2(curr.point.x + 0.5f, curr.point.y - 0.75f), rightupPref);
			Tile rightup = rightupPref.GetComponent<Tile>();
			rightup.point =new Vector2(curr.point.x + 0.5f, curr.point.y - 0.75f);

			GameObject leftdownPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x - 0.5f, 0, curr.point.y + 0.75f), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile leftdown = Tile.CreateComponent(new Vector2(curr.point.x - 0.5f, curr.point.y + 0.75f), leftdownPref);
			Tile leftdown = leftdownPref.GetComponent<Tile>();
			leftdown.point =new Vector2(curr.point.x - 0.5f, curr.point.y + 0.75f);

			GameObject rightdownPref = Network.Instantiate(GrassPrefab, new Vector3(curr.point.x - 0.5f, 0, curr.point.y - 0.75f), GrassPrefab.transform.rotation, 0) as GameObject;
			//Tile rightdown = Tile.CreateComponent(new Vector2(curr.point.x - 0.5f, curr.point.y - 0.75f), rightdownPref);
			Tile rightdown = rightdownPref.GetComponent<Tile>();
			rightdown.point =new Vector2(curr.point.x - 0.5f, curr.point.y - 0.75f);

			unvisited_vertices.RemoveAt(0);
			
			insertTile(curr, up);
			insertTile(curr, down);
			insertTile(curr, leftup);
			insertTile(curr, rightup);
			insertTile(curr, leftdown);
			insertTile(curr, rightdown);
		}

		List<Tile> outerTiles = map.vertices.Where (t => t.neighbours.Count< 6).ToList();

		// the outer tilees were generated by thier neighbors. we need to link them back.
		foreach(Tile curr in outerTiles)
		{
			Tile tmp = map.GetTile(curr.point.x + 1, curr.point.y);
			
			if(tmp != null)
			{
				curr.addNeighbour(tmp);
			}
			
			tmp = map.GetTile(curr.point.x-1, curr.point.y);
			
			if(tmp != null)
			{
				curr.addNeighbour(tmp);
			}
			
			tmp = map.GetTile(curr.point.x + 0.5f, curr.point.y + 0.75f);
			
			if(tmp != null)
			{
				curr.addNeighbour(tmp);
			}
			
			tmp = map.GetTile(curr.point.x + 0.5f, curr.point.y - 0.75f);
			
			if(tmp != null)
			{
				curr.addNeighbour(tmp);
			}
			
			tmp = map.GetTile(curr.point.x - 0.5f, curr.point.y + 0.75f);
			
			if(tmp != null)
			{
				curr.addNeighbour(tmp);
			}
			
			tmp = map.GetTile(curr.point.x - 0.5f, curr.point.y - 0.75f);
			
			if(tmp != null)
			{
				curr.addNeighbour(tmp);
			}
			
		}
		int tileRemoved = 0;
		int index = 0, count = 0;

		int tilesToRemove = rand.Next (10, 40);
		while(tileRemoved < tilesToRemove)
		{
			if(count > tilesToRemove)
			{
				// avoid infinite loop
				break;
			}

			//its a little tricky here. we want to remove tiles that are on the outer outer, so <4 is more outer
			//than <6.
			List<Tile> sideTiles = map.vertices.Where (t =>t.neighbours.Count<4 ).ToList();

			if(sideTiles.Count == 0)
			{
				count++;
				continue;
			}
			
			index = rand.Next(sideTiles.Count);
			Tile curr = sideTiles[index];
			
			foreach(Tile neighboor in curr.neighbours)
			{

				neighboor.neighbours.Remove(curr);


			}

			map.vertices.Remove(curr);
			gameObject.networkView.RPC("logMsg", RPCMode.AllBuffered, "Removed 1 tile");

			//Network.destroy calls are not buffered!! use RPC
			//Destroy (curr.gameObject);

			curr.gameObject.networkView.RPC("destroyTile", RPCMode.AllBuffered, curr.gameObject.networkView.viewID);
			tileRemoved++;
			
			count++;
		}
		//colors
		foreach(Tile n in map.vertices)
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

				GameObject mpref = Network.Instantiate(MeadowPrefab, new Vector3(n.point.x, 0.2f, n.point.y), TreePrefab.transform.rotation, 0) as GameObject;
				n.networkView.RPC ("setPrefab", RPCMode.AllBuffered, mpref.networkView.viewID);
				n.networkView.RPC ("setLandTypeNet", RPCMode.AllBuffered, (int)LandType.Meadow);

			}
			n.setColor(color);
			n.colorTile();
		}

	}

	[RPC]
	void logMsg(string text){
		Debug.Log (text);
	}

	
	// Update is called once per frame
	void Update () {
		
	}

	public void initializeVillagesOnMap(List<Player> players)
	{

		foreach ( Tile t in this.map.vertices )
		{
			// player.count is the neutral color.
			if ( t.getVisited() == false  && t.getColor() != players.Count )
			{
				List<Tile> TilesToReturn = new List<Tile>();
				t.setVisited(true);
				int color = t.getColor();
			
				searchVillages( t, TilesToReturn, color );


				if( TilesToReturn.Count >= 3 )
				{

					Player p = players[color];
			
					Tile location = TilesToReturn[0];

					GameObject hovel = Network.Instantiate(HovelPrefab, new Vector3(location.point.x, 0, location.point.y), HovelPrefab.transform.rotation, 0) as GameObject;
					Village newVillage = Village.CreateComponent(p, TilesToReturn, location, hovel );
					newVillage.addGold( 7 );
					p.addVillage( newVillage );
				} 
			}
		}

		cleanVisitedTiles();
	}

	public void cleanVisitedTiles()
	{
		foreach(Tile t in map.vertices)
		{
			t.setVisited(false);
		}
	}

	public void searchVillages(Tile toSearch, List<Tile> TilesToReturn, int color )
	{
		foreach( Tile n in toSearch.neighbours )
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
			curr.addNeighbour(t);
		}
		else
		{
			Tile tmpTile = map.GetTile(t.point.x, t.point.y);
			curr.addNeighbour(tmpTile);
			//Destroy(t.gameObject);
			t.gameObject.networkView.RPC("destroyTile", RPCMode.AllBuffered, t.gameObject.networkView.viewID);
		}
	}
	
}