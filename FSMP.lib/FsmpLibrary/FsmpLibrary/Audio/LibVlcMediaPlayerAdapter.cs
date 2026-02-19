using FSMP.Core.Audio;
using System.Runtime.InteropServices;
using LibVLCSharp.Shared;

namespace FsmpLibrary.Audio;

/// <summary>
/// Thin adapter wrapping LibVLCSharp types. Contains no business logic —
/// just delegates to LibVLC and forwards events with simplified signatures.
/// </summary>
public class LibVlcMediaPlayerAdapter : IMediaPlayerAdapter
{
    private static bool _coreInitialized;
    private static readonly object _initLock = new();

    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    private Media? _currentMedia;
    private bool _disposed;

    public LibVlcMediaPlayerAdapter()
    {
        lock (_initLock)
        {
            if (!_coreInitialized)
            {
                var archFolder = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.Arm64 => "win-arm64",
                    Architecture.X86 => "win-x86",
                    _ => "win-x64"
                };
                var libvlcPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "libvlc", archFolder));
                var pluginsPath = Path.Combine(libvlcPath, "plugins");
                Environment.SetEnvironmentVariable("VLC_PLUGIN_PATH", pluginsPath);
                Core.Initialize(libvlcPath);
                _coreInitialized = true;
            }
        }

        _libVLC = new LibVLC("--quiet");
        _mediaPlayer = new MediaPlayer(_libVLC);

        _mediaPlayer.Playing += (s, e) => Playing?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.Paused += (s, e) => Paused?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.Stopped += (s, e) => Stopped?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.EndReached += (s, e) => EndReached?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.EncounteredError += (s, e) => EncounteredError?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.TimeChanged += (s, e) => TimeChanged?.Invoke(this, e.Time);
    }

    public bool HasMedia => _currentMedia != null;
    public long TimeMs { get => _mediaPlayer.Time; set => _mediaPlayer.Time = value; }
    public long DurationMs => _currentMedia?.Duration ?? 0;
    public int Volume { get => _mediaPlayer.Volume; set => _mediaPlayer.Volume = value; }
    public bool Mute { get => _mediaPlayer.Mute; set => _mediaPlayer.Mute = value; }

    public event EventHandler? Playing;
    public event EventHandler? Paused;
    public event EventHandler? Stopped;
    public event EventHandler? EndReached;
    public event EventHandler? EncounteredError;
    public event EventHandler<long>? TimeChanged;

    public void Play() => _mediaPlayer.Play();
    public void Pause() => _mediaPlayer.Pause();
    public void Stop() => _mediaPlayer.Stop();

    public void SetMedia() => _mediaPlayer.Media = _currentMedia;

    public void DisposeCurrentMedia()
    {
        _currentMedia?.Dispose();
        _currentMedia = null;
    }

    public async Task LoadMediaAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _currentMedia?.Dispose();
        _currentMedia = new Media(_libVLC, filePath, FromType.FromPath);
        await _currentMedia.Parse(MediaParseOptions.ParseLocal, cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _currentMedia?.Dispose();
        _mediaPlayer.Dispose();
        _libVLC.Dispose();
    }
}
