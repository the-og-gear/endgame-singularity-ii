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

//This file contains the base class.

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.Linq;

public class Base_Class:BuyableClass
{
	public int size;
	public string force_cpu;
	public List<string> regions;
	public SerializableDictionary<string, int> detect_chance;
    public long[] maintenance;
	public List<string> flavor;
	
	public Base_Class()
	{
	}
	
	public Base_Class (string _name, string _description, int _size, string _force_cpu, List<string> _regions, SerializableDictionary<string, int> _detect_chance, long[] _cost, List<string> _prerequisities, long[] _maintenance)
		: base(_name, _description, _cost, _prerequisities, "base")
	{
		this.size = _size;
		this.force_cpu = _force_cpu;
		this.regions = _regions;
		if (this.regions.Contains ("pop"))
		{
			this.regions = new List<string> ();
			this.regions.Add ("N AMERICA");
			this.regions.Add ("S AMERICA");
			this.regions.Add ("EUROPE");
			this.regions.Add ("ASIA");
			this.regions.Add ("AFRICA");
			this.regions.Add ("AUSTRALIA");
		}

        this.detect_chance = _detect_chance;
		this.maintenance = _maintenance;
		this.flavor = new List<string> ();
	}

    public SerializableDictionary<string, int> calc_discovery_chance (bool _accurate, float _extra_factor)
    {
    	// Get the default settings for this base type.
    	SerializableDictionary<string, int> detect_chance_copy = new SerializableDictionary<string, int> (this.detect_chance);
  
        // Adjust by the current suspicion levels ...
        foreach(string group in this.detect_chance.Keys)
		{
			int suspicion = G.pl.groups[group].suspicion;
			detect_chance_copy[group] *= 10000 + suspicion;
			detect_chance_copy[group] /= 10000;
		}

		// ... and further adjust based on technology ...
        foreach (string	group in this.detect_chance.Keys)
		{
			int discover_bonus = G.pl.groups[group].discover_bonus;
            detect_chance_copy[group] *= discover_bonus;
            detect_chance_copy[group] /= 10000;
		}

        // ... and the given factor.
        foreach(string group in this.detect_chance.Keys)
            detect_chance_copy[group] = (int)(detect_chance_copy[group] * _extra_factor);

        // Lastly, if we're told to be inaccurate, adjust the values to their
        // nearest percent.
        if (!_accurate)
            foreach(string group in this.detect_chance.Keys	)
                detect_chance_copy[group] = G.nearest_percent(detect_chance_copy[group]);

        return detect_chance_copy;
	}

    public string get_detect_info(Location _location)
	{
        if (!G.techs["Socioanalytics"].done)
            return G.strings["detect_chance_unknown_base"];

        bool accurate = G.techs["Advanced Socioanalytics"].done;
        float detect_modifier = 1 / ( _location.modifiers.ContainsKey("stealth") ? _location.modifiers["stealth"] : 1 );
        SerializableDictionary<string, int> chance = this.calc_discovery_chance(accurate, detect_modifier);
				
        return string.Format("Detection chance: NEWS:{0} SCIENCE:{1} COVERT:{2} PUBLIC:{3}",
			G.to_percent( chance.ContainsKey("news")  ? chance["news"] : 0, false),
			G.to_percent( chance.ContainsKey("science") ? chance["science"] : 0, false),
			G.to_percent( chance.ContainsKey("covert") ? chance["covert"] : 0, false),
			G.to_percent( chance.ContainsKey("public") ? chance["public"] : 0, false));
	}

    public string get_info (Location location)
    {
    	long[] raw_cost = this.cost;
    	location.modify_cost (raw_cost);
    	string cost = this.describe_cost (raw_cost, false);
  
    	long[] raw_maintenance = new long[3];
    	this.maintenance.CopyTo(raw_maintenance,0);
    	location.modify_maintenance (raw_maintenance);
    	string maint = this.describe_cost (raw_maintenance, true);

        string detect = this.get_detect_info (location);

        string size = "";
    	if (this.size > 1)
    		size = string.Format ("\nHas space for {0} computers.", this.size);

        string location_message = "";
    	if (location.modifiers.ContainsKey ("cpu"))
		{
    		string modifier = "";
    		if (location.modifiers["cpu"] > 1)
    			modifier = G.strings["cpu_bonus"];
    		else
    			modifier = G.strings["cpu_penalty"];

    		location_message = string.Format ("\n\n" + G.strings["location_modifiers"], modifier);
    	}

        return string.Format ("{0}\nBuild cost:{1}\nMaintenance:{2}\n{3} {4}\n---\n{5}{6}", this.name, cost, maint, detect, size, this.description, location_message);
	}
}

