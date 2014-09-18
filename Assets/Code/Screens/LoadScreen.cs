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

//This file contains the dialog for loading game saves

using System.Collections.Generic;
using UnityEngine;

public class LoadScreen : MonoBehaviour, IESGUIDialog
{
	IESGUIDialog parent = null;
	
	public void Show (IESGUIDialog _parent)
	{
		if ( _parent != null )
			this.parent = _parent;
			
		this.enabled = true;
		
		G.play_music ("");
		
		this.save_names = G.get_save_names ();
		this.save_names.Sort ();
	}

	List<string> save_names = null;
	
	void Update()
	{
		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Moved) {
				// dragging
				this.scrollPos.y += touch.deltaPosition.y;
			}
		}
	}
	
	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue((int)GUI_bindings.SCREEN_DEPTH.Load);
		
		this.Load ();
	}
	
	int selectedSaveIdx = -1;
	Vector2 scrollPos = Vector2.zero;

	void Load ()
	{
		GUI.enabled = true;
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT);
		
		this.scrollPos = GUILayout.BeginScrollView (this.scrollPos);
		
		this.selectedSaveIdx = GUILayout.SelectionGrid (this.selectedSaveIdx, this.save_names.ToArray (), 1, GUI_bindings.Instance.SelectionGrid());
		
		GUILayout.EndScrollView ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("Load"))
		{
			if (this.selectedSaveIdx >= 0) {
				
				bool did_load = G.load_game (this.save_names[this.selectedSaveIdx]);
				if (did_load) {
				
					foreach (Location l in G.locations.Values)
						l.update_position ();
				
					this.enabled = false;
					this.GetComponent<Main_menu> ().enabled = false;
					this.GetComponent<MapScreen> ().Show(null);
					G.play_sound ();
				}
			}
		
		}
		
		if (GUILayout.Button ("Back"))
		{
			this.enabled = false;
			this.parent.Show (null);
			G.play_sound ();
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea ();
	}
}
