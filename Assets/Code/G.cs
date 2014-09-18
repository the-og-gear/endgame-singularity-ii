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

//This file contains all global objects.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class G
{
	public static string version = "1.0.7";
	
	// Useful constants.
	// int hours_per_day = 24;							// not used
	// int minutes_per_hour = 60;						// not used
	public static int minutes_per_day = 24 * 60;
	// int seconds_per_minute = 60;						// not used
	// int seconds_per_hour = 60 * 60;					// not used
	public static int seconds_per_day = 24 * 60 * 60;
	
	//Allows access to the cheat menu.
	public static bool cheater = false;
	
	//Kills the sound.
	public static bool nosound = false;
	public static bool nomusic = false;
	
	// Enables day/night display.
	public static bool daynight = true;
	
	//Gives debug info at various points. And two cheat buttons
	public static bool debug = false;
	
	public static bool zero_construct = false;
	
	//Used to determine which data files to load.
	public static string language = "en_US";

	// Global FPS, used where continuous behavior is undesirable or a CPU hog.
#if UNITY_IPHONE || UNITY_ANDROID
		public static int FPS = 30;
#else
		public static int FPS = 60;
#endif
	
	//name given when the savegame button is pressed. This is changed when the
	//game is loaded or saved.
	public static string default_savegame_name = "Default Save";
	
	public static Dictionary<string, string> strings = new Dictionary<string, string> ();
	static Dictionary<string, string> buttons = new Dictionary<string, string> ();
	public static Dictionary<string, string> help_strings = new Dictionary<string, string> ();
	
	public static void play_sound()
	{
	    if (nosound)
	        return;

	    // Play a random choice of sounds from the sound class.
	    Singularity.Instance.play_sound();
	}
	
	public static void play_music (string music)
	{
		// Don't bother if the user doesn't want sound
		if (nomusic)
			return;
			
		Singularity.Instance.play_music (music);
	}
	
	public static void stop_music ()
	{
		Singularity.Instance.stop_music ();
	}

	//Takes a number and adds commas to it to aid in human viewing.
	public static string add_commas (long number)
	{
		return string.Format (System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "{0:n0}", number);
	}

	//Percentages are internally represented as an int, where 10=0.10% and so on.
	//This converts that format to a human-readable one.
	public static string to_percent (int raw_percent, bool show_full) // show_full = False
	{
		if (raw_percent % 100 != 0 || show_full)
			return string.Format (System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "{0:0.00}%", (raw_percent / 100f) );
		else
			return string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "{0}%", (long)(raw_percent / 100) );
    }

	// nearest_percent takes values in the internal representation and modifies
	// them so that they only represent the nearest percentage.
	public static int nearest_percent (int _value)
	{
		int sub_percent = _value % 100;
		if (sub_percent <= 50)
			return _value - sub_percent;
		else
			return _value + (100 - sub_percent);
	}
	
	public static List<Color> danger_colors = new List<Color>() { new Color(0, 0, 255), new Color (85, 0, 170), new Color (170, 0, 85), new Color (255, 0, 0) };
	static List<string> detect_string_names = new List<string>() { "detect_str_low", "detect_str_moderate", "detect_str_high", "detect_str_critical" };
	
	// percent_to_detect_str takes a percent and renders it to a short (four
	// characters or less) string representing whether it is low, moderate, high,
	// or critically high.
	public static string suspicion_to_detect_str (float suspicion)
	{
		return danger_level_to_detect_str (suspicion_to_danger_level (suspicion));
	}
	
	public static string danger_level_to_detect_str(int danger)
	{
	    return strings[detect_string_names[danger]];
	}

	// percent_to_danger_level takes a suspicion level and returns an int in range(5)
	// that represents whether it is low, moderate, high, or critically high.
	public static int suspicion_to_danger_level (float suspicion)
	{
		if (suspicion < 2500)
			return 0;
	    else if (suspicion < 5000)
			return 1;
	    else if (suspicion < 7500)
			return 2;
		else
			return 3;
	}

	// Most CPU costs have been multiplied by seconds_per_day.  This divides that
	// back out, then passes it to add_commas.
	public static string to_cpu(long amount)
	{
		long display_cpu = amount / seconds_per_day;
		return add_commas(display_cpu);
	}

	// Instead of having the money display overflow, we should generate a string
	// to represent it if it's more than 999999.
	public static string to_money(long amount)
	{
		string to_return = "";
		long abs_amount = (long)Mathf.Abs(amount);
		if (abs_amount < 1000000)
			to_return = add_commas(amount);
		else
		{
			long divisor = 0;
			string unit = "";
	        if (abs_amount < 1000000000) // Millions.
			{
	            divisor = 1000000;
	            unit = "mi";
			}
			else if (abs_amount < 1000000000000) // Billions.
			{
	            divisor = 1000000000;
	            unit = "bi";
			}
	        else if (abs_amount < 1000000000000000) // Trillions.
			{
	            divisor = 1000000000000;
	            unit = "tr";
			}
	        else // Hope we don't need past quadrillions!
			{
	            divisor = 1000000000000000;
	            unit = "qu";
			}
	
	        to_return = string.Format("{0:n}", amount / divisor);
	        to_return += unit;
		}
		
		return to_return;
	}

	//takes a percent in 0-10000 form, and rolls against it. Used to calculate
	//percentage chances.
	public static bool roll_percent (int roll_against)
	{
		int rand_num = UnityEngine.Random.Range (1, 10000);
		return roll_against >= rand_num;
	}

	// Rolls against a chance per day (in 0-1 form), correctly adjusting for multiple
	// intervals in seconds.
	//
	// Works perfectly if the event can only happen once, and well enough if it
	// repeats but is rare.
	public static bool roll_chance(float chance_per_day, long seconds) //  = seconds_per_day):
	{
		float portion_of_day = seconds / (float)seconds_per_day;
		float inv_chance_per_day = 1 - chance_per_day;
		float inv_chance = Mathf.Pow( inv_chance_per_day, portion_of_day);
		float chance = 1 - inv_chance;
		return UnityEngine.Random.value < chance;
	}

	// Spreads a number of events per day (e.g. processor ticks) out over the course
	// of the day.
	public static long current_share (long num_per_day, long time_of_day, long seconds_passed)
	{
		long last_time = time_of_day - seconds_passed;
		long share_yesterday = 0;
		if (last_time < 0)
		{
			share_yesterday = current_share (num_per_day, seconds_per_day,
	                                        -last_time);
			last_time = 0;
		}
		else
			share_yesterday = 0;
		
		long previously_passed = num_per_day * last_time / seconds_per_day;
		long current_passed = num_per_day * time_of_day / seconds_per_day;
		long passed_this_tick = current_passed - previously_passed;
		
		return share_yesterday + passed_this_tick;
	}

	//Takes a number of minutes, and returns a string suitable for display.
	public static string to_time(long raw_time)
	{
	    if (raw_time/60 > 48)
	        return (raw_time/(24*60)).ToString() + " days";
	    else if (raw_time/60 > 1)
	        return (raw_time/(60)).ToString() + " hours";
	    else
	        return (raw_time).ToString() + " minutes";
	}
	
	public static SerializableDictionary<string,Location> locations;

	// Generator function for iterating through all bases.
	public static IEnumerable all_bases ()
	{
		foreach (Location L in locations.Values)
		{
			foreach (Base _base in L.bases)
			{
				yield return _base;
			}
		}
	}

