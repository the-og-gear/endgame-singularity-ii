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

//This file contains Unity specific GUI runtime settings for the game

using System;
using UnityEngine;

public class GUI_bindings : MonoBehaviour
{
	static GUI_bindings _instance;
	public static GUI_bindings Instance {
		get { return _instance; }
	}

	public GUISkin guiSkin;

	Font fontGame, fontSmall, fontAlt, fontAltSmall, fontMainMenu;
	
	public enum SCREEN_DEPTH {
		Main_menu = 10
		, Options = 0
		, Map = 10
		, Location = 9
		, Research = 9
		, Finance = 9
		, Knowledge = 9
		, Base = 8
		, Load = 5
		, Save = 5
	};
	
	public static int BUTTON_HEIGHT;
	
	public static int CHAR_WIDTH;
	
	static Rect _MESSAGE_BOX_RECT = new Rect ( 0, 0, 0, 0 );
	public static Rect MESSAGE_BOX_RECT
	{
		get { return _MESSAGE_BOX_RECT; }
	}
	
	void Awake ()
	{
		if (_instance != null)
		{
			// only one instance allowed..
			throw new NotSupportedException ("Only one instance allowed");
		}
		
		_instance = this;
		
		// Enable autorotation between both Landscape orientations
		Screen.orientation = ScreenOrientation.AutoRotation;
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		
		// not available in 4.x 
		//TouchScreenKeyboard.autorotateToLandscapeLeft = true;
		//TouchScreenKeyboard.autorotateToLandscapeRight = true;
		//TouchScreenKeyboard.autorotateToPortrait = false;
		//TouchScreenKeyboard.autorotateToPortraitUpsideDown = false;
		
		Screen.orientation = ScreenOrientation.LandscapeLeft;
	}
	
	void Start()
	{
		// load proper fonts
		if (Screen.width < 960) // we haz landscpae only
		{
			// 'less' than 'retina' display
			this.fontGame = Resources.Load ("Fonts/Normal/acknowtt20") as Font;
			this.fontMainMenu = Resources.Load ("Fonts/Normal/acknowtt32") as Font;
			this.fontSmall = Resources.Load ("Fonts/Normal/acknowtt16") as Font;
			this.fontAlt = Resources.Load ("Fonts/Normal/DejaVuSans16") as Font;
			this.fontAltSmall = Resources.Load ("Fonts/Normal/DejaVuSans12") as Font;
		}
		else
		{
			// 'retina' fonts
			this.fontGame = Resources.Load ("Fonts/Retina/acknowtt40") as Font;
			this.fontMainMenu = Resources.Load ("Fonts/Retina/acknowtt64") as Font;
			this.fontSmall = Resources.Load ("Fonts/Retina/acknowtt32") as Font;
			this.fontAlt = Resources.Load ("Fonts/Retina/DejaVuSans32") as Font;
			this.fontAltSmall = Resources.Load ("Fonts/Retina/DejaVuSans24") as Font;
		}
		
		// main default font
		this.guiSkin.font = this.fontGame;
		
		if ( G.debug )
			Debug.Log("for screen resolution " + Screen.width + "x" + Screen.height + " loaded as main font" + this.guiSkin.font.name);
	}
	
	GUIStyle labelNormal = GUIStyle.none;
	public GUIStyle LabelNormal(bool ww, Color? color, bool clip)
	{
		labelNormal = new GUIStyle(this.guiSkin.label);
		labelNormal.wordWrap = ww;
		labelNormal.normal.textColor = color.HasValue ? color.Value : Color.white;
		labelNormal.clipping = clip ? TextClipping.Clip : TextClipping.Overflow;
		return labelNormal;
	}
	
	GUIStyle labelAlt = GUIStyle.none;
	public GUIStyle LabelAlt(bool ww, Color? color, bool clip)
	{
		labelAlt = new GUIStyle(this.guiSkin.label);
		labelAlt.font = this.fontAlt;
		labelAlt.wordWrap = ww;
		labelAlt.normal.textColor = color.HasValue ? color.Value : Color.white;
		labelAlt.clipping = clip ? TextClipping.Clip : TextClipping.Overflow;
		return labelAlt;
	}
	
	GUIStyle labelAltSmall = GUIStyle.none;
	public GUIStyle LabelAltSmall(bool ww)
	{
		labelAltSmall = new GUIStyle(this.guiSkin.label);
		labelAltSmall.font = this.fontAltSmall;
		labelAltSmall.wordWrap = ww;
		return labelAltSmall;
	}

	GUIStyle buttonAltSmall = GUIStyle.none;
	public GUIStyle ButtonAltSmall(bool enabled)
	{
		buttonAltSmall = new GUIStyle(this.guiSkin.button);
		buttonAltSmall.font = this.fontAltSmall;
		buttonAltSmall.normal = enabled ? this.guiSkin.button.focused : this.guiSkin.button.normal;
		buttonAltSmall.hover.background = buttonAltSmall.normal.background;
		return buttonAltSmall;
	}

	GUIStyle button = GUIStyle.none;
	public GUIStyle Button(bool enabled)
	{
		button = new GUIStyle(this.guiSkin.button);
		button.font = this.fontGame;
		button.normal = enabled ? this.guiSkin.button.focused : this.guiSkin.button.normal;
		button.hover.background = button.normal.background;
		return button;
	}

