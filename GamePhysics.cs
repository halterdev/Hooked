using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hooked
{
	static class GamePhysics
	{
		public static bool IsLargeScreen = false;

		// device constants
		public const int LargeScreenHeight = 1200;
		public const int LargeScreenFishHeight = 100;
		public const int FishHeight = 50;

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

		// how fast things move to left
		public const float HookSpeed = 2.5f;
		public const float HookSpeedLargeScreen = 3.5f;
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

		// score formatting
		public const int SingleDegitScoreOffset = 21;
		public const int DoubleDegitScoreOffset = 20;

		// ad stuff
		public const string AdMobID = "ca-app-pub-6337111060808844/8537149016";
	}
}

