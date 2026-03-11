using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace FSMP.MAUI.Platforms.Android;

/// <summary>
/// Android foreground service that keeps audio playing when the app is backgrounded.
/// Shows a persistent notification with playback info.
/// </summary>
[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaPlayback)]
public class AudioPlaybackService : Service
{
    private const int NotificationId = 1;
    private const string ChannelId = "fsmp_playback";
    private const string ChannelName = "FSMP Playback";

    public override IBinder? OnBind(Intent? intent) => null;

    public override void OnCreate()
    {
        base.OnCreate();
        CreateNotificationChannel();
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var action = intent?.Action;

        switch (action)
        {
            case "STOP":
                StopForeground(StopForegroundFlags.Remove);
                StopSelf();
                return StartCommandResult.NotSticky;

            case "UPDATE":
                var title = intent?.GetStringExtra("Title") ?? "FSMP Music Player";
                var artist = intent?.GetStringExtra("Artist") ?? "Unknown Artist";
                var isPlaying = intent?.GetBooleanExtra("IsPlaying", true) ?? true;
                UpdateNotification(title, artist, isPlaying);
                return StartCommandResult.Sticky;

            default:
                // Start foreground with default notification
                StartForeground(NotificationId, BuildNotification("FSMP Music Player", "Playing...", true));
                return StartCommandResult.Sticky;
        }
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.Low)
            {
                Description = "Audio playback controls"
            };
            var manager = GetSystemService(NotificationService) as NotificationManager;
            manager?.CreateNotificationChannel(channel);
        }
    }

    private Notification BuildNotification(string title, string artist, bool isPlaying)
    {
        // Intent to open the app when notification is tapped
        var openIntent = new Intent(this, typeof(MainActivity));
        openIntent.SetFlags(ActivityFlags.SingleTop);
        var pendingOpen = PendingIntent.GetActivity(this, 0, openIntent, PendingIntentFlags.Immutable);

        // Stop action
        var stopIntent = new Intent(this, typeof(AudioPlaybackService));
        stopIntent.SetAction("STOP");
        var pendingStop = PendingIntent.GetService(this, 1, stopIntent, PendingIntentFlags.Immutable);

        var builder = new NotificationCompat.Builder(this, ChannelId);
        builder.SetContentTitle(title);
        builder.SetContentText(artist);
        builder.SetSmallIcon(Resource.Mipmap.appicon);
        builder.SetContentIntent(pendingOpen);
        builder.SetOngoing(true);
        builder.SetSilent(true);
        builder.AddAction(global::Android.Resource.Drawable.IcMediaPause, "Stop", pendingStop);
        builder.SetCategory(NotificationCompat.CategoryTransport);

        return builder.Build() ?? throw new InvalidOperationException("Failed to build notification");
    }

    private void UpdateNotification(string title, string artist, bool isPlaying)
    {
        var notification = BuildNotification(title, artist, isPlaying);
        var manager = GetSystemService(NotificationService) as NotificationManager;
        manager?.Notify(NotificationId, notification);
    }

    /// <summary>
    /// Start the foreground playback service.
    /// </summary>
    public static void Start(Context context, string? title = null, string? artist = null)
    {
        var intent = new Intent(context, typeof(AudioPlaybackService));
        if (title != null)
        {
            intent.SetAction("UPDATE");
            intent.PutExtra("Title", title);
            intent.PutExtra("Artist", artist ?? "Unknown Artist");
            intent.PutExtra("IsPlaying", true);
        }

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            context.StartForegroundService(intent);
        else
            context.StartService(intent);
    }

    /// <summary>
    /// Stop the foreground playback service.
    /// </summary>
    public static void Stop(Context context)
    {
        var intent = new Intent(context, typeof(AudioPlaybackService));
        intent.SetAction("STOP");
        context.StartService(intent);
    }
}
