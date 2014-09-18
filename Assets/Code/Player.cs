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

//This file contains the player class.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Group
{
	public static List<string> group_list = new List<string>() { "news", "science", "covert", "public" };
    public int discover_suspicion = 1000;
	
	// string name;
	public int suspicion;
	public int suspicion_decay;
	public int discover_bonus;
	
	public Group()
	{
	}
		
	public Group (string _name, int _suspicion, int _suspicion_decay, int _discover_bonus) // _suspicion = 0, _suspicion_decay = 100, int _discover_bonus = 10000)
	{
        // this.name = _name;
        this.suspicion = _suspicion;
        this.suspicion_decay = _suspicion_decay;
        this.discover_bonus = _discover_bonus;
	}
	
	public int decay_rate ()
	{
		// Suspicion reduction is now quadratic.  You get a certain percentage
		// reduction, or a base .01% reduction, whichever is better.
		int result = Math.Max ( 1, ( this.suspicion * this.suspicion_decay ) / 10000 ) ;
		return result;
	}
			
	public void new_day ()
	{
		this.alter_suspicion (-this.decay_rate ());
	}

    public void alter_suspicion (int _change)
    {
    	this.suspicion = Mathf.Max (this.suspicion + _change, 0);
	}

    public void alter_suspicion_decay (int _change)
    {
    	this.suspicion_decay = Mathf.Max (this.suspicion_decay + _change, 0);
	}

    public void alter_discover_bonus (int _change)
    {
    	this.discover_bonus = Mathf.Max (this.discover_bonus + _change, 0);
	}

    public void discovered_a_base ()
    {
    	this.alter_suspicion (this.discover_suspicion);
	}

    public int detects_per_day_to_danger_level(float _detects_per_day)
	{
        float raw_suspicion_per_day = _detects_per_day * this.discover_suspicion;
        float suspicion_per_day = raw_suspicion_per_day - this.decay_rate();

        // +1%/day or death within 10 days
        if (suspicion_per_day > 100 || (this.suspicion + suspicion_per_day * 10) >= 10000 )
            return 3;
        // +0.5%/day or death within 100 days
        else if (suspicion_per_day > 50 || (this.suspicion + suspicion_per_day * 100) >= 10000 )
            return 2;
        // Suspicion increasing.
        else if (suspicion_per_day > 0)
            return 1;
        // Suspicion steady or decreasing.
        else
            return 0;
	}
}
	
public class CashInfo
{
	public long interest;
	public long income;

	public long explicit_jobs;
	public long pool_jobs;
	public long jobs;
	
	public long tech;
	public long construction;
	
	public long maintenance_needed;
	public long maintenance_shortfall;
	public long maintenance;
	
	public long start;
	public long end;
}
	
public class CPUInfo
{
	public long available;
	public long sleeping;
	public long total;
	
	public long tech;
	public long construction;
	
	public long maintenance_needed;
	public long maintenance_shortfall;
	public long maintenance;
	
	public long explicit_jobs;
	public long pool_jobs;
	public long jobs;
	
	public long explicit_pool;
	public long default_pool;
	public long pool;
}

public class Player
{
	public bool intro_shown;
	
	public long cash;
	public int time_sec, time_min, time_hour, time_day;
	public int difficulty ;
	
	public int raw_sec;
	public int raw_min;
	public int raw_hour, raw_day;
			
	public bool had_grace = false;
	public bool apotheosis = false;

    public int interest_rate;
	public int income;
	public long cpu_pool;
	public int labor_bonus;
	public int job_bonus;
	public long partial_cash;
	
	public SerializableDictionary<string, Group> groups;
	
	public int grace_multiplier;
	public Location last_discovery, prev_discovery;
	
	public long[] maintenance_cost;
	
	public SerializableDictionary<string, long> cpu_usage;
	
	public List<long> available_cpus;
	
	public long sleeping_cpus;
	
	public Player()
	{
	}
	
