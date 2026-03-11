using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace FSMP.MAUI;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private const int RequestPermissionsCode = 1001;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        RequestAudioPermissions();
    }

    private void RequestAudioPermissions()
    {
        var permissionsToRequest = new System.Collections.Generic.List<string>();

        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            // Android 13+ uses granular media permissions
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadMediaAudio) != Permission.Granted)
                permissionsToRequest.Add(Manifest.Permission.ReadMediaAudio);
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) != Permission.Granted)
                permissionsToRequest.Add(Manifest.Permission.PostNotifications);
        }
        else
        {
            // Android 11-12 uses READ_EXTERNAL_STORAGE
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted)
                permissionsToRequest.Add(Manifest.Permission.ReadExternalStorage);
        }

        if (permissionsToRequest.Count > 0)
        {
            ActivityCompat.RequestPermissions(this, permissionsToRequest.ToArray(), RequestPermissionsCode);
        }
    }
}
