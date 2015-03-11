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
	public GameObject PeasantPrefab;

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

	private Tile _move;

	public VillageManager villageManager;
	public UnitManager unitManager;
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
	}
	
	//Functions for when a Village is selected
	public void hirePeasantPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hirePeasant (v,PeasantPrefab);
		int redrawUnits = v.getControlledUnits().Count();
		_UnitsText.text = redrawUnits.ToString();
		VillageCanvas.enabled = false;
	}
	public void villageUpgradePressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.upgradeVillage (v);
		int redrawWood = v.getWood();
		int redrawGold = v.getGold();
		_WoodText.text = redrawWood.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
	}
	public void closeVillagePressed()
	{
		VillageCanvas.enabled = false;
	}

	//Functions for when a Unit is selected
	public void unitPressed()
	{
		UnitCanvas.enabled = true;
	}
	void ClearSelections()
	{
		_Unit = null;
		_move = null;
		_Tile = null;
		_isAUnitSelected = false;
	}

	public void cancelUnitPressed()
	{
		UnitCanvas.enabled = false;
		ClearSelections ();
	}

	public void unitUpgradeInfantryPressed()
	{
		//When you upgrade a unit, you only need to redraw the gold on the HUD
		Unit u = _Unit.GetComponent<Unit>();
		unitManager.upgradeUnit(u,UnitType.INFANTRY);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();

	}

	public void unitUpgradeSoldierPressed()
	{
		Unit u = _Unit.GetComponent<Unit>();
		unitManager.upgradeUnit(u,UnitType.SOLDIER);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();

	}

	public void unitUpgradeKnightPressed()
	{
		Unit u = _Unit.GetComponent<Unit>();
		unitManager.upgradeUnit(u,UnitType.KNIGHT);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();

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

				if( _move.canUnitMove(u.getUnitType() ) )
				{
					print ("doing the move now");
					u.movePrefab(new Vector3(_move.point.x, 0.15f, _move.point.y));
					unitManager.moveUnit(u, _move);
					print ("finished moving");
				}
				else
				{
					ErrorCanvas.enabled = true;
				}
				ClearSelections();
			}

		}
	}

	// Update is called once per frame
	void Update()
	{

		Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		//if clicked
		if (Input.GetMouseButtonDown(0))
		{

			if (Physics.Raycast(ray, out hit)){

				switch(hit.collider.tag)
				{
					case "Town":
					{
						VillageCanvas.enabled = true;
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
					case "Peasant": case "Infantry": case "Soldier": case "Knight":
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
						print (hit.collider.tag);
						_isAUnitSelected = true;
						break;
					}
					case "Grass":
					{
						print ("in tile");
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
			}
			if(UnitCanvas.enabled == true)
			{
				UnitCanvas.enabled = false;
			}
			if(ErrorCanvas.enabled == true)
			{
				ErrorCanvas.enabled = false;
			}

			//TODO: bring up the esc menu
			else{

			}
		}

	}

}