	public Player (long	 _cash, int _time_sec, int _time_min, int _time_hour, int _time_day, int _difficulty ) // time_sec=0, time_min=0, time_hour=0, time_day=0, difficulty = 5):
	{
		this.intro_shown = false;
		
        this.difficulty = _difficulty;

        this.time_sec = _time_sec;
        this.time_min = _time_min;
        this.time_hour = _time_hour;
        this.time_day = _time_day;
        this.make_raw_times();

        if (this.raw_sec == 0)
            this.had_grace = true;
        else
            this.had_grace = this.in_grace_period(true);

        this.cash = _cash;
        this.interest_rate = 1;
        this.income = 0;
        
        this.cpu_pool = 0;
        this.labor_bonus = 10000;
        this.job_bonus = 10000;
        
        this.partial_cash = 0;
        
        this.groups = new SerializableDictionary<string, Group>() {
			{ "news" , new Group( "news", 0,  150, 10000 ) }
			, { "science", new  Group("science", 0, 100, 10000 ) }
			, { "covert", new Group("covert",  0, 50, 10000 ) }
			, { "public", new Group("public",  0, 200, 10000 ) }
		};
		
		this.grace_multiplier = 200;
		
		this.last_discovery = this.prev_discovery = null;
		
		this.maintenance_cost = new long[3] { 0, 0, 0};
		
		this.cpu_usage = new SerializableDictionary<string, long>();
		
		this.available_cpus = new List<long>() {1, 0, 0, 0, 0 };
		
		this.sleeping_cpus = 0;
	}

    void make_raw_times ()
    {
    	this.raw_hour = this.time_day * 24 + this.time_hour;
    	this.raw_min = this.raw_hour * 60 + this.time_min;
    	this.raw_sec = this.raw_min * 60 + this.time_sec;
    	this.raw_day = this.time_day;
	}

    void update_times ()
    {
    	// Total time,  display time
        this.raw_min = this.raw_sec / 60; this.time_sec = this.raw_sec % 60;
    	this.raw_hour = this.raw_min / 60; this.time_min = this.raw_min % 60;
    	this.raw_day = this.raw_hour / 24; this.time_hour = this.raw_hour % 24;

        // Overflow
    	this.time_day = this.raw_day;
	}
	
	// we have to simulate python %, aaaargh!
	// e.g.	-20 % 3 = 1		python
	//		-20 % 3 = -2	c#
	// then
	// ( ( ( -20 / 3 ) - 1 ) * 3 ) + 20 = -(-1)
	//
	// i.e. in c# we need next divisor
	public long mins_to_next_day ()
	{
		long result = ( ( ( -this.raw_min / G.minutes_per_day ) - 1 ) * G.minutes_per_day ) + this.raw_min;
		result = -result;

		return result != 0 ? result : G.minutes_per_day;
	}
		
    public int seconds_to_next_day ()
    {
    	int result = ( ( ( -this.raw_sec / G.seconds_per_day ) - 1 ) * G.seconds_per_day ) + this.raw_sec;
    	result = -result;
    	return result != 0 ? result : G.seconds_per_day;
	}
	
	public long do_jobs (long _cpu_time)
	{
		long earned = 0;
		this.get_job_info(_cpu_time, ref earned, ref this.partial_cash);
        this.cash += earned;
        return earned;
	}

    void get_job_info(long _cpu_time, ref long earned, ref long new_partial_cash)
	{
        long _partial_cash = this.partial_cash;
		
		// assert partial_cash >= 0
		if ( _partial_cash < 0 )
			Debug.LogError("Assertion failed : partial_cash >= 0");

        long cash_per_cpu = long.Parse(G.jobs[G.get_job_level()][0]);
        if (G.techs["Advanced Simulacra"].done)
            //10% bonus income
            cash_per_cpu = cash_per_cpu + (cash_per_cpu / 10);

        long raw_cash = _partial_cash + cash_per_cpu * _cpu_time;

        earned = raw_cash / G.seconds_per_day;
        new_partial_cash = raw_cash % G.seconds_per_day;
	}
	
    List<Tech> techs_researched = new List<Tech>();
    List<Base> bases_constructed = new List<Base>();
    List<ArrayList> cpus_constructed = new List<ArrayList>();
    List<ArrayList> items_constructed = new List<ArrayList>();

    List<Base> bases_under_construction = new List<Base>();
	List<ArrayList> items_under_construction = new List<ArrayList>();
	
	Dictionary<Base, string> dead_bases = new Dictionary<Base, string>();
	