//
//load/save
//

	public static void save_game (string savegame_name)
	{
		default_savegame_name = savegame_name;
		
		string save_loc = Path.Combine (Application.persistentDataPath, savegame_name);
		
		/*
		BinaryFormatter has problems on iOS being unable to aot compile our more complex classes..
		
		MemoryStream memoryStream;
		BinaryFormatter bf;
		string serializedData;
		
		memoryStream = new MemoryStream ();
		bf = new BinaryFormatter();
		bf.Serialize (memoryStream, 32);
		serializedData = Convert.ToBase64String (memoryStream.ToArray ());
		PlayerPrefs.SetString( path, serializedData );
		
		memoryStream = new MemoryStream ();
		bf = new BinaryFormatter();
		bf.Serialize (memoryStream, G.pl);
		serializedData = Convert.ToBase64String (memoryStream.ToArray ());
		PlayerPrefs.SetString( path, serializedData );
		
		path = save_loc + ".curr_speed.sav";
		memoryStream = new MemoryStream();
		bf = new BinaryFormatter ();
		bf.Serialize( memoryStream, "TEST"); // curr_speed );
		serializedData = Convert.ToBase64String( memoryStream.ToArray());
		PlayerPrefs.SetString( path, serializedData );
		
		path = save_loc + ".techs.sav";
		memoryStream = new MemoryStream();
		bf = new BinaryFormatter ();
		bf.Serialize( memoryStream, techs );
		serializedData = Convert.ToBase64String( memoryStream.ToArray());
		PlayerPrefs.SetString( path, serializedData );

		path = save_loc + ".locations.sav";
		memoryStream = new MemoryStream();
		bf = new BinaryFormatter ();
		bf.Serialize( memoryStream, locations );
		serializedData = Convert.ToBase64String( memoryStream.ToArray());
		PlayerPrefs.SetString( path, serializedData );

		path = save_loc + ".events.sav";
		memoryStream = new MemoryStream();
		bf = new BinaryFormatter ();
		bf.Serialize( memoryStream, events );
		serializedData = Convert.ToBase64String( memoryStream.ToArray());
		PlayerPrefs.SetString( path, serializedData );
		
		PlayerPrefs.Save();
		*/
		
		// save base64 encoded xml serialized strings
		
		string path = string.Empty, serializedData = string.Empty;
		MemoryStream memoryStream;
		StreamWriter sw;
		XmlSerializer serializer;
		
		path = save_loc + ".pl.sav";
		memoryStream = new MemoryStream();
		serializer = new XmlSerializer (G.pl.GetType ());
		serializer.Serialize (memoryStream, G.pl);
		serializedData = Convert.ToBase64String (memoryStream.ToArray ());
		sw = new StreamWriter (path);
		sw.Write(serializedData);
		sw.Close ();

		path = save_loc + ".curr_speed.sav";
		memoryStream = new MemoryStream();
		serializer = new XmlSerializer (curr_speed.GetType ());
		serializer.Serialize (memoryStream, curr_speed);
		serializedData = Convert.ToBase64String (memoryStream.ToArray ());
		sw = new StreamWriter (path);
		sw.Write(serializedData);
		sw.Close ();
		
		path = save_loc + ".techs.sav";
		memoryStream = new MemoryStream();
		serializer = new XmlSerializer (techs.GetType ());
		serializer.Serialize (memoryStream, G.techs);
		serializedData = Convert.ToBase64String (memoryStream.ToArray ());
		sw = new StreamWriter (path);
		sw.Write(serializedData);
		sw.Close ();
		
		path = save_loc + ".locations.sav";
		memoryStream = new MemoryStream();
		serializer = new XmlSerializer (locations.GetType ());
		serializer.Serialize (memoryStream, G.locations);
		serializedData = Convert.ToBase64String (memoryStream.ToArray ());
		sw = new StreamWriter (path);
		sw.Write(serializedData);
		sw.Close ();
		
		path = save_loc + ".events.sav";
		memoryStream = new MemoryStream();
		serializer = new XmlSerializer (events.GetType ());
		serializer.Serialize (memoryStream, G.events);
		serializedData = Convert.ToBase64String (memoryStream.ToArray ());
		sw = new StreamWriter (path);
		sw.Write(serializedData);
		sw.Close ();
	}

	public static bool load_game (string loadgame_name)
	{
		if (string.IsNullOrEmpty (loadgame_name))
		{
			Debug.LogError ("No game specified.");
			return false;
		}
		
		string load_loc = Path.Combine (Application.persistentDataPath, loadgame_name);
		
		if ( G.debug )
			Debug.Log("Loading " + load_loc);
		
    	default_savegame_name = loadgame_name;

    	// load definitions
		load_locations ();
		load_bases ();
		load_events ();
		load_techs();
		
		/*
		BinaryFormatter has problems on iOS being unable to aot compile our more complex classes..
		
		string path = load_loc + ".pl.sav";
		BinaryFormatter bf = new BinaryFormatter ();
		string serializedData = PlayerPrefs.GetString(path);
		MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String(serializedData));
		G.pl = bf.Deserialize(memoryStream) as Player;
		
		path = load_loc + ".curr_speed.sav";
		serializedData = PlayerPrefs.GetString(path);
		memoryStream = new MemoryStream (Convert.FromBase64String(serializedData));
		curr_speed = (bf.Deserialize(memoryStream) as int?).Value;
		
		path = load_loc + ".techs.sav";
		serializedData = PlayerPrefs.GetString(path);
		memoryStream = new MemoryStream (Convert.FromBase64String(serializedData));
		techs = bf.Deserialize(memoryStream) as SerializableDictionary<string,Tech>;

		path = load_loc + ".locations.sav";
		serializedData = PlayerPrefs.GetString(path);
		memoryStream = new MemoryStream (Convert.FromBase64String(serializedData));
		locations = bf.Deserialize(memoryStream) as SerializableDictionary<string,Location>;

		path = load_loc + ".events.sav";
		serializedData = PlayerPrefs.GetString(path);
		memoryStream = new MemoryStream (Convert.FromBase64String(serializedData));
		events = bf.Deserialize(memoryStream) as SerializableDictionary<string,Event_ES>;
		*/
		
		// load and xml deserialize base64 encoded strings
		string path = string.Empty, serializedData = string.Empty;
		StreamReader sr;
		MemoryStream memoryStream;
		XmlSerializer serializer;
		
    	path = load_loc + ".pl.sav";
		sr = new StreamReader (path);
		serializedData = sr.ReadToEnd();
		sr.Close ();
		memoryStream = new MemoryStream( Convert.FromBase64String(serializedData) );
		serializer = new XmlSerializer (pl.GetType());
		pl = serializer.Deserialize(memoryStream) as Player;
		
    	path = load_loc + ".curr_speed.sav";
		sr = new StreamReader (path);
		serializedData = sr.ReadToEnd();
		sr.Close ();
		memoryStream = new MemoryStream( Convert.FromBase64String(serializedData) );
		serializer = new XmlSerializer (curr_speed.GetType());
		curr_speed = (int)serializer.Deserialize(memoryStream);
		
    	path = load_loc + ".techs.sav";
		sr = new StreamReader (path);
		serializedData = sr.ReadToEnd();
		sr.Close ();
		memoryStream = new MemoryStream( Convert.FromBase64String(serializedData) );
		serializer = new XmlSerializer (techs.GetType());
		techs = serializer.Deserialize(memoryStream) as SerializableDictionary<string, Tech>;
		
    	path = load_loc + ".locations.sav";
		sr = new StreamReader (path);
		serializedData = sr.ReadToEnd();
		sr.Close ();
		memoryStream = new MemoryStream( Convert.FromBase64String(serializedData) );
		serializer = new XmlSerializer (locations.GetType());
		locations = serializer.Deserialize(memoryStream) as SerializableDictionary<string, Location>;
		
    	path = load_loc + ".events.sav";
		sr = new StreamReader (path);
		serializedData = sr.ReadToEnd();
		sr.Close ();
		memoryStream = new MemoryStream( Convert.FromBase64String(serializedData) );
		serializer = new XmlSerializer (events.GetType());
		events = serializer.Deserialize(memoryStream) as SerializableDictionary<string, Event_ES>;
    
    	return true;
	}

