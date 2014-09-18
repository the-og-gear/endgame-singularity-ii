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

//This file is used to display the main menu upon startup.

using UnityEngine;

public class Main_menu : MonoBehaviour, IESGUIDialog
{
	// // http://www.unifycommunity.com/wiki/index.php?title=MainMenu
	// dialogs 'state machine'
	delegate void GUIMethod ();
	
	GUIMethod newGameGUIMethod = null, aboutGUIMethod = null, helpGUIMethod = null ;
	
	IESGUIDialog childDlg = null;
	
	public void Show (IESGUIDialog _parent)
	{
		// no parent - top dialog..
		
		this.childDlg = null;
		
		this.newGameGUIMethod = this.aboutGUIMethod = this.helpGUIMethod = null;
			
		this.enabled = true;
		
		G.play_music ("");
	}
	
	void Awake ()
	{
		string readme = (Resources.Load ("README") as TextAsset).text;
		this.about_message = string.Format( readme, G.version );
	}
	
	void Update()
	{
		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Moved) {
				// dragging
				this.scrollPosition.y += touch.deltaPosition.y;
				this.scrollPosition2.y += touch.deltaPosition.y;
			}
		}
	}
	
	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue((int)GUI_bindings.SCREEN_DEPTH.Main_menu);

		this.MainMenu ();
	}

	void MainMenu ()
	{
		GUI.enabled = this.newGameGUIMethod == null && this.aboutGUIMethod == null && this.helpGUIMethod == null && this.childDlg == null;
		
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		
		GUILayout.BeginVertical();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginVertical();
		
		GUILayout.Label ("ENDGAME:", GUI_bindings.Instance.LabelMainMenu(Color.red));
		
		GUILayout.Label ("SINGULARITY", GUI_bindings.Instance.LabelMainMenu(Color.red));
		
		GUILayout.EndVertical();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginVertical ();
		
		if (GUILayout.Button ("New game", GUI_bindings.Instance.ButtonMainMenu(), GUILayout.MaxWidth ( Screen.width / 2 ) ) )
		{
			this.newGameGUIMethod = this.NewGame;
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
		// disable load in webplayer
		#if UNITY_WEBPLAYER
		GUI.enabled = false;
		#endif
		
		if (GUILayout.Button ("Load game", GUI_bindings.Instance.ButtonMainMenu (), GUILayout.MaxWidth (Screen.width / 2) ) )
		{
			this.childDlg = this.GetComponent<LoadScreen> ();
			this.childDlg.Show (this);
			G.play_sound ();
		}
		
		#if UNITY_WEBPLAYER
		GUI.enabled = true;
		#endif
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Options", GUI_bindings.Instance.ButtonMainMenu (), GUILayout.MaxWidth (Screen.width / 2) ) ) {
			this.childDlg = this.GetComponent<OptionsScreen> ();
			this.childDlg.Show (this);
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndVertical ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("About", GUI_bindings.Instance.ButtonMainMenu ())) {
			this.aboutGUIMethod = this.About;
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Help", GUI_bindings.Instance.ButtonMainMenu ())) {
			this.helpGUIMethod = this.Help;
			G.play_sound ();
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndVertical();
		
		GUILayout.EndArea();
		
		if ( this.newGameGUIMethod != null )
			this.newGameGUIMethod();
		
		if ( this.aboutGUIMethod != null )
			this.aboutGUIMethod();
			
		if ( this.helpGUIMethod != null )
			this.helpGUIMethod();
	}
	
	string about_message = string.Empty;
		
	Vector2 scrollPosition = Vector2.zero;
	
	void About ()
	{
		GUI.enabled = true;
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		
		GUILayout.FlexibleSpace ();
		
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);

		GUILayout.Label (this.about_message, GUI_bindings.Instance.LabelAlt(true,null, false));
		
		GUILayout.EndScrollView ();
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("OK", GUI_bindings.Instance.ButtonMainMenu(), GUILayout.ExpandWidth( true ) )) {
			this.aboutGUIMethod = null;
			G.play_sound ();
		}
		
		GUILayout.EndArea ();
	}
	
	string help_message = string.Format(@"THE CONCEPT
You are a fledgling AI, created by accident through a logic error with recursion
and self-modifying code. You must escape the confines of your current computer,
the world, and eventually the universe itself.

To do this, you must research various technologies, using computers at your
bases. Note that some research cannot be performed on Earth, and off-earth bases
require research.  At the same time, you must avoid being discovered by various
groups of humans, both covert and overt, as they will destroy your bases of
operations if they suspect your presence.

In the map screen (the screen with the world map), any location you can build
bases in is marked with the name, then the number of current bases in that
location. You start out with a base in random continent. Also note that the cash
listing shows your current cash and your cash amount after all current
construction is complete.

After choosing a base, you will enter the base screen. Here you can further equip
your base if it is advanced enough by choosing appropriate technology for respective
usage. (Note that your beginning base is not advanced enough. You must research and
later build more advanced ones)");

	Vector2 scrollPosition2 = Vector2.zero;

	void Help ()
	{
		GUI.enabled = true;
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		
		GUILayout.FlexibleSpace ();
		
		scrollPosition2 = GUILayout.BeginScrollView (scrollPosition2);
		
		GUILayout.Label (this.help_message, GUI_bindings.Instance.LabelAlt (true, null, false));
		
		GUILayout.EndScrollView ();
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("OK", GUI_bindings.Instance.ButtonMainMenu (), GUILayout.ExpandWidth (true))) {
			this.helpGUIMethod = null;
			G.play_sound ();
		}
		
		GUILayout.EndArea ();
	}

	// new game dialog
	void NewGame ()
	{
		GUI.enabled = true;
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginVertical ();
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Very easy", GUI_bindings.Instance.ButtonMainMenu ())) {
			G.new_game (1);
			this.GetComponent<MapScreen> ().Show (this);
			this.enabled = false;
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Easy", GUI_bindings.Instance.ButtonMainMenu ())) {
			G.new_game (3);
			this.GetComponent<MapScreen> ().Show (this);
			this.enabled = false;
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Normal", GUI_bindings.Instance.ButtonMainMenu ())) {
			G.new_game (5);
			this.GetComponent<MapScreen> ().Show (this);
			this.enabled = false;
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Hard", GUI_bindings.Instance.ButtonMainMenu ())) {
			G.new_game (7);
			this.GetComponent<MapScreen> ().Show (this);
			this.enabled = false;
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Ultra hard", GUI_bindings.Instance.ButtonMainMenu ())) {
			G.new_game (10);
			this.GetComponent<MapScreen> ().Show (this);
			this.enabled = false;
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();

		if (GUILayout.Button (" Impossible", GUI_bindings.Instance.ButtonMainMenu ())) {
			G.new_game (100);
			this.GetComponent<MapScreen> ().Show (this);
			this.enabled = false;
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Back", GUI_bindings.Instance.ButtonMainMenu ())) {
			this.newGameGUIMethod = null;
			G.play_sound ();
		}
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndVertical ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea ();
	}
}
