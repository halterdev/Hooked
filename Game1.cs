
#region File Description
//-----------------------------------------------------------------------------
// SharkBaitGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.iAd;
using MonoTouch.GameKit;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endregion
namespace Hooked
{
	public enum GameState
	{
		Menu,
		Playing,
		Score,
		Paused
	}

	public enum Alignment{Center = 0, Left = 1, Right = 2, Top = 4, Bottom = 8}

	/// <summary>
	/// Default Project Template
	/// </summary>
	public class Game1 : Game
	{
		#region Fields

		public static GameState State{ get; set; }

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		// represents the player
		Player player;

		// represents the floor
		Floor floor;

		// keyboard states for key presses
		KeyboardState currentKeyboardState;
		KeyboardState previousKeyboardState;

		// gamepad states used to determine button presses
		GamePadState currentGamePadState;
		GamePadState previousGamePadState;

		TouchCollection previousTouches;
		TouchCollection currentTouches;

		SpriteFont font;
		Texture2D playerTexture, hookTexture, wormTexture, coralTexture;
		Texture2D insideEnergyTexture, energyTexture;
		Texture2D backgroundTexture, sandTexture, blackSquareTexture;

		SoundEffect belchSound, bubbleSound, reelSound, energyDeathSound;
		bool playSound, hasPlayedSound;

		List<Hook> hooks = new List<Hook>();
		List<Worm> worms = new List<Worm>();
		List<Coral> corals = new List<Coral>();
		double hookSpanTime, previousHookSpanTime;
		double wormSpanTime, previousWormSpanTime;
		int minimumCoralSpanTime, maxCoralSpanTime, coralSpanTime, previousCoralSpanTime;

		// random number generator
		Random random = new Random();

		// players score
		int score;
		int untilSpeedUpdate;

		float energy;
		int hookHeight;

		bool deadFromHook, deadFromEnergy, deadFromFloor, flippedForEnergyDeath;
		bool deadFromFloorDrawn;

		ADBannerView topAdView;
		ADBannerView adView;
		#endregion

		#region Initialization

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager(this) {
				#if __OUYA__
				SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LanscapeRight,
				#else
				SupportedOrientations = DisplayOrientation.Portrait,
				#endif
				IsFullScreen = true,
			} ;

			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Overridden from the base Game.Initialize. Once the GraphicsDevice is setup,
		/// we'll use the viewport to initialize some values.
		/// </summary>
		protected override void Initialize ()
		{
			State = GameState.Menu;

			hookHeight = Math.Min (GraphicsDevice.Viewport.Height, MaxHookheight);
			player = new Player ();
			floor = new Floor ();

			playSound = true;
			hasPlayedSound = false;
			untilSpeedUpdate = GamePhysics.WhenToUpdateHookSpeed;

			previousTouches = new TouchCollection ();
			currentTouches = new TouchCollection ();

			// ad stuff
			UIViewController view = this.Services.GetService (typeof(UIViewController)) as UIViewController;
			adView = new ADBannerView ();

			NSMutableSet nsM = new NSMutableSet ();
			nsM.Add (ADBannerView.SizeIdentifierPortrait);
			adView.RequiredContentSizeIdentifiers = nsM;

			// delegate for ad is loaded
			adView.AdLoaded += delegate {
				adView.Frame = new System.Drawing.RectangleF(0, (UIScreen.MainScreen.Bounds.Height - adView.Frame.Height), 
					adView.Frame.Width, adView.Frame.Height);
				adView.Hidden = false;
			};

			// delegate for failed ad receive
			adView.FailedToReceiveAd += delegate(object sender, AdErrorEventArgs e) {
				Console.WriteLine(e.Error);
				adView.Hidden = true;
			};

			// delegate for click on ad
			adView.ActionShouldBegin = delegate(ADBannerView banner, bool willLeaveApp) {
				// pause game here
				State = GameState.Paused;
				return true;
			};

			// delegate for ad interaction finished
			adView.ActionFinished += delegate {
				// continue game now
				State = GameState.Menu;
			};

			view.Add (adView);
			base.Initialize ();
		}

