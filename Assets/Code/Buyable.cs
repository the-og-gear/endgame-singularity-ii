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

//This file contains the Buyable base class/es.

using System;
using System.Collections.Generic;

public class BuyableClass: IComparable
{
	public static int cash = 0, cpu = 1, labor = 2;

	public string id;

	public string name;
	public string description;
	
	// public for XMl serializer
	public long[] _cost;
	
	public List<string> prerequisites;
	
	// string prefix; // used in stats ( which are not used )
	
	public BuyableClass()
	{
	}
	
	public BuyableClass (string _id, string _description, long[] __cost, List<string> _prerequisites, string _type)
	{
		this.name = this.id = _id;
		this.description = _description;
		this._cost = __cost;
		this.prerequisites = _prerequisites;
		
//		if (!string.IsNullOrEmpty (_type))
//			this.prefix = _type + "_";
//		else
//			this.prefix = "";
	}
	
	public virtual long[] cost
	{
		get
		{
			long[] c = new long[3];
			this._cost.CopyTo (c, 0);
			
			c[BuyableClass.labor] *= G.minutes_per_day * G.pl.labor_bonus;
			c[BuyableClass.labor] /= 10000;
			c[BuyableClass.cpu] *= G.seconds_per_day;
			return c;
		}
	}

	public virtual string describe_cost(long[] _cost, bool _hide_time) // hide_time=False)
	{
		string cpu_cost = G.to_cpu(_cost[BuyableClass.cpu]);
		string cash_cost = G.to_money(_cost[BuyableClass.cash]);
		string labor_cost = "";
        if (!_hide_time)
            labor_cost = string.Format(", {0}", G.to_time(_cost[BuyableClass.labor]));
				
        return string.Format("{0} CPU, {1} money {2}", cpu_cost, cash_cost, labor_cost);
	}
	
	public virtual string get_info ()
	{
		string cost_str = this.describe_cost (this.cost, false);
		return string.Format ("{0}\nCost: {1}\n---\n{2}", this.name, cost_str, this.description);
	}
	
	// For sorting buyables, we sort by cost
	// The first element is price in cash, which is the one we care about the most.
	public virtual int CompareTo (object other)
	{
		long[] cost1 = this.cost;
		long[] cost2 = (other as BuyableClass).cost;
		if ( cost1[0] > cost2[0] )
			return 1;
		else if ( cost1[0] < cost2[0] )
			return -1;
		else
			return this.name.CompareTo((other as BuyableClass).name);
	}
	
	public virtual bool available ()
	{
		bool or_mode = false;
		
		foreach (string prerequisite in this.prerequisites)
		{
			if (prerequisite == "OR")
				or_mode = true;
			if (G.techs.ContainsKey (prerequisite) && G.techs[prerequisite].done)
			{
				if (or_mode)
					return true;
			}
            else
			{
				if (!or_mode)
					return false;
			}
		}
		// If we're not in OR mode, we met all our prerequisites.  If we are, we
		// didn't meet any of the OR prerequisites.
		return !or_mode;
	}
}

public class Buyable
{
	// [XmlElement("BuyableClass_Type")]
	// public BuyableClass type; // <= moved this to derived classes because of ambiguity for XmlSerializer
	
	public string name;
	public string id;
	public string description;
	public List<string> prerequisites;
	
	public long[] total_cost;
	public long[] cost_left;
	
	public int count;
	public bool done;
	
	public Buyable()
	{
	}
	
	public Buyable (BuyableClass _type, int _count)
	{
		// this.type = _type;
        this.name = _type.name;
		this.id = _type.id;
		this.description = _type.description;
		this.prerequisites = _type.prerequisites;
		
		long[] type_cost = _type.cost;
        this.total_cost = new long[] { type_cost[0] * _count, type_cost[1] * _count, type_cost[2] * _count };
        this.total_cost[BuyableClass.labor] /= _count;
        this.cost_left = new long[this.total_cost.Length];
        this.total_cost.CopyTo( this.cost_left, 0 );
		
        this.count = _count;
        this.done = false;
	}
	
	public virtual void finish()
    {
    	if (!this.done)
    	{
            this.cost_left[0] = this.cost_left[1] = this.cost_left[2] = 0 ;
            this.done = true;
         }
    }
    
    protected virtual long[] get_cost_paid()
    {
    	return new long[3] { this.total_cost[0] - this.cost_left[0], this.total_cost[1] - this.cost_left[1], this.total_cost[2] - this.cost_left[2] };
    }
    