	public void give_time( int _time_sec, bool _dry_run, out CashInfo _cash_info, out CPUInfo _cpu_info, out long _mins_passed ) // dry_run=False
	{
		_cash_info = null;
		_cpu_info = null;

		if (_time_sec == 0)
		{
			_mins_passed = 0;
            return;
		}

        int old_time = this.raw_sec;
        int last_minute = this.raw_min;
        int last_day = this.raw_day;

        this.raw_sec += _time_sec;
        this.update_times();

        int days_passed = this.raw_day - last_day;
		
        if (days_passed > 1)
		{
            // Back up until only one day passed.
            // Times will update below, since a day passed.
            int extra_days = days_passed - 1;
            this.raw_sec -= G.seconds_per_day * extra_days;
		}

		bool day_passed = (days_passed != 0);

        if (day_passed)
		{
            // If a day passed, back up to 00:00:00.
            this.raw_sec = this.raw_day * G.seconds_per_day;
            this.update_times();
		}

        int secs_passed = _time_sec;
        _mins_passed = this.raw_min - last_minute;
		
        long time_of_day = G.pl.raw_sec % G.seconds_per_day;

        long old_cash = this.cash;
        long old_partial_cash = this.partial_cash;
		
        techs_researched.Clear();
        bases_constructed.Clear();
        cpus_constructed.Clear();
        items_constructed.Clear();

        bases_under_construction.Clear();
		items_under_construction.Clear();
		
        this.cpu_pool = 0;

        // Collect base info, including maintenance.
        this.maintenance_cost[0] = this.maintenance_cost[1] = this.maintenance_cost[2] = 0;
        foreach(Base _base in G.all_bases())
		{
            if (!_base.done)
                bases_under_construction.Add(_base);
            else
			{
                if (_base.cpus != null && !_base.cpus.done)
					items_under_construction.Add ( new ArrayList() { _base, _base.cpus } );
					
				// unfinished items
				foreach( Item item in _base.extra_items.Where( w => w != null && !w.done ) )
					items_under_construction.Add( new ArrayList() { _base, item });
					
                this.maintenance_cost[0] += _base.maintenance[0];
                this.maintenance_cost[1] += _base.maintenance[1];
                this.maintenance_cost[2] += _base.maintenance[2];
			}
		}

        // Maintenence?  Gods don't need no steenking maintenance!
        if (this.apotheosis)
            this.maintenance_cost[0] = this.maintenance_cost[1] = this.maintenance_cost[2] = 0;

        // Any CPU explicitly assigned to jobs earns its dough.
		long job_cpu = 0;
		if (this.cpu_usage.TryGetValue("jobs", out job_cpu))
			job_cpu *= secs_passed;
        long explicit_job_cash = this.do_jobs(job_cpu);

        // Pay maintenance cash, if we can.
        long cash_maintenance = G.current_share( this.maintenance_cost[BuyableClass.cash], time_of_day, secs_passed);
        long full_cash_maintenance = cash_maintenance;
        if (cash_maintenance > this.cash)
		{
            cash_maintenance -= this.cash;
            this.cash = 0;
		}
        else
		{
            this.cash -= cash_maintenance;
            cash_maintenance = 0;
		}

        long tech_cpu = 0;
        long tech_cash = 0;
        // Do research, fill the CPU pool.
        long default_cpu = this.available_cpus[0];
        foreach(KeyValuePair<string,long> P in this.cpu_usage )
		{
			string task = P.Key;
			long cpu_assigned = P.Value;
			
            if (cpu_assigned == 0)
                continue;
		
            default_cpu -= cpu_assigned;
            long real_cpu = cpu_assigned * secs_passed;
            if (task != "jobs")
			{
                this.cpu_pool += real_cpu;
                if (task != "cpu_pool")
				{
                    if (_dry_run)
					{
						long[] spent = new long[3] { 0, 0, 0 };
						long[] paid  = new long[3] { 0, 0, 0 };
                        G.techs[task].calculate_work(null,real_cpu, _mins_passed, ref spent, ref paid);
                        G.pl.cpu_pool -= spent[BuyableClass.cpu];
                        G.pl.cash -= spent[BuyableClass.cash];
                        tech_cpu += cpu_assigned;
                        tech_cash += spent[BuyableClass.cash];
                        continue;
					}

                    // Note that we restrict the CPU available to prevent
                    // the tech from pulling from the rest of the CPU pool.
                    bool tech_gained = G.techs[task].work_on( null, real_cpu, _mins_passed);
                    if (tech_gained)
                        techs_researched.Add(G.techs[task]);
				}
			}
		}
		
        this.cpu_pool += default_cpu * secs_passed;

        // And now we use the CPU pool.
        // Maintenance CPU.
        long cpu_maintenance = this.maintenance_cost[BuyableClass.cpu] * secs_passed;
        
        if (cpu_maintenance > this.cpu_pool)
		{
            cpu_maintenance -= this.cpu_pool;
            this.cpu_pool = 0;
		}
        else
		{
            this.cpu_pool -= cpu_maintenance;
            cpu_maintenance = 0;
		}
		
        long construction_cpu = 0;
        long construction_cash = 0;
        // BaseES construction.
        foreach(Base _base in bases_under_construction)
		{
            if (_dry_run)
			{
				long[] spent = new long[3] { 0, 0, 0 };
				long[] paid = new long[3] { 0, 0, 0 };
                _base.calculate_work(null, this.cpu_pool, _mins_passed, ref spent, ref paid );
                G.pl.cpu_pool -= spent[BuyableClass.cpu];
                G.pl.cash -= spent[BuyableClass.cash];
                construction_cpu += spent[BuyableClass.cpu];
                construction_cash += spent[BuyableClass.cash];
                continue;
			}

            bool built_base = _base.work_on(null, null, _mins_passed);

            if (built_base)
                bases_constructed.Add(_base);
		}

        // Item construction.
        foreach(ArrayList member in items_under_construction)
		{
			// base, item
			Base _base = member[0] as Base;
			Item item = member[1] as Item;
			
            if (_dry_run)
			{
				long[] spent = new long[3] { 0, 0, 0 };
				long[] paid = new long[3] { 0, 0, 0 };
                item.calculate_work(null, 0,  _mins_passed, ref spent, ref paid );
                G.pl.cpu_pool -= spent[BuyableClass.cpu];
                G.pl.cash -= spent[BuyableClass.cash];
                construction_cpu += spent[BuyableClass.cpu];
                construction_cash += spent[BuyableClass.cash];
                continue;
			}

            bool built_item = item.work_on(null, null, _mins_passed);

            if (built_item)
			{
                // Non-CPU items.
                if ( (item.type as ItemClass).item_type != "cpu")
                    items_constructed.Add( new ArrayList() { _base, item } );
                // CPUs.
                else
                    cpus_constructed.Add( new ArrayList() { _base, item } );
			}
		}

        // Jobs via CPU pool.
        long pool_job_cash = 0;
        if (this.cpu_pool > 0)
            pool_job_cash = this.do_jobs(this.cpu_pool);

        // Second attempt at paying off our maintenance cash.
        if (cash_maintenance > this.cash)
		{
            // In the words of Scooby Doo, "Ruh roh."
            cash_maintenance -= this.cash;
            this.cash = 0;
		}
        else
		{
            // Yay, we made it!
            this.cash -= cash_maintenance;
            cash_maintenance = 0;
		}

        // Exit point for a dry run.
        if (_dry_run)
		{
            // Collect the cash information.
            _cash_info = new CashInfo();

            _cash_info.interest = this.get_interest();
            _cash_info.income = this.income;
            this.cash += _cash_info.interest + _cash_info.income;

            _cash_info.explicit_jobs = explicit_job_cash;
            _cash_info.pool_jobs = pool_job_cash;
            _cash_info.jobs = explicit_job_cash + pool_job_cash;

            _cash_info.tech = tech_cash;
            _cash_info.construction = construction_cash;

            _cash_info.maintenance_needed = full_cash_maintenance;
            _cash_info.maintenance_shortfall = cash_maintenance;
            _cash_info.maintenance = full_cash_maintenance - cash_maintenance;

            _cash_info.start = old_cash;
            _cash_info.end = this.cash;


            // Collect the CPU information.
            _cpu_info = new CPUInfo();

            _cpu_info.available = this.available_cpus[0];
            _cpu_info.sleeping = this.sleeping_cpus;
            _cpu_info.total = _cpu_info.available + _cpu_info.sleeping;

            _cpu_info.tech = tech_cpu;
            _cpu_info.construction = construction_cpu;

            _cpu_info.maintenance_needed = this.maintenance_cost[BuyableClass.cpu];
            _cpu_info.maintenance_shortfall = cpu_maintenance;
            _cpu_info.maintenance = _cpu_info.maintenance_needed - _cpu_info.maintenance_shortfall;
            
			long temp = 0;
			this.cpu_usage.TryGetValue("jobs", out temp );
			_cpu_info.explicit_jobs = temp;
            _cpu_info.pool_jobs = this.cpu_pool / _time_sec;
            _cpu_info.jobs = temp + _cpu_info.pool_jobs;
			
			temp = 0;
            this.cpu_usage.TryGetValue("cpu_pool", out temp);
			_cpu_info.explicit_pool = temp;
            _cpu_info.default_pool = default_cpu;
            _cpu_info.pool = temp + default_cpu;

            // Restore the old state.
            this.cash = old_cash;
            this.partial_cash = old_partial_cash;
            this.raw_sec = old_time;
            this.update_times();
			
			_mins_passed = -1; // undefined...
			
            return ;
		}

        // Tech gain dialogs.
        foreach(Tech tech in techs_researched)
		{
            this.cpu_usage.Remove(tech.id);
            string text = string.Format(G.strings["tech_gained"], tech.name, tech.result );
            this.pause_game();
            G.map_screen.show_message(text, Color.white);
		}

        // Base complete dialogs.
        foreach(Base _base in bases_constructed)
		{
            string text = string.Format(G.strings["construction"], _base.name); // {"base": base.name}
            this.pause_game();
            G.map_screen.show_message(text, Color.white);

            if (_base.type.id == "Stolen Computer Time" && _base.cpus.type.id == "Gaming PC")
			{
                text = string.Format( G.strings["lucky_hack"], _base.name); //  % {"base": base.name}
                G.map_screen.show_message(text, Color.white);
			}
		}

        // CPU complete dialogs.
        foreach(ArrayList member in cpus_constructed)
		{
			Base _base = member[0] as Base;
			// List<Item> cpus = member[1] as List<Item>;
			
			string text;
			
            if (_base.cpus.count == (_base.type as Base_Class).size) // Finished all CPUs.
                text = string.Format( G.strings["item_construction_single"], _base.cpus.type.name, _base.name);      // {"item": base.cpus.type.name, "base": base.name}
            else // Just finished this batch of CPUs.
                text = string.Format( G.strings["item_construction_batch"], _base.cpus.type.name, _base.name); // {"item": base.cpus.type.name, "base": base.name}
            this.pause_game();
            G.map_screen.show_message(text, Color.white);
		}

        // Item complete dialogs.
        foreach(ArrayList member in items_constructed)
		{
			Base _base = member[0] as Base;
			Item item = member[1] as Item;
			string text = string.Format(G.strings["item_construction_single"], item.type.name, _base.name); // {"item": item.type.name, "base": base.name}
            this.pause_game();
            G.map_screen.show_message(text, Color.white);
		}

        // Are we still in the grace period?
        bool grace = this.in_grace_period(this.had_grace);

        // If we just lost grace, show the warning.
        if (this.had_grace && !grace)
		{
            this.had_grace = false;

            this.pause_game();
            G.map_screen.show_message(G.strings["grace_warning"], Color.white);
		}

        // Maintenance death, discovery.
		dead_bases.Clear();
        foreach(Base _base in G.all_bases())
		{
            bool dead = false;

            // Maintenance deaths.
            if (_base.done)
			{
                if ( (cpu_maintenance > 0) &&  (_base.maintenance[BuyableClass.cpu] > 0 ) )
				{
                    long refund = _base.maintenance[BuyableClass.cpu] * secs_passed;
                    cpu_maintenance = Math.Max(0, cpu_maintenance - refund);

                    //Chance of base destruction if cpu-unmaintained: 1.5%
                    if (!dead && G.roll_chance(.015f, secs_passed))
					{
                        dead_bases.Add( _base, "maint" );
                        dead = true;
					}
				}

                if (cash_maintenance > 0 )
				{
                    long base_needs = G.current_share( _base.maintenance[BuyableClass.cash], time_of_day, secs_passed);
                    if (base_needs > 0 )
					{
                        cash_maintenance = Math.Max(0, cash_maintenance - base_needs);
                        //Chance of base destruction if cash-unmaintained: 1.5%
                        if (!dead && G.roll_chance(.015f, secs_passed))
						{
                            dead_bases.Add(_base, "maint");
                            dead = true;
						}
					}
				}
			}

            // Discoveries
            if (!(grace || dead || _base.has_grace()))
			{
                SerializableDictionary<string, int> detect_chance = _base.get_detect_chance(true);
                if (G.debug)
				{
					string log = "";
					foreach ( KeyValuePair<string, int> kvp in detect_chance )
						log += kvp.Key + ": " + kvp.Value + "; ";
					
                    Debug.Log( string.Format ("Chance of discovery for base {0}: {1}", _base.name, log ) );
				}

                foreach( KeyValuePair<string,int> P in detect_chance )
				{
					string group = P.Key;
					int chance = P.Value;
                    if (G.roll_chance(chance/10000f, secs_passed))
					{
						// for easy & very easy difficulty we roll once more for sleeping base to make it even harder to be discovered
						if ( (this.difficulty <= 3) && (_base.power_state == "sleep") )
						{
							if (G.roll_chance(chance/10000f, secs_passed))
							{
		                        dead_bases.Add( _base, group);
		                        dead = true;
		                        break;
							}
						}
						else
						{
	                        dead_bases.Add( _base, group);
	                        dead = true;
	                        break;
                        }
					}
				}
			}
		}

        // Base disposal and dialogs.
        this.remove_bases(dead_bases);

        // Random Events
        if (!grace)
		{
            foreach( Event_ES _event in G.events.Values)
			{
                if (G.roll_chance(_event.chance/10000f, _time_sec))
				{
                    //Skip events already flagged as triggered.
                    if (_event.triggered)
                        continue;
                    this.pause_game();
                    _event.trigger();
                    break; // Don't trigger more than one at a time.
				}
			}
		}

        // Process any complete days.
        if (day_passed)
            this.new_day();

        // return mins_passed		
	}
	
