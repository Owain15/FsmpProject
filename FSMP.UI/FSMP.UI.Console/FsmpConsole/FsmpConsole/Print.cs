using System;
using System.Collections.Generic;
using System.Text;
using FsmpLibrary.Models;

namespace FsmpConsole
{
	public class Print
	{
		static string title = "-- FSMP Console --\n";

		static string separator = "-----------------\n";

		public static void NewDisplay()
		{
			Console.Clear();

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

		/// <summary>
		/// Writes a numbered selection menu with consistent formatting.
		/// Items are numbered 1..N, with a "0) Back" option (unless backLabel is null).
		/// </summary>
		public static void WriteSelectionMenu(
			TextWriter output,
			string title,
			IList<string> items,
			string prompt = "Select",
			string? backLabel = "Back")
		{
			ArgumentNullException.ThrowIfNull(output);
			ArgumentNullException.ThrowIfNull(title);
			ArgumentNullException.ThrowIfNull(items);

			output.WriteLine();                        // space
			output.WriteLine($"== {title} ==");        // title
			output.WriteLine();                        // space
			for (int i = 0; i < items.Count; i++)
				output.WriteLine($"  {i + 1}) {items[i]}");
			if (backLabel != null)
				output.WriteLine($"  0) {backLabel}");
			output.WriteLine();                        // space
			output.Write($"{prompt}: ");               // prompt
		}

		/// <summary>
		/// Writes a detail card with aligned label: value fields.
		/// </summary>
		public static void WriteDetailCard(
			TextWriter output,
			string title,
			IList<(string Label, string Value)> fields)
		{
			ArgumentNullException.ThrowIfNull(output);
			ArgumentNullException.ThrowIfNull(title);
			ArgumentNullException.ThrowIfNull(fields);

			output.WriteLine();                        // space
			output.WriteLine($"== {title} ==");        // title
			output.WriteLine();                        // space
			if (fields.Count == 0) return;

			int maxLabel = 0;
			foreach (var (label, _) in fields)
				if (label.Length > maxLabel) maxLabel = label.Length;

			foreach (var (label, value) in fields)
				output.WriteLine($"  {label.PadRight(maxLabel)} {value}");
		}

		/// <summary>
		/// Writes a read-only numbered data list (no selection prompt).
		/// </summary>
		public static void WriteDataList(
			TextWriter output,
			string title,
			IList<string> items,
			string emptyMessage = "No items.")
		{
			ArgumentNullException.ThrowIfNull(output);
			ArgumentNullException.ThrowIfNull(title);
			ArgumentNullException.ThrowIfNull(items);

			output.WriteLine();                        // space
			output.WriteLine($"== {title} ==");        // title
			output.WriteLine();                        // space
			if (items.Count == 0)
			{
				output.WriteLine($"  {emptyMessage}");
				return;
			}

			for (int i = 0; i < items.Count; i++)
				output.WriteLine($"  {i + 1,2}) {items[i]}");
		}

		/// <summary>
		/// Formats data into an aligned text table with headers.
		/// </summary>
		public static string FormatTable(List<string[]> rows, List<string> headers)
		{
			ArgumentNullException.ThrowIfNull(rows);
			ArgumentNullException.ThrowIfNull(headers);

			if (headers.Count == 0)
				return string.Empty;

			var colCount = headers.Count;
			var widths = new int[colCount];

			// Measure header widths
			for (int c = 0; c < colCount; c++)
				widths[c] = headers[c].Length;

			// Measure row widths
			foreach (var row in rows)
			{
				for (int c = 0; c < colCount && c < row.Length; c++)
				{
					var cellLen = (row[c] ?? "").Length;
					if (cellLen > widths[c])
						widths[c] = cellLen;
				}
			}

			var sb = new StringBuilder();

			// Header row
			for (int c = 0; c < colCount; c++)
			{
				if (c > 0) sb.Append("  ");
				sb.Append(headers[c].PadRight(widths[c]));
			}
			sb.AppendLine();

			// Separator
			for (int c = 0; c < colCount; c++)
			{
				if (c > 0) sb.Append("  ");
				sb.Append(new string('-', widths[c]));
			}
			sb.AppendLine();

			// Data rows
			foreach (var row in rows)
			{
				for (int c = 0; c < colCount; c++)
				{
					if (c > 0) sb.Append("  ");
					var cell = c < row.Length ? (row[c] ?? "") : "";
					sb.Append(cell.PadRight(widths[c]));
				}
				sb.AppendLine();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Renders a text-based progress bar.
		/// </summary>
		public static string FormatProgressBar(int current, int total, int width = 20)
		{
			if (total <= 0) return $"[{new string(' ', width)}] 0%";
			if (width <= 0) width = 20;

			var fraction = Math.Clamp((double)current / total, 0.0, 1.0);
			var filled = (int)Math.Round(fraction * width);
			var empty = width - filled;
			var percent = (int)Math.Round(fraction * 100);

			return $"[{new string('#', filled)}{new string(' ', empty)}] {percent}%";
		}

		/// <summary>
		/// Formats a track's metadata as a card-style display.
		/// </summary>
		public static string FormatMetadataCard(Track track)
		{
			ArgumentNullException.ThrowIfNull(track);

			var sb = new StringBuilder();
			sb.AppendLine($"  Title:    {track.DisplayTitle}");
			sb.AppendLine($"  Artist:   {track.DisplayArtist}");
			sb.AppendLine($"  Album:    {track.DisplayAlbum}");

			if (track.Duration.HasValue)
				sb.AppendLine($"  Duration: {track.Duration.Value:mm\\:ss}");

			if (track.BitRate.HasValue)
				sb.AppendLine($"  BitRate:  {track.BitRate} kbps");

			if (track.Rating.HasValue)
				sb.AppendLine($"  Rating:   {new string('*', track.Rating.Value)}/{new string('-', 5 - track.Rating.Value)}");

			sb.AppendLine($"  Plays:    {track.PlayCount}");

			if (track.IsFavorite)
				sb.AppendLine("  Favorite: Yes");

			return sb.ToString();
		}
	}
}
