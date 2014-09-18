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

//This file contains the event class.

using System.Collections.Generic;
using UnityEngine;

//detection = (news, science, covert, person)

// [the-plague]
// type = global
// allowed = all
// result_list = discover_public | 2000
// chance = 20
// unique = 1

public class Event_ES
{
	public string name;
	public string event_id;
	public string description;
	public string event_type;
	public List<string> result;
	public float chance;
	public bool unique;
	public bool triggered;
	
	public Event_ES()
	{
	}
			
	public Event_ES (string _name, string _description, string _event_type, List<string> _result, int _chance, bool _unique)
	{
		this.name = _name;
		this.event_id = _name;
		this.description = _description;
		this.event_type = _event_type;
		this.result = _result;
		this.chance = _chance;
		this.unique = _unique;
		this.triggered = false;
	}
	
	public void trigger ()
	{
		G.map_screen.show_message (this.description, Color.white);

        // If this is a unique event, mark it as triggered.
		if (this.unique)
			this.triggered = true;

        // TODO: refactor - Merge this code with its duplicate in tech.py. - original remark
		string what = this.result[0].Split ('_')[0];
		string who = this.result[0].Split ('_')[1];
		
		if (G.pl.groups.ContainsKey (who))
		{
			if (what == "suspicion")
				G.pl.groups[who].alter_suspicion_decay (int.Parse (this.result[1]));
			else if (what == "discover")
				G.pl.groups[who].alter_discover_bonus (-int.Parse (this.result[1]));
			else
				Debug.LogError (string.Format ("Unknown bonus '{0}' in event {1}.", what, this.name));
		}
		else if ( who == "onetime" && what == "suspicion")
		{
			foreach (  Group group in G.pl.groups.Values )
                group.alter_suspicion( - int.Parse(this.result[1]));
		}
		else
			Debug.LogError(string.Format("Unknown group/bonus '{0}' in event {1}. ", this.result[0], this.name ));
	}
}
