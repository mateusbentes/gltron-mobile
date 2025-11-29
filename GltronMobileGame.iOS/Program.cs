using Foundation;
using UIKit;
using System;

namespace GltronMobileGame
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        private static Game1 game;

        internal static void RunGame()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: iOS - Creating Game1 instance...");
                game = new Game1();
                
                if (game == null)
                {
                    throw new InvalidOperationException("Game1 instance creation returned null");
                }
                
                System.Diagnostics.Debug.WriteLine("GLTRON: iOS - Starting game...");
                game.Run();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: iOS - Game initialization failed: {ex}");
                
                // Show error alert on iOS
                var alert = UIAlertController.Create(
                    "GLTron Mobile - Error", 
                    $"Game initialization failed:\n{ex.Message}\n\nPlease restart the application.", 
                    UIAlertControllerStyle.Alert
                );
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                
                // Get the root view controller to present the alert
                var window = UIApplication.SharedApplication.KeyWindow;
                var rootViewController = window?.RootViewController;
                rootViewController?.PresentViewController(alert, true, null);
                
                throw;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: iOS - Application starting...");
                UIApplication.Main(args, null, typeof(Program));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: iOS - Application startup failed: {ex}");
                throw;
            }
        }

        public override void FinishedLaunching(UIApplication app)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: iOS - FinishedLaunching called");
                RunGame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: iOS - FinishedLaunching failed: {ex}");
                throw;
            }
        }

        public override void OnActivated(UIApplication application)
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: iOS - OnActivated");
            base.OnActivated(application);
        }

        public override void OnResignActivation(UIApplication application)
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: iOS - OnResignActivation");
            base.OnResignActivation(application);
        }

        public override void DidEnterBackground(UIApplication application)
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: iOS - DidEnterBackground");
            base.DidEnterBackground(application);
        }

        public override void WillEnterForeground(UIApplication application)
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: iOS - WillEnterForeground");
            base.WillEnterForeground(application);
        }
    }
}
