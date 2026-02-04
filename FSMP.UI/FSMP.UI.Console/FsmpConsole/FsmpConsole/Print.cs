using System;
using System.Collections.Generic;
using System.Text;

namespace FsmpConsole
{
	internal class Print
	{
		static string title = "-- FSMP Console --\n";

		static string separator = "-----------------\n";

		public static void NewDisplay()
		{
			try
			{
				Console.Clear();
			}
			catch (IOException)
			{
				// Console.Clear() fails when no console is attached (e.g., redirected output)
			}

			Console.WriteLine(title);

			Console.WriteLine(separator);
			
			Console.WriteLine("Current track : ");
			Console.WriteLine("Artist : ");
			Console.WriteLine("Album : ");
			Console.WriteLine("Is Playing : ");

			Console.WriteLine("Tracks in cue :");

			Console.WriteLine("\n"+separator);

			Console.WriteLine("Input : ");
		}
	}
}