public class Base:Buyable, IComparable
{
	public long started_at;
	public string location_id;
	public SerializableDictionary<string, int> suspicion;
	public long raw_cpu;
	public long cpu;
	
	public List<Item> extra_items;
	public Item cpus;
	
	public string power_state;
	public bool grace_over;
	
	public long[] maintenance;
	
	public Base_Class type;
	
	public Base()
	{
	}
	
	public Base (string _name, Base_Class _type, bool _built)
		: base(_type, 1)
	{
		this.type = _type;
		
		this.name = _name;
		this.started_at = G.pl.raw_min;
		
        this.location_id = string.Empty;

        //BaseES suspicion is currently unused
		this.suspicion = new SerializableDictionary<string, int> ();

        this.raw_cpu = 0;
		this.cpu = 0;

        //Reactor, network, security.
		this.extra_items = new List<Item> () { null, null, null } ; // [None] * 3

        this.cpus = null;
		
        if (!string.IsNullOrEmpty ((this.type as Base_Class).force_cpu))
		{
			// 1% chance for a Stolen Computer Time base to have a Gaming PC
			// instead.  If the base is pre-built, ignore this.
			if (this.type.id == "Stolen Computer Time" && G.roll_percent (100) && !_built)
				this.cpus = new Item (G.items["Gaming PC"], this, (this.type as Base_Class).size, true);
			else
				this.cpus = new Item (G.items[(this.type as Base_Class).force_cpu], this, (this.type as Base_Class).size, true);
			
			// this.cpus.finish (); ..(we cannot haz Base as a member in Item because of circular ref in XMLSerializer, we haz this.id instead and iterate over current bases, but this one is not in G.all_bases() since its being created now..)]
			// so instead we have _built = true for Item ctor above
		}

        if (_built)
			this.finish ();

        this.power_state = "active";
		this.grace_over = false;
		
		this.maintenance = new long[(this.type as Base_Class).maintenance.Length];

   		(this.type as Base_Class).maintenance.CopyTo(this.maintenance,0);
	}

    public void check_power ()
    {
    	if (this.power_state == "sleep")
		{
    		if (this.done)
			{
    			List<Item> items = new List<Item> (this.extra_items);
    			items.Add (this.cpus);
				
    			foreach (Item item in items)
				{
    				if (item != null && !item.done)
    					this.power_state = "active";
    			}
    		}
            else
			{
    			this.power_state = "active";
    		}
    	}
	}

    public void recalc_cpu ()
    {
    	if (this.raw_cpu == 0)
		{
    		this.cpu = 0;
    		return;
    	}

        int compute_bonus = 10000;
    	// Network bonus
    	if (this.extra_items[1] != null && this.extra_items[1].done)
    		compute_bonus += this.extra_items[1].item_qual;

        // Location modifier
        Location location;
        if ( G.locations.TryGetValue( this.location_id, out location ) )
        {
	    	if (location.modifiers.ContainsKey ("cpu"))
	    		compute_bonus = (int)((float)compute_bonus * location.modifiers["cpu"]);
    	}
		
        this.cpu = (long)Mathf.Max (1f, (this.raw_cpu * compute_bonus) / 10000f );
	}
	
    // Get the detection chance for the base, applying bonuses as needed.  If
    // accurate is False, we just return the value to the nearest full
    // percent.
	public SerializableDictionary<string, int> get_detect_chance(bool _accurate) // = true
	{
        // Get the base chance from the universal function.
        SerializableDictionary<string, int> _detect_chance = calc_base_discovery_chance(this.type.id, _accurate, 1f);

        foreach (string group in G.pl.groups.Keys)
			if ( !_detect_chance.ContainsKey(group) )
				_detect_chance.Add(group, 0);

        // Factor in the suspicion adjustments for this particular base ...
        foreach( KeyValuePair<string,int> kvp in this.suspicion )
		{
            _detect_chance[kvp.Key] *= 10000 + kvp.Value;
            _detect_chance[kvp.Key] /= 10000;
		}

        // ... any reactors built ...
        if (this.extra_items[0] != null && this.extra_items[0].done )
		{
            int item_qual = this.extra_items[0].item_qual;
            foreach(string group in _detect_chance.Keys.ToList() )
			{
                _detect_chance[group] *= 10000 - item_qual;
                _detect_chance[group] /= 10000;
			}
		}

        // ... and any security systems built ...
        if (this.extra_items[2] != null && this.extra_items[2].done)
		{
            int item_qual = this.extra_items[2].item_qual;
            foreach(string group in _detect_chance.Keys.ToList() )
			{
                _detect_chance[group] *= 10000 - item_qual;
                _detect_chance[group] /= 10000;
			}
		}
		
		// ... and its location ...
        Location location;
        if ( G.locations.TryGetValue( this.location_id, out location ) )
        {
            int multiplier = location.discovery_bonus();
            foreach(string group in _detect_chance.Keys.ToList() )
			{
                _detect_chance[group] *= multiplier;
                _detect_chance[group] /= 100;
			}
		}

        // ... and its power state.
        if (this.done && this.power_state == "sleep")
            foreach(string group in _detect_chance.Keys.ToList() )
                _detect_chance[group] /= 2;

        // Lastly, if we're not returning the accurate values, adjust
        // to the nearest percent.
        if (!_accurate)
            foreach(KeyValuePair<string, int> group in _detect_chance.ToList() )
                _detect_chance[group.Key] = G.nearest_percent(group.Value);

        return _detect_chance;
	}

