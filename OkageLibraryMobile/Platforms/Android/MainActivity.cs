using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Runtime.Versioning;

namespace OkageLibraryMobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    [SupportedOSPlatform("android30.0")] // This is the "Full Media" access for newer Android versions. Required to load files from the 'Downloads' folder.
    protected override void OnCreate(Bundle savedInstanceState)
    {
        if (!Android.OS.Environment.IsExternalStorageManager)
        {
            Intent intent = new();
            intent.SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", this.PackageName, null);
            intent.SetData(uri);
            StartActivity(intent);
        }
        base.OnCreate(savedInstanceState);
    }
}