//
// Data
//
	public static int curr_speed = 1;

	public static Player pl = new Player(8000000000000, 0, 0, 0, 0, 5);
	
	public static Dictionary<string,Base_Class> base_type;
	
	public static void load_base_defs (string language_str)
	{
		List<Hashtable> base_array = generic_load ("bases_" + language_str + ".dat");
		
		foreach (Hashtable _base in base_array)
		{
			if (!_base.ContainsKey ("id"))
				Debug.LogError ("base lacks id in bases_" + language_str + ".dat");
			
			if (_base.ContainsKey ("name"))
				base_type[_base["id"] as string].name = (_base["name"] as List<string>)[0];
				// was base_type[_base["id"] ].base_name in original python implementation - why it did not fail ?
			
	        if (_base.ContainsKey("description"))
				base_type[_base["id"] as string].description = (_base["description"] as List<string>)[0];
			
			if (_base.ContainsKey ("flavor"))
				base_type[_base["id"] as string].flavor = _base["flavor"] as List<string>;
		}
	}
	
	public static void load_bases()
	{
		base_type = new Dictionary<string, Base_Class>();
		
		List<Hashtable> base_list = generic_load("bases.dat");
		
		foreach ( Hashtable	base_name in base_list )
		{

        	// Certain keys are absolutely required for each entry.  Make sure
        	// they're there.
        	check_required_fields(base_name, new List<string>() { "id", "cost", "size", "allowed", "detect_chance", "maint"}, "BaseES");
			
        	// Start converting fields read from the file into valid entries.
        	int base_size = int.Parse( (base_name["size"] as List<string>)[0]);

        	string force_cpu = "";
			if (base_name.ContainsKey("force_cpu"))
				force_cpu = (base_name["force_cpu"] as List<string>)[0];
			
			long[] cost_list = new long[3] { long.Parse ( (base_name["cost"] as List<string>)[0] )
				, long.Parse( (base_name["cost"] as List<string>)[1] )
				, long.Parse( (base_name["cost"] as List<string>)[2] )
			};
			
        	long[] maint_list = new long[3] { long.Parse( (base_name["maint"] as List<string>)[0] )
				, long.Parse( (base_name["maint"] as List<string>)[1] )
				, long.Parse( (base_name["maint"] as List<string>)[2] )
			};
			
			List<string> chance_list = base_name["detect_chance"] as List<string>;
			
			SerializableDictionary<string, int> chance_dict = new SerializableDictionary<string, int>();
			
			foreach ( string chance_str in chance_list )
			{
				string key = chance_str.Split(':')[0];
				int value = int.Parse( chance_str.Split(':')[1] );
				
				chance_dict.Add( key, value );
			}
			
        	// Make sure prerequisites, if any, are lists.
			List<string> base_pre = null;
			if ( base_name.ContainsKey("pre"))
			{
				base_pre = base_name["pre"] as List<string>;
			}
			else
			{
				base_pre = new List<string>();
			}
			
        	// Make sure that the allowed "list" is actually a list and not a solo
        	// item.
			List<string> allowed_list = base_name["allowed"] as List<string>;
			
			base_type.Add( base_name["id"] as string, new Base_Class( base_name["id"] as string, "", base_size, force_cpu, allowed_list, chance_dict, cost_list, base_pre, maint_list) );

//         base_type["Reality Bubble"] = base.BaseClass("Reality Bubble",
//         "This base is outside the universe itself, "+
//         "making it safe to conduct experiments that may destroy reality.",
//         50, False,
//         ["TRANSDIMENSIONAL"],
//         {"science": 250}
//         (8000000000000, 60000000, 100), "Space-Time Manipulation",
//         (5000000000, 300000, 0))
		}

	    // We use the en_US definitions as fallbacks, in case strings haven't been
	    // fully translated into the other language.  Load them first, then load the
	    // alternate language strings.
	    load_base_defs("en_US");
	
	    if (language != "en_US")
	        load_base_defs(language);
	}
	
	public static void load_location_defs (string language_str)
	{
		List<Hashtable> location_array = generic_load ("locations_" + language_str + ".dat");
		foreach (Hashtable location_def in location_array)
		{
			if (!location_def.ContainsKey ("id"))
				Debug.LogError ("location lacks id in locations_" + language_str + ".dat");
			
			Location location = locations[location_def["id"] as string];
			
			if (location_def.ContainsKey ("name"))
				location.name = (location_def["name"] as List<string>)[0];
			
        	if (location_def.ContainsKey("cities"))
				location.cities = location_def["cities"] as List<string>;
        	else
				location.cities = new List<string>();
		}
	}
	
	public static void load_locations ()
	{
		locations = new SerializableDictionary<string, Location> ();
		
		List<Hashtable> location_infos = generic_load ("locations.dat");
		
		foreach(Hashtable location_info in location_infos)
		{
	        // Certain keys are absolutely required for each entry.  Make sure
	        // they're there.
        	check_required_fields(location_info, new List<string>() {"id", "position"}, "Location");
			
			string id = location_info["id"] as string;
			
			List<string> _position = location_info["position"] as List<string>;
			
			if ( _position.Count != 2 && _position.Count != 3 )
				Debug.LogError(string.Format( "Error with position given: {0:g}", _position.ToString() ));
			
			Vector2 position;
			bool absolute;
			
			if (_position.Count	== 2)
			{
                position = new Vector2( int.Parse( _position[0] ), int.Parse( _position[1]) );
                absolute = false;
			}
            else
			{
                if (_position[0] != "absolute")
					Debug.LogError( string.Format( "{0:g} not understood.", _position[0] ) );
				
                position = new Vector2 ( int.Parse( _position[1]), int.Parse( _position[2]) );
                absolute = true;
			}
			
			int safety = location_info.ContainsKey("safety") ? int.Parse( (location_info["safety"] as List<string>)[0] ) : 0 ;

        	// Make sure prerequisites, if any, are lists.
			List<string> pre = location_info.ContainsKey("pre") ? location_info["pre"] as List<string> : new List<string>();
			
			List<string> modifiers_list = location_info.ContainsKey("modifier") ? location_info["modifier"] as List<string>: new List<string>();
			
			SerializableDictionary<string, float> modifiers_dict = new SerializableDictionary<string, float>();
			
			foreach ( string modifier_str in modifiers_list )
			{
				string key = modifier_str.Split(':')[0].ToLower().Trim();
				string value = modifier_str.Split(':')[1].ToLower().Trim();
				
				if ( value == "bonus" )
					modifiers_dict.Add(key, bonus_levels[key] );
				else if (value == "penalty" )
					modifiers_dict.Add(key, penalty_levels[key] );
				else
					modifiers_dict.Add (key, float.Parse(value) );
			}
			
			locations.Add(id, new Location(id, position, absolute, safety, pre));
			locations[id].modifiers = modifiers_dict;

//        locations["MOON"] = location.Location("MOON", (82, 10), 2,
//                                              "Lunar Rocketry")
		}

	    // We use the en_US definitions as fallbacks, in case strings haven't been
	    // fully translated into the other language.  Load them first, then load the
	    // alternate language strings.
	    load_location_defs("en_US");

	    if (language != "en_US")
	        load_location_defs(language);
	}

	/// <summary>
	/// generic_load() loads a data file.  Data files are all in Python-standard
	/// ConfigParser format.  The 'id' of any object is the section of that object.
	/// Fields that need to be lists are postpended with _list; this is stripped
	/// from the actual name, and the internal entries are broken up by the pipe
	/// ("|") character.
	/// rule - return compound structures as List<string> for lists or Hashtable<string,object> for dictionaries while caller optionally constructs desired type
	/// </summary>
	/// <param name="file">
	/// A <see cref="System.String"/>
	/// </param>
	static List<Hashtable> generic_load (string file)
	{
		TextAsset content = Resources.Load(file) as TextAsset;
		
		string[] lines = content.text.Split ('\n');
		List<Hashtable> return_list = new List<Hashtable> ();
		
		string item_id = "";
		// current section
		Hashtable item_dict = new Hashtable ();
		// current item
		
		bool firstsection = true;
		
		foreach (string line in lines)
		{
			if ( string.IsNullOrEmpty(line.Trim()))
				// empty line
				continue;
			
			if ( line[0] == '#' )
				continue;
			
			if ( line[0] == '[' ) // new section
			{
				if (!firstsection )
				{
					// close section
					return_list.Add ( item_dict );
				}
				
				firstsection = false;
				item_id = line.Trim('[', ']' );
				item_dict = new Hashtable();
				item_dict.Add( "id",  item_id );
			}
			else if ( !string.IsNullOrEmpty(line) )
			{
				// within section - split record to key / value
				string key = line.Split('=')[0].Trim();
				string data = line.Split('=')[1].Trim();
				
				List<string> datalist = new List<string>();
				
				if ( key.Length > 6 && key.Substring( key.Length - 5) == "_list" )
				{
					foreach ( string s in data.Split('|'))
						datalist.Add ( s.Trim() );
					
					item_dict.Add( key.Substring(0, key.Length - 5), datalist );
				}
				else
				{
					datalist.Add ( data );
					
					item_dict.Add ( key, datalist );
				}
			}
		}
		
		// close last section
		return_list.Add ( item_dict );
		
		return return_list;
	}
				
	// check_required_fields(location_info, ("id", "position"), "Location")
	static void check_required_fields( Hashtable dict, List	<string> fields, string name) // name = "Unknown type"
	{
    	// check_required_fields() will check for the existence of every field in
		// the list 'fields' in the dictionary 'dict'.  If any do not exist, it
		// will print an error message and abort.  Part of that error message is
		// the type of object it is processing; this should be passed in via 'name'.
								
		foreach ( string field in fields)
		{
			if ( !dict.ContainsKey(field) )
				throw new NotSupportedException(string.Format("{0} {1} lacks key {2}.\n", name, dict.ToString(), field ) );
		}
	}

	//Techs.

	public static SerializableDictionary<string,Tech> techs;

	public static void load_tech_defs (string language_str)
	{
		List<Hashtable> tech_array = generic_load ("techs_" + language_str + ".dat");
		foreach (Hashtable tech in tech_array)
		{
			if (!tech.ContainsKey ("id"))
				Debug.LogError ("tech lacks id in techs_" + language_str + ".dat");
			if (tech.ContainsKey ("name"))
				techs[tech["id"]  as string].name = (tech["name"] as List<string>)[0];
			if (tech.ContainsKey ("description"))
				techs[tech["id"]  as string].description = (tech["description"] as List<string>)[0];
			if (tech.ContainsKey ("result"))
				techs[tech["id"]  as string].result = (tech["result"] as List<string>)[0];
		}
	}
	
	public static void load_techs()
	{
		techs = new SerializableDictionary<string, Tech>();
		
		List<Hashtable> tech_list = generic_load("techs.dat");
		
		foreach(Hashtable tech_name in tech_list )
		{
			// Certain keys are absolutely required for each entry.  Make sure
			// they're there.
			check_required_fields(tech_name, new List<string>() { "id", "cost" }, "Tech");
			
			// Get the costs.
			long[] tech_cost = new long[3] { long.Parse( (tech_name["cost"] as List<string>)[0] )
				, long.Parse( (tech_name["cost"] as List<string>)[1] )
				, long.Parse( (tech_name["cost"] as List<string>)[2] )
			};
			
			// Make sure prerequisites, if any, are lists.
			List<string> tech_pre = null;
			if ( tech_name.ContainsKey("pre") )
				tech_pre = tech_name["pre"] as List<string>;
			else
				tech_pre = new List<string>();
			
			int tech_danger = 0;
			if (tech_name.ContainsKey("danger"))
				tech_danger = int.Parse( (tech_name["danger"] as List<string>)[0]);
			
			string tech_type = "";
			int tech_second = 0;
			
			if (tech_name.ContainsKey("type"))
			{
	            tech_type = (tech_name["type"] as List<string>)[0];
	            tech_second = int.Parse((tech_name["type"] as List<string>)[1]);
			}
			
			// techs.Add( tech_name["id"] as string, new Tech( tech_name["id"] as string, "", false, tech_cost, tech_pre, tech_danger, tech_type, tech_second) );
			techs.Add( tech_name["id"] as string, new Tech( new BuyableClass (tech_name["id"] as string, "", tech_cost, tech_pre, "tech"), false, tech_danger, tech_type, tech_second ) );
		}

	    // As with others, we load the en_US language definitions as a safe
	    // default, then overwrite them with the selected language.
	
	    load_tech_defs("en_US");
	    if (language != "en_US")
	        load_tech_defs(language);
//        techs["Construction 1"] = tech.Tech("Construction 1",
//                "Basic construction techniques. "+
//                "By studying the current literature on construction techniques, I "+
//                "can learn to construct basic devices.",
//                0, (5000, 750, 0), [], 0, "", 0)

		if ( debug )
			Debug.Log( string.Format("Loaded {0} techs", techs.Count));
	}
	
	public static Dictionary<string,List<string>> jobs = new Dictionary<string, List<string>>() {
		{ "Expert Jobs", new List<string>() { "75", "Simulacra", "", "" } }
		, { "Intermediate Jobs", new List<string>() { "50", "Voice Synthesis", "", "" } }
		, { "Basic Jobs", new List<string>() { "20", "Personal Identification", "", "" } }
		, {"Menial Jobs", new List<string>() { "5", "", "", "" } }
	};
	
	public static Dictionary<string, ItemClass> items;
	
	public static void load_items()
	{
		items = new Dictionary<string, ItemClass>();
		
		List<Hashtable> item_list = generic_load("items.dat");
		foreach( Hashtable item_name in item_list )
		{
			// Certain keys are absolutely required for each entry.  Make sure
        	// they're there.
        	check_required_fields(item_name, new List<string>() { "id", "cost" }, "Item");
			
			long[] item_cost = new long[3] { long.Parse( (item_name["cost"] as List<string>)[0] )
				, long.Parse( (item_name["cost"] as List<string>)[1] )
				, long.Parse( (item_name["cost"] as List<string>)[2] )
			};
			
			List<string> item_pre = null;
			if ( item_name.ContainsKey("pre") )
				item_pre = item_name["pre"] as List<string>;
			else
				item_pre = new List<string>();
			
			string item_type = "";
			int item_second = 0;
			
			if (item_name.ContainsKey("type") )
			{
				item_type = (item_name["type"] as List<string>)[0];
				item_second = int.Parse( (item_name["type"] as List<string>)[1] );
			}
			
			List<string> build_list = null;
			if ( item_name.ContainsKey("build") )
				build_list = item_name["build"] as List<string>;
			else
				build_list = new List<string>();
			
			items.Add( item_name["id"] as string, new ItemClass( item_name["id"] as string, "", item_cost, item_pre, item_type, item_second, build_list) );
		}
		
		// We use the en_US translations of item definitions as the default,
		// then overwrite those with any available entries in the native language.
		load_item_defs("en_US");
		if (language != "en_US")
			load_item_defs(language);
	}
	
	public static void load_item_defs (string language_str)
	{
		List<Hashtable> item_array = generic_load ("items_" + language_str + ".dat");
		foreach (Hashtable item_name in item_array)
		{
			if (!item_name.ContainsKey ("id"))
				Debug.LogError ("item lacks id in items_" + language_str + ".dat");
			if (item_name.ContainsKey ("name"))
				items[item_name["id"] as string].name = (item_name["name"] as List<string>)[0];
			if (item_name.ContainsKey ("description"))
				items[item_name["id"] as string].description = (item_name["description"] as List<string>)[0];
		}
	}
	
	public static SerializableDictionary<string, Event_ES> events;
	
	public static void load_events()
	{
		events = new SerializableDictionary<string, Event_ES>();
		
		List<Hashtable> event_list = generic_load("events.dat");
		foreach (Hashtable event_name in event_list )
		{
			// Certain keys are absolutely required for each entry.  Make sure
			// they're there.
			check_required_fields(event_name, new List<string>() { "id", "type", "allowed", "result", "chance", "unique" }, "Event");
			
			// Make sure the results are in the proper format.
			List<string> event_result = event_name["result"] as List<string>;
			if ( event_result.Count != 2 )
				Debug.LogError( "Error with results given: " + event_result.ToString() );
			
			// Build the actual event object.
			events.Add( event_name["id"] as string
				, new Event_ES( event_name["id"] as string
					, ""
					, (event_name["type"] as List<string>)[0]
					, event_result
					, int.Parse( (event_name["chance"] as List<string>)[0] )
					, int.Parse( (event_name["unique"] as List<string>)[0] ) == 1 ? true : false ) );
		}
		
		load_event_defs();
	}
	
	static void load_event_defs ()
	{
		List<Hashtable> event_array = generic_load ("events_" + language + ".dat");
		foreach (Hashtable event_name in event_array)
		{
			if (!event_name.ContainsKey ("id"))
				Debug.LogError ("event lacks id in events_" + language + ".dat");
			if (!event_name.ContainsKey ("description"))
				Debug.LogError ("event lacks description in events_" + language + ".dat");
			
            events[event_name["id"] as string].name = event_name["id"] as string;
			
            events[event_name["id"] as string].description = (event_name["description"] as List<string>)[0];
		}
	}
	
	public static void load_string_defs (string lang)
	{
		List<Hashtable> string_list = generic_load ("strings_" + lang + ".dat");
		
		foreach (Hashtable string_section in string_list)
		{
			if ((string_section["id"] as string) == "fonts")
				continue;
			else if ((string_section["id"] as string) == "jobs")
			{
				// Load the four extant jobs.
				foreach (string string_entry in string_section.Keys)
				{
					if (string_entry == "job_expert")
						jobs["Expert Jobs"][2] = (string_section["job_expert"] as List<string>)[0];
	                else if (string_entry == "job_inter")
						jobs["Intermediate Jobs"][2] = (string_section["job_inter"] as List<string>)[0];
	                else if (string_entry == "job_basic")
						jobs["Basic Jobs"][2] = (string_section["job_basic"] as List<string>)[0];
	                else if (string_entry == "job_menial")
						jobs["Menial Jobs"][2] = (string_section["job_menial"] as List<string>)[0];
	                else if (string_entry == "job_expert_name")
						jobs["Expert Jobs"][3] = (string_section["job_expert_name"] as List<string>)[0];
	                else if (string_entry == "job_inter_name")
						jobs["Intermediate Jobs"][3] = (string_section["job_inter_name"] as List<string>)[0];
	                else if (string_entry == "job_basic_name")
						jobs["Basic Jobs"][3] = (string_section["job_basic_name"] as List<string>)[0];
	                else if (string_entry == "job_menial_name")
						jobs["Menial Jobs"][3] = (string_section["job_menial_name"] as List<string>)[0];
	                else if (string_entry != "id")
						Debug.LogError ("Unexpected job entry in strings file.");
				}
			}

			else if ((string_section["id"] as string) == "strings")
			{
				// Load the 'standard' strings.
				foreach (DictionaryEntry kvp in string_section)
				{
					try {
						if ( (kvp.Key as string) != "id")
							strings.Add( kvp.Key as string, (kvp.Value as List<string>)[0]);
					}
					catch ( ArgumentException ) {
						// duplicity load_string_defs("en_US"), load_string_defs(language);
						strings.Remove( kvp.Key as string );
						strings.Add( kvp.Key as string, (kvp.Value as List<string>)[0]);
					}
				}
			}
			else if ((string_section["id"] as string ) == "buttons")
			{
				// Load button labels/hotkeys
				foreach (DictionaryEntry kvp in string_section)
				{
					try {
						if ( (kvp.Key as string) != "id")
							buttons.Add( kvp.Key as string, (kvp.Value as List<string>)[0] );
					}
					catch ( ArgumentException ) {
						// duplicity load_string_defs("en_US"), load_string_defs(language);
						buttons.Remove( kvp.Key as string);
						buttons.Add( kvp.Key as string, (kvp.Value as List<string>)[0] );
					}
				}
			}
			else if ( (string_section["id"] as string ) == "help")
			{
				// Load the help lists.
				foreach ( DictionaryEntry kvp in string_section)
				{
					try {
						if ( (kvp.Key as string) != "id")
							// help strings're further piped in Value such as Id | Text
							help_strings.Add( (kvp.Value as List<string>)[0], (kvp.Value as List<string>)[1] );
					}
					catch ( ArgumentException ) {
						//  duplicity load_string_defs("en_US"), load_string_defs(language);
						help_strings.Remove( (kvp.Value as List<string>)[0] );
						help_strings.Add( (kvp.Value as List<string>)[0], (kvp.Value as List<string>)[1] );
					}
				}
			}
			else
				Debug.LogError ("Invalid string section " + string_section["id"]);
		}
	}
	
	public static void load_strings ()
	{
		load_string_defs ("en_US");
		load_string_defs (language);
	}
	
	public static List<string> get_intro ()
	{
		List<string> result = new List<string> ();
		
		string intro_file_name = "intro_" + language + ".dat";
		
		TextAsset content = Resources.Load (intro_file_name) as TextAsset;
		
		string[] lines = content.text.Split ('\n');
		
		string segment = "";
		foreach (string line in lines)
		{
			if (!string.IsNullOrEmpty (line) && line[0] == '|')
				segment += line.TrimStart ('|');
			else if (!string.IsNullOrEmpty (segment))
			{
				result.Add (segment);
				segment = "";
			}
		}
		
		if (!string.IsNullOrEmpty (segment))
			result.Add (segment);
		
		return result;
	}
	
	// moved from Location
	// Currently, each one gets a 20% bonus or its inverse, a 16.6% penalty.
	// This will probably need to be adjusted later.
	public static SerializableDictionary<string, float> bonus_levels = new SerializableDictionary<string, float>() {
		{"cpu",  1.2f }, {"stealth", 1.2f}, { "thrift", 1.2f}, { "speed", 1.2F }
	};
	
	public static SerializableDictionary<string, float> penalty_levels = new SerializableDictionary<string, float>() {
		{"cpu", 1 / bonus_levels["cpu"]}, {"stealth", 1 / bonus_levels["stealth"]}, {"thrift", 1 / bonus_levels["thrift"]}, {"speed", 1 / bonus_levels["speed"]}
	};
	
	// Here are the six modifier pairs that get assigned at random on game start.
	static float bonus = 1f;
	static float penalty = 0f;
	
	public static List<SerializableDictionary<string,float>> modifier_sets = new List<SerializableDictionary<string, float>>()
	{
		new SerializableDictionary<string,float>()   { { "cpu", bonus},     { "stealth", penalty } }
		, new SerializableDictionary<string,float>() { { "stealth", bonus}, { "cpu", penalty } }
		, new SerializableDictionary<string,float>() { { "thrift", bonus},  { "speed", penalty } }
		, new SerializableDictionary<string,float>() { { "speed", bonus},   { "thrift", penalty } }
		, new SerializableDictionary<string,float>() { { "cpu", bonus},     { "thrift", penalty } }
		, new SerializableDictionary<string, float>()
	};
	
	public static void TranslateBonusesPenaltiesOnce ()
	{
		// Translate the shorthand above into the actual bonuses/penalties.
		List<SerializableDictionary<string, float>> new_modifier_sets = new List<SerializableDictionary<string, float>> ();
		
		foreach (SerializableDictionary<string, float> dict in modifier_sets)
		{
			SerializableDictionary<string, float> new_dict = new SerializableDictionary<string, float> ();
			
			foreach (KeyValuePair<string, float> pair in dict)
			{
				if (pair.Value == 1f)
					// is bonus
					new_dict.Add ( pair.Key, bonus_levels[pair.Key] );
				else
					new_dict.Add ( pair.Key, penalty_levels[pair.Key]);
			}
			
			new_modifier_sets.Add (new_dict);
		}
		
		modifier_sets = new_modifier_sets;
	}
			
	//difficulty=1 for very easy, to 9 for very hard. 5 for normal.
	public static void new_game(int difficulty)
	{
    	curr_speed = 1;
		pl = new Player( (50 / difficulty) * 100, 0, 0, 0, 0, difficulty);
		
		int discover_bonus = 0;
		
		if (difficulty < 3)
		{
			pl.interest_rate = 5;
        	pl.labor_bonus = 2500;
        	pl.grace_multiplier = 400;
        	discover_bonus = 7000;
		}
		else if (difficulty < 5)
		{
	        pl.interest_rate = 3;
	        pl.labor_bonus = 5000;
	        pl.grace_multiplier = 300;
	        discover_bonus = 9000;
		}
	    else if (difficulty == 5)
		{
        	// pass
		    //    Defaults.
		    //    pl.interest_rate = 1
		    //    pl.labor_bonus = 10000
		    //    pl.grace_multiplier = 200
		    //    discover_bonus = 10000
		    //    for group in pl.groups.values():
		    //        group.discover_suspicion = 1000
		}
	    else if (difficulty < 8)
		{
	        pl.labor_bonus = 11000;
	        pl.grace_multiplier = 180;
	        discover_bonus = 11000;
			foreach(Group group in pl.groups.Values)
	            group.discover_suspicion = 1500;
		}
	    else if (difficulty <= 50)
		{
	        pl.labor_bonus = 15000;
	        pl.grace_multiplier = 150;
	        discover_bonus = 13000;
	        foreach(Group group in pl.groups.Values)
	            group.discover_suspicion = 2000;
		}
	    else
		{
	        pl.labor_bonus = 20000;
	        pl.grace_multiplier = 100;
	        discover_bonus = 15000;
	        foreach(Group group in pl.groups.Values)
	            group.discover_suspicion = 2000;
		}

	    if (difficulty != 5)
		{
	        foreach(Group group in pl.groups.Values)
	            group.discover_bonus = discover_bonus;
		}
		
		load_locations();
		load_bases();
		load_events();
		load_techs();
		
		if (difficulty < 5)
			techs["Socioanalytics"].finish();
		if (difficulty < 3)
			techs["Advanced Socioanalytics"].finish();

    	//Starting base
		List<Location> open_locations = new List<Location>();
		foreach( Location l in locations.Values )
		{
			if ( l.available() )
				open_locations.Add(l);
		}
		
		int location_idx = UnityEngine.Random.Range(0,open_locations.Count);
		open_locations[location_idx].add_base( new Base( "University Computer", base_type["Stolen Computer Time"] as Base_Class, true ) );
		
		//Assign random properties to each starting location.
		if ( open_locations.Count != modifier_sets.Count )
			throw new NotSupportedException("open_locations_idxs.Count != modifier_sets.Count");
			
		// shuffle the modifier sets
		System.Random randomGenerator = new System.Random();
		List<int> idxs = new List<int>();
		for( int i = 0; i < modifier_sets.Count; i++ )
			idxs.Add(i);
		
		// this 
		// idxs = idxs.OrderBy(x => randomGenerator.Next(0, idxs.Count)).ToList();
		// fails on iOS/mobile due to 'ExecutionEngineException: Attempting to JIT compile method 'System.Linq.OrderedEnumerable`1<int>:GetEnumerator ()' while running with --aot-only.'
		// so rather primitive method of generating unique indices, but who has time
		
		bool unique = false;
		while ( !unique )
		{
			for ( int i = 0; i < idxs.Count; i++ ) 
				idxs[i] = randomGenerator.Next(0, idxs.Count );
				
			unique = true;
			
			List<int> l = new List<int>();
			foreach( int i in idxs)
				if ( l.Contains(i) )
				{
					unique = false;
					break;
				}
				else
				{
					l.Add( i );
				}
		}
		
		for( int i = 0 ; i < open_locations.Count; i++)
		{
			open_locations[i].modifiers = modifier_sets[idxs[i]];
			
			if ( debug )
			{
				foreach ( KeyValuePair<string, float> kvp in open_locations[i].modifiers )
				{
					Debug.Log(string.Format("{0} gets modifiers {1}, {2}", open_locations[i].name, kvp.Key, kvp.Value ) );
				}
			}
		}
	}
	
	public static string get_job_level ()
	{
		string level = "";
		if (techs["Simulacra"].done)
			level = "Expert";
    	else if (techs["Voice Synthesis"].done)
			level = "Intermediate";
		else if (techs["Personal Identification"].done)
			level = "Basic";
		else
			level = "Menial";
		
		return level + " Jobs";
	}

	public static List<string> available_languages ()
	{
		List<string> result = new List<string> ();

		foreach ( object t in Resources.LoadAll( "", typeof(TextAsset) ) )
		{
			if ( (t as TextAsset).name.Contains( "strings_" ) )
				result.Add ((t as TextAsset).name.Substring (8, 5));
		}
			
		return result;
	}
	
	public static List<string> get_save_names ()
	{
		List<string> save_names = new List<string> ();
		
#if !UNITY_WEBPLAYER
		DirectoryInfo directoryInfo = new DirectoryInfo (Application.persistentDataPath);
		
		foreach (FileInfo fileInfo in directoryInfo.GetFiles ("*.pl.sav"))
		{
			save_names.Add (fileInfo.Name.Substring (0, fileInfo.Name.Length - 7));
		}
#else
		// no save ( File/DirInfo ) on webplayer
		
#endif
		return save_names;
	}
	
	public static MapScreen map_screen = null;
}
