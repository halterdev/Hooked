using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.GameKit;

namespace Hooked
{
	public class GameCenterManager
	{
		private static GameCenterManager instance = null;

		public GameCenterManager ()
		{
		}

		public static GameCenterManager getInstance()
		{
			if (instance == null) {
				instance = new GameCenterManager ();
			}
			return instance;
		}

		public static bool SetAuthenticatingUser()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0))
			{
				//
				// iOS 6.0 and newer
				//
				GKLocalPlayer.LocalPlayer.AuthenticateHandler = (ui, error) => {

					// If ui is null, that means the user is already authenticated,
					// for example, if the user used Game Center directly to log in

					if (ui != null)
						;//current.PresentModalViewController (ui, true);
					else
					{
						// Check if you are authenticated:
						var authenticated = GKLocalPlayer.LocalPlayer.Authenticated;
					}
					Console.Out.WriteLine("something");
				};
			}
			else
			{
				// Versions prior to iOS 6.0
				GKLocalPlayer.LocalPlayer.Authenticate ((err) => {
					Console.Out.WriteLine("something2");
				});
			}
			return true;
		}

		public static bool isGameCenterAvailable()
		{
			return UIDevice.CurrentDevice.CheckSystemVersion (6, 0);
		}

		public GKLeaderboard reloadLeaderboard(string category)
		{
			GKLeaderboard leaderboard = new GKLeaderboard ();
			leaderboard.Category = category;
			leaderboard.TimeScope = GKLeaderboardTimeScope.AllTime;
			leaderboard.Range = new NSRange (1, 1);
			return leaderboard;

		}

		public void reportScore(long score, string category)
		{
			GKScore scoreReporter = new GKScore (category);
			scoreReporter.Value = score;
			scoreReporter.ReportScore (new GKNotificationHandler ((error) => {
				if(error == null){
					new UIAlertView ("Score reported", "Score Reported successfully", null, "OK", null).Show ();
				}
				else{
					new UIAlertView ("Score Reported Failed", "Score Reported Failed", null, "OK", null).Show ();
				}
				NSThread.SleepFor(1);
				//controller.updateHighScore();
			}));
		}
	}
}

