//Copyright (c) 2011 Martin Cvengros (r6)
//This file is part of Endgame: Singularity II.

//Endgame: Singularity II is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 2 of the License, or
//(at your option) any later version.

//Endgame: Singularity II is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Endgame: Singularity II; if not, write to the Free Software
//Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

//This file is used to display the base list at a given location

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocationScreen : MonoBehaviour, IESGUIDialog {
	
	// dialogs 'state machine'
	delegate void GUIMethod ();

	GUIMethod newBaseGUI = null, confirm_destroyGUI = null, newBaseNameGUI = null ;
	
	IESGUIDialog parent = null, childDlg = null;
	
	public void Show (IESGUIDialog _parent)
	{
		if (_parent != null)
			this.parent = _parent;
			
		this.childDlg = null;
		this.newBaseGUI = this.confirm_destroyGUI = this.newBaseNameGUI = null;
		this.enabled = true;
		
		this.selectedBaseIdx = this.selectedNewBaseTypeIdx = this.selectedNewBaseTypeIdxPrev = -1;
		
		this.base_type_info = string.Empty;
		
		G.play_music ("");
		
		this.rebuild( true );
	}
	
	void rebuild( bool preselect )
	{
		if (this.location != null) {
			
			this.location.bases.Sort ();
			
			int count = 0;
			this.baseNames = new string[this.location.bases.Count];
			this.baseStatuses = new string[this.location.bases.Count];
			this.basePowerStates = new string[this.location.bases.Count];
			this.basePowerStateColors = new Color[this.location.bases.Count];
			this.baseSelectionItems = new string[this.location.bases.Count];
			
			foreach (Base _base in this.location.bases) {
				if (_base != null) {
					this.baseNames[count] = _base.name;
					
					this.basePowerStates[count] = _base.power_state.ToUpper ();
					this.basePowerStateColors[count] = this.state_colors[_base.power_state];
					
					string status = string.Empty;
					
					if (!_base.done)
						status = "Building Base";
					else if (!string.IsNullOrEmpty ((_base.type as Base_Class).force_cpu))
						status = "";
					else if (_base.cpus == null && _base.extra_items[0] == null && _base.extra_items[1] == null && _base.extra_items[2] == null)
						// == [None] * 3
						status = "Empty";
					else if (_base.cpus == null)
						status = "Incomplete";
					else if (!_base.cpus.done)
						status = "Building CPU";
					else if (_base.extra_items.Exists (e => e != null && !e.done))
						status = "Building Item";
					else
						status = "Complete";
					
					this.baseStatuses[count] = status;
					
					this.baseSelectionItems[count] = string.Format ("{0,-30} {1,-15} {2,-15}", this.baseNames[count], this.baseStatuses[count], this.basePowerStates[count]);
					
					count++;
				}
			}
			
			if (preselect && count > 0)
				this.selectedBaseIdx = 0;
			
			if ( count == 0 )
				this.selectedBaseIdx = -1;
			else if ( count == 1 )
				this.selectedBaseIdx = 0;
		}
	}
	
	public Location location = null;
	
	Dictionary<string,Color> state_colors = new Dictionary<string, Color>() {
		{"active", Color.green}
		,{"sleep", Color.yellow}
		,{"stasis", Color.gray}
		,{"overclocked", new Color( 255, 125, 0, 255)} // orange
		,{"suicide", Color.red}
		,{"entering_stasis", Color.gray}
		,{"leaving_stasis", Color.gray}
	};
	
	List<string> state_list = new List<string>() { "sleep", "active" };
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	
	void Update ()
	{
		foreach( Touch touch in Input.touches )
		{
			if ( touch.phase == TouchPhase.Moved )
			{
				// dragging
				this.scrollPositionBases.y += touch.deltaPosition.y;
				this.scrollPositionBaseTypes.y += touch.deltaPosition.y;
				this.scrollPositionLabel.y += touch.deltaPosition.y;
			}
		}
	}
	
	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue((int)GUI_bindings.SCREEN_DEPTH.Location);
		
		this.LocationGUI ();
	}
	
	int selectedBaseIdx = -1;
	
	Vector2 scrollPositionBases = Vector2.zero;
	
	string[] baseNames =  null, baseStatuses = null, basePowerStates = null, baseSelectionItems = null;
	Color[] basePowerStateColors = null;
	
	void LocationGUI ()
	{
		GUI.enabled = this.childDlg == null && this.newBaseGUI == null && this.confirm_destroyGUI == null && this.newBaseNameGUI == null;
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT);
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label (this.location.name);
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal ();
		
		scrollPositionBases = GUILayout.BeginScrollView (scrollPositionBases);
		
		this.selectedBaseIdx = GUILayout.SelectionGrid (this.selectedBaseIdx, this.baseSelectionItems, 1, GUI_bindings.Instance.SelectionGridAlt() );
		
		GUILayout.EndScrollView ();
		
		GUILayout.FlexibleSpace();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("Open Base"))
		{
			if (selectedBaseIdx >= 0)
			{
				Base _base = this.location.bases.Find (f => f.name == this.baseNames[selectedBaseIdx]);
				if ( _base.done )
				{
					this.childDlg = this.GetComponent<BaseScreen> ();
					(this.childDlg as BaseScreen)._base = _base;
					this.childDlg.Show (this);
				}
				G.play_sound();
			}
		}
		
		if (GUILayout.Button ("Power State"))
		{
			if (selectedBaseIdx >= 0)
			{
				Base _base = this.location.bases.Find (f => f.name == this.baseNames[selectedBaseIdx] ) ;
				int old_index = state_list.IndexOf (_base.power_state);
				_base.power_state = state_list[(old_index + 1) % state_list.Count];
				_base.check_power ();
				this.rebuild( false );
				G.play_sound();
			}
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("New base"))
		{
			List<Base_Class> list = G.base_type.Values.Where ( w => w.available() && w.regions.Contains( this.location.id ) ).ToList();
			list.Sort();
			list.Reverse();
			
			this.base_types_names = list.Select( s => s.name).ToArray();
			
			if ( this.base_types_names.Count() == 1 )
			{
				this.selectedNewBaseTypeIdx = 0;
				this.selectedNewBaseTypeIdxPrev = -1;
			}
			
			this.newBaseGUI = this.NewBaseGUI;
			G.play_sound();
		}
		
		if (GUILayout.Button ("Destroy base"))
		{
			if ( this.selectedBaseIdx >= 0 )
			{
				this.confirm_destroyGUI = this.DestroyConfirmationGUI;
				G.play_sound();
			}
		}
		
		if (GUILayout.Button ("Back"))
		{
			this.enabled = false;
			this.parent.Show (null);
			G.play_sound();
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea ();
		
		if (this.newBaseGUI != null)
			this.newBaseGUI ();
		
		if (this.confirm_destroyGUI != null)
			this.confirm_destroyGUI ();
	}
	
	string[] base_types_names;
	
	int selectedNewBaseTypeIdx = -1, selectedNewBaseTypeIdxPrev = -1;
	Vector2 scrollPositionBaseTypes = Vector2.zero, scrollPositionLabel = Vector2.zero;
	
	string base_type_info ;
	Base_Class base_type;
	
	void NewBaseGUI ()
	{
		GUI.enabled = this.newBaseNameGUI == null;
		
		GUI.Box ( GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUI.Box ( GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUI.Box ( GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUILayout.BeginArea ( GUI_bindings.MESSAGE_BOX_RECT );
		
		GUILayout.BeginHorizontal ();
		
		scrollPositionBaseTypes = GUILayout.BeginScrollView (scrollPositionBaseTypes);
		
		this.selectedNewBaseTypeIdx = GUILayout.SelectionGrid (this.selectedNewBaseTypeIdx, base_types_names, 1, GUI_bindings.Instance.SelectionGrid() );
		
		GUILayout.EndScrollView ();
		
		if (this.selectedNewBaseTypeIdx != this.selectedNewBaseTypeIdxPrev)
		{
			this.selectedNewBaseTypeIdxPrev = this.selectedNewBaseTypeIdx;
			this.base_type = G.base_type.Values.First( f => f.name == this.base_types_names[this.selectedNewBaseTypeIdx] );
			this.base_type_info = this.base_type.get_info (this.location);
		}
		
		this.scrollPositionLabel = GUILayout.BeginScrollView (this.scrollPositionLabel, GUILayout.MaxWidth( (int)(Screen.width / 2 ) ) );
		
		GUILayout.Label (this.base_type_info, GUI_bindings.Instance.LabelAlt(true,null, false), GUILayout.MaxWidth( (int)(Screen.width / 2 ) ) );
		
		GUILayout.EndScrollView ();

		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("OK"))
		{
			if (this.selectedNewBaseTypeIdx >= 0)
			{
				this.new_base_name = this.generate_base_name (this.location, this.base_type);
				this.newBaseNameGUI = this.NewBaseName;
				G.play_sound();
			}
		}
		
		if (GUILayout.Button ("Back"))
		{
			this.newBaseGUI = null;
			G.play_sound();
		}

		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea();
		
		if (this.newBaseNameGUI != null)
			this.newBaseNameGUI ();
	}
	
	List<int> significant_numbers = new List<int>() {
		42,	// The Answer.
		7,	// Classic.
		23,   // Another.
		51,   // Area.
		19,   // From the Dark Tower.
		4,
		8,
		15,
		16,   // Four of the Lost numbers.  The other two are '23' and '42'.
		13,   // Lucky or unlucky?
		1414, // Square root of 2
		1947, // Roswell.
		2012, // Mayan calendar ending.
		2038, // End of UNIX 32-bit time.
		1969, // Man lands on the moon.
		2043, // No meaning--confusion! :)
		2029, // Predicted date of AI passing a Turing Test by Kurzweil.
		3141, // ... if you don't know what this is, you should go away.
		1618, // Golden ratio.
		2718, // e
		29979 // Speed of light in a vacuum. (m/s, first 5 digits.)
	};
	
	//// Generates a name for a base, given a particular location.
	string generate_base_name(Location location, Base_Class base_type)
	{
		string name = string.Empty;
		string city = string.Empty;
		
	    // First, decide whether we're going to try significant values or just
	    // choose one randomly.
	    if (Random.value < 0.3 ) // 30% chance.
		{
			int attempts = 0;
			bool done = false;
			
			bool duplicate = false;
	        while (!done && attempts < 5)
			{
				city = location.cities.Count > 0 ? location.cities[Random.Range(0, location.cities.Count)] : string.Empty;
	            name =  city + " " + base_type.flavor[Random.Range(0,base_type.flavor.Count)] + " " + this.significant_numbers[Random.Range(0,this.significant_numbers.Count)].ToString();
	            duplicate = false;
				foreach( Base _base in location.bases )	{
	                if ( _base.name == name ) {
	                    duplicate = true;
	                    break;
					}
				}

				if (duplicate)
					attempts += 1;
				else
					done = true;
			}
			
			if (done)
            	return name;
		}
		
		// This is both the else case and the general case.
		city = location.cities.Count > 0 ? location.cities[Random.Range(0, location.cities.Count)] : string.Empty;
		name = city + " " + base_type.flavor[Random.Range(0,base_type.flavor.Count)] + " " + Random.Range(0, 32767).ToString();

    	return name;
	}
	
	string new_base_name = string.Empty;
	
	void NewBaseName ()
	{
		GUI.enabled = true;
		
		GUI.Box ( GUI_bindings.MESSAGE_BOX_RECT, G.strings["new_base_text"]);
		
		GUI.Box ( GUI_bindings.MESSAGE_BOX_RECT, G.strings["new_base_text"]);
		
		GUI.Box ( GUI_bindings.MESSAGE_BOX_RECT, G.strings["new_base_text"]);
		
		GUILayout.BeginArea ( GUI_bindings.MESSAGE_BOX_RECT);
		
		this.new_base_name = GUILayout.TextField (this.new_base_name, GUI_bindings.Instance.TextFieldAlt());
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("OK"))
		{
			this.location.add_base (new Base (this.new_base_name, this.base_type, false));
			this.rebuild ( false );
			this.newBaseNameGUI = null;
			this.newBaseGUI = null;
			G.play_sound();
		}
		
		if (GUILayout.Button ("Cancel"))
		{
			this.newBaseNameGUI = null;
			this.newBaseGUI = null;
			G.play_sound();
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea();
	}
	
	void DestroyConfirmationGUI ()
	{
		GUI.enabled = true;
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT );
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label (this.location.name + " / " + this.baseNames[this.selectedBaseIdx]);
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label ("Are you sure you want to destroy this base?", GUI_bindings.Instance.LabelAlt(true,null, false));
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("Yes") )
		{
			Base _base = this.location.bases.Find (f => f.name == this.baseNames[selectedBaseIdx] );
			_base.destroy ();
			this.rebuild( true );
			this.confirm_destroyGUI = null;
			G.play_sound();
		}
		
		if (GUILayout.Button ("No") )
		{
			this.confirm_destroyGUI = null;
			G.play_sound();
		}
		
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
		
	}
}