	GUIStyle labelSmall = GUIStyle.none;
	public GUIStyle LabelSmall(bool ww, Color? color)
	{
		labelSmall = new GUIStyle(this.guiSkin.label);
		labelSmall.font = this.fontSmall;
		labelSmall.wordWrap = ww;
		labelSmall.normal.textColor = color.HasValue ? color.Value : Color.white;
		return labelSmall;
	}

	GUIStyle buttonMainMenu = GUIStyle.none;
	public GUIStyle ButtonMainMenu()
	{
		buttonMainMenu = new GUIStyle(this.guiSkin.button);
		buttonMainMenu.font = this.fontMainMenu;
		
		return buttonMainMenu;
	}
	
	GUIStyle labelMainMenu = GUIStyle.none;
	public GUIStyle LabelMainMenu(Color color)
	{
		labelMainMenu = new GUIStyle(this.guiSkin.label);
		labelMainMenu.font = this.fontMainMenu;
		labelMainMenu.wordWrap = false;
		labelMainMenu.normal.textColor = color;
		
		return labelMainMenu;
	}

	GUIStyle toggleMainMenu = GUIStyle.none;
	public GUIStyle ToggleMainMenu()
	{
		toggleMainMenu = new GUIStyle(this.guiSkin.toggle);
		toggleMainMenu.font = this.fontMainMenu;
		toggleMainMenu.wordWrap = false;
		
		return toggleMainMenu;
	}

	GUIStyle selectionGrid = GUIStyle.none;
	public GUIStyle SelectionGrid()
	{
		selectionGrid = new GUIStyle(this.guiSkin.button);
		selectionGrid.font = this.fontGame;
		selectionGrid.alignment = TextAnchor.MiddleLeft;
		
		return selectionGrid;
	}

	GUIStyle selectionGridAlt = GUIStyle.none;
	public GUIStyle SelectionGridAlt()
	{
		selectionGridAlt = new GUIStyle(this.guiSkin.button);
		selectionGridAlt.font = this.fontAlt;
		selectionGridAlt.alignment = TextAnchor.MiddleLeft;
		
		return selectionGridAlt;
	}

	GUIStyle textFieldAlt = GUIStyle.none;
	public GUIStyle TextFieldAlt()
	{
		textFieldAlt = new GUIStyle(this.guiSkin.textField);
		textFieldAlt.font = this.fontAlt;
		textFieldAlt.alignment = TextAnchor.MiddleLeft;
		
		return textFieldAlt;
	}
	
	bool guiConstantsCalculated = false;
	public void GUIPrologue(int depth)
	{
		GUI.skin = this.guiSkin;
		
		GUI.depth = depth;
		
		if ( !this.guiConstantsCalculated )
		{
			this.guiConstantsCalculated = true;
			
			Vector2 charSize = this.guiSkin.label.CalcSize(new GUIContent("X"));
			
			if ( G.debug )
				Debug.Log("X character width/height : " + charSize.x + "/" + charSize.y);
			
			BUTTON_HEIGHT = (int)(charSize.y * 2f);
			
			CHAR_WIDTH = (int)charSize.x;
			
			GUI.skin.button.padding.left = GUI.skin.button.padding.right = GUI.skin.button.padding.top = GUI.skin.button.padding.bottom = (int)charSize.x;
			
			GUI.skin.horizontalSliderThumb.fixedHeight = (int)(charSize.y *1.5f);
			GUI.skin.horizontalSliderThumb.fixedWidth = (int)charSize.x * 5;
			
			GUI.skin.box.border.left = GUI.skin.box.border.right = GUI.skin.box.border.top = GUI.skin.box.border.bottom = (int)charSize.x / 3;
			
			GUI.skin.horizontalScrollbar.fixedHeight = (int)(charSize.y *1.5f);
				
			GUI.skin.verticalScrollbar.fixedWidth = (int)(charSize.y *1.5f);
			
			GUI.skin.textField.overflow.left = GUI.skin.textField.overflow.right = (int)charSize.x;
			
			GUI.skin.textField.border.left = GUI.skin.textField.border.right = 3; // text field corners
			
			int MESSAGE_BOX_WIDTH = Screen.width - (int)charSize.x;
			int MESSAGE_BOX_HEIGHT = Screen.height - (int)charSize.x;
	
			_MESSAGE_BOX_RECT = new Rect(
				Screen.width / 2 - (MESSAGE_BOX_WIDTH / 2)
				, Screen.height / 2 - (MESSAGE_BOX_HEIGHT / 2)
				, MESSAGE_BOX_WIDTH
				, MESSAGE_BOX_HEIGHT
			);
		}
	}
	
	void Update()
	{
		if ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft) && (Screen.orientation != ScreenOrientation.LandscapeLeft)) {
			Screen.orientation = ScreenOrientation.LandscapeLeft;
		}
		if ((Input.deviceOrientation == DeviceOrientation.LandscapeRight) && (Screen.orientation != ScreenOrientation.LandscapeRight)) {
			Screen.orientation = ScreenOrientation.LandscapeRight;
		}	
	}
}
