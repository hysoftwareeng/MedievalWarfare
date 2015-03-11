﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum LandType
{
	Grass,
	Trees,
	Meadow,
	TombStone
}

//Tile Data Structure for building Graphs
public class Tile : MonoBehaviour
{
	public Vector2 point;
	public List<Tile> neighbours;
	private LandType myType;
	private Unit occupyingUnit;
	private bool visited;
	private int color;
	private bool isRoad;
	private Village myVillage;
	public Shader outline;
	public System.Random rand = new System.Random();
	public GameObject prefab;
	private Structure occupyingStructure;


	//This function should not be used, the Tile component is now always attached to a Grass Tile
	public static Tile CreateComponent (Vector2 pt, GameObject g) {
		Debug.Log ("----------Tile.CreateComponent() ran--------------");
		Tile myTile = g.AddComponent<Tile>();
		myTile.point = pt;
		myTile.visited = false;
		myTile.neighbours = new List<Tile>();
		return myTile;
	}

	//newly created constructor. This will be called whenever a gameobject containing Tile.cs gets instantiated
	public Tile (){
		visited = false;
		neighbours = new List<Tile>();
	}
	

	public void addNeighbour(Tile t)
	{
		if(this.neighbours.Where(
			n => n.point.x == t.point.x && n.point.y == t.point.y).Count() == 0)
		{
			this.neighbours.Add(t);
		}
	}
	//This method shouldn't be called
	public void InstantiateTree( GameObject TreePrefab)
	{
		Debug.Log ("------Tile.InstanciateTree------");
		prefab = Instantiate(TreePrefab, new Vector3(this.point.x, 0.15f, this.point.y), TreePrefab.transform.rotation) as GameObject;
		this.setLandType( LandType.Trees );
	}


	//This method shouldn't be called
	public void InstantiateMeadow( GameObject MeadowPrefab )
	{
		Debug.Log ("-----Tile.InstanciateMeadow------");
		prefab = Instantiate(MeadowPrefab, new Vector3(this.point.x, 0.15f, this.point.y), MeadowPrefab.transform.rotation) as GameObject;
		this.setLandType( LandType.Meadow );
	}

	void Start()
	{
		outline = Shader.Find("Glow");
	}

	public void replace(GameObject newPref)
	{
		Destroy (this.prefab);
		this.prefab = newPref;
	}

	public void colorTile()
	{
		if( this.color == 0 )
		{
			gameObject.renderer.material.color = new Color(1.0f, 0.0f, 1.0f, 0.05f);
		}
		else if ( this.color == 1 )
		{
			gameObject.renderer.material.color = new Color(0.0f, 0.0f, 1.0f, 0.05f);
		}
	}
	
	void OnMouseEnter()
	{
		this.renderer.material.shader = outline;
	}

	void OnMouseExit()
	{
		this.renderer.material.shader = Shader.Find("Diffuse");
	}

	public void setLandType(LandType type)
	{
		this.myType = type;
	}
	public LandType getLandType()
	{
		return this.myType;
	}
	
	public Unit getOccupyingUnit()
	{
		return this.occupyingUnit;
	}

	public void setOccupyingUnit(Unit u)
	{
		this.occupyingUnit = u;
	}

	public Village getVillage()
	{
		return myVillage;
	}

	public void setVillage(Village v)
	{
		this.myVillage = v;
	}

	public List<Tile> getNeighbours()
	{
		return neighbours;
	}

	public int getColor()
	{
		return color;
	}
	
	public void setColor(int i)
	{
		this.color = i;
	}

	public bool getVisited()
	{
		return this.visited;
	}
	
	public void setVisited(bool isVisited)
	{
		this.visited = isVisited;
	}

	public void buildRoad()
	{
		this.isRoad = true;
	}

	public bool checkRoad()
	{
		return this.isRoad;
	}

	public bool canUnitMove(UnitType type)
	{
		if(occupyingStructure == null || myType != LandType.Trees)
		{
			return true;
		}
		else if(occupyingStructure != null || (type == UnitType.KNIGHT && myType == LandType.Trees) || occupyingUnit != null)
		{
			return false;
		}
		return false;
	}

	[RPC]
	public void setLandTypeNet(int type)
	{
		this.myType = (LandType)type;
	}

	[RPC]
	void destroyTile(NetworkViewID tileid){
		Destroy (NetworkView.Find (tileid).gameObject);
	}
	[RPC]
	void setPrefab (NetworkViewID prefID ){
		prefab = NetworkView.Find (prefID).gameObject;
	}
	[RPC]
	void setAndColor(int newColor){
		color = newColor;
		if( color == 0 )
		{
			gameObject.renderer.material.color = new Color(1.0f, 0.0f, 1.0f, 0.05f);
		}
		else if ( color == 1 )
		{
			gameObject.renderer.material.color = new Color(0.0f, 0.0f, 1.0f, 0.05f);
		}
		else if ( color == 2 )
		{
			gameObject.renderer.material.color = Color.white;
		}
	}
	
	[RPC]
	void setPointN(Vector3 pt){
		this.point.x = pt.x;
		this.point.y = pt.z;
	}
	
	[RPC]
	public void addNeighbourN(NetworkViewID tileID)
	{
		Tile t = NetworkView.Find (tileID).GetComponent<Tile>();
		if(this.neighbours.Where(
			n => n.point.x == t.point.x && n.point.y == t.point.y).Count() == 0)
		{
			this.neighbours.Add(t);
		}
	}
	[RPC]
	//replaces the prefab on this tile ie: replace tree with hovel
	void replaceTilePrefabNet(NetworkViewID prefID){
		Destroy (prefab);
		prefab = NetworkView.Find (prefID).gameObject;
	}

	[RPC]
	//sets the village of the tile to new village attached to that gameObject
	void setVillageNet(NetworkViewID villageID){
		myVillage = NetworkView.Find (villageID).gameObject.GetComponent<Village>();
	}

}