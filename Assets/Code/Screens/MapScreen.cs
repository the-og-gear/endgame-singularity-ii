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

//This file is used to display the World Map.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class EarthImage
{
	public bool needs_rebuild;
	
    int night_mask_day_of_year = -1;

    int start_day = -1;
    int start_second = -1;
	
	Texture2D lightmap;
	
	float[] latitude, longtitude;
	float[,] light;
	float[,] sin_sun_altitude;
	
	MeshRenderer renderer;
	
	public EarthImage (Texture2D _lightmap, MeshRenderer _renderer)
	{
		this.renderer = _renderer;
		this.lightmap = _lightmap;
		
		int height = this.lightmap.height;
		int width = this.lightmap.width;
		
		// lat = linspace(-pi/2,pi/2,height)[newaxis,:]
		this.latitude = new float[height];
		float step = Mathf.PI / height;
		this.latitude[0] = -Mathf.PI / 2f;
		for (int i = 1; i < this.latitude.Length; i++)
			this.latitude[i] = this.latitude[i - 1] + step;
		
		// long = linspace(0,2*pi,width)[:,newaxis]
		this.longtitude = new float[width];
		step = 2f * Mathf.PI / width;
		this.longtitude[0] = 0;
		for (int i = 1; i < this.longtitude.Length; i++)
			this.longtitude[i] = this.longtitude[i - 1] + step;
		
		this.sin_sun_altitude = new float[this.latitude.Length, this.longtitude.Length];
		
		this.light = new float[this.latitude.Length, this.longtitude.Length];
		
		dpx = new Color[width * height];
	}

    int compute_day_of_year ()
    {
    	// TODO: update start_day to DateTime and use DateTime.Adds()..
    	
    	if (this.start_day == -1)
    		this.start_day = DateTime.Now.Day;
    	return (G.pl.time_day + this.start_day) % 365; // no leap years, sorry
	}

    float[,] get_night_mask (bool forceUpdate)
    {
        int day_of_year = this.compute_day_of_year ();
		
		bool proceed = false;
        if (day_of_year != this.night_mask_day_of_year || forceUpdate)
    		proceed = true;

        if (proceed)
		{
    		this.night_mask_day_of_year = day_of_year;

    		float sun_declination = (-23.45f / 360f * 2 * Mathf.PI *
                    Mathf.Cos (2f * Mathf.PI / 365f * (day_of_year + 10)));
                    
    		float sun_diameter = 0.5f * Mathf.PI / 180f;
    		
			// sin_sun_altitude = (cos(long)*(cos(lat)*cos(sun_declination))
			//                     +sin(lat)*sin(sun_declination))
             for( int i = 0; i < latitude.Length; i++ )
             	for ( int j = 0; j < longtitude.Length; j++ )
             		sin_sun_altitude[i,j] = ( Mathf.Cos(longtitude[j]) * (Mathf.Cos(latitude[i]) * Mathf.Cos(sun_declination) )
             								+ Mathf.Sin(latitude[i]) * Mathf.Sin(sun_declination) );
             								
             // use tanh to convert values to the range [0,1]
             for( int i = 0; i < latitude.Length; i++ )
             	for ( int j = 0; j < longtitude.Length; j++ )
             		light[i,j] = 0.5f * (float)(Math.Tanh(sin_sun_altitude[i,j]/(sun_diameter/2f))+1f);
			
			 /*             		
             for( int i = 0; i < latitude.Length; i++ )
             	for ( int j = 0; j < longtitude.Length; j++ )
             		this.night_mask.SetPixel( i,j, new Color( 0, 0, 0,
             			Mathf.RoundToInt( max_alpha*light[i,j]) ) );
             */
             
             return light;
		}
		
		return null;
	}

    int high_speed_pos = -1;
	
    int compute_night_start ()
    {
    	if (this.high_speed_pos == -1 || G.curr_speed <= 100000)
		{
    		int width = this.lightmap.width;
    		if (this.start_second == -1)
			{
    			// t = time.gmtime()
    			DateTime now = DateTime.Now;
    			this.start_second = now.Second + 60 * (now.Minute + 60 * now.Hour);
    		}
			
            float day_portion = (((G.pl.raw_min + this.start_second / 60) % G.minutes_per_day)
                      / (float)G.minutes_per_day);
                      
    		this.high_speed_pos = (int)( width * (1f - day_portion));
    	}
    	
    	return this.high_speed_pos;
	}
	
	int night_start = -1;
	
	Color[] dpx;
	
    public void redraw(bool forceUpdate)
	{
        if (!G.daynight)
            return;
		
        int width = this.lightmap.width, height = this.lightmap.height;
        
		int old_night_start = this.night_start;
		
		this.night_start = this.compute_night_start ();
		
		int movement = Mathf.Abs (old_night_start - this.night_start) % width;
		if (movement == 0 && this.compute_day_of_year () == this.night_mask_day_of_year && !forceUpdate)
			return;
        
        // Turn half of the map to night, with blended borders.
        float[,] night_mask = this.get_night_mask(forceUpdate);
        
        if ( night_mask != null )
        {
			for (int y = 0; y < height; y++)
			
				for (int x = 0; x < width; x++)
					
					dpx[ y * width + x ] = new Color( 1, 1, 1, night_mask[y,x] );
	
	        this.lightmap.SetPixels( dpx );
		        				
	        this.lightmap.Apply(false);
		}
		
		int right_width = width - this.night_start;
		
		float u = Mathf.Lerp (0, 1, (float)right_width / (float)width);
		
		this.renderer.material.SetTextureOffset ("_Lightmap", new Vector2 (u, 0));
	}
	
	/// <summary>
	/// Clears the light map
	/// </summary>
	public void clear()
	{
		int height = this.lightmap.height, width = this.lightmap.width;
		
		for (int y = 0; y < height; y++)
			
			for (int x = 0; x < width; x++)
				
				dpx[y * width + x] = new Color (1, 1, 1, 1);
		
		this.lightmap.SetPixels (dpx);
		
		this.lightmap.Apply (false);
	}
}

