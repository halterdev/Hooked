using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hooked
{
	static class GamePhysics
	{
		public static bool IsLargeScreen = false;

		// game center
		public const string LeaderboardID = "fishbait_leaderboard";

		// device constants
		public const int LargeScreenHeight = 1200;
		public const int LargeScreenFishHeight = 100;
		public const int FishHeight = 50;

		public const float ScaleScore = 3.0f;
		public const float ScaleScoreLarge = 6.0f;

		public const float EndGameFontScale = 1.8f;
		public const float LargeScreenEndGameFontScale = 2.5f;

		public const float ScoreScale = 3.0f;
		public const float LargeScreenScoreScale = 6.0f;

		public const int EndGameScoreStringY = 100;
		public const int LargeEndGameScoreStringY = 140;
		public const int EndGameCurrentScoreY = 70;
		public const int LargeEndGameCurrentScoreY = 105;
		public const int EndGameHighScoreStringY = 30;
		public const int LargeEndGameHighScoreStringY = 45;

		public const float TapToSwimStringScale = 1.8f;
		public const float LargeTapToSwimStringScale = 3.0f;
		public const float EatWormsStringScale = 2.0f;
		public const float LargeEatWormsStringscale = 4.0f; 

		public const double PlayerJumpLength = 500;
		public const double PlayerJumpHeight = -10;
		public const double PlayerFallSpeed = .6;

		// milliseconds
		public const double MinimumHookSpawnRate = 2000;
		public const double MinimumWormSpawnRate = 2000;

		public const int MinimumCoralSpawnRate = 2000;
		public const int MaximumCoralSpawnRate = 8000;

		// used to gradually make it more difficult
		public const double StartHookSpawnRate = 3500;
		public const double StartWormSpawnRate = 5000;

		public const double HookSpawnIncrease = 500;

		public const float EnergyGainedFromWorm = .12f;
		public const float EnergyLoss = .0005f;

		// how fast things move to left
		public const float StartHookSpeed = 2.5f;
		public const float LargeStartHookSpeed = 3.5f;

		public static float HookSpeed = 2.5f;
		public static float HookSpeedLargeScreen = 3.5f;
		public const int WhenToUpdateHookSpeed = 8;

		public const float WormSpeed = 2.5f;
		public const float WormSpeedLargeScreen = 3.5f;
		public const float CoralSpeed = 1.5f;

		public const float HookSurfaceSpeed = 2.5f;
		public const float PlayerSurfaceSpeed = 2.5f;

		// pixels
		public const int TopOffset = 25;

		// strings
		public const string TapToSwimString = "Tap to Start Swimming!";
		public const string EatWormsString = "Eat Worms to Survive!";
		public const string EnergyDeathString = "You Ran Out of Energy.";
		public const string EnergyDeathStringTwo = "Eat More Worms!";
		public const string ScoreString = "Score";
		public const string HighScoreString = "High Score";
		public const string SendHighScoreString = "@FishBaitApp";

		// score formatting
		public const int SingleDegitScoreOffset = 12;
		public const int DoubleDegitScoreOffset = 20;

		// ad stuff
		public const string AdMobID = "ca-app-pub-6337111060808844/8537149016";
	}
}

