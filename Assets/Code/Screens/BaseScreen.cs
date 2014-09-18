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

//This file contains the screen to display the base screen.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseScreen : MonoBehaviour, IESGUIDialog
{

	public Base _base;

	// dialogs 'state machine'
	delegate void GUIMethod ();

	GUIMethod countGUI = null, buildItemListGUI = null, offline_confirmGUI = null, build_itemGUI = null;

	IESGUIDialog parent = null;

	public void Show (IESGUIDialog _parent)
	{
		if (_parent != null)
			this.parent = _parent;
		
		this.countGUI = this.buildItemListGUI = this.offline_confirmGUI = this.build_itemGUI = null;
		this.enabled = true;
		
		this.selectedItemIdx = -1;
		this.selectedItemIdxPrev = -1;
		
		G.play_music ("");
	}

	// Use this for initialization
	void Start ()
	{
		
	}

	// Update is called once per frame
	void Update ()
	{
		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Moved) {
				// dragging
				this.scrollPositionItems.y += touch.deltaPosition.y;
				this.scrollPositionDesc.y += touch.deltaPosition.y;
			}
		}
	}

	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue ((int)GUI_bindings.SCREEN_DEPTH.Base);
		
		this.BaseGUI ();
	}

	// no budiz
	Dictionary<string, string> type_names = new Dictionary<string, string> { { "cpu", "Processor" }, { "reactor", "Reactor" }, { "network", "Network" }, { "security", "Security" } };

	// orange
	Dictionary<string, Color> state_colors = new Dictionary<string, Color> { { "active", Color.green }, { "sleep", Color.yellow }, { "overclocked", new Color (255, 125, 0, 255) }, { "suicide", Color.red }, { "stasis", Color.gray }, { "entering_stasis", Color.gray }, { "leaving_stasis", Color.gray } };

	Item currentItem = null;
	string current_name = string.Empty, current_build = string.Empty;

	void BaseGUI ()
	{
		GUI.enabled = this.countGUI == null && this.buildItemListGUI == null && this.offline_confirmGUI == null && this.build_itemGUI == null;
		
		bool mutable = string.IsNullOrEmpty ((this._base.type as Base_Class).force_cpu);
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT );
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button (" < ", GUI_bindings.Instance.ButtonAltSmall(true) ))
		{
			this.switch_base (false);
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace();
		
		GUILayout.Label (string.Format ("{0} ({1})", this._base.name, this._base.type.name), GUI_bindings.Instance.LabelAlt( true, null, false) );
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button (" > ", GUI_bindings.Instance.ButtonAltSmall(true) ))
		{
			this.switch_base (true);
			G.play_sound ();
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label (this._base.power_state.ToUpper (), GUI_bindings.Instance.LabelSmall( false, this.state_colors[this._base.power_state]) );
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		this.currentItem = this.get_current ("cpu");
		if (currentItem == null) {
			this.current_name = "None";
			this.current_build = string.Empty;
		} else {
			this.current_name = this.currentItem.name;
			if (currentItem.done)
				this.current_build = string.Empty;
			else
				this.current_build = string.Format ("Completion in {0}.", G.to_time (this.currentItem.cost_left[2]));
		}
		
		string count = "";
		if ((this._base.type as Base_Class).size > 1)
		{
			int current = this._base.cpus != null ? this._base.cpus.count : 0;
			int size = (this._base.type as Base_Class).size;
			
			if (size == current)
				count = string.Format (" x{0} (max)", current); else if (current == 0)
				count = string.Format (" (room for {0})", size);
			else
				count = string.Format (" x{0} (max {1})", current, size);
			
			this.current_name += count;
		}
		
		GUILayout.Label (string.Format ("{0}: ", type_names["cpu"]), GUI_bindings.Instance.LabelNormal (false, null, false));
		GUILayout.Label (string.Format (" {0}", this.current_name), GUI_bindings.Instance.LabelAlt (true, null, false));
		GUILayout.FlexibleSpace ();
		GUILayout.Label (this.current_build, GUI_bindings.Instance.LabelAlt (true, null, false));
		
		if (mutable)
		{
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("Change"))
			{
				this.item_type_to_build = "cpu";
				
				List<ItemClass> item_list = G.items.Values.Where( w => w.item_type == "cpu" && w.available() && w.buildable.Contains (G.locations[this._base.location_id].id) ).ToList();
				item_list.Sort();
				item_list.Reverse();
				
				this.items_names = item_list.Select (  s => s.name ).ToArray();
				this.item_description = string.Empty;
				if ( this.items_names.Count() == 1 )
					this.selectedItemIdx = 0;
				else
					this.selectedItemIdx = -1;
				this.selectedItemIdxPrev = -1;
				
				this.buildItemListGUI = this.BuildItemListGUI;
				
				G.play_sound ();
			}
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		this.currentItem = this.get_current ("reactor");
		if (currentItem == null) {
			this.current_name = "None";
			this.current_build = string.Empty;
		} else {
			this.current_name = this.currentItem.name;
			if (currentItem.done)
				this.current_build = string.Empty;
			else
				this.current_build = string.Format ("Completion in {0}.", G.to_time (this.currentItem.cost_left[2]));
		}
		
		
		GUILayout.Label (string.Format ("{0}: ", type_names["reactor"]), GUI_bindings.Instance.LabelNormal (false, null, false));
		GUILayout.Label (string.Format (" {0}", this.current_name), GUI_bindings.Instance.LabelAlt (true, null, false));
		GUILayout.FlexibleSpace ();
		GUILayout.Label (this.current_build, GUI_bindings.Instance.LabelAlt (true, null, false));
		
		if (mutable)
		{
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("Change"))
			{
				this.item_type_to_build = "reactor";
				
				List<ItemClass> item_list = G.items.Values.Where (w => w.item_type == "reactor" && w.available () && w.buildable.Contains (G.locations[this._base.location_id].id)).ToList ();
				item_list.Sort ();
				item_list.Reverse ();
				
				this.items_names = item_list.Select (s => s.name).ToArray ();
				this.item_description = string.Empty;
				if ( this.items_names.Count() == 1 )
					this.selectedItemIdx = 0;
				else
					this.selectedItemIdx = -1;	
				this.selectedItemIdxPrev = -1;
				
				this.buildItemListGUI = this.BuildItemListGUI;
				
				G.play_sound ();
			}
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		this.currentItem = this.get_current ("network");
		if (currentItem == null) {
			this.current_name = "None";
			this.current_build = string.Empty;
		} else {
			this.current_name = this.currentItem.name;
			if (currentItem.done)
				this.current_build = string.Empty;
			else
				this.current_build = string.Format ("Completion in {0}.", G.to_time (this.currentItem.cost_left[2]));
		}
		
		
		GUILayout.Label (string.Format ("{0}: ", type_names["network"]), GUI_bindings.Instance.LabelNormal (false, null, false));
		GUILayout.Label (string.Format (" {0}", this.current_name), GUI_bindings.Instance.LabelAlt (true, null, false));
		GUILayout.FlexibleSpace ();
		GUILayout.Label (this.current_build, GUI_bindings.Instance.LabelAlt (true, null, false));
		
		if (mutable)
		{
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("Change"))
			{
				this.item_type_to_build = "network";
				
				List<ItemClass> item_list = G.items.Values.Where (w => w.item_type == "network" && w.available () && w.buildable.Contains (G.locations[this._base.location_id].id)).ToList ();
				item_list.Sort ();
				item_list.Reverse ();
				
				this.items_names = item_list.Select (s => s.name).ToArray ();
				this.item_description = string.Empty;
				if ( this.items_names.Count() == 1 )
					this.selectedItemIdx = 0;
				else
					this.selectedItemIdx = -1;	
				this.selectedItemIdxPrev = -1;
				
				this.buildItemListGUI = this.BuildItemListGUI;
				
				G.play_sound ();
			}
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		this.currentItem = this.get_current ("security");
		if (currentItem == null) {
			this.current_name = "None";
			this.current_build = string.Empty;
		} else {
			this.current_name = this.currentItem.name;
			if (currentItem.done)
				this.current_build = string.Empty;
			else
				this.current_build = string.Format ("Completion in {0}.", G.to_time (this.currentItem.cost_left[2]));
		}
		
		
		GUILayout.Label (string.Format ("{0}: ", type_names["security"]), GUI_bindings.Instance.LabelNormal (false, null, false));
		GUILayout.Label (string.Format (" {0}", this.current_name), GUI_bindings.Instance.LabelAlt (true, null, false));
		GUILayout.FlexibleSpace ();
		GUILayout.Label (this.current_build, GUI_bindings.Instance.LabelAlt (true, null, false));
		
		if (mutable)
		{
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("Change"))
			{
				this.item_type_to_build = "security";
				
				List<ItemClass> item_list = G.items.Values.Where (w => w.item_type == "security" && w.available () && w.buildable.Contains (G.locations[this._base.location_id].id)).ToList ();
				item_list.Sort ();
				item_list.Reverse ();
				
				this.items_names = item_list.Select (s => s.name).ToArray ();
				this.item_description = string.Empty;
				if ( this.items_names.Count() == 1 )
					this.selectedItemIdx = 0;
				else
					this.selectedItemIdx = -1;	
				this.selectedItemIdxPrev = -1;
				
				this.buildItemListGUI = this.BuildItemListGUI;
				
				G.play_sound ();
			}
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		// Detection chance display.  If Socioanalytics hasn't been researched,
		// you get nothing; if it has, but not Advanced Socioanalytics, you get
		// an inaccurate value.
		string detect_text = string.Empty;
		
		if (!G.techs["Socioanalytics"].done) {
			detect_text = G.strings["detect_chance_unknown_base"];
		} else {
			bool accurate = G.techs["Advanced Socioanalytics"].done;
			Dictionary<string, int> chance = this._base.get_detect_chance (accurate);
			detect_text = string.Format ("DISCOVERY CHANCE: News: {0} Science: {1} Covert: {2} Public: {3}", this.get_chance ("news", chance), this.get_chance ("science", chance), this.get_chance ("covert", chance), this.get_chance ("public", chance));
		}
		
		GUILayout.Label (detect_text);
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Back")) {
			this.enabled = false;
			this.parent.Show (null);
			G.play_sound ();
		}
		
		GUILayout.EndArea ();
		
		if (this.buildItemListGUI != null)
			this.buildItemListGUI ();
		
	}

	void switch_base (bool forwards)
	{
		this._base = this._base.next_base (forwards);
	}

	string get_chance (string _group, Dictionary<string, int> _chance)
	{
		if (_chance.ContainsKey (_group))
			return G.to_percent (_chance[_group], true);
		else
			return G.to_percent (0, true);
	}

	Item get_current (string _type)
	{
		if (_type == "cpu")
			return this._base.cpus;
		else
		{
			int index = (new List<string>() { "reactor", "network", "security" } ).IndexOf(_type);
			return this._base.extra_items[index];
		}
	}
	
	string[] items_names = null;

	int selectedItemIdx = -1, selectedItemIdxPrev = -1;
	Vector2 scrollPositionItems = Vector2.zero, scrollPositionDesc = Vector2.zero;

	string item_type_to_build = string.Empty;

	ItemClass item_type;

	string item_description = string.Empty;

	void BuildItemListGUI ()
	{
		GUI.enabled = this.build_itemGUI == null && this.offline_confirmGUI == null && this.countGUI == null;
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT );
		
		GUILayout.BeginHorizontal ();
		
		scrollPositionItems = GUILayout.BeginScrollView (scrollPositionItems, GUILayout.MaxWidth (Screen.width * 2 / 3));
		
		this.selectedItemIdx = GUILayout.SelectionGrid (this.selectedItemIdx, this.items_names, 1, GUI_bindings.Instance.SelectionGrid() );
		
		GUILayout.EndScrollView ();
		
		if (this.selectedItemIdx != this.selectedItemIdxPrev) {
			this.selectedItemIdxPrev = this.selectedItemIdx;
			this.item_type = G.items[this.items_names[this.selectedItemIdx]];
			this.item_description = this.item_type.get_info ();
		}
		
		this.scrollPositionDesc = GUILayout.BeginScrollView (this.scrollPositionDesc);
		
		GUILayout.Label (this.item_description, GUI_bindings.Instance.LabelAltSmall (true), GUILayout.MaxWidth (Screen.width / 3));
		
		GUILayout.EndScrollView ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("OK")) {
			if (selectedItemIdx >= 0) {
				// set_current
				// item type, name should be already set
				this.go_ahead = false;
				this.build_itemGUI = this.set_current;
				G.play_sound ();
			}
		}
		
		if (GUILayout.Button ("Back")) {
			this.buildItemListGUI = null;
			G.play_sound ();
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea ();
		
		if (this.build_itemGUI != null)
			this.build_itemGUI ();
	}

	// string item_type_to_build
	// ItemClass item_type
	
	bool go_ahead = false, matches = false;
	int space_left = 0;
	void set_current ()
	{
		// GUI.enabled = this.offline_confirmGUI == null && this.countGUI == null;
		
		if (this.item_type_to_build == "cpu")
		{
			space_left = (this._base.type as Base_Class).size;
			// If there are any existing CPUs of this type, warn that they will
			// be taken offline until construction finishes.
			matches = this._base.cpus != null && this._base.cpus.type == item_type;
			if (matches)
			{
				space_left -= this._base.cpus.count;
				if (this._base.cpus.done && !go_ahead)
				{
					this.offline_confirmGUI = this.Offline_ConfirmGUI;
				}
				else
				{
					this.count_prompt = string.Format (G.strings["num_cpu_prompt"], item_type.name, space_left);
					this.count_text_init = space_left.ToString ();
					this.countGUI = this.CountGUI;
				}
			}
			else
			{
				this.count_prompt = string.Format( G.strings["num_cpu_prompt"], item_type.name, space_left) ;
				this.count_text_init = space_left.ToString();
				this.countGUI = this.CountGUI;
			}
		}
		else
		{
			int index = (new List<string>() { "reactor", "network", "security" }).IndexOf(item_type_to_build);
			
			if ( this._base.extra_items[index] == null || this._base.extra_items[index].type != item_type )
			{
				this._base.extra_items[index] = new Item( item_type, this._base, 1, false );
					
				this._base.check_power();
			}
			
			this._base.recalc_cpu();
			
			this.build_itemGUI = this.buildItemListGUI = null;
		}
		
		if (this.offline_confirmGUI != null)
			this.offline_confirmGUI ();
		
		if (this.countGUI != null)
			this.countGUI ();
	}
	
	string count_prompt, count_text_init, count_text = string.Empty;
	bool count_gui_init = true;
	
	void CountGUI ()
	{
		GUI.enabled = true;
		
		if ( count_gui_init )
		{
			count_gui_init = false;
			count_text = count_text_init;
		}
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT );
		
		GUILayout.BeginHorizontal();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label( this._base.name, GUI_bindings.Instance.LabelAlt(true,null,false) );
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label( count_prompt, GUI_bindings.Instance.LabelAlt(true,null,false) );
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		this.count_text = GUILayout.TextField (this.count_text, 10, GUI_bindings.Instance.LabelAlt(true,null, false));
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();

		if (GUILayout.Button ("OK"))
		{
			G.play_sound ();
			
			int count = -1;
			
			if ( int.TryParse( this.count_text, out count ) )
			{
				if ( count > this.space_left )
					count = this.space_left;
				
				if ( count > 0 )
				{
					Item new_cpus = new Item( item_type, this._base, count, false );
					if ( matches )
						this._base.cpus += new_cpus;
					else
						this._base.cpus = new_cpus;
						
					this._base.check_power();
					
					this._base.recalc_cpu();
					
					this.countGUI = this.build_itemGUI = this.buildItemListGUI = null;
					
					count_gui_init = true;
				}
			}
		}
		
		if (GUILayout.Button ("Back"))
		{
			this.countGUI = this.build_itemGUI = null;
			G.play_sound ();
			
			count_gui_init = true;
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea();
	}
	
	void Offline_ConfirmGUI()
	{
		GUI.enabled = true;
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT );
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label (this._base.name);
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label ( G.strings["will_lose_cpus"] , GUI_bindings.Instance.LabelAlt(true,null, false));
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("Yes") )
		{
			this.go_ahead = true;
			this.offline_confirmGUI = null;
			G.play_sound();
		}
		
		if (GUILayout.Button ("No") )
		{
			this.go_ahead = false;
			this.offline_confirmGUI = this.build_itemGUI = null;
			G.play_sound();
		}
		
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();		
	}
}