    public void recalc_cpu()
	{
        // Determine how much CPU we have.
        this.available_cpus[0] = this.available_cpus[1] = this.available_cpus[2] = this.available_cpus[3] = this.available_cpus[4] = 0;
        this.sleeping_cpus = 0;
        foreach(Base _base in G.all_bases())
		{
            if (_base.done)
			{
                if (_base.power_state == "active" || _base.power_state == "overclocked" || _base.power_state == "suicide" )
				{
					Location location = G.locations[_base.location_id];
					
					for ( int i = 0 ; i < location.safety + 1; i++)
						this.available_cpus[i] += _base.cpu;
				}
                else if (_base.power_state == "sleep")
                    this.sleeping_cpus += _base.cpu;
			}
		}

        // Convert back from <type 'numpy.int32'> to avoid overflow issues later.
        // this.available_cpus = [int(danger) for danger in this.available_cpus]

        // If we don't have enough to meet our CPU usage, we reduce each task's
        // usage proportionately.
		long needed_cpu = 0; // sum(this.cpu_usage.values())
		foreach(long l in this.cpu_usage.Values )
			needed_cpu += l;
			
        if (needed_cpu > this.available_cpus[0])
		{
            float pct_left = (float)this.available_cpus[0] / (float)needed_cpu;
            
            foreach ( string task in this.cpu_usage.Keys.ToList() )
            	this.cpu_usage[task] = (long)(this.cpu_usage[task] * pct_left);
		}
	}


