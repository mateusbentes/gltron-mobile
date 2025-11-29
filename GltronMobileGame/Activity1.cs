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
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout
    )]
    public class Activity1 : Activity
    {
        private Game1 _game;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "Activity OnCreate start");
                base.OnCreate(bundle);

                _game = new Game1();
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