    protected virtual void set_cost_paid(long[] value)
   	{
   		this.cost_left = new long[3] { this.total_cost[0] - value[0], this.total_cost[1] - value[1], this.total_cost[2] - value[2] };
   	}
   	

	List<double> _percent_complete(long[] _available)
	{
		long[] cost_paid = this.get_cost_paid();
		
		List<double> result = new List<double>() {
			this.total_cost[0] > 0 ? (double)( cost_paid[0] + _available[0] ) / this.total_cost[0] : 0
			, this.total_cost[1] > 0 ? (double)( cost_paid[1] + _available[1] ) / this.total_cost[1] : 0
			, this.total_cost[2] > 0 ? (double)( cost_paid[2] + _available[2] ) / this.total_cost[2] : 0
		};
		
		return result;
	}
	
	double min_valid (List<double> _complete)
	{
		// return complete[this.total_cost > 0].min()
		// -- returns only those complete[i] for which is this.total_cost[i] > 0
		
		List<double> complete_subset = new List<double> ();
		for (int i = 0; i < _complete.Count; i++)
			if (this.total_cost[i] > 0)
				complete_subset.Add (_complete[i]);
		
		double result = complete_subset[0];
		foreach( double d in complete_subset)
			if ( d < result )
				result = d;
			
		return result; // Math.Min( complete_subset.ToArray() );
	}

    public virtual double percent_complete ()
    {
    	return this.min_valid (this._percent_complete (new long[3] { 0, 0, 0 }));
	}
	
	public virtual void calculate_work (long? _cash_available, long? _cpu_available, long _time, ref long[] _spent, ref long[] _cost_paid)
	{
		// Given an amount of available resources, calculates and returns the
		// amount that would be spent and the progress towards completion.

        // cash_available defaults to all the player's cash.
		if (_cash_available == null)
			_cash_available = G.pl.cash;

        // cpu_available defaults to the entire CPU Pool.
		if (_cpu_available == null)
			_cpu_available = G.pl.cpu_pool;
		
        // Figure out how much we could complete.
		List<double> pct_complete = this._percent_complete (new long[3] { _cash_available.Value, _cpu_available.Value, _time });
		
		if ( G.zero_construct )
			pct_complete = new List<double>() { 1f, 1f, 1f } ;
		
		// Find the least-complete resource.
		double least_complete = this.min_valid (pct_complete);

        // Let the other two be up to 5 percentage points closer to completion.
		double complete_cap = Math.Min (1f, least_complete + .05f);
		
		for (int i = 0; i < pct_complete.Count; i++)
			if (pct_complete[i] > complete_cap)
				pct_complete[i] = complete_cap;
				
        // Translate that back to the total amount complete.
		List<long> raw_paid = new List<long>() {
			(long)Math.Ceiling(pct_complete[0] * (double)this.total_cost[0])
			, (long)Math.Ceiling(pct_complete[1] * (double)this.total_cost[1])
			, (long)Math.Ceiling(pct_complete[2] * (double)this.total_cost[2]) };

        // And apply it.
		long[] was_complete = this.get_cost_paid();
		
		_cost_paid = new long[3] {
			Math.Max ( raw_paid[0], was_complete[0])
			, Math.Max ( raw_paid[1], was_complete[1])
			, Math.Max ( raw_paid[2], was_complete[2]) };
		
		_spent = new long[3] { _cost_paid[0] - was_complete[0], _cost_paid[1] - was_complete[1], _cost_paid[2] - was_complete[2] } ;
	}
	
	public virtual bool work_on (long? _cash_available, long? _cpu_available, long _time)
	{
		// As calculate_work, but apply the changes.
		// Returns a boolean indicating whether this buyable is done afterwards.

        if (this.done)
			return true;
		
		long[] _spent = new long[3], _cost_paid = new long[3];
		
		this.calculate_work (_cash_available, _cpu_available, _time, ref _spent, ref _cost_paid);
		
		this.set_cost_paid(_cost_paid);
		
        // Consume CPU and Cash.
		// Note the cast from <type 'numpy.int64'> to <type 'int'> to avoid
		// poisoning other calculations (like, say, g.pl.do_jobs).
		G.pl.cpu_pool -= _spent[BuyableClass.cpu];
		G.pl.cash -= _spent[BuyableClass.cash];

		bool _done = true;
		
		for (int i = 0; i < this.cost_left.Length; i++)
		{
			if (this.cost_left[i] > 0)
			{
				_done = false;
				break;
			}
		}
		
		if ( _done )
			this.finish();
		
		return _done;
	}	
	
	public virtual void destroy()
	{
//        self.type.count -= self.count
//        if self.done:
//            self.type.complete_count -= self.count
    }
}