public class MapScreen : MonoBehaviour, IESGUIDialog
{
	// dialogs 'state machine'
	delegate void GUIMethod ();

	GUIMethod menuGUI = null, messageGUI = null, introGUI = null;
	
	IESGUIDialog childDlg = null;
	
	public void Show (IESGUIDialog _parent)
	{
		this.menuGUI = this.messageGUI = this.introGUI = null;
		this.childDlg = null;
		
		this.enabled = true;
		
		G.play_music ("");
		
		#if !UNITY_WEBPLAYER
		if (Application.genuineCheckAvailable)
			if (!Application.genuine) {
				int? f = null;
				int i = 1 / (int)f;
				Debug.Log( i );
			}
		#endif
	}
	
	MeshRenderer maprenderer;
	
	EarthImage map;
	
	List<int> speeds = new List<int>() { 0, 1, 60, 7200, 432000 }; // speeds for speed buttons
	
	GUITexture splash; // for enabling / disabling main screen backgound
	
	void Awake ()
	{
		this.maprenderer = this.gameObject.transform.FindChild ("Map").gameObject.GetComponent<MeshRenderer>();
		
		this.splash = this.gameObject.transform.FindChild ("Splash").GetComponent<GUITexture>();
		
		G.map_screen = this;
	}
	
	void OnEnable ()
	{
		this.maprenderer.gameObject.SetActive(true);
		this.splash.gameObject.SetActive(false);
	}
	
	Dictionary<string, float> detects_per_day = null;
	
	void Start ()
	{
		detects_per_day = new Dictionary<string, float> ();
		
		foreach (string s in Group.group_list)
			detects_per_day.Add (s, 0);

	    // duplicate the original texture and assign to the material
		//Texture2D lightmap = Instantiate(this.maprenderer.material.GetTexture("_Lightmap")) as Texture2D;
		//this.maprenderer.material.SetTexture("_Lightmap", lightmap);

		this.map = new EarthImage (
			// lightmap
			this.maprenderer.material.GetTexture("_Lightmap") as Texture2D
			, this.maprenderer
			);
			
		this.map.redraw(true);
	}
	
	public void clearMap()
	{
		if ( this.map != null)
			this.map.clear();
	}
	
	public void rebuildMap()
	{
		if ( this.map != null)
			this.map.redraw(true);
	}
	
	// on_tick runs every 1000 / G.FPS millisecond in original implementation
	// ESframetime in seconds since Time.deltaTime is in seconds..
	float ESframetime =  1f / G.FPS ;
	float nextUpdate = 0f;
	
	void Update ()
	{
		if ( Time.time > this.nextUpdate && this.needs_timer)
		{
			this.nextUpdate = Time.time + this.ESframetime;
			
			this.on_tick ();
			
			if ( this.map.needs_rebuild)
				this.map.redraw(false);
		}
		
		foreach( Touch touch in Input.touches )
		{
			if ( touch.phase == TouchPhase.Moved )
			{
				// dragging
				this.scrollPositionMessage.y += touch.deltaPosition.y;
				this.scrollPosIntrolabel.y += touch.deltaPosition.y;
			}
		}
	}
	
