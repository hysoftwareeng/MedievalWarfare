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
	void Awake () 
	{
		// add tag for selection
		TreePrefab.tag = "Trees";
		MeadowPrefab.tag = "Meadow";
		GrassPrefab.tag = "Grass";

		Tile firstTile = Tile.CreateComponent(new Vector2 (0, 0), gameObject);
		map = new Graph (firstTile, null);
		unvisited_vertices = new List<Tile>();
		unvisited_vertices.Add(firstTile);

		int maxNumberTile = rand.Next (500, 600);
		
		while(map.vertices.Count < maxNumberTile)
		{
			Tile curr = unvisited_vertices[0];

			Tile up = Tile.CreateComponent(new Vector2(curr.point.x-1, curr.point.y), gameObject);
			Tile down =Tile.CreateComponent(new Vector2(curr.point.x-1, curr.point.y), gameObject);
			Tile leftup = Tile.CreateComponent(new Vector2(curr.point.x + 0.5f, curr.point.y + 0.75f), gameObject);
			Tile rightup = Tile.CreateComponent(new Vector2(curr.point.x + 0.5f, curr.point.y - 0.75f), gameObject);
			Tile leftdown = Tile.CreateComponent(new Vector2(curr.point.x - 0.5f, curr.point.y + 0.75f), gameObject);
			Tile rightdown = Tile.CreateComponent(new Vector2(curr.point.x - 0.5f, curr.point.y - 0.75f), gameObject);
			
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

		int tilesToRemove = rand.Next (100, 150);
		while(tileRemoved < tilesToRemove)
		{
			if(count > 10000)
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
			tileRemoved++;
			
			count++;
		}
		
		foreach(Tile n in map.vertices)
		{
			int probability = rand.Next(0,100);
			if( probability > 0 && probability <= 20)
			{
				n.InstantiateTree(TreePrefab);
			}
			else if( probability > 20 && probability <=30)
			{
				n.InstantiateMeadow(MeadowPrefab);
			}
			else
			{
				n.InstantiateGrass(GrassPrefab);
			}
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void initializeVillagesOnMap(List<Player> players)
	{

		foreach ( Tile t in map.vertices )
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

					int num = rand.Next(0, TilesToReturn.Count - 1);
					Tile location = TilesToReturn[num];

					Village newVillage = Village.CreateComponent(p, TilesToReturn, location, HovelPrefab );
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
		}
	}
	
}