		/// <summary>
		/// Load your graphics content.
		/// </summary>
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be use to draw textures.
			spriteBatch = new SpriteBatch (graphics.GraphicsDevice);

			if (graphics.GraphicsDevice.Viewport.Height > GamePhysics.LargeScreenHeight) {
				GamePhysics.IsLargeScreen = true;

				playerTexture = Content.Load<Texture2D> ("fishlarge");
				player.DesiredHeight = GamePhysics.LargeScreenFishHeight;

				hookTexture = Content.Load<Texture2D> ("hooklarge");
				wormTexture = Content.Load<Texture2D> ("wormlarge");

			} else {
				playerTexture = Content.Load<Texture2D> ("fishdefault");
				player.DesiredHeight = GamePhysics.FishHeight;

				hookTexture = Content.Load<Texture2D> ("hookdefault");
				wormTexture = Content.Load<Texture2D> ("wormdefault");
			}
			player.Initialize (playerTexture, new Vector2 (graphics.GraphicsDevice.Viewport.Width / 5, graphics.GraphicsDevice.Viewport.Height / 2));

			sandTexture = Content.Load<Texture2D> ("Sand2");
//			floor.Initialize (sandTexture, new Rectangle(0, graphics.GraphicsDevice.Viewport.Bounds.Height - (int)((sandTexture.Height*2) + adView.Frame.Height), 
//				sandTexture.Width, sandTexture.Height));
			floor.Initialize (sandTexture, new Rectangle (0, graphics.GraphicsDevice.Viewport.Height - (int)(adView.Frame.Height * 3),
				graphics.GraphicsDevice.Viewport.Width, (int)adView.Frame.Height * 3));

			energyTexture = Content.Load<Texture2D> ("HealthBar-1");
			blackSquareTexture = Content.Load<Texture2D> ("blacksquare");

			if (graphics.GraphicsDevice.Viewport.Height > GamePhysics.LargeScreenHeight) {
				backgroundTexture = Content.Load<Texture2D> ("waterbackgroundbig");
			} else {
				backgroundTexture = Content.Load<Texture2D> ("waterbackground");
			}
			hookHeight = graphics.GraphicsDevice.Viewport.Height - (int)(adView.Frame.Height *3);

			coralTexture = Content.Load<Texture2D> ("pinkcoral25x25");

			belchSound = Content.Load<SoundEffect> ("burp");
			bubbleSound = Content.Load<SoundEffect> ("bubbles");
			reelSound = Content.Load<SoundEffect> ("reel");
			energyDeathSound = Content.Load<SoundEffect> ("energydeath");

			insideEnergyTexture = new Texture2D (GraphicsDevice, energyTexture.Width, energyTexture.Height, false, SurfaceFormat.Color);
			Color[] colorData = new Color[energyTexture.Width * energyTexture.Height];

			for (int i = 0; i < energyTexture.Width * energyTexture.Height; i++) {
				colorData [i] = Color.GreenYellow;
			}
			insideEnergyTexture.SetData<Color> (colorData);

			font = Content.Load<SpriteFont> ("gameFont");

			Reset ();
		}

		public void Reset()
		{
			State = GameState.Menu;
			hookSpanTime = GamePhysics.StartHookSpawnRate;
			wormSpanTime = GamePhysics.StartWormSpawnRate;

			minimumCoralSpanTime = GamePhysics.MinimumCoralSpawnRate;
			maxCoralSpanTime = GamePhysics.MaximumCoralSpawnRate;

			flippedForEnergyDeath = false;
			deadFromHook = false;
			deadFromEnergy = false;
			deadFromFloor = false;
			deadFromFloorDrawn = false;

			untilSpeedUpdate = GamePhysics.WhenToUpdateHookSpeed;
			GamePhysics.HookSpeed = GamePhysics.StartHookSpeed;
			GamePhysics.HookSpeedLargeScreen = GamePhysics.LargeStartHookSpeed;

			hasPlayedSound = false;
			playSound = true;

			player.Active = true;
			player.Health = 1;
			score = 0;
			energy = 1.0f;
			hooks.Clear ();
			worms.Clear ();
			corals.Clear ();

			var playerPosition = new Vector2 (graphics.GraphicsDevice.Viewport.Width / 5, graphics.GraphicsDevice.Viewport.Height / 2);
			player.Position = playerPosition;

			Color[] colorData = new Color[energyTexture.Width * energyTexture.Height];

			for (int i = 0; i < energyTexture.Width * energyTexture.Height; i++) {
				colorData [i] = Color.GreenYellow;
			}
			insideEnergyTexture.SetData<Color> (colorData);
		}

