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

	public Text _WoodText;
	public Text _GoldText;

	private Tile _move;
	private List<Tile> _moves;

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
	
	//Functions for when Village is pressed
	public void peasantPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hirePeasant (v,PeasantPrefab);
		VillageCanvas.enabled = false;
	}
	public void villageUpgradePressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.upgradeVillage (v);
		VillageCanvas.enabled = false;
	}
	public void closeVillagePressed()
	{
		VillageCanvas.enabled = false;
	}

	public void unitPressed()
	{
		UnitCanvas.enabled = true;

	}
	void ClearSelections()
	{
		_Unit = null;
		_move = null;
		_moves = null;
		_Tile = null;
	}

	public void cancelUnitMovePressed()
	{
		UnitCanvas.enabled = false;
		ClearSelections ();
	}

	void validateMove(RaycastHit hit)
	{
		if(_Unit != null && _moves != null && _Unit.GetComponent<Unit>().myAction == ActionType.ReadyForOrders)
		{
			_Tile = hit.collider.gameObject;
			Tile selection = _Tile.GetComponent<Tile>();
			Debug.LogWarning (_move);
			if(_moves.Contains( selection ))
			{
				_move = selection;
			}
			if( _move != null )
			{
				UnitCanvas.enabled = false;
				Unit u = _Unit.GetComponent<Unit>();

				if( _move.canUnitMove(u.getUnitType() ) )
				{
					print ("doing the move now");
					u.movePrefab(new Vector3(_move.point.x, 0.15f, _move.point.y));
					unitManager.performMove(u, _move);
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
					case "Hovel": case "Town": case "Fort":
					{
						VillageCanvas.enabled = true;
						_Village = hit.collider.gameObject;
						Village v = _Village.GetComponent<Village>();
						int redrawWood = v.getWood();
						int redrawGold = v.getGold ();
						_WoodText.text = redrawWood.ToString();
						_GoldText.text = redrawGold.ToString ();
						break;
					}
					case "Peasant": case "Infantry": case "Soldier": case "Knight":
					{
						_Unit = hit.collider.gameObject;
						Tile onIt = _Unit.GetComponent<Unit>().getLocation();
						_moves = onIt.neighbours;
						foreach(Tile t in _moves)
						{
							t.gameObject.renderer.material.color = Color.white;
						}
						UnitCanvas.enabled = true;
						print (hit.collider.tag);
						break;
					}
					case "Tile":
					{
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
			if(ErrorCanvas.enabled == true)
			{
				ErrorCanvas.enabled = false;
			}
			//TODO: bring up the esc menu
			else{

			}
		}

		if (_Unit != null && _Unit.GetComponent<Unit>().myAction == ActionType.Moved ) 
		{
			UnitCanvas.enabled = false;
			ErrorCanvas.enabled = true;
		}
	}

}
