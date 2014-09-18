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

//This file contains the Location class.

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

// Location is a subclass of BuyableClass so that it can use .available():
public class Location:BuyableClass, IComparable
{
    // The cities at this location.
	public List<string> cities = new List<string>();

    // The bonuses and penalties of this location.
    public SerializableDictionary<string, float> modifiers = new SerializableDictionary<string, float>();
	
	public float x, y; 			// screen position - can vary between devices ( such as iphone / ipad ) after save / load
	public float x_def, y_def; 	// config
	public bool absolute;
	public int safety;
	
	public List<Base> bases = new List<Base>();
	
	public Location()
	{
	}
					
	public Location( string _id, Vector2 _position, bool _absolute, int _safety, List<string> _prerequsities)
		: base ( _id, "", new long[3] { 0, 0, 0, }, _prerequsities, "" )
	{
        this.absolute = _absolute;
        
        this.x_def = _position.x;
        this.y_def = _position.y;
        
        this.update_position();
		
        this.safety = _safety;

        // A list of the bases at this location.  Often sorted for the GUI.
        this.bases = new List<Base>();
	}
	
	public void update_position()
	{
		if ( this.absolute )
		{
			// cocords relative to whole screen
			this.x = (this.x_def / 100f ) * Screen.width;
			this.y = (this.y_def / 110f ) * Screen.height;
		}
		else
		{
			// cocords relative to map region
			Rect region = new Rect( 0, Screen.height / 8, Screen.width, Screen.height - ( Screen.height / 8 ));
			
			this.x = (this.x_def / 100f ) * region.width;
			this.y = (this.y_def / 118f ) * region.height + ( Screen.height / 16 );
		}
		
		// adjust coordinates for mobile screen
		
		if ( this.id == "FAR REACHES")
			this.x = Screen.width / 2;
			
		if ( this.id == "TRANSDIMENSIONAL")
			this.y = (this.y_def / 103f ) * Screen.height;
	}
	
	public bool had_last_discovery {
		get {
			return G.pl.last_discovery != null && G.pl.last_discovery.id == this.id;
		}
	}

	public bool had_prev_discovery {
		get {
			return G.pl.prev_discovery != null && G.pl.prev_discovery.id == this.id;
		}
	}

    public int discovery_bonus()
	{
		float discovery_bonus = 1f;
        if (this.had_last_discovery)
            discovery_bonus *= 1.2f;
        if (this.had_prev_discovery)
            discovery_bonus *= 1.1f;
		if (this.modifiers.ContainsKey("stealth"))
            discovery_bonus /= this.modifiers["stealth"];
        return (int)(discovery_bonus * 100);
	}

    public void modify_cost (long[] _cost)
    {
    	float mod = 0f;
    	if (this.modifiers.ContainsKey ("thrift"))
		{
    		mod = this.modifiers["thrift"];

            // Invert it and apply to the CPU/cash cost.
    		_cost[BuyableClass.cash] = (long)(_cost[BuyableClass.cash] / mod);
    		_cost[BuyableClass.cpu] = (long)(_cost[BuyableClass.cpu] / mod);
    	}

        if (this.modifiers.ContainsKey ("speed"))
		{
    		mod = this.modifiers["speed"];

            // Invert it and apply to the labor cost.
    		_cost[BuyableClass.labor] = (long)(_cost[BuyableClass.labor] / mod);
    	}
	}

    public void modify_maintenance (long[] _maintenance)
    {
    	if (this.modifiers.ContainsKey ("thrift"))
		{
    		float mod = this.modifiers["thrift"];

            // Invert it and apply to the cash maintenance.  CPU is not changed.
    		_maintenance[BuyableClass.cash] = (long)(_maintenance[BuyableClass.cash] / mod);
    	}
	}

    public void add_base (Base abase)
    {
		abase.location_id = this.id;
    	this.bases.Add (abase);
		
        this.modify_cost (abase.total_cost);
    	this.modify_cost (abase.cost_left);
    	this.modify_maintenance (abase.maintenance);

        // Make sure the location's CPU modifier is applied.
    	abase.recalc_cpu ();
	}
	
	public override int CompareTo (object other)
	{
		return this.id.CompareTo((other as Location).id);
	}
}
