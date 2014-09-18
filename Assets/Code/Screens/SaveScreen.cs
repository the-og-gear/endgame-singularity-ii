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

//This file contains the dialog for saving a game

using UnityEngine;

public class SaveScreen : MonoBehaviour, IESGUIDialog
{
	IESGUIDialog parent = null;

	public void Show (IESGUIDialog _parent)
	{
		this.parent = _parent;
		this.enabled = true;
		
		G.play_music ("");
		
		this.savegame_name = G.default_savegame_name;
	}
	
	string savegame_name ;

	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue((int)GUI_bindings.SCREEN_DEPTH.Save);
		
		this.SaveGUI ();
	}
	
	void SaveGUI ()
	{
		GUI.enabled = true;
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "Enter a name for this save.");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "Enter a name for this save.");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT );
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		this.savegame_name = GUILayout.TextField (this.savegame_name);
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();

		if (GUILayout.Button ("OK")) {
			G.save_game (this.savegame_name);
			this.enabled = false;
			this.parent.Show (null);
			G.play_sound ();
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
