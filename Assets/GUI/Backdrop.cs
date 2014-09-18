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

using UnityEngine;
using System.Collections;

public class Backdrop : MonoBehaviour
{
	public float distance = 500;
	
	public GameObject backdrop;
	Mesh mesh;
	
	void Start ()
	{
		//check that we are on a camera!
		if(camera == null){
			Debug.LogError("Backdrop must be used on a camera!");
			Destroy(this);
			return;
		}
		
		//error for attempting a backdrop placement beyond camera's far clip plane:
		if(distance > camera.farClipPlane){
			Debug.LogError("Backdrop's distance is further than the camera's far clip plane. Extend the camera's far clip plane or reduce the billboard's distance.");
			return;
		}
		
		//error for attempting a backdrop placement before camera's near clip plane:
		if(distance < camera.nearClipPlane){
			Debug.LogError("Backdrop's distance is closer than the camera's near clip plane. Extend the distance or reduce the camera's near clip plane.");
			return;
		}
		
		mesh = backdrop.GetComponent<MeshFilter>().mesh;
		mesh.vertices = CalcVerts();
		mesh.uv = new Vector2[] {new Vector2(0,0), new Vector2(1,0), new Vector2(0,1), new Vector2(1,1)};
		mesh.triangles = new int[] {1,0,3,3,0,2};
		
		//set backdrop's placement:
		backdrop.transform.position = transform.forward * distance;
		
		//calculate mesh:
		mesh.vertices = CalcVerts();
		mesh.RecalculateNormals();
	}
	
	Vector3[] CalcVerts()
	{
		float verticalInset = ( Screen.height / 100f ) * 10.6f;
		return new Vector3[] {
			backdrop.transform.InverseTransformPoint(camera.ScreenToWorldPoint(new Vector3(0,verticalInset,distance))),
			backdrop.transform.InverseTransformPoint(camera.ScreenToWorldPoint(new Vector3(Screen.width,verticalInset,distance))),
			backdrop.transform.InverseTransformPoint(camera.ScreenToWorldPoint(new Vector3(0,Screen.height - verticalInset,distance))),
			backdrop.transform.InverseTransformPoint(camera.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height - verticalInset,distance)))
		};
	}
}