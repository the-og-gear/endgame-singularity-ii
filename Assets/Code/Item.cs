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

//This file contains the item class.

using System.Collections.Generic;
using UnityEngine;

public class ItemClass: BuyableClass
{
	public string item_type;
	public int item_qual;
	public List<string> buildable;
	
	public ItemClass()
	{
	}
		
	public ItemClass (string _name, string _description, long[] _cost, List<string> _prerequisites, string _item_type, int _item_qual, List<string> _buildable)
		: base(_name, _description, _cost, _prerequisites, "item")
	{
		this.item_type = _item_type;
		this.item_qual = _item_qual;
		this.buildable = _buildable;

		if (this.buildable.Contains ("all"))
		{
			this.buildable = new List<string> ();
			this.buildable.Add ("N AMERICA");
			this.buildable.Add ("S AMERICA");
			this.buildable.Add ("EUROPE");
			this.buildable.Add ("ASIA");
			this.buildable.Add ("AFRICA");
			this.buildable.Add ("ANTARCTIC");
			this.buildable.Add ("OCEAN");
			this.buildable.Add ("MOON");
			this.buildable.Add ("FAR REACHES");
			this.buildable.Add ("TRANSDIMENSIONAL");
			this.buildable.Add ("AUSTRALIA");
		}
		if (this.buildable.Contains ("pop"))
		{
			this.buildable = new List<string> ();
			this.buildable.Add ("N AMERICA");
			this.buildable.Add ("S AMERICA");
			this.buildable.Add ("EUROPE");
			this.buildable.Add ("ASIA");
			this.buildable.Add ("AFRICA");
			this.buildable.Add ("AUSTRALIA");
		}
	}
	
	public override string get_info ()
	{
		string basic_text = base.get_info ();
		if (this.item_type == "cpu")
			return basic_text.Replace ("---", string.Format ("Generates {0} CPU.\n---", G.add_commas (this.item_qual)));
		return basic_text;
	}
}

public class Item: Buyable
{
	public int item_qual;
	
	public string _base_name;
	
	public ItemClass type;
	
	public Item()
	{
	}
	
	public Item (ItemClass _item_type, Base _abase, int _count, bool _built)
		: base(_item_type, _count)
	{
		this.item_qual = _item_type.item_qual;
		this._base_name = _abase.name;
		this.type = _item_type;
		
		if ( _built )
		{
			base.finish();
			
    		if ((this.type as ItemClass).item_type == "cpu")
                _abase.raw_cpu = this.item_qual * this.count;
            
            _abase.recalc_cpu();
		}
	}
	
    public override void finish ()
    {
    	base.finish ();
    	
		foreach ( Base b in G.all_bases() )
		{
			if ( b.name == this._base_name ) {

	    		if ((this.type as ItemClass).item_type == "cpu")
	                b.raw_cpu = this.item_qual * this.count;
	            
	            b.recalc_cpu();
	
				break;
			}
		}
	}
	
	public static Item operator +(Item item, Item other )
    {
    	if ( item._base_name == other._base_name && item.type == other.type)
		{
    		if (other.count == 0)
    			return item;
   
            // Calculate what's been paid and what is left to be paid.
            long[] this_cost_paid = item.get_cost_paid();
            long[] other_cost_paid = other.get_cost_paid();
    		long[] total_cost_paid = new long[3] {
    			this_cost_paid[0] + other_cost_paid[0]
    			, this_cost_paid[1] + other_cost_paid[1]
    			, this_cost_paid[2] + other_cost_paid[2]
    		};
    		
    		item.total_cost[0] += other.total_cost[0];
    		item.total_cost[1] += other.total_cost[1];
    		item.total_cost[2] += other.total_cost[2];
    		
            // Labor takes as long as the less complete item would need.
    		total_cost_paid[BuyableClass.labor] = (long)Mathf.Min (item.get_cost_paid()[BuyableClass.labor],
                                                 other.get_cost_paid()[BuyableClass.labor]);
    		item.total_cost[BuyableClass.labor] = other.total_cost[BuyableClass.labor];

            // Set what we've paid (and hence what we have left to pay).
    		item.set_cost_paid(total_cost_paid);

            // Increase the size of this stack.
    		item.count += other.count;

            // Tell the base it has no CPU for now.
	    	Base _base = null;
    		foreach ( Base b in G.all_bases() )
    		{
    			if ( b.name == item._base_name ) {
    				_base = b;
    				break;
    			}
    		}
    			
	    	if ( _base != null )
	    	{
	    		_base.raw_cpu = 0;
	    		_base.recalc_cpu ();
	    	}

            // See if we're done or not.
    		item.done = false;
    		item.work_on(0, 0, 0);

            return item;
    	}
        else
    		throw new System.NotImplementedException();
	}
	
}