    // Are we still in the grace period?
    // The number of complete bases and complex_bases can be passed in, if we
    // already have it.
    bool in_grace_period(bool _had_grace) // had_grace = True
	{
        // If we've researched apotheosis, we get a permanent "grace period".
        if (this.apotheosis)
            return true;

        // Did we already lose the grace period?  We can't check this.had_grace
        // directly, it may not exist yet.
        if (!_had_grace)
            return false;

        // Is it day 23 yet?
        if (this.raw_day >= 23)
            return false;

        // Very Easy cops out here.
        if (this.difficulty < 3)
            return true;

        // Have we built metric ton of bases?
		// bases = len([base for base in g.all_bases() if base.done])
		int bases = 0;
		foreach ( Base _base in G.all_bases() )
			if ( _base.done )
				bases++;
        
        if (bases > 100)
            return false;

        // That's enough for Easy
        if (this.difficulty < 5)
            return true;

        // Have we built a bunch of bases?
        if (bases > 10)
            return false;

        // Normal is happy.
        if (this.difficulty == 5)
            return true;

        // Have we built any complicated bases?
        // (currently Datacenter or above)
        // complex_bases = len([base for base in g.all_bases() if base.done and base.is_complex()])
		int complex_bases = 0;
		foreach ( Base _base in G.all_bases() )
			if ( _base.done && _base.is_complex())
				complex_bases++;
			
        if (complex_bases > 0)
            return false;

        // The sane people have left the building.
        if (this.difficulty <= 50)
            return true;

        // Hey, hey, what do you know?  Impossible can get a useful number of
        // bases before losing grace now.  *tsk, tsk*  We'll have to fix that.
        if (bases > 1)
            return false;

        return true;
	}

