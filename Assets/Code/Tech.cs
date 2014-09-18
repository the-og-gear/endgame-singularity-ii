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

//This file contains the tech class.

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Tech : Buyable, IComparable
{
	public int danger;
	public string result;
	public string tech_type;
	
	public int secondary_data;
	
	public BuyableClass type;
	
	public Tech()
	{
	}
	
	public Tech ( BuyableClass _type, bool _known, int _danger, string _tech_type, int _secondary_data )
	// public Tech (string _id, string _description, bool _known, long[] _cost, List<string> _prerequisites, int _danger, string _tech_type, int _secondary_data)
	//	: base( new BuyableClass (_id, _description, _cost, _prerequisites, "tech"), 1)
		:base( _type, 1 )
	{
		this.type = _type;
		
		this.danger = _danger;
		this.result = "";
		this.tech_type = _tech_type;
		this.secondary_data = _secondary_data;
		
        if (_known)
        {
			// self.finish would re-apply the tech benefit, which is already in
			// place.
			base.finish ();
		}
	}
	
	public bool available {
		get { return this.type.available (); }
	}
	
	public int CompareTo (object other)
	{
		return this.type.CompareTo((other as Tech).type );
	}
	
	public string get_info ()
	{
		string cost = this.type.describe_cost (this.total_cost, true);
		string left = this.type.describe_cost (this.cost_left, true);
		return string.Format ("{0}\nTotal cost: {1}\nCost left: {2}\n---\n{3}", this.name, cost, left, this.description);
	}

    public override void finish ()
    {
    	base.finish ();
    	
    	this.gain_tech ();
	}

    void gain_tech()
	{
        //give the effect of the tech
        if (this.tech_type == "interest")
            G.pl.interest_rate += this.secondary_data;
        else if (this.tech_type == "income")
            G.pl.income += this.secondary_data;
        else if (this.tech_type == "cost_labor_bonus")
            G.pl.labor_bonus -= this.secondary_data;
        else if (this.tech_type == "job_expert")
            G.pl.job_bonus += this.secondary_data;
        else if (this.tech_type == "endgame_sing")
		{
            G.play_music("win");
            G.map_screen.show_message(G.strings["wingame"], Color.white);
			foreach ( Group group in G.pl.groups.Values )
                group.discover_bonus = 0;
            G.pl.apotheosis = true;
            G.pl.had_grace = true;
		}
        else if (!string.IsNullOrEmpty(this.tech_type))
		{
			string what = this.tech_type.Split('_')[0];
			string who = this.tech_type.Split('_')[1];
			
			if ( G.pl.groups.ContainsKey( who ))
			{
                if (what == "suspicion")
                    G.pl.groups[who].alter_suspicion_decay(this.secondary_data);
                else if (what == "discover")
                    G.pl.groups[who].alter_discover_bonus(-this.secondary_data);
                else
                    Debug.LogError( string.Format("Unknown action '{0}' in tech {1}.", what, this.name));
			}
            else if (who == "onetime" && what == "suspicion")
                foreach(Group group in G.pl.groups.Values)
                    group.alter_suspicion(-this.secondary_data);
            else
				Debug.LogError(string.Format("tech: {0} is unknown bonus can't be applied", this.tech_type));
		}
	}
}