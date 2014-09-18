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

//This file contains the screen to display finance information.

using UnityEngine;

public class FinanceScreen : MonoBehaviour, IESGUIDialog {
	
	string[] financial_report, cpu_report, financial_numbers, cpu_numbers;
	
	IESGUIDialog parent = null;
	
	public void Show (IESGUIDialog _parent)
	{
		if (_parent != null)
			this.parent = _parent;
		
		this.enabled = true;
		
		G.play_sound ();
		
		G.play_music("");
		
        int seconds_left = G.pl.seconds_to_next_day();
        CashInfo cash_info;
		CPUInfo cpu_info;
		long mins_passed;
		
		G.pl.give_time(seconds_left, true, out cash_info, out cpu_info, out mins_passed );
		
		string interest = string.Format("+ Interest ({0}):", G.to_percent(G.pl.interest_rate,true));
		
		financial_report = new string[] {
			string.Format( "{0,-20}", "  Current Money:")
			, string.Format ("{0,-20}", "+ Jobs:")
			, string.Format ("{0,-20}", " - Research:")
			, string.Format ("{0,-20}", " - Maintenance:")
			, string.Format ("{0,-20}", " - Construction:")
			, string.Format ("{0,-20}", interest)
			, string.Format ("{0,-20}", "+ Income:")
			, string.Format ("{0,-20}", "= Money at Midnight:")
			};
			
		financial_numbers = new string[] {
			string.Format( "{0,-10}", G.to_money(cash_info.start) )
			, string.Format ("{0,-10}", G.to_money (cash_info.jobs))
			, string.Format ("{0,-10}", G.to_money (cash_info.tech))
			, string.Format ("{0,-10}", G.to_money (cash_info.maintenance))
			, string.Format ("{0,-10}", G.to_money (cash_info.construction))
			, string.Format ("{0,-10}", G.to_money (cash_info.interest))
			, string.Format ("{0,-10}", G.to_money (cash_info.income))
			, string.Format ("{0,-10}", G.to_money (cash_info.end))
		};

        cpu_report = new string[] {
        	string.Format( "{0,-20}", "  Total CPU:")
        	, string.Format( "{0,-20}", " - Sleeping CPU:")
        	, string.Format( "{0,-20}", " - Research CPU:")
        	, string.Format( "{0,-20}", " - Job CPU:")
        	, string.Format( "{0,-20}", "= CPU pool:")
        	, string.Format( "{0,-20}", "" )
        	, string.Format( "{0,-20}", " - Maintenance CPU:")
        	, string.Format( "{0,-20}", " - Construction CPU:")
        	, string.Format( "{0,-20}", "= Pool Overflow (Jobs):")
        	};
        	
        cpu_numbers = new string[] {
        	string.Format( "{0,-10}", G.to_money(cpu_info.total))
        	, string.Format( "{0,-10}", G.to_money(cpu_info.sleeping))
        	, string.Format( "{0,-10}", G.to_money(cpu_info.tech))
        	, string.Format( "{0,-10}", G.to_money(cpu_info.explicit_jobs))
        	, string.Format( "{0,-10}", G.to_money(cpu_info.pool))
        	, string.Format( "{0,-10}", "")
        	, string.Format ("{0,-10}", G.to_money (cpu_info.maintenance))
        	, string.Format ("{0,-10}", G.to_money (cpu_info.construction))
        	, string.Format ("{0,-10}", G.to_money (cpu_info.pool_jobs))
        };
	}

	void OnGUI ()
	{
		GUI_bindings.Instance.GUIPrologue((int)GUI_bindings.SCREEN_DEPTH.Finance);
		
		this.FinanceScreenGUI ();
	}
	
	int selectedF = -1, selectedC = -1;
	
	void FinanceScreenGUI ()
	{
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUI.Box (GUI_bindings.MESSAGE_BOX_RECT, "");
		
		GUILayout.BeginArea (GUI_bindings.MESSAGE_BOX_RECT);

		GUILayout.BeginHorizontal ();
		
		GUILayout.BeginVertical();
		
		GUILayout.Label ("\nFinancial Report" );
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal();
		
		GUILayout.SelectionGrid ( selectedF, financial_report, 1, GUI_bindings.Instance.LabelAlt( true, null, false));
		
		GUILayout.SelectionGrid ( selectedF, financial_numbers, 1, GUI_bindings.Instance.LabelAlt( true, null, false));
		
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndVertical();
		
		GUILayout.FlexibleSpace();
		
		GUILayout.BeginVertical ();
		
		GUILayout.Label ("\nCPU usage");
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.BeginHorizontal();
		
		GUILayout.SelectionGrid( selectedC, cpu_report, 1, GUI_bindings.Instance.LabelAlt( true, null, false) );
		
		GUILayout.SelectionGrid( selectedC, cpu_numbers, 1, GUI_bindings.Instance.LabelAlt( true, null, false) );
		
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndVertical ();
		
		GUILayout.FlexibleSpace ();
		
		GUILayout.EndHorizontal ();
		
		if (GUILayout.Button ("Back"))
		{
			this.enabled = false;
			this.parent.Show (null);
			G.play_sound ();
		}
		
		GUILayout.EndArea ();		
	}
}
