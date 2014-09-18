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

//This file contains first empty loading screen

using System.Collections;
using UnityEngine;

public class __0 : MonoBehaviour {

	void Awake()
	{
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

	// Use this for initialization
	IEnumerator Start () {
		yield return null;
		Application.LoadLevel(1);
	}
	
	// Update is called once per frame
	void Update () {
	
		if ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft) && (Screen.orientation != ScreenOrientation.LandscapeLeft)) {
			Screen.orientation = ScreenOrientation.LandscapeLeft;
		}
		
		if ((Input.deviceOrientation == DeviceOrientation.LandscapeRight) && (Screen.orientation != ScreenOrientation.LandscapeRight)) {
			Screen.orientation = ScreenOrientation.LandscapeRight;
		}	
	}
}