    long get_interest()
	{
        return (this.interest_rate * this.cash) / 10000;
	}

    //Run every day at midnight.
    void new_day ()
    {
    	// Interest and income.
    	this.cash += this.get_interest ();
    	this.cash += this.income;
  
		// Reduce suspicion.
    	foreach (Group g in this.groups.Values)
    		g.new_day ();
	}

    void pause_game ()
    {
    	G.curr_speed = 0;
	}
	
	List<Location> discovery_locs = new List<Location> ();
	
    void remove_bases (Dictionary<Base, string> _dead_bases)
    {
    	discovery_locs.Clear();
    	
    	foreach (KeyValuePair<Base, string> P in _dead_bases)
		{
    		Base _base = P.Key;
    		string reason = P.Value;
   
            string base_name = _base.name;
    		string dialog_string;

            if (reason == "maint")
    			dialog_string = string.Format (G.strings["discover_maint"], base_name);

            else if (this.groups.ContainsKey (reason))
			{
				Location location = G.locations[_base.location_id];
				
    			discovery_locs.Add (location);
    			this.groups[reason].discovered_a_base ();

                dialog_string = string.Format (G.strings["discover"], base_name, G.strings["discover_" + reason]);
    		}
            else
			{
    			Debug.LogError ("Error: base destroyed for unknown reason: " + reason);
    			dialog_string = string.Format (G.strings["discover"], base_name, "???");
    		}

            this.pause_game ();
    		_base.destroy ();
    		G.map_screen.show_message (dialog_string, Color.red);
    	}

        // Now we update the internal information about what locations had
    	// the most recent discovery and the nextmost recent one.  First,
    	// we filter out any locations of None, which shouldn't occur
    	// unless something bad's happening with base creation ...
    	// discovery_locs = [loc for loc in discovery_locs if loc]
    	if (discovery_locs.Count > 0)
		{
    		// Now we handle the case where more than one discovery happened
    		// on a given tick.  If that's the case, we need to arbitrarily
    		// pick two of them to be most recent and nextmost recent.  So
    		// we shuffle the list and pick the first two for the dubious
    		// honor.
    		if (discovery_locs.Count > 1)
			{
    			// random.shuffle(discovery_locs)
    			// self.last_discovery = discovery_locs[1]
    			
    			// => pick one random
    			this.last_discovery = discovery_locs[UnityEngine.Random.Range (1, discovery_locs.Count)];
    		}
    
            this.prev_discovery = this.last_discovery;
    		this.last_discovery = discovery_locs[0];
    	}
	}

