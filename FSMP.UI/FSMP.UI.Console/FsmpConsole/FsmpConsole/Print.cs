using System;
using System.Collections.Generic;
using System.Text;

namespace FsmpConsole
{
	internal class Print
	{
		static string title = "-- FSMP Console --\n\n";

		public static void NewDisplay()
		{
			Console.Clear();

			Console.WriteLine(title);
		}
	}
}
