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

//This file contains GUI workarounds/helpers

using UnityEngine;

public class GUIHelpers
{
	// GUI system workaround about a GUI control layered on top of a button
	// see e.g. http://forum.unity3d.com/threads/96563-corrected-GUI.Button-code-%28works-properly-with-layered-controls%29
	public static bool goodButton (Rect bounds, string caption)
	{
		GUIStyle btnStyle = GUI.skin.FindStyle ("button");
		int controlID = GUIUtility.GetControlID (bounds.GetHashCode (), FocusType.Passive);
		
		bool isMouseOver = bounds.Contains (UnityEngine.Event.current.mousePosition);
		bool isDown = GUIUtility.hotControl == controlID;
		
		if (GUIUtility.hotControl != 0 && !isDown) {
			// ignore mouse while some other control has it
			// (this is the key bit that GUI.Button appears to be missing)
			isMouseOver = false;
		}
		
		if (UnityEngine.Event.current.type == EventType.Repaint)
		{
			btnStyle.Draw (bounds, new GUIContent (caption), isMouseOver, isDown, false, false);
		}
		
		switch (UnityEngine.Event.current.GetTypeForControl (controlID))
		{
		
		case EventType.MouseDown:
			if (isMouseOver) {
				// (note: isMouseOver will be false when another control is hot)
				GUIUtility.hotControl = controlID;
			}
			
			// GUIUtility.hotControl = controlID; // button is broken: we get slider AND button ( which is what we want )
			
			break;
		
		case EventType.MouseUp:
			if (GUIUtility.hotControl == controlID)
				GUIUtility.hotControl = 0;
			if (bounds.Contains (UnityEngine.Event.current.mousePosition))
				return true;
			break;
		}
		
		return false;
	}
}
