using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;


public class InGameGUI : MonoBehaviour {
	public Camera myCamera;
	public Canvas VillageCanvas;
	public Canvas UnitCanvas;
	public Canvas HUDCanvas;
	public Canvas ErrorCanvas;


	// prefabs
	public GameObject UnitPrefab;

	//selections
	private GameObject _Village;
	private GameObject _Unit;
	private GameObject _Tile;
	private GameObject _WoodValue;
	private GameObject _GoldValue;

	private bool _isAUnitSelected;

	public Text _WoodText;
	public Text _GoldText;
	public Text _RegionText;
	public Text _UnitsText;
	public Text _ErrorText;

	private Tile _move;

	private VillageManager villageManager;
	private UnitManager unitManager;

	private bool menuUp;
	// Use this for initialization
	void Start () 
	{
		myCamera =  GameObject.FindGameObjectWithTag("MainCamera").camera;
		villageManager = GameObject.Find("VillageManager").GetComponent<VillageManager>();
		unitManager = GameObject.Find("UnitManager").GetComponent<UnitManager>();
		HUDCanvas.enabled = true;
		VillageCanvas.enabled = false;
		UnitCanvas.enabled = false;
		ErrorCanvas.enabled = false;
		menuUp = false;
	}
	
	//Functions for when a Village is selected
	public void trainPeasantPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hirePeasant (v,UnitPrefab);
		int redrawUnits = v.getControlledUnits().Count();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}

	public void trainInfantryPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hireInfantry (v,UnitPrefab);
		int redrawUnits = v.getControlledUnits().Count();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}

	public void trainSoldierPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hireSoldier (v,UnitPrefab);
		int redrawUnits = v.getControlledUnits().Count();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}

	public void trainKnightPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hireKnight (v,UnitPrefab);
		int redrawUnits = v.getControlledUnits().Count();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}


	public void villageUpgradePressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.upgradeVillage (v);
		int redrawWood = v.getWood();
		_WoodText.text = redrawWood.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}
	public void closeVillagePressed()
	{
		VillageCanvas.enabled = false;
		menuUp = false;
	}

	//Functions for when a Unit is selected
	public void unitPressed()
	{
		UnitCanvas.enabled = true;
		menuUp = false;
	}

	public void cancelUnitPressed()
	{
		UnitCanvas.enabled = false;
		menuUp = false;
		ClearSelections ();
	}
	public void moveUnitPressed()
	{
		UnitCanvas.enabled = false;
		_isAUnitSelected = true;
		this.displayError("Please select a friendly or neutral tile 1 distance away to move to.");
		menuUp = false;

	}
	public void unitUpgradeInfantryPressed()
	{
		//When you upgrade a unit, you only need to redraw the gold on the HUD
		Unit u = _Unit.GetComponent<Unit>();
		unitManager.upgradeUnit(u,UnitType.INFANTRY);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();
		UnitCanvas.enabled = false;
		menuUp = false;

	}

	public void unitUpgradeSoldierPressed()
	{
		Unit u = _Unit.GetComponent<Unit>();
		unitManager.upgradeUnit(u,UnitType.SOLDIER);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();
		UnitCanvas.enabled = false;
		menuUp = false;

	}

	public void unitUpgradeKnightPressed()
	{
		Unit u = _Unit.GetComponent<Unit>();
		unitManager.upgradeUnit(u,UnitType.KNIGHT);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();
		UnitCanvas.enabled = false;
		menuUp = false;

	}

	public void displayError(string error)
	{
		VillageCanvas.enabled = false;
		UnitCanvas.enabled = false;
		_ErrorText.text = error;
		ErrorCanvas.enabled = true;

	}

	void validateMove(RaycastHit hit)
	{
		print ("in validateMove");
		if(_isAUnitSelected && (_Unit.GetComponent<Unit>().myAction == UnitActionType.ReadyForOrders || 
		                        _Unit.GetComponent<Unit>().myAction == UnitActionType.Moved) )
		{
			_Tile = hit.collider.gameObject;
			Tile selection = _Tile.GetComponent<Tile>();
			print (selection != null);
			Debug.Log(_Unit.GetComponent<Unit>().getLocation().neighbours);
			if(_Unit.GetComponent<Unit>().getLocation().neighbours.Contains( selection ))
			{
				_move = selection;
			}
			Debug.LogWarning (_move);
			if( _move != null )
			{
				UnitCanvas.enabled = false;
				Unit u = _Unit.GetComponent<Unit>();
				Village v = u.getVillage ();

				if( _move.canUnitMove(u.getUnitType() ) )
				{
					print ("doing the move now");
					u.movePrefab(new Vector3(_move.point.x, 0.15f, _move.point.y));
					unitManager.moveUnit(u, _move);

					//TODO This code is for taking over neutral tiles.
					//This code doesnt' work because MapGenerator isn't making a Game :( maybe it's an easy fix? :S
					if (selection.getVillage() == null)
					{
						v.addTile (selection);
						int redrawRegion = v.getControlledRegion().Count;
						_RegionText.text = redrawRegion.ToString();
					}
					//TODO This code is for cutting trees
					if(selection.getLandType () == LandType.Trees)
					{
						
						int redrawWood = v.getWood();
						_WoodText.text = redrawWood.ToString();

					}

				}
				else
				{
					this.displayError("Something's wrong with moving.");
				}
				ClearSelections();
			}

		}
	}
	
	void ClearSelections()
	{
		_Unit = null;
		_move = null;
		_Tile = null;
		_isAUnitSelected = false;
	}
	// Update is called once per frame
	void Update()
	{

		Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		//if clicked
		if (Input.GetMouseButtonDown(0)&& !menuUp)
		{

			if (Physics.Raycast(ray, out hit)){

				switch(hit.collider.tag)
				{
					case "Town":
					{
						VillageCanvas.enabled = true;
						menuUp = true;
						_Village = hit.collider.gameObject;
						Village v = _Village.GetComponent<Village>();
						int redrawWood = v.getWood();
						int redrawGold = v.getGold();
						int redrawRegion = v.getControlledRegion().Count();
						int redrawUnits = v.getControlledUnits().Count();
						_WoodText.text = redrawWood.ToString();
						_GoldText.text = redrawGold.ToString();
						_RegionText.text = redrawRegion.ToString();
						_UnitsText.text = redrawUnits.ToString();
						break;
					}

					case "Unit":
					{
						_Unit = hit.collider.gameObject;
						
						Unit u = _Unit.GetComponent<Unit>();
						Village v = u.getVillage();
						int redrawWood = v.getWood();
						int redrawGold = v.getGold();
						int redrawRegion = v.getControlledRegion().Count();
						int redrawUnits = v.getControlledUnits().Count();
						_WoodText.text = redrawWood.ToString();
						_GoldText.text = redrawGold.ToString();
						_RegionText.text = redrawRegion.ToString();
						_UnitsText.text = redrawUnits.ToString();

						Tile onIt = _Unit.GetComponent<Unit>().getLocation();
		
						UnitCanvas.enabled = true;
						menuUp = true;
						print (hit.collider.tag);
						break;
					}
					case "Grass":
					{
						ErrorCanvas.enabled = false;
						validateMove(hit);
						break;
					}
				}

			}
		}

		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			if(VillageCanvas.enabled == true)
			{
				VillageCanvas.enabled = false;
				menuUp = false;
			}
			if(UnitCanvas.enabled == true)
			{
				UnitCanvas.enabled = false;
				_isAUnitSelected = false;
				menuUp = false;
			}
			if(ErrorCanvas.enabled == true)
			{
				ErrorCanvas.enabled = false;
				menuUp = false;
			}

			//TODO: bring up the esc menu
			else{

			}
		}

	}

}
