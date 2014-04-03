﻿
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
		Texture2D backgroundTexture, sandTexture;

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
		float energy;
		int hookHeight;

		bool deadFromHook;
		bool deadFromEnergy;
		bool flippedForEnergyDeath;

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

			playSound = true;
			hasPlayedSound = false;

			previousTouches = new TouchCollection ();
			currentTouches = new TouchCollection ();

			// ad stuff
			UIViewController view = this.Services.GetService (typeof(UIViewController)) as UIViewController;
			adView = new ADBannerView ();

			NSMutableSet nsM = new NSMutableSet ();
			nsM.Add (ADBannerView.SizeIdentifier320x50);
			adView.RequiredContentSizeIdentifiers = nsM;
			adView.Hidden = true;

			// delegate for ad is loaded
			adView.AdLoaded += delegate {
				adView.Frame = new System.Drawing.RectangleF(0,430, adView.Frame.Width, adView.Frame.Height);
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

			playerTexture = Content.Load<Texture2D> ("Fish85");
			player.Initialize (playerTexture, new Vector2 (graphics.GraphicsDevice.Viewport.Width / 5, graphics.GraphicsDevice.Viewport.Height / 2));

			hookTexture = Content.Load<Texture2D> ("Fishhook");
			wormTexture = Content.Load<Texture2D> ("worm-1");
			energyTexture = Content.Load<Texture2D> ("HealthBar-1");
			backgroundTexture = Content.Load<Texture2D> ("waterbackground");
			sandTexture = Content.Load<Texture2D> ("Sand");
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

			adView.Hidden = true;

			flippedForEnergyDeath = false;
			deadFromHook = false;
			deadFromEnergy = false;

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
					adView.Hidden = true;
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
				} else {
					player.Update (gameTime, shouldSwim, hookHeight + 1, State == GameState.Menu);
				}
				
				if (State != GameState.Score) {

				}

				if (State == GameState.Playing) {
					adView.Hidden = true;
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
			Vector2 textCenter = new Vector2 (GraphicsDevice.Viewport.Width / 2, 50f);

			// Clear the backbuffer
			graphics.GraphicsDevice.Clear (Color.Blue);

			spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

			spriteBatch.Draw (backgroundTexture, new Vector2 (0, 0), Color.White);
			spriteBatch.Draw(sandTexture, new Vector2(0, GraphicsDevice.Viewport.Height - sandTexture.Height), Color.White);

			hooks.ForEach (x => x.Draw (spriteBatch));
			worms.ForEach (x => x.Draw (spriteBatch));
			//corals.ForEach (x => x.Draw (spriteBatch));

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
				
			spriteBatch.Draw (insideEnergyTexture, new Vector2 (GraphicsDevice.Viewport.Width - (energyTexture.Width - 35), GamePhysics.TopOffset - 10), null, 
				energyColor, 0, new Vector2 (0, 0), energy, SpriteEffects.None, 0);

			if (State == GameState.Menu) {
				// draw tap to swim memo before start
				spriteBatch.DrawString (font, GamePhysics.TapToSwimString,
					new Vector2 ((this.Window.ClientBounds.Width / 2) - GamePhysics.TapToSwimString.Length * 10, GamePhysics.TopOffset + 20), Color.White, 0,
					new Vector2 (0, 0), 1.8f, SpriteEffects.None, 0);
//				Vector2 textSize = font.MeasureString(
//				spriteBatch.DrawString (font, GamePhysics.TapToSwimString, textCenter - (textSize / 2), Color.White, 0, 
//					new Vector2 (0, 0), 1.8f, SpriteEffects.None, 0);
			}

			if (State == GameState.Playing) {
				// draw score if playing
				spriteBatch.DrawString (font, score.ToString (),
					new Vector2 (this.Window.ClientBounds.Width / 2 - score.ToString ().Length / 2, GamePhysics.TopOffset), Color.White, 0,
					new Vector2 (0, 0), 3.0f, SpriteEffects.None, 0);

				// if beginning, show eat worm string
				if (!(worms.Count > 0 || hooks.Count > 0)) {
					spriteBatch.DrawString (font, GamePhysics.EatWormsString,
						new Vector2 (this.Window.ClientBounds.Width / 2 - (GamePhysics.EatWormsString.Length*10), this.Window.ClientBounds.Height / 2),
						Color.Red, 0, new Vector2 (0, 0), 2.0f, SpriteEffects.None, 0);
				}
			}
				
			if (State == GameState.Score) {
				// energy warning if died from energy
				if (deadFromEnergy) {
//					spriteBatch.DrawString(font, GamePhysics.EnergyDeathString, 
//						new Vector2((this.Window.ClientBounds.Width / 2 - (GamePhysics.EnergyDeathString.Length*6)), GamePhysics.TopOffset), Color.Red, 0,
//						new Vector2(0,0), 2f, SpriteEffects.None, 0);
				}

				// show current score and high score
				spriteBatch.DrawString(font, GamePhysics.ScoreString, 
					new Vector2((this.Window.ClientBounds.Width / 2 - (GamePhysics.ScoreString.Length + 40)), (this.Window.ClientBounds.Height / 2) - 100), Color.White, 0,
					new Vector2(0,0), 1.8f, SpriteEffects.None, 0);

				int currentScoreOffset, highScoreOffset;
				currentScoreOffset = highScoreOffset = GamePhysics.SingleDegitScoreOffset;
				if (score.ToString ().Length > 1) {
					currentScoreOffset = GamePhysics.DoubleDegitScoreOffset;
				}
				if (HighScore.Current.ToString ().Length > 1) {
					currentScoreOffset = GamePhysics.DoubleDegitScoreOffset;
				}

				spriteBatch.DrawString (font, score.ToString (),
					new Vector2 ((this.Window.ClientBounds.Width / 2 - currentScoreOffset), (this.Window.ClientBounds.Height / 2) - 70), Color.White, 0,
					new Vector2 (0, 0), 1.8f, SpriteEffects.None, 0);
				spriteBatch.DrawString (font, GamePhysics.HighScoreString,
					new Vector2 ((this.Window.ClientBounds.Width / 2 - (GamePhysics.HighScoreString.Length + 80)), (this.Window.ClientBounds.Height / 2) - 30), Color.White, 0,
					new Vector2 (0, 0), 1.8f, SpriteEffects.None, 0);
				spriteBatch.DrawString (font, HighScore.Current.ToString (),
					new Vector2 ((this.Window.ClientBounds.Width / 2 - highScoreOffset), (this.Window.ClientBounds.Height / 2)), Color.White, 0,
					new Vector2 (0, 0), 1.8f, SpriteEffects.None, 0);
			}

			// draw the player
			if (deadFromEnergy && !flippedForEnergyDeath) {
				player.Draw (spriteBatch, true);
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
			var yPos = random.Next (GamePhysics.TopOffset + 10, GraphicsDevice.Viewport.Height - hookTexture.Height);
			var xPos = (GraphicsDevice.Viewport.Width + hookTexture.Width);
			var posRect = new Rectangle (xPos, yPos, hookTexture.Width, hookTexture.Height);

			var hook = new Hook ();
			hook.Initialize (hookTexture, posRect);

			hooks.Add (hook);
		}

		private void AddWorm()
		{
			var yPos = random.Next (0, GraphicsDevice.Viewport.Height - wormTexture.Height);
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
					wormEaten = true;

					if (hookSpanTime <= 1000) {
						if (hookSpanTime > 500) {
							hookSpanTime -= 40;
						}
					} else {
						hookSpanTime -= 500;
					}
				}
			}

			if (wormEaten) {
				energy = energy + .12f;
				if (energy > 1.0f) {
					energy = 1.0f;
				}
			} else {
				energy = energy - .0004f;
			}

			if (energy <= 0) {
				deadFromEnergy = true;
				energyDeathSound.Play ();
				gameOver ();
			}

			foreach (var worm in eatenWorms) {
				worms.Remove (worm);
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
			adView.Hidden = false;
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