		#endregion

		#region Update and Draw

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			base.Update (gameTime);

			if (adView.BannerLoaded)
			{
				adView.Hidden = false;
			} else
			{
				adView.Hidden = true;
			}

			if (!(State == GameState.Paused)) {
				// save previous state of keyboard and gamepad to determine key/button presses
				previousGamePadState = currentGamePadState;
				previousKeyboardState = currentKeyboardState;
				previousTouches = currentTouches;

				// read current state of keyboard and gamepad and store it
				currentGamePadState = GamePad.GetState (PlayerIndex.One);
				currentKeyboardState = Keyboard.GetState ();
				currentTouches = TouchPanel.GetState ();

				// update player
				var shouldSwim = currentTouches.Any () || currentKeyboardState.IsKeyDown (Keys.Space) || currentGamePadState.IsButtonDown (Buttons.A);
				if (shouldSwim && State == GameState.Menu) {
					State = GameState.Playing;
				} else if (shouldSwim && State == GameState.Score) {
					shouldSwim = false;
				}

				// update player
				if (deadFromHook) {
					player.Update (true);
					foreach (var hook in hooks) {
						if (hook.Collides (player.Rectangle)) {
							hook.Update (deadFromHook);
						}
					}
				} else if (deadFromEnergy) {
					player.Update (true);
				} else if (deadFromFloor) {
					player.Update (true);
				} else {
					player.Update (gameTime, shouldSwim, graphics.GraphicsDevice.Viewport.Height - (int)(adView.Frame.Height *3), State == GameState.Menu);
				}
				
				if (State != GameState.Score) {

				}

				if (State == GameState.Playing) {
					UpdateHooks (gameTime);
					UpdateWorms (gameTime);
					UpdateCorals (gameTime);
					UpdateCollision ();

					if (score == 0 && !hasPlayedSound) {
						hasPlayedSound = true;
						bubbleSound.Play ();
					}
					else if (score != 0 && score % 3 == 0 && playSound) {
						playSound = false;
						bubbleSound.Play ();
					} else {
						if (score % 3 != 0) {
							playSound = true;
						}
						bubbleSound.Dispose ();
					}

				} else if (State == GameState.Score) {
					UpdateGameOver (gameTime);
					if (gameOverAnimationDuration <= gameOverTimer && Toggled ()) {
						Reset ();
					}
				}
			}
		}

		bool Toggled(Buttons button)
		{
			return previousGamePadState.IsButtonUp (button) && currentGamePadState.IsButtonDown (button);
		}

		bool Toggled(Keys key)
		{
			return previousKeyboardState.IsKeyUp (key) && currentKeyboardState.IsKeyDown (key);
		}

		bool ToggledTapped()
		{
			return !previousTouches.Any () && currentTouches.Any ();
		}

		bool Toggled()
		{
			return ToggledTapped () || Toggled (Buttons.A) || Toggled (Keys.Space);
		}

		/// <summary>
		/// This is called when the game should draw itself. 
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			// Clear the backbuffer
			graphics.GraphicsDevice.Clear (Color.Blue);

			spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

			spriteBatch.Draw (backgroundTexture, new Vector2 (0, 0), Color.White);

			floor.Draw (spriteBatch);

			hooks.ForEach (x => x.Draw (spriteBatch));
			worms.ForEach (x => x.Draw (spriteBatch));

			if (State == GameState.Menu) {
				// draw tap to swim memo before start
				Vector2 tapToSwimStringLength = font.MeasureString (GamePhysics.TapToSwimString);
				float tapToSwimScale;
				if (GamePhysics.IsLargeScreen) {
					tapToSwimScale = GamePhysics.LargeTapToSwimStringScale;
				} else {
					tapToSwimScale = GamePhysics.TapToSwimStringScale;
				}
				spriteBatch.DrawString (font, GamePhysics.TapToSwimString,
					new Vector2 ((this.Window.ClientBounds.Width / 2) - ((tapToSwimStringLength.X / 2) * tapToSwimScale), GamePhysics.TopOffset + 20), Color.Black, 0,
					new Vector2 (0, 0), tapToSwimScale, SpriteEffects.None, 0);
			}

			if (State == GameState.Playing) {
				// draw score if playing
				float scoreScale;
				if (GamePhysics.IsLargeScreen) {
					scoreScale = GamePhysics.ScaleScoreLarge;
				} else {
					scoreScale = GamePhysics.ScaleScore;
				}

				Vector2 scoreLength = font.MeasureString (score.ToString ());
				spriteBatch.DrawString (font, score.ToString (),
					new Vector2 (this.Window.ClientBounds.Width / 2 - ((scoreLength.X/2) * scoreScale), GamePhysics.TopOffset), Color.Black, 0,
					new Vector2 (0, 0), scoreScale, SpriteEffects.None, 0);

				// if beginning, show eat worm string
				float eatWormsScale;
				if (GamePhysics.IsLargeScreen) {
					eatWormsScale = GamePhysics.LargeEatWormsStringscale;
				} else {
					eatWormsScale = GamePhysics.EatWormsStringScale;
				}
				Vector2 eatWormStringSize = font.MeasureString (GamePhysics.EatWormsString);
				if (!(worms.Count > 0 || hooks.Count > 0)) {
					spriteBatch.DrawString (font, GamePhysics.EatWormsString,
						new Vector2 (this.Window.ClientBounds.Width / 2 - ((eatWormStringSize.X / 2) * eatWormsScale), this.Window.ClientBounds.Height / 2),
						Color.Red, 0, new Vector2 (0, 0), eatWormsScale, SpriteEffects.None, 0);
				}

				// handle the energy bar
				Color energyColor = Color.LightGreen;
				if (energy < .25f) {
					energyColor = Color.Red;
					Color[] colorData = new Color[energyTexture.Width * energyTexture.Height];

					for (int i = 0; i < energyTexture.Width * energyTexture.Height; i++) {
						colorData [i] = Color.Red;
					}
					insideEnergyTexture.SetData<Color> (colorData);
				} else {
					energyColor = Color.Green;
					Color[] colorData = new Color[energyTexture.Width * energyTexture.Height];

					for (int i = 0; i < energyTexture.Width * energyTexture.Height; i++) {
						colorData [i] = Color.GreenYellow;
					}
					insideEnergyTexture.SetData<Color> (colorData);
				}

				spriteBatch.Draw (insideEnergyTexture, new Vector2 (GraphicsDevice.Viewport.Width / 2 - ((energyTexture.Width / 2) * energy), GamePhysics.TopOffset - 10), null, 
					energyColor, 0, new Vector2 (0, 0), energy, SpriteEffects.None, 0);
			}
				
			if (State == GameState.Score) {
				// energy warning if died from energy
				if (deadFromEnergy) {

				}

				// show current score and high score
				float scoreFontScale;
				int scoreStringY, scoreY, highScoreStringY;
				if (GamePhysics.IsLargeScreen) {
					scoreFontScale = GamePhysics.LargeScreenEndGameFontScale;

					scoreStringY = GamePhysics.LargeEndGameScoreStringY;
					scoreY = GamePhysics.LargeEndGameCurrentScoreY;
					highScoreStringY = GamePhysics.LargeEndGameHighScoreStringY;
				} else {
					scoreFontScale = GamePhysics.EndGameFontScale;

					scoreStringY = GamePhysics.EndGameScoreStringY;
					scoreY = GamePhysics.EndGameCurrentScoreY;
					highScoreStringY = GamePhysics.EndGameHighScoreStringY;
				}

				Vector2 scoreStringVector = font.MeasureString (GamePhysics.ScoreString);
				Vector2 highScoreStringVector = font.MeasureString (GamePhysics.HighScoreString);
				Vector2 scoreVector = font.MeasureString (score.ToString ());
				Vector2 highscoreVector = font.MeasureString (HighScore.Current.ToString ());

				spriteBatch.DrawString(font, GamePhysics.ScoreString, 
					new Vector2((this.Window.ClientBounds.Width / 2 - ((scoreStringVector.X / 2) * scoreFontScale)), (this.Window.ClientBounds.Height / 2) - scoreStringY), Color.White, 0,
					new Vector2(0,0), scoreFontScale, SpriteEffects.None, 0);
				spriteBatch.DrawString (font, score.ToString (),
					new Vector2 ((this.Window.ClientBounds.Width / 2) - ((scoreVector.X / 2) * scoreFontScale), (this.Window.ClientBounds.Height / 2) - scoreY), Color.White, 0,
					new Vector2 (0, 0), scoreFontScale, SpriteEffects.None, 0);
				spriteBatch.DrawString (font, GamePhysics.HighScoreString,
					new Vector2 ((this.Window.ClientBounds.Width / 2 - ((highScoreStringVector.X / 2) * scoreFontScale)), (this.Window.ClientBounds.Height / 2) - highScoreStringY), Color.White, 0,
					new Vector2 (0, 0), scoreFontScale, SpriteEffects.None, 0);
				spriteBatch.DrawString (font, HighScore.Current.ToString (),
					new Vector2 ((this.Window.ClientBounds.Width / 2) - ((highscoreVector.X / 2) * scoreFontScale), (this.Window.ClientBounds.Height / 2)), Color.White, 0,
					new Vector2 (0, 0), scoreFontScale, SpriteEffects.None, 0);
			}

			// draw the player
			if (deadFromEnergy && !flippedForEnergyDeath) {
				player.Draw (spriteBatch, true);
			} else if (deadFromFloor) {
				if (!deadFromFloorDrawn) {
					deadFromFloorDrawn = true;
					player.Draw (spriteBatch, GraphicsDevice.Viewport.Height - ((int)(adView.Frame.Height * 3) + player.Height));
				} else {
					player.Draw (spriteBatch, false);
				}
			} else {
				player.Draw (spriteBatch);
			}
				

			spriteBatch.End ();

			//TODO: Add your drawing code here
			base.Draw (gameTime);
		}

		const int MaxHookheight = 920;
		private void AddHook()
		{
			var yPos = random.Next (GamePhysics.TopOffset + 10, GraphicsDevice.Viewport.Height - ((int)(adView.Frame.Height *3) + hookTexture.Height));
			var xPos = (GraphicsDevice.Viewport.Width + (hookTexture.Width / 2));
			var posRect = new Rectangle (xPos, yPos, hookTexture.Width, hookTexture.Height);

			var hook = new Hook ();
			hook.Initialize (hookTexture, posRect);

			hooks.Add (hook);
		}

		private void AddWorm()
		{
			var yPos = random.Next (0, GraphicsDevice.Viewport.Height - ((int)(adView.Frame.Height *3) + wormTexture.Height));
			var xPos = ((GraphicsDevice.Viewport.Width + wormTexture.Width)) + 10;
			var posRect = new Rectangle (xPos, yPos, wormTexture.Width, wormTexture.Height);

			var worm = new Worm ();
			worm.Initialize (wormTexture, posRect);

			worms.Add (worm);
		}

		private void AddCoral()
		{
			var yPos = (GraphicsDevice.Viewport.Height - 50) + coralTexture.Height;
			var xPos = GraphicsDevice.Viewport.Width + coralTexture.Width;
			var posRect = new Rectangle (xPos, yPos, coralTexture.Width, coralTexture.Height);

			var coral = new Coral ();
			coral.Initialize (coralTexture, posRect);

			corals.Add (coral);
		}

		private void UpdateHooks(GameTime gameTime)
		{
			previousHookSpanTime += gameTime.ElapsedGameTime.TotalMilliseconds;
			if (previousHookSpanTime > hookSpanTime) {
				previousHookSpanTime = 0;

				// add hook
				AddHook ();
			}

			var deadHooks = new List<Hook> ();
			foreach (var hook in hooks) {
				hook.Update (gameTime);
				if (hook.Position.X < (0 - hook.Position.X)) {
					deadHooks.Add (hook);
				}
			}

			foreach (var hook in deadHooks) {
				hooks.Remove (hook);
			}
		}

		private void UpdateWorms(GameTime gameTime)
		{
			// spawn worm
			previousWormSpanTime += gameTime.ElapsedGameTime.TotalMilliseconds;
			if (previousWormSpanTime > wormSpanTime) {
				previousWormSpanTime = 0;
				// add worm
				AddWorm ();
			}

			var deadWorms = new List<Worm> ();
			foreach (var worm in worms) {
				worm.Update (gameTime);
				if (worm.Position.X < (0 - worm.Position.X)) {
					deadWorms.Add (worm);
				}
			}

			foreach (var worm in deadWorms) {
				worms.Remove (worm);
			}
		}

		private void UpdateCorals(GameTime gameTime)
		{
			// spawn coral
			previousWormSpanTime += gameTime.ElapsedGameTime.TotalMilliseconds;
			coralSpanTime = random.Next (minimumCoralSpanTime, maxCoralSpanTime);
			if (previousWormSpanTime > coralSpanTime) {
				previousCoralSpanTime = 0;
				// add coral
				AddCoral ();
			}

			var deadCorals = new List<Coral> ();
			foreach (var coral in corals) {
				coral.Update (gameTime);
				if (coral.Position.X < -100) {
					deadCorals.Add (coral);
				}
			}

		}

		public void UpdateCollision()
		{
			// determine if two objects are overlapping
			var rectangle1 = player.Rectangle;
			var playerYBeforeRect = player.Position.Y;

			// if collision with any hook, dead
			foreach (var hook in hooks) {
				if (hook.Collides (rectangle1)) {
					deadFromHook = true;
					reelSound.Play ();
					gameOver ();
				}
			}

			// collision with worm, point and energy stuff
			bool wormEaten = false;
			var eatenWorms = new List<Worm> ();
			foreach (var worm in worms) {
				if (worm.Collides (rectangle1)) {
					belchSound.Play ();
					eatenWorms.Add (worm);

					score++;
					untilSpeedUpdate--;
					if (untilSpeedUpdate == 0)
					{
						GamePhysics.HookSpeed += .2f;
						untilSpeedUpdate = GamePhysics.WhenToUpdateHookSpeed;
					}

					wormEaten = true;

					if (hookSpanTime <= 1000) {
						if (hookSpanTime > GamePhysics.HookSpawnIncrease) {
							hookSpanTime -= 40;
						}
					} else {
						hookSpanTime -= GamePhysics.HookSpawnIncrease;
					}
				}
			}

			if (wormEaten) {
				energy = energy + GamePhysics.EnergyGainedFromWorm;
				if (energy > 1.0f) {
					energy = 1.0f;
				}
			} else {
				energy = energy - GamePhysics.EnergyLoss;
			}

			if (energy <= 0) {
				deadFromEnergy = true;
				energyDeathSound.Play ();
				gameOver ();
			}

			foreach (var worm in eatenWorms) {
				worms.Remove (worm);
			}

			if (rectangle1.Y == (graphics.GraphicsDevice.Viewport.Height - (rectangle1.Height + (int)(adView.Frame.Height *3)))) {
				deadFromFloor = true;
				energyDeathSound.Play ();
				gameOver ();
			}
		}

		double gameOverTimer = 0;
		Vector2 gameOverPosition = Vector2.Zero;
		Rectangle scoreBoardRect = Rectangle.Empty;
		double gameOverAnimationDuration = 500;
		void gameOver()
		{
			gameOverTimer = 0;
			player.Health = 0;
			player.Active = false;

			State = GameState.Score;
			HighScore.Current = score;
		}

		void UpdateGameOver(GameTime gameTime)
		{
			gameOverTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
			if (gameOverTimer > gameOverAnimationDuration + 10) {
				return;
			}
			var sin = (float)Math.Sin (gameOverTimer * .7 * Math.PI / gameOverAnimationDuration);
			var y = (int)((GraphicsDevice.Viewport.Height / 3) * sin);
			gameOverPosition.Y = y;
			scoreBoardRect.Y = GraphicsDevice.Viewport.Height - y - scoreBoardRect.Height;
		}
		#endregion
	}
}
