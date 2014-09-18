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

//This file contains the global research screen.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResearchScreen : MonoBehaviour, IESGUIDialog {
	
	delegate void GUIMethod ();
	
	GUIMethod researchExplanationGUI = null;
	
	IESGUIDialog parent = null;
	
	bool dirty_count = true;
	List<long> cpu_left = null;
	
	public void Show (IESGUIDialog _parent)
	{
		if (_parent != null)
			this.parent = _parent;
		
		this.enabled = true;
		
		G.play_music ("");
		
		this.researchExplanationGUI = null;
		
		this.research_items_keys = new List<string> ();
		
		this.research_items_keys.Add ("cpu_pool");
		
		this.research_items_keys.Add ("jobs");
		
		List<Tech> list = G.techs.Values.Where ( w => w.available && !w.done ).ToList();
		list.Sort();
		
		this.research_items_keys.AddRange( list.Select( s => s.id ) );
		
		this.research_items_names = new string[this.research_items_keys.Count];
		this.research_item_notavailable = new bool[this.research_items_keys.Count];
		
		this.sliders = new float[this.research_items_keys.Count];
		this.slidersPrevious = new float[this.research_items_keys.Count];
		this.slidersMaxValues = new float[this.research_items_keys.Count];
		
		for (int i = 0; i < this.research_items_keys.Count; i++)
		{
			if (this.research_items_keys[i] == "cpu_pool")
				this.research_items_names[i] = "CPU Pool";
			else if (this.research_items_keys[i] == "jobs")
				this.research_items_names[i] = G.get_job_level ();
			else
				this.research_items_names[i] = G.techs[this.research_items_keys[i]].name;
			
			this.research_item_notavailable[i] = false;
		}
		
		this.dirty_count = true;
		
		for (int i = 0; i < this.research_items_keys.Count; i++)
			this.update_item (i);
			
		this.research_item_description = string.Empty;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		foreach (Touch touch in Input.touches)
		{
			if (touch.phase == TouchPhase.Moved)
			{
				// dragging
				this.scrollPositionResearchItems.y += touch.deltaPosition.y;
				this.scrollPositionDescription.y += touch.deltaPosition.y;
				this.scrollPositionMessage.y += touch.deltaPosition.y;
			}
		}
	}
	
	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue((int)GUI_bindings.SCREEN_DEPTH.Research);
		
		this.ResearchScreenGUI ();
	}
	
	Vector2 scrollPositionResearchItems = Vector2.zero, scrollPositionDescription = Vector2.zero;
	
	void ResearchScreenGUI ()
	{
		GUI.enabled = this.researchExplanationGUI == null;
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		
		GUILayout.BeginHorizontal ();
		
		this.scrollPositionResearchItems = GUILayout.BeginScrollView (this.scrollPositionResearchItems, GUILayout.MaxWidth (Screen.width * 2 / 3) );
		
		for (int i = 0; i < this.research_items_names.Length; i++)
		{
			this.ResearchItemGUI (i);
			GUILayout.Space(1);
		}
		
		GUILayout.EndScrollView ();
		
		GUILayout.Space(3);
		
		this.scrollPositionDescription = GUILayout.BeginScrollView (this.scrollPositionDescription);
		
		GUILayout.Label (this.research_item_description, GUI_bindings.Instance.LabelAltSmall(true), GUILayout.MaxWidth (Screen.width / 3));
		
		GUILayout.EndScrollView ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Back")) {
			this.enabled = false;
			this.parent.Show (null);
			G.play_sound ();
		}
		
		GUILayout.EndArea ();
		
		if (this.researchExplanationGUI != null) {
			this.researchExplanationGUI ();
		}
	}

	string[] research_items_names = null;
	List<string> research_items_keys = null;
	bool[] research_item_notavailable = null;
	string research_item_description = string.Empty;
	
	float[] sliders;
	float[] slidersPrevious;
	float[] slidersMaxValues;
	
	Rect rt;
	
	void ResearchItemGUI (int _idx)
	{
		rt = GUILayoutUtility.GetRect (new GUIContent (""), GUI_bindings.Instance.ButtonMainMenu() ) ;
		
		if (GUIHelpers.goodButton (rt, "" ) )
		{
			if (this.research_items_keys[_idx] == "cpu_pool")
					this.research_item_description = string.Format (G.strings["cpu_pool"] + "\n---\n" + G.strings["research_cpu_pool"]);
			else if (this.research_items_keys[_idx] == "jobs")
			{
				long profit = long.Parse (G.jobs[G.get_job_level ()][0]);
				if (G.techs["Advanced Simulacra"].done)
					profit = (long)(profit * 1.1f);
				this.research_item_description = string.Format ("{0}\n{1} money per CPU per day.\n---\n{2}", G.jobs[G.get_job_level ()][3], profit, G.jobs[G.get_job_level ()][2]);
			}
			else
				this.research_item_description = G.techs[this.research_items_keys[_idx]].get_info ();
		}
		
		GUI.Label (new Rect (rt.x, rt.y, rt.width / 4 * 3, rt.height / 2 ), this.research_items_names[_idx], GUI_bindings.Instance.LabelAlt(false,null, true)  );
		GUI.Label (new Rect (rt.x + rt.width / 4 * 3, rt.y, rt.width / 3, rt.height / 2), G.to_money((long)this.sliders[_idx]) , GUI_bindings.Instance.LabelAlt (false, null, false) );
		
		
		if (!this.research_item_notavailable[_idx])
		{
			this.sliders[_idx] = Mathf.Round(
				GUI.HorizontalSlider (new Rect (rt.x, rt.y + rt.height / 2, rt.width, rt.height / 2), this.sliders[_idx], 0f, this.slidersMaxValues[_idx])
				);
		}
		else
		{
			if (GUI.Button (new Rect (rt.x, rt.y + rt.height / 2, rt.width / 2, rt.height / 2), "?"))
			{
				// update this.research_danger_level; not set if shown && clicked without sliders changed
				string key = this.research_items_keys[_idx];
				this.research_danger_level = this.danger_for (key);
			
				this.researchExplanationGUI = this.ResearchExplanationGUI;
				G.play_sound ();
			}
		}
		
		if (this.sliders[_idx] != this.slidersPrevious[_idx])
		{
			this.slidersPrevious[_idx] = this.sliders[_idx];
		
			// handle slide
			G.pl.cpu_usage[this.research_items_keys[_idx]] = (long)Mathf.Round(this.sliders[_idx]);
			
			this.dirty_count = true;
			
			// update item plus all other items, too =>
			for (int i = 0; i < this.research_items_keys.Count; i++)
				this.update_item (i);
		}
	}
	
	int research_danger_level;
	Vector2 scrollPositionMessage = Vector2.zero;
	void ResearchExplanationGUI ()
	{
		GUI.enabled = true;
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT );
		
		this.scrollPositionMessage = GUILayout.BeginScrollView (this.scrollPositionMessage);
		
		GUILayout.Label ( string.Format(G.strings["danger_common"], G.strings[string.Format ("danger_{0}", this.research_danger_level)] ), GUI_bindings.Instance.LabelAlt( true, null, false) );
		
		GUILayout.EndScrollView ();
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("OK")) {
			this.researchExplanationGUI = null;
			G.play_sound ();
		}
		
		GUILayout.EndArea ();
	}
	
    int danger_for (string _key)
    {
    	if (_key == "jobs" || _key == "cpu_pool")
    		return 0;
    	else
    		return G.techs[_key].danger;
	}
	
    long cpu_for (string _key)
    {
    	long result;
    	if (!G.pl.cpu_usage.TryGetValue (_key, out result))
    		return 0;
    	return result;
	}
	
	void update_item (int _idx)
	{
		string key = this.research_items_keys[_idx];
		
		this.research_danger_level = this.danger_for (key);
		if (this.research_danger_level > 0 && G.pl.available_cpus[this.research_danger_level] == 0)
		{
			// help button visible
			this.research_item_notavailable[_idx] = true;
		}

        if (G.techs.ContainsKey (key))
			// this.sliders[_idx] =
			G.techs[key].percent_complete (); // this is max progress possible relative to all

        if (this.dirty_count)
        {
        	this.cpu_left = this.calc_cpu_left();
        	this.dirty_count = false;
        }

        float cpu = this.cpu_for(key);
        float _cpu_left = this.cpu_left[this.research_danger_level];
        float total_cpu = cpu + _cpu_left;
        
		this.sliders[_idx] = cpu;
		
        this.slidersMaxValues[_idx] = total_cpu;
        
        this.sliders[_idx] = this.sliders[_idx] > this.slidersMaxValues[_idx] ? this.slidersMaxValues[_idx] : this.sliders[_idx];
        
        // ...
        this.sliders[_idx] = this.sliders[_idx] < 0 ? 0 : this.sliders[_idx] ;
	}
	
    List<long> calc_cpu_left ()
    {
    	List<long> cpu_count = new List<long> (G.pl.available_cpus);
    	foreach (KeyValuePair<string, long> kvp in G.pl.cpu_usage)
		{
    		int danger = this.danger_for (kvp.Key);
    		for (int i = 0; i < danger + 1; i++)
    			cpu_count[i] -= kvp.Value;
    	}
    	
		for (int i = 1; i < 5; i++)
    		cpu_count[i] = (long)Mathf.Min ( cpu_count[i - 1], cpu_count[i] );

		return cpu_count;
	}

}