    bool is_building ()
    {
    	List<Item> items = new List<Item> (this.extra_items);
    	items.Add(this.cpus);
    	foreach (Item item in items)
    		if (item != null && !item.done)
    			return true;
    	return false;
	}

    // Can the base study the given tech?
    bool allow_study (string _tech_name)
    {
    	if (!this.done)
    		return false;
        else if (G.jobs.ContainsKey (_tech_name) || _tech_name == "CPU Pool" || _tech_name == "")
    		return true;
        else if (_tech_name == "Sleep")
    		return !this.is_building ();
    	else
		{
	        Location location;
	        if ( G.locations.TryGetValue( this.location_id, out location ) )
    			return location.safety >= G.techs[_tech_name].danger;

            // Should only happen for the fake base.
    		foreach (string region in (this.type as Base_Class).regions)
    			if (G.locations[region].safety >= G.techs[_tech_name].danger)
    				return true;
    		return false;
    	}
	}

    public bool has_grace()
	{
        if (this.grace_over)
            return false;

        long age = G.pl.raw_min - this.started_at;
        float grace_time = (this.total_cost[BuyableClass.labor] * G.pl.grace_multiplier) / 100;
        if (age > grace_time)
		{
            this.grace_over = true;
            return false;
		}
        else
            return true;
	}

    public bool is_complex ()
    {
    	return (this.type as Base_Class).size > 1 || this.raw_cpu > 20;
	}

    public override void destroy ()
    {
    	base.destroy();
    	
    	Location location;
	    if ( G.locations.TryGetValue( this.location_id, out location ) )
    		location.bases.Remove (this);
  
        if (this.cpus != null)
    		this.cpus.destroy();

        foreach(Item item in this.extra_items)
            if (item != null )
                item.destroy();
	}

    public Base next_base (bool _forwards)
    {
    	Location location = G.locations[this.location_id];
    
    	int index = 0;
    	for (int i = 0; i < location.bases.Count; i++)
		{
    		if (location.bases[i] == this)
			{
    			index = i;
    			break;
    		}
    	}
  
		int increment = 0;

    	if (_forwards)
    		increment = 1;
    	else
    		increment = -1;

        while (true)
		{
    		index += increment;
    		index %= location.bases.Count;
    		Base abase = location.bases[index];
    		if (abase.done)
    			return abase;
    	}
	}

    // We sort based on size (descending), CPU (descending),
    // then name (ascending).
    // id(self) is thrown in at the end to make sure identical-looking
    // bases aren't considered equal.
	
	public int CompareTo (object other)
	{
		if ((this.type as Base_Class).size > ((other as Base).type as Base_Class).size)
			return -1;
		else if ((this.type as Base_Class).size < ((other as Base).type as Base_Class).size )
			return 1;
		else
		{
			if (this.cpu > (other as Base).cpu)
				return -1;
			else if (this.cpu < (other as Base).cpu)
				return 1;
			else
			{
				int cmp = string.Compare (this.name, (other as Base).name);
				if ( cmp != 0)
					return cmp;
					
				// just for sure, probably not needed
				return this.GetHashCode().CompareTo(other.GetHashCode());
			}
		}
	}
	
	// calc_base_discovery_chance is a globally-accessible function that can
	// calculate basic discovery chances given a particular class of base.  If
	// told to be inaccurate, it rounds the value to the nearest percent.
	public static SerializableDictionary<string, int> calc_base_discovery_chance (string _base_type_name, bool _accurate, float _extra_factor)
	{
		return G.base_type[_base_type_name].calc_discovery_chance (_accurate, _extra_factor);
	}
}