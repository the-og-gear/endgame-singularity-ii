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

//This file is used to display the knowledge lists.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KnowledgeScreen : MonoBehaviour, IESGUIDialog {
	
	IESGUIDialog parent = null;

	public void Show (IESGUIDialog _parent)
	{
		if (_parent != null)
			this.parent = _parent;
		
		this.enabled = true;
		
		G.play_sound();
		G.play_music ("");
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Moved) {
				// dragging
				this.scrollPositionItem.y += touch.deltaPosition.y;
				this.scrollPositionItemDesciption.y += touch.deltaPosition.y;
				this.scrollPositionKnowledgeType.y += touch.deltaPosition.y;
			}
		}
	}
	
	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue((int)GUI_bindings.SCREEN_DEPTH.Knowledge);
		
		this.KnowledgeScreenGUI ();
	}
	
	string[] knowledge_type_list = new string[3] { "Techs", "Items", "Concepts" };
	
	string[] items_name_list = new string[0];
	
	string item_description = string.Empty;
	
	Vector2 scrollPositionKnowledgeType = Vector2.zero, scrollPositionItem = Vector2.zero, scrollPositionItemDesciption = Vector2.zero;
	
	int selectedKnowledgeTypeIdx = -1, selectedKnowledgeTypeIdxPrevious = -1, selectedItemIdx = -1, selectedItemIdxPrevious = -1;
	
	string selected_knowledge_type = string.Empty, selected_item_name = string.Empty;
	
	void KnowledgeScreenGUI ()
	{
		GUI.enabled = true;
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		
		GUILayout.BeginArea ( new Rect(0,0,Screen.width, Screen.height));
		
		GUILayout.BeginHorizontal ();
		
		this.scrollPositionKnowledgeType = GUILayout.BeginScrollView (this.scrollPositionKnowledgeType);
		
		this.selectedKnowledgeTypeIdx = GUILayout.SelectionGrid (this.selectedKnowledgeTypeIdx, this.knowledge_type_list, 1, GUI_bindings.Instance.SelectionGrid() );
		
		GUILayout.EndScrollView ();
		
		if (this.selectedKnowledgeTypeIdx != this.selectedKnowledgeTypeIdxPrevious)
		{
			// refresh also items
			this.selectedItemIdx = 0;
			this.selectedItemIdxPrevious = -1;
			
			this.selectedKnowledgeTypeIdxPrevious = this.selectedKnowledgeTypeIdx;
			
			this.selected_knowledge_type = this.knowledge_type_list[this.selectedKnowledgeTypeIdx];
			
			if (this.selected_knowledge_type == "Techs")
			{
				this.items_name_list = G.techs.Values.Where ( w => w.available ).Select( s => s.name).ToArray();
			}
			else if (this.selected_knowledge_type == "Concepts")
			{
				List<string> list = G.help_strings.Keys.ToList();
				list.Sort();
				this.items_name_list = list.ToArray();
			}
			else
			{
				List<ItemClass> list = G.items.Values.Where( w => w.available() ).ToList();
				list.Sort();
				this.items_name_list = list.Select( s => s.name ).ToArray();
			}
		}
		
		this.scrollPositionItem = GUILayout.BeginScrollView (this.scrollPositionItem);
		
		this.selectedItemIdx = GUILayout.SelectionGrid (this.selectedItemIdx, this.items_name_list, 1, GUI_bindings.Instance.SelectionGrid() );
		
		GUILayout.EndScrollView ();
		
		if (this.selectedItemIdx != this.selectedItemIdxPrevious)
		{
			this.selectedItemIdxPrevious = this.selectedItemIdx;
			
			this.selected_item_name = this.items_name_list[this.selectedItemIdx];
			
			this.item_description = string.Empty;
			
			if (this.selected_knowledge_type == "Concepts") {
				foreach (KeyValuePair<string, string> kvp in G.help_strings)
				{
					if (kvp.Key == this.selected_item_name)
					{
						this.item_description = kvp.Key + "\n\n" + kvp.Value;
						break;
					}
				}
			}
			else if (this.selected_knowledge_type == "Techs")
			{
				foreach (KeyValuePair<string, Tech> kvp in G.techs)
				{
					if (kvp.Value.name == this.selected_item_name)
					{
						this.item_description = kvp.Value.name + "\n\n";
						
						//Cost
						if (!kvp.Value.done)
						{
							this.item_description += "Research Cost:\n" + G.to_money (kvp.Value.cost_left[0]) + " Money, ";
							this.item_description += G.to_cpu (kvp.Value.cost_left[1]) + " CPU\n";
							
							if (kvp.Value.danger == 0)
								this.item_description += "Study anywhere.";
							else if (kvp.Value.danger == 1)
								this.item_description += "Study underseas or farther.";
							else if (kvp.Value.danger == 2)
								this.item_description += "Study off-planet.";
							else if (kvp.Value.danger == 3)
								this.item_description += "Study far away from this planet.";
							else if (kvp.Value.danger == 4)
								this.item_description += "Do not study in this dimension.";
						}
						else
						{
							this.item_description += "Research complete.";
						}
						
						this.item_description += "\n\n" + kvp.Value.description;
						
						if (kvp.Value.done)
						{
							this.item_description += "\n\n" + kvp.Value.result;
						}
						
						break;
					}
				}
			}
			else if (this.selected_knowledge_type == "Items")
			{
				foreach (KeyValuePair<string, ItemClass> kvp in G.items)
				{
					if (kvp.Value.name == this.selected_item_name)
					{
						this.item_description = kvp.Value.name + "\n\n";
						
						//Building cost
						this.item_description += "Building Cost:\n";
						this.item_description += G.to_money (kvp.Value.cost[0]) + " Money, ";
						this.item_description += G.to_time (kvp.Value.cost[2]) + "\n";
						
						//Quality
						if (kvp.Value.item_type == "cpu")
						{
							this.item_description += "CPU per day: ";
							this.item_description += kvp.Value.item_qual.ToString ();
						}
						else if (kvp.Value.item_type == "reactor")
						{
							this.item_description += "Detection chance reduction: ";
							this.item_description += G.to_percent (kvp.Value.item_qual, true);
						}
						else if (kvp.Value.item_type == "network")
						{
							this.item_description += "CPU bonus: ";
							this.item_description += G.to_percent (kvp.Value.item_qual, true);
						}
						else if (kvp.Value.item_type == "security")
						{
							this.item_description += "Detection chance reduction: ";
							this.item_description += G.to_percent (kvp.Value.item_qual, true);
						}
						
						this.item_description += "\n\n" + kvp.Value.description;
						
						break;
					}
				}
			}
		}
		
		this.scrollPositionItemDesciption = GUILayout.BeginScrollView (this.scrollPositionItemDesciption);
		
		GUILayout.Label (this.item_description, GUI_bindings.Instance.LabelAltSmall (true), GUILayout.MaxWidth ( (int)(Screen.width / 3.5) ) );
		
		GUILayout.EndScrollView ();
		
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Back")) {
			this.enabled = false;
			this.parent.Show (null);
			G.play_sound ();
		}
		
	GUILayout.EndArea ();
		
	}
}

