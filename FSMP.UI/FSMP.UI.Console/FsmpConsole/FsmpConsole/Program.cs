using Microsoft.Extensions.DependencyInjection;
using FsmpConsole;
using FsmpLibrary.Audio;
using FsmpLibrary.Interfaces;
using FsmpLibrary.Services;



using var services = new ServiceCollection()
    .AddSingleton<IAudioPlayerFactory, LibVlcAudioPlayerFactory>()
    .AddSingleton<IAudioService, AudioService>()
    .BuildServiceProvider();


string musicRoot = GetMusicRoot();
var audioService = services.GetRequiredService<IAudioService>();


// Find and play the first available audio file
var firstTrack = FindFirstAudioFile(musicRoot);



while (true)
{ 
    Print.NewDisplay();
    
    var input = Console.ReadLine();
    
    if (string.IsNullOrEmpty(input)) continue;

    // Basic playback controls
    switch (input.ToLowerInvariant())
    {
        case "p":
        case "pause":
            await audioService.PauseAsync();
            break;
        case "r":
        case "resume":
            await audioService.ResumeAsync();
            break;
        case "s":
        case "stop":
            await audioService.StopAsync();
            break;
        case "q":
        case "quit":
            return;
    }
}

string GetMusicRoot()
{
    string solutionName = "FsmpProject";
    string testDataDir = @"\res\sampleMusic\Music";

    var dir = new DirectoryInfo(AppContext.BaseDirectory);

    while (dir != null)
    {
        if (dir.Name.Equals(solutionName, StringComparison.OrdinalIgnoreCase))
            return dir.FullName + testDataDir;

        if (dir.GetFiles("*.sln").Any())
            return dir.FullName + testDataDir;

        dir = dir.Parent;
    }

    throw new DirectoryNotFoundException("Could not find the solution directory.");
}

string? FindFirstAudioFile(string rootPath)
{
    if (!Directory.Exists(rootPath))
        return null;

    var supportedExtensions = new[] { ".wav", ".wma", ".mp3", ".flac", ".ogg", ".m4a" };

    return Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
        .FirstOrDefault(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));
}
