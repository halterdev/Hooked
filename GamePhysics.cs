using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hooked
{
	static class GamePhysics
	{
		public const double PlayerJumpLength = 500;
		public const double PlayerJumpHeight = -10;
		public const double PlayerFallSpeed = .6;

		// milliseconds
		public const double MinimumHookSpawnRate = 2000;
		public const double MinimumWormSpawnRate = 2000;

		// used to gradually make it more difficult
		public const double StartHookSpawnRate = 4000;

		public const double StartWormSpawnRate = 5000;

		public const float HookSpeed = 2.5f;
		public const float WormSpeed = 2.5f;

		// pixels
		public const int TopOffset = 25;

		// strings
		public const string TapToSwimString = "Tap to Start Swimming!";
		public const string ScoreString = "Score";
		public const string HighScoreString = "High Score";
	}
}

