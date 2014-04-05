using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.GameKit;

namespace Hooked
{
	public class GameCenterManager
	{
		NSMutableDictionary earnedAchievementCache;

		public GameCenterManager ()
		{
		}

		public static bool isGameCenterAvailable()
		{
			return UIDevice.CurrentDevice.CheckSystemVersion (4, 1);
		}

		public GKLeaderboard reloadLeaderboard(string category)
		{
			GKLeaderboard leaderboard = new GKLeaderboard ();
			leaderboard.Category = category;
			leaderboard.TimeScope = GKLeaderboardTimeScope.AllTime;
			leaderboard.Range = new NSRange (1, 1);
			return leaderboard;

		}

		public void reportScore(long score, string category, MTGKTapperViewController controller)
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
				controller.updateHighScore();
			}));
		}
	}
}

