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

//This file is the starting MonoBehaviour file for the game.

using System;
using UnityEngine;

public class Singularity : MonoBehaviour {

	// sound clips references; original implementation had some generic intentions about defining multiple sound 'classes' each comprising of several soundclips
	// since only 'click' class was used and implemented at the time of fork for simplicity sake we reference each four defined sound clips and play one of them as demanded
	public AudioClip[] clicks;
	
	public AudioClip[] music;
	
	public AudioClip[] music_lose;
	
	public AudioClip[] music_win;
	
	public void play_sound()
	{
		(this.GetComponent<AudioSource> () as AudioSource).volume = 1f;
		(this.GetComponent<AudioSource>() as AudioSource).PlayOneShot(this.clicks[UnityEngine.Random.Range(0,this.clicks.Length)]);
	}
	
	float delay_time = 1;
	
	public void stop_music()
	{
		(this.GetComponent<AudioSource> () as AudioSource).Stop();
	}
	
	public void play_music(string kind)
	{
		AudioSource _as = this.GetComponent<AudioSource>() as AudioSource;
		
		if ( string.IsNullOrEmpty(kind) && _as.isPlaying )
			return;
		
		if ( !string.IsNullOrEmpty(kind) )
		{
			_as.Stop();
			
			if ( kind == "win" )
				_as.clip = music_win[UnityEngine.Random.Range(0, music_win.Length)];
			else if ( kind == "lose")
				_as.clip = music_lose[UnityEngine.Random.Range (0, music_lose.Length)];
				
			_as.volume = .6f;
			_as.Play ();
		}
		
		if (delay_time == 0)
		{
			delay_time = (Time.timeSinceLevelLoad) + UnityEngine.Random.value * 10 + 2; // silent for 2..12 secs
		}
		else
		{
			if (delay_time > Time.timeSinceLevelLoad )
				return;
				
			delay_time = 0;
			
			_as.clip = music[UnityEngine.Random.Range (0, music.Length)];
			_as.volume = .6f;
			_as.Play();
		}
	}
	
	static Singularity instance;
	public static Singularity Instance
	{
		get { return instance; }
	}
	
	void Awake()
	{
		if (instance != null) {
			// only one instance allowed..
			throw new NotSupportedException ("Only one instance allowed");
		}
		
		instance = this;
	}
	
	void Start ()
	{
		//init data
		
		// original singularity prefs and command line options and init
		
		if (PlayerPrefs.HasKey ("nosound"))
			G.nosound = PlayerPrefs.GetInt ("nosound") == 1;
		else
			G.nosound = false;

		if (PlayerPrefs.HasKey ("nomusic"))
			G.nomusic = PlayerPrefs.GetInt ("nomusic") == 1;
		else
			G.nomusic = false;

        if (PlayerPrefs.HasKey ("daynight"))
			G.daynight = PlayerPrefs.GetInt ("daynight") == 1;
		else
			G.daynight = false;

        //If language is unset, default to English.
		string desired_language = "en_US";
		if (PlayerPrefs.HasKey ("lang"))
			desired_language = PlayerPrefs.GetString ("lang");
		
		G.language = desired_language;
		
		// G.cheater = false;
		// G.debug = false;
		
		G.load_strings ();

		//init data:
		G.load_items ();
		
		// end: original singularity prefs and command line options and init
		
		G.TranslateBonusesPenaltiesOnce ();
		
		// Display the main menu
		GetComponent<Main_menu> ().Show( null );
		
	}
}
		