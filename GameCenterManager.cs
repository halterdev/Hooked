using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.GameKit;
using Microsoft.Xna.Framework.GamerServices;

namespace Hooked
{
	public class GameCenterManager
	{
		private string leaderboardID;
		private SignedInGamer gamer;
		private long score;

		public GameCenterManager(string leaderboardID, long score)
		{
			this.leaderboardID = leaderboardID;
			gamer = GetSignedInGamer ();
			this.score = score;
		}

		public bool IsSignedIntoGameCenter()
		{
			if ((Gamer.SignedInGamers.Count > 0) && (Gamer.SignedInGamers [0].IsSignedInToLive)) {
				return true;
			}
			return false;
		}

		private SignedInGamer GetSignedInGamer()
		{
			if (IsSignedIntoGameCenter ()) {
				return Gamer.SignedInGamers [0];
			}
			return null;
		}

		public void SaveScoreToIosGameCenter()
		{
			if (IsSignedIntoGameCenter ()) {
				if (gamer != null) {
					gamer.UpdateScore (leaderboardID, score);
				}
			}
		}

		public GKLeaderboard GetLeaderboard()
		{
			GKLeaderboard leaderboard = new GKLeaderboard ();
			leaderboard.Category = GamePhysics.LeaderboardID;
			return leaderboard;
		}

		public bool Authenticate ()
		{
			//
			// This shows how to authenticate on both iOS 6.0 and older versions
			//



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
	}
}