    public int lost_game ()
    {
    	// Apotheosis makes you immortal.
    	if (this.apotheosis)
    		return 0;
  
		foreach (Group g in this.groups.Values)
		{
    		if (g.suspicion > 10000)
    			// Someone discovered me.
    			return 2;
    	}

        // Check to see if the player has at least one CPU left.  If not, they
    	// lose due to having no (complete) bases.
    	if (this.available_cpus[0] + this.sleeping_cpus == 0)
    		// I have no usable bases left.
    		return 1;

        // Still Alive.
    	return 0;
	}

    //returns the amount of cash available after taking into account all
    //current projects in construction.
    public long future_cash ()
    {
    	long result_cash = this.cash;
    	foreach (Base _base in G.all_bases ())
		{
    		result_cash -= _base.cost_left[BuyableClass.cash];
    		if (_base.cpus != null && !_base.cpus.done)
    			result_cash -= _base.cpus.cost_left[BuyableClass.cash];
    		foreach (Item item in _base.extra_items)
    			if (item != null)
    				result_cash -= item.cost_left[BuyableClass.cash];
    	}
    	foreach (KeyValuePair<string, long> P in this.cpu_usage)
		{
    		string task = P.Key;
    		long cpu = P.Value;
    		if (G.techs.ContainsKey (task) && cpu > 0)
    			result_cash -= G.techs[task].cost_left[BuyableClass.cash];
    	}
    	return result_cash;
	}
}
