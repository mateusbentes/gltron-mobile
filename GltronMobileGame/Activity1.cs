using Android.App;
using Android.Content.PM;
using Android.OS;

namespace GltronMobileGame
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen"
    )]
    public class Activity1 : Activity
    {
        private SimpleGame _game;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "Activity OnCreate start");
                base.OnCreate(bundle);

                // Set fullscreen
                Window?.SetFlags(Android.Views.WindowManagerFlags.Fullscreen, Android.Views.WindowManagerFlags.Fullscreen);
                
                // Create and run the simple game first
                _game = new SimpleGame();
                _game.Run();
                
                Android.Util.Log.Info("GLTRON", "Activity OnCreate complete");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Activity OnCreate failed: {ex}");
                Finish();
            }
        }

        protected override void OnPause()
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "Activity OnPause");
                base.OnPause();
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Activity OnPause error: {ex}");
            }
        }

        protected override void OnResume()
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "Activity OnResume");
                base.OnResume();
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Activity OnResume error: {ex}");
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "Activity OnDestroy");
                _game?.Dispose();
                base.OnDestroy();
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Activity OnDestroy error: {ex}");
            }
        }
    }
}
