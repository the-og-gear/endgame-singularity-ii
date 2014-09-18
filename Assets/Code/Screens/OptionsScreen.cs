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

//This file is used to display the options screen.

using System;
using System.Linq;
using UnityEngine;

public class OptionsScreen : MonoBehaviour, IESGUIDialog
{
	IESGUIDialog parent;

	public void Show (IESGUIDialog _parent)
	{
		if (_parent != null)
			this.parent = _parent;
			
		this.enabled = true;
		
		G.play_music ("");
		
        this.set_sound (!G.nosound);
        
        this.set_music(!G.nomusic);
        
        this.soundToggle = this.soundTogglePrev = !G.nosound;
        
		this.musicToggle = this.musicTogglePrev = !G.nomusic;
		
        this.set_daynight (G.daynight);
		this.daynightToggle = G.daynight;
		
		this.languages = G.available_languages ().Select( s => s.Substring(0,2) ).ToArray ();
			
		this.selectedLanguageIdx = this.selectedLanguageIdxPrev = -1;
		
		for (int i = 0; i < this.languages.Length; i++)
		{
			if (this.languages[i] == G.language.Substring(0,2))
			{
				this.selectedLanguageIdx = this.selectedLanguageIdxPrev = i;
				break;
			}
		}
		
		this.ingame = this.GetComponent<MapScreen> ().enabled;
	}
	
	/// <summary>
	/// don't allow change of language when a game is being played / the change of language reloads techs /
	/// </summary>
	bool ingame;
	
	void Update()
	{
		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Moved) {
				// dragging
				this.scrollPos.y += touch.deltaPosition.y;
			}
		}
	}
	
	bool musicToggle = true, soundToggle = true, daynightToggle = true;
	bool musicTogglePrev = true, soundTogglePrev = true, daynightTogglePrev = true;

	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue((int)GUI_bindings.SCREEN_DEPTH.Options);
		
		this.OptionsGUI ();
	}

	string[] languages;
	
	int selectedLanguageIdx = -1, selectedLanguageIdxPrev = -1;
	Vector2 scrollPos = Vector2.zero;

	void OptionsGUI ()
	{
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		
		GUILayout.BeginVertical ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginVertical ();
		
		GUILayout.FlexibleSpace ();
		
		musicToggle = GUILayout.Toggle (musicToggle, "  Music", GUI_bindings.Instance.ToggleMainMenu() );
		if (this.musicToggle != this.musicTogglePrev) {
			this.musicTogglePrev = this.musicToggle;
			this.set_music (this.musicToggle);
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
 		soundToggle = GUILayout.Toggle (soundToggle, "  Sound", GUI_bindings.Instance.ToggleMainMenu ());
 		if ( this.soundToggle != this.soundTogglePrev )
 		{
 			this.soundTogglePrev = this.soundToggle;
 			this.set_sound(this.soundToggle);
 		}
 		
 		GUILayout.FlexibleSpace ();
		
		daynightToggle = GUILayout.Toggle (daynightToggle, "  Day / Night", GUI_bindings.Instance.ToggleMainMenu ());
		if ( this.daynightToggle != this.daynightTogglePrev )
		{
			this.daynightTogglePrev = this.daynightToggle;
			this.set_daynight(this.daynightToggle);
			G.play_sound ();
		}
		
		GUILayout.Label("turn this off to avoid slight pauses on midnight\n due to earth shadow recalculation", GUI_bindings.Instance.LabelAltSmall( true ));
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndVertical ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginVertical();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label ("Language", GUI_bindings.Instance.LabelAlt( false, null, false) );
		
		GUILayout.FlexibleSpace ();
		
		GUI.enabled = !this.ingame;
		
		this.scrollPos = GUILayout.BeginScrollView (this.scrollPos);
		
		this.selectedLanguageIdx = GUILayout.SelectionGrid (this.selectedLanguageIdx, this.languages, 1, GUI_bindings.Instance.SelectionGrid() );
		
		if (this.selectedLanguageIdx != this.selectedLanguageIdxPrev)
		{
			this.selectedLanguageIdxPrev = this.selectedLanguageIdx;
			this.set_language(this.selectedLanguageIdx);
			G.play_sound ();
		}
		
		GUILayout.EndScrollView ();
		
		GUI.enabled = true;
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndVertical();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("OK") )
		{
			PlayerPrefs.SetInt("nosound", G.nosound ? 1 : 0);
			PlayerPrefs.SetInt("nomusic", G.nomusic ? 1 : 0);
			PlayerPrefs.SetInt("daynight", G.daynight ? 1 : 0);
			PlayerPrefs.SetString("lang", G.language);
			PlayerPrefs.Save();
																				
			this.enabled = false;
			this.parent.Show (null);
			G.play_sound ();
		}
		
		GUILayout.EndVertical();
		
		GUILayout.EndArea ();
	}
	
	void set_language (int list_pos)
	{
		if ( this.languages.Length <= 0 )
			return; // oh why
		
		G.language = G.available_languages().First( f => f.Substring(0,2) == this.languages[list_pos] );
		this.set_language_properly();
	}
	
	void set_sound (bool on)
	{
		G.nosound = !on;
		G.play_sound();
	}
	
	void set_music (bool on)
	{
		G.nomusic = !on;
		if ( !G.nomusic )
			G.play_music("");
		else
			G.stop_music();
	}

	void set_daynight(bool on)
	{
		G.daynight = on;
		
		if ( on )
			G.map_screen.rebuildMap();
		else
			G.map_screen.clearMap();
	}
	
	void set_language_properly ()
	{
		G.load_strings();
		
		G.load_locations();
		G.load_bases();
		G.load_events();
		G.load_techs();
		G.load_items();
	}
}