	void OnDisable ()
	{
		if (this.maprenderer != null)
			this.maprenderer.gameObject.SetActive(false);
		if ( this.splash != null )
			this.splash.gameObject.SetActive(true);
	}
	
	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue((int)GUI_bindings.SCREEN_DEPTH.Map);
		
		this.MainScreen ();
	}
	
	Dictionary<string, string> suspicion_display_dict = new Dictionary<string, string>();
	Dictionary<string, Color> suspicion_display_dict_colors = new Dictionary<string, Color>();
	Dictionary<string, string> danger_display_dict = new Dictionary<string, string>();
	Dictionary<string, Color> danger_display_dict_colors = new Dictionary<string, Color>();
	
	Color color;
		
	/// <summary>
	/// Main game screen
	/// </summary>
	void MainScreen ()
	{
		GUI.enabled = this.menuGUI == null && this.introGUI == null && this.messageGUI == null && this.childDlg == null;
		
		// G.pl.recalc_cpu ();
		
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button ("Menu")) // , GUILayout.ExpandHeight (true)))
		{
			this.menuGUI = this.MenuGUI;
			G.play_sound();
		}
		
		GUILayout.Space ( 3 );

		GUILayout.BeginVertical();
		
			// turn off word wrap here due layout ( we need it otherwise for labels )
		
			GUILayout.Label (string.Format("DAY  {0,4}, {1:00}:{2:00}:{3:00}", G.pl.time_day, G.pl.time_hour, G.pl.time_min, G.pl.time_sec ), GUI_bindings.Instance.LabelSmall(false,null) );

			GUILayout.Label (string.Format("CASH {0} ({1})", G.to_money(G.pl.cash), G.to_money(G.pl.future_cash() ) ), GUI_bindings.Instance.LabelSmall(false,null) );

			long cpu_left = G.pl.available_cpus[0];
			long total_cpu = cpu_left + G.pl.sleeping_cpus;
		
			foreach ( KeyValuePair<string, long> kvp in G.pl.cpu_usage)
				cpu_left -= kvp.Value;
		
			long cpu_pool_value = 0;
			G.pl.cpu_usage.TryGetValue( "cpu_pool", out cpu_pool_value ) ;
		
			long cpu_pool = cpu_left + cpu_pool_value;
			
			if (UnityEngine.Event.current.type == EventType.Repaint && this.needs_timer)
			{
				long maint_cpu = 0;
				foreach ( string group in Group.group_list )
					detects_per_day[group] = 0;
			
				foreach ( Base _base in G.all_bases() )
				{
					if (_base.done)
						maint_cpu += _base.maintenance[1];
					Dictionary<string, int> detect_chance = _base.get_detect_chance(true);
				    foreach( string	group in Group.group_list )
				    {
						detects_per_day[group] += detect_chance[group] / 10000f;
					}
				}
		
				if (cpu_pool < maint_cpu)
					this.color = Color.red;
				else
					this.color = Color.white;
			}
			
			GUILayout.Label (string.Format("CPU  {0} ({1})", G.to_money(total_cpu), G.to_money(cpu_pool) ), GUI_bindings.Instance.LabelSmall(false, this.color) );
		
		GUILayout.EndVertical();
		
		if ( G.debug )
		{
		
			GUILayout.FlexibleSpace();
			
				if ( GUILayout.Button ("T") )
				{
					foreach( var t in G.techs )
					{
						if ( t.Key != "unknown_tech" )
							t.Value.finish();
					}
					G.play_sound();
				}
			
			GUILayout.FlexibleSpace();
			
				if ( GUILayout.Button ("$") )
				{
					G.pl.cash += 1000000000;
					G.play_sound();
				}
		}
		
		// (we can not has unicode in acknowtt font)
		
		GUILayout.FlexibleSpace();
		
			// if ( GUILayout.Button ("  " + '\u25AE'.ToString() + " " + '\u25AE'.ToString(), GUI_unitybindings.Instance.Button(G.curr_speed == speeds[0])  ) ) //, GUILayout.ExpandHeight (true) ) )
			if ( GUILayout.Button (" ii ", GUI_bindings.Instance.Button(G.curr_speed == speeds[0])  ) ) //, GUILayout.ExpandHeight (true) ) )
			{
				this.set_speed ( speeds[0] );
				G.play_sound();
			}
		
			// if ( GUILayout.Button ("  " + '\u25B6'.ToString(), GUI_unitybindings.Instance.Button(G.curr_speed == speeds[1]) )) //, GUILayout.ExpandHeight (true) ) )
			if ( GUILayout.Button (" > ", GUI_bindings.Instance.Button(G.curr_speed == speeds[1]) )) //, GUILayout.ExpandHeight (true) ) )
			{
				this.set_speed ( speeds[1] );
				G.play_sound();
			}
		
			// if ( GUILayout.Button (" " + '\u25B6'.ToString() + '\u25B6'.ToString(), GUI_unitybindings.Instance.Button(G.curr_speed == speeds[2]) )) //, GUILayout.ExpandHeight (true) ) )
			if ( GUILayout.Button (" >> ", GUI_bindings.Instance.Button(G.curr_speed == speeds[2]) )) //, GUILayout.ExpandHeight (true) ) )
			{
				this.set_speed ( speeds[2] );
				G.play_sound();
			}
		
			// if ( GUILayout.Button ('\u25B6'.ToString() + '\u25B6'.ToString() + '\u25B6'.ToString(), GUI_unitybindings.Instance.Button(G.curr_speed == speeds[3]) )) //, GUILayout.ExpandHeight (true) ) )
			if ( GUILayout.Button (">>>", GUI_bindings.Instance.Button(G.curr_speed == speeds[3]) )) //, GUILayout.ExpandHeight (true) ) )
			{
				this.set_speed ( speeds[3] );
				G.play_sound();
			}
		
			// if ( GUILayout.Button ('\u25B6'.ToString() + '\u25B6'.ToString() + '\u25B6'.ToString() + '\u25B6'.ToString(), GUI_unitybindings.Instance.Button(G.curr_speed == speeds[4]) )) //, GUILayout.ExpandHeight (true) ) )
			if ( GUILayout.Button (">>>>", GUI_bindings.Instance.Button(G.curr_speed == speeds[4]) )) //, GUILayout.ExpandHeight (true) ) )
			{
				this.set_speed ( speeds[4] );
				G.play_sound();
			}
		
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button ("Res/Tasks")) //, GUILayout.ExpandHeight (true)))
		{
			this.childDlg = this.GetComponent<ResearchScreen>();
			this.childDlg.Show( this );
			G.play_sound();
		}
		
		GUILayout.FlexibleSpace();
		
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();
		
		foreach (KeyValuePair<string, Location> l in G.locations)
		{
			if (l.Value.available ())
			{
				int width = (l.Value.name.Length + 5) * GUI_bindings.CHAR_WIDTH;
				int height = GUI_bindings.BUTTON_HEIGHT;
				if (GUI.Button (new Rect (l.Value.x - (width / 2), l.Value.y - ( height / 2 ), width, height), string.Format( "{0} ({1,2})", l.Value.name, l.Value.bases.Count ) ) )
				{
					this.open_location ( l.Value.id );
					G.play_sound();
				}
			}
		}
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button ("Finance")) //, GUILayout.ExpandHeight (true)))
		{
			this.childDlg = this.GetComponent<FinanceScreen>();
			this.childDlg.Show( this );
		}
		
		GUILayout.FlexibleSpace();
		
		if (GUILayout.Button ("Knowledge")) //, GUILayout.ExpandHeight (true)))
		{
			this.childDlg = this.GetComponent<KnowledgeScreen>();
			this.childDlg.Show( this );
		}
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space ( 5 );
		
	    // What we display in the suspicion section depends on whether
	    // Advanced Socioanalytics has been researched.  If it has, we
	    // show the standard percentages.  If not, we display a short
	    // string that gives a range of 25% as to what the suspicions
	    // are.
	    // A similar system applies to the danger levels shown.
	
		foreach( string group in Group.group_list )
		{
			int suspicion = G.pl.groups[group].suspicion;
			this.suspicion_display_dict_colors[group] = G.danger_colors[G.suspicion_to_danger_level(suspicion)];
			
			float detects = detects_per_day[group];
			int danger_level = G.pl.groups[group].detects_per_day_to_danger_level(detects);
			this.danger_display_dict_colors[group] = G.danger_colors[danger_level];

			if (G.techs["Advanced Socioanalytics"].done)
			{
				suspicion_display_dict[group] = G.to_percent(suspicion, true);
				danger_display_dict[group] = G.to_percent((int)(detects*10000), true);
			}
			else
			{
				suspicion_display_dict[group] = G.suspicion_to_detect_str(suspicion);
				danger_display_dict[group] = G.danger_level_to_detect_str(danger_level);
			}
		}
		
		if ( !G.pl.had_grace )
		{
			GUILayout.BeginHorizontal();
			
			GUILayout.Label( "[SUSPICION]", GUI_bindings.Instance.LabelSmall(false,null));
			
			GUILayout.Label( string.Format ( "NEWS:{0:5}", this.suspicion_display_dict["news"] ), GUI_bindings.Instance.LabelSmall(false,this.suspicion_display_dict_colors["news"]) );
			
			GUILayout.Label( string.Format ( " SCIENCE:{0:5}", this.suspicion_display_dict["science"] ), GUI_bindings.Instance.LabelSmall(false,this.suspicion_display_dict_colors["science"]) );

			GUILayout.Label( string.Format ( " COVERT:{0:5}", this.suspicion_display_dict["covert"] ), GUI_bindings.Instance.LabelSmall(false,this.suspicion_display_dict_colors["covert"]) );

			GUILayout.Label( string.Format ( " PUBLIC:{0:5}", this.suspicion_display_dict["public"] ), GUI_bindings.Instance.LabelSmall(false,this.suspicion_display_dict_colors["public"]) );
			
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			
			GUILayout.Label( "[DET.RATE] ", GUI_bindings.Instance.LabelSmall(false,null));
			
			GUILayout.Label( string.Format ( " NEWS:{0}", this.danger_display_dict["news"] ), GUI_bindings.Instance.LabelSmall(false,this.danger_display_dict_colors["news"]) );
			
			GUILayout.Label( string.Format ( " SCIENCE:{0}", this.danger_display_dict["science"] ), GUI_bindings.Instance.LabelSmall(false,this.danger_display_dict_colors["science"]) );

			GUILayout.Label( string.Format ( " COVERT:{0}", this.danger_display_dict["covert"] ), GUI_bindings.Instance.LabelSmall(false,this.danger_display_dict_colors["covert"]) );

			GUILayout.Label( string.Format ( " PUBLIC:{0}", this.danger_display_dict["public"] ), GUI_bindings.Instance.LabelSmall(false,this.danger_display_dict_colors["public"]) );

			GUILayout.EndHorizontal();
		}
		else
		{
			GUILayout.Label( "", GUI_bindings.Instance.LabelSmall(false,null) );
			GUILayout.Label( "", GUI_bindings.Instance.LabelSmall(false,null) );
		}
		
		GUILayout.EndArea();
		
		if ( this.menuGUI != null )
			this.menuGUI();
		
		if ( this.introGUI != null )
			this.introGUI();
			
		// show any message/s
		if ( this.messages.Count > 0 )
		{
			this.messageGUI = this.MessageGUI;
			this.messageGUI();
		}
		else
		{
			this.messageGUI = null;
		}
	}
	
	Vector2 scrollPositionMessage = Vector2.zero;
	
	bool exit = false;

	void MessageGUI ()
	{
		GUI.enabled = true;
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT);
		
		this.scrollPositionMessage = GUILayout.BeginScrollView (this.scrollPositionMessage);
		
		GUILayout.Label ( this.messages[0] as string, GUI_bindings.Instance.LabelAlt(true,this.messages[1] as Color?, false) );
		
		GUILayout.EndScrollView ();
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("OK"))
		{
			this.messages.RemoveAt(0);
			this.messages.RemoveAt(0);
			
			G.play_sound();
			
			if ( this.exit )
			{
				this.enabled = false;
				this.GetComponent<Main_menu>().Show(null);
			}
		}
		
		GUILayout.EndArea ();
	}
	
	List<string> intro_text_all = null;
	string intro_text = "";
	int intro_text_pos = 0;
	bool intro_next = true;
	
	Vector2 scrollPosIntrolabel = Vector2.zero;
	
	void IntroGUI ()
	{
		GUI.enabled = true;
		
		if (this.intro_next)
		{
			this.intro_next = false;
			
			// yield ftw
			
			if (intro_text_pos < intro_text_all.Count)
			{
				this.intro_text = this.intro_text_all[intro_text_pos];
				this.intro_text_pos++;
			}
			else
			{
				this.introGUI = null;
			}
		}
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT , "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT);
		
		this.scrollPosIntrolabel = GUILayout.BeginScrollView (this.scrollPosIntrolabel);
		
		GUILayout.Label (intro_text, GUI_bindings.Instance.LabelAlt(true,null, false));
		
		GUILayout.EndScrollView ();
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("Continue") )
		{
			this.intro_next = true;
			G.play_sound();
		}
		
		if (GUILayout.Button ("Skip"))
		{
			this.introGUI = null;
			G.play_sound();
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea ();
	}
	
	void MenuGUI ()
	{
		GUI.enabled = this.childDlg == null;
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT);
		
		// center
		GUILayout.BeginHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginVertical ();
		
		// disable save && load in webplayer
		#if UNITY_WEBPLAYER
		GUI.enabled = false;
		#endif
		// nice trick.. here MaxWidth actually *stretches* the width of a button
		if (GUILayout.Button (" Save game", GUI_bindings.Instance.ButtonMainMenu(), GUILayout.MaxWidth (Screen.width / 2)))
		{
			this.childDlg = this.GetComponent<SaveScreen> ();
			this.childDlg.Show (this);
			G.play_sound();
		}
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button (" Load game", GUI_bindings.Instance.ButtonMainMenu()))
		{
			this.childDlg = this.GetComponent<LoadScreen> ();
			this.childDlg.Show (this);
			G.play_sound();
		}
		
		#if UNITY_WEBPLAYER
		GUI.enabled = true;
		#endif
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Options", GUI_bindings.Instance.ButtonMainMenu()))
		{
			this.childDlg = this.GetComponent<OptionsScreen> ();
			this.childDlg.Show (this);
		}
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Quit", GUI_bindings.Instance.ButtonMainMenu()))
		{
			this.enabled = false;
			this.GetComponent<Main_menu>().Show(null);
			G.play_sound();
		}
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Back", GUI_bindings.Instance.ButtonMainMenu()))
		{
			this.menuGUI = null;
			G.play_sound();
		}
		
		GUILayout.EndVertical ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea();
	}
	
	// TODO: lerp skin button border, padding, margin, on start. some g/fx for menu items..
	
	ArrayList messages = new ArrayList();
	
	public void show_message (string message, Color? color)
	{
		this.messages.Add(message);
		this.messages.Add(color);
	}
	
	bool needs_timer = true; // == time update running
	
    void set_speed(int speed)
	{
        G.curr_speed = speed;
        if (speed == 0)
		{
            this.needs_timer = false;
		}
        else
		{
            this.needs_timer = true;
		}
	}

    void open_location (string location)
    {
    	this.childDlg = this.GetComponent<LocationScreen> ();
    	(this.childDlg as LocationScreen).location = G.locations[location];
		this.childDlg.Show( this );
	}
	
    void show_intro ()
    {
    	this.intro_text_all = G.get_intro ();
    	this.introGUI = this.IntroGUI;
	}

	float leftovers = 1f;
	
    void on_tick ()
    {
    	if (this.menuGUI != null || this.messageGUI != null || this.introGUI != null || this.childDlg != null)
    		// lost 'focus'
    		return;
		
    	if (!G.pl.intro_shown)
		{
    		G.pl.intro_shown = true;
    		this.show_intro ();
		}
		
		G.pl.recalc_cpu ();

        this.leftovers += G.curr_speed / (float)(G.FPS);
        if (this.leftovers < 1)
            return;
		
        int secs = (int)this.leftovers;
        this.leftovers %= 1f;

        // Run this tick.
		long mins_passed = 0;
		CashInfo cashi = null;
		CPUInfo cpui = null;
		
        G.pl.give_time(secs, false, out cashi, out cpui, out mins_passed );

        // Update the day/night image every minute of game time, or at
        // midnight if going fast.
        // if (G.curr_speed == 0 || ( mins_passed > 0 && G.curr_speed < 100000) || (G.curr_speed>=100000 && G.pl.time_hour==0))
        if ( ( mins_passed > 0 && G.curr_speed < 100000) || (G.curr_speed>=100000 && G.pl.time_hour==0) )
            this.map.needs_rebuild = true;
        else
        	this.map.needs_rebuild = false;
		
		this.exit = false;
        int lost = G.pl.lost_game();
        if (lost == 1)
		{
            G.play_music("lose");
            this.exit = true;
            this.show_message(G.strings["lost_nobases"], Color.white);
		}
        else if (lost == 2)
		{
            G.play_music("lose");
            this.exit = true;
            this.show_message(G.strings["lost_sus"], Color.white);
		}
	}
}