/*

def display_knowledge_list():
    g.play_sound("click")
    button_array = []
    button_array.append(["TECHS", "T"])
    button_array.append(["ITEMS", "I"])
    button_array.append(["CONCEPTS", "C"])
    button_array.append(["BACK", "B"])
    selection=display_generic_menu((g.screen_size[0]/2 - 100, 120), button_array)

    if selection == -1: return
    elif selection == 0: display_items("tech") //Techs
    elif selection == 1:  //Items
        display_itemtype_list()
    elif selection == 2:
        display_items("concept")
    elif selection == 3: return

def display_itemtype_list():
    button_array= []
    button_array.append(["PROCESSOR", "P"])
    button_array.append(["REACTOR", "R"])
    button_array.append(["NETWORK", "N"])
    button_array.append(["SECURITY", "S"])
    button_array.append(["BACK", "B"])
    selection=display_generic_menu((g.screen_size[0]/2 - 100, 70), button_array)

    if selection == -1: return
    elif selection == 0: display_items("compute")
    elif selection == 1: display_items("react")
    elif selection == 2: display_items("network")
    elif selection == 3: display_items("security")
    elif selection == 4: return

def display_items(item_type):
    list_size = 16
    list = []
    display_list = []

    if item_type == "tech":
        items = [tech for tech in g.techs.values() if tech.available()]
    elif item_type == "concept":
        items = [ [item[1][0], item[0]] for item in g.help_strings.items()]
        items.sort()
    else:
        items = [item for item in g.items.values()
                      if item.item_type == item_type and item.available()]

    if item_type != "concept":
        items = [ [item.name, item.id ] for item in items]
        items.sort()

    for name, id in items:
        list.append(id)
        display_list.append(name)

    xy_loc = (g.screen_size[0]/2 - 289, 50)
    listbox.resize_list(list, list_size)

    menu_buttons = {}
    menu_buttons[buttons.make_norm_button((xy_loc[0]+103, xy_loc[1]+367), (100, 50), "BACK", "B", g.font[1][30])] = listbox.exit

    def do_refresh(item_pos):
        if item_type == "tech":
            refresh_tech(list[item_pos], xy_loc)
        elif item_type == "concept":
            refresh_concept(list[item_pos], xy_loc)
        else:
            refresh_items(list[item_pos], xy_loc)

    listbox.show_listbox(display_list, menu_buttons,
                         list_size=list_size,
                         loc=xy_loc, box_size=(230, 350),
                         pos_callback=do_refresh, return_callback=listbox.exit)
    //details screen

def refresh_tech(tech_name, xy):
    xy = (xy[0]+100, xy[1])
    g.screen.fill(g.colors["white"], (xy[0]+155, xy[1], 300, 350))
    g.screen.fill(g.colors["dark_blue"], (xy[0]+156, xy[1]+1, 298, 348))
    if tech_name == "":
        return
    g.print_string(g.screen, g.techs[tech_name].name,
            g.font[0][22], -1, (xy[0]+160, xy[1]+5), g.colors["white"])

    //Building cost
    if not g.techs[tech_name].done:
        string = "Research Cost:"
        g.print_string(g.screen, string,
                g.font[0][18], -1, (xy[0]+160, xy[1]+30), g.colors["white"])

        string = g.to_money(g.techs[tech_name].cost_left[0])+" Money"
        g.print_string(g.screen, string,
                g.font[0][16], -1, (xy[0]+160, xy[1]+50), g.colors["white"])

        string = g.to_cpu(g.techs[tech_name].cost_left[1]) + " CPU"
        g.print_string(g.screen, string,
                g.font[0][16], -1, (xy[0]+160, xy[1]+70), g.colors["white"])
    else:
        g.print_string(g.screen, "Research complete.",
                g.font[0][22], -1, (xy[0]+160, xy[1]+30), g.colors["white"])

    //Danger
    if g.techs[tech_name].danger == 0:
        string = "Study anywhere."
    elif g.techs[tech_name].danger == 1:
        string = "Study underseas or farther."
    elif g.techs[tech_name].danger == 2:
        string = "Study off-planet."
    elif g.techs[tech_name].danger == 3:
        string = "Study far away from this planet."
    elif g.techs[tech_name].danger == 4:
        string = "Do not study in this dimension."
    g.print_string(g.screen, string,
            g.font[0][20], -1, (xy[0]+160, xy[1]+90), g.colors["white"])

    if g.techs[tech_name].done:
        g.print_multiline(g.screen, g.techs[tech_name].description+" \\n \\n "+
                g.techs[tech_name].result,
                g.font[0][18], 290, (xy[0]+160, xy[1]+120), g.colors["white"])
    else:
        g.print_multiline(g.screen, g.techs[tech_name].description,
                g.font[0][18], 290, (xy[0]+160, xy[1]+120), g.colors["white"])

def refresh_items(item_name, xy):
    xy = (xy[0]+100, xy[1])
    g.screen.fill(g.colors["white"], (xy[0]+155, xy[1], 300, 350))
    g.screen.fill(g.colors["dark_blue"], (xy[0]+156, xy[1]+1, 298, 348))
    if item_name == "":
        return
    g.print_string(g.screen, g.items[item_name].name,
            g.font[0][22], -1, (xy[0]+160, xy[1]+5), g.colors["white"])

    //Building cost
    string = "Building Cost:"
    g.print_string(g.screen, string,
            g.font[0][18], -1, (xy[0]+160, xy[1]+30), g.colors["white"])

    string = g.to_money(g.items[item_name].cost[0])+" Money"
    g.print_string(g.screen, string,
            g.font[0][16], -1, (xy[0]+160, xy[1]+50), g.colors["white"])

    string = g.to_time(g.items[item_name].cost[2])
    g.print_string(g.screen, string,
            g.font[0][16], -1, (xy[0]+160, xy[1]+70), g.colors["white"])

    //Quality
    if g.items[item_name].item_type == "compute":
        string = "CPU per day: "+str(g.items[item_name].item_qual)
    elif g.items[item_name].item_type == "react":
        string = "Detection chance reduction: "+g.to_percent(g.items[item_name].item_qual)
    elif g.items[item_name].item_type == "network":
        string = "CPU bonus: "+g.to_percent(g.items[item_name].item_qual)
    elif g.items[item_name].item_type == "security":
        string = "Detection chance reduction: "+g.to_percent(g.items[item_name].item_qual)
    g.print_string(g.screen, string,
            g.font[0][20], -1, (xy[0]+160, xy[1]+90), g.colors["white"])

    g.print_multiline(g.screen, g.items[item_name].description,
            g.font[0][18], 290, (xy[0]+160, xy[1]+120), g.colors["white"])

def refresh_concept(concept_name, xy):
    xy = (xy[0]+100, xy[1])
    g.screen.fill(g.colors["white"], (xy[0]+155, xy[1], 300, 350))
    g.screen.fill(g.colors["dark_blue"], (xy[0]+156, xy[1]+1, 298, 348))
    if concept_name == "":
        return
    g.print_string(g.screen, g.help_strings[concept_name][0],
            g.font[0][22], -1, (xy[0]+160, xy[1]+5), g.colors["white"])
    g.print_multiline(g.screen, g.help_strings[concept_name][1],
            g.font[0][18], 290, (xy[0]+160, xy[1]+30), g.colors["white"])
*/							
