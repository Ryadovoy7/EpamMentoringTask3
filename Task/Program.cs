// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SampleSupport;
using SampleQueries;
using System.IO;

// See the ReadMe.html for additional information
namespace SampleQueries
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			List<SampleHarness> harnesses = new List<SampleHarness>();

			harnesses.Add(new LinqSamples());
			harnesses.Add(new EpamSamples());
						
			Application.EnableVisualStyles();
				
			using (SampleForm form = new SampleForm("HomeWork - Mihail Romanov", harnesses))
			{
				form.ShowDialog();
			}
		}
	}
}