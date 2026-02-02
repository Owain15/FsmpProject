

//using System;
//using System.IO;
using System.Media;
using WMPLib;

namespace FsmpLibrary
{

    public static class Fsmp
    {
        public static string TestLink()
        { return "FsmpLibrary Test String"; }

        public static void CheckFileLocation(string filePath)
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(filePath);
            
            // Example: play if the file is a .wav
            var artists = dir.GetDirectories();

            var albums = artists[0].GetDirectories();

            var tracks = albums[0].GetFiles();

            var trackInfo = tracks[0];


			// Play .wma files using Windows Media Player COM interop
			if (trackInfo.Extension.Equals(".wma", StringComparison.OrdinalIgnoreCase))
			{
				// Returns the WMPLib.WindowsMediaPlayer instance so caller can manage lifetime if needed
				var wmp = PlayWma(trackInfo.FullName);
				// Optionally: subscribe to events or stop later:
				// wmp.controls.stop();
			}
			else if (trackInfo.Extension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
			{
				PlayWav(trackInfo.FullName);
			}

		}

		public static void PlayWav(string wavPath)
		{
			if (!OperatingSystem.IsWindows())
				throw new PlatformNotSupportedException("SoundPlayer requires Windows. Use a cross-platform audio library for non-Windows runtimes.");

			if (string.IsNullOrWhiteSpace(wavPath))
				throw new ArgumentException("Path is null or empty.", nameof(wavPath));

			if (!File.Exists(wavPath))
				throw new FileNotFoundException("WAV file not found.", wavPath);

			try
			{
				using var player = new SoundPlayer(wavPath);
				player.Load();    // synchronous load - will throw if invalid
				player.Play();    // Play() returns immediately; use PlaySync() if you need blocking playback
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to play WAV file '{wavPath}'. See inner exception for details.", ex);
			}
		}

		public static WindowsMediaPlayer PlayWma(string wmaPath)
		{
			if (!OperatingSystem.IsWindows())
				throw new PlatformNotSupportedException("WMA playback requires Windows Media Player (Windows only).");

			if (string.IsNullOrWhiteSpace(wmaPath))
				throw new ArgumentException("Path is null or empty.", nameof(wmaPath));

			if (!File.Exists(wmaPath))
				throw new FileNotFoundException("WMA file not found.", wmaPath);

			var player = new WindowsMediaPlayer();
			player.URL = wmaPath;
			player.controls.play();
			return player;
		}

	};

	

	//public class Fsmp
 //   {

 //   }
}
