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
		Score
	}

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
		Texture2D playerTexture, hookTexture, wormTexture, energyTexture;
		Texture2D insideEnergyTexture;

		List<Hook> hooks = new List<Hook>();
		List<Worm> worms = new List<Worm>();
		double hookSpanTime, previousHookSpanTime;
		double wormSpanTime, previousWormSpanTime;

		// random number generator
		Random random = new Random();

		// players score
		int score;
		float energy;
		int hookHeight;

		bool doneIncreasingSpawn;


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

			previousTouches = new TouchCollection ();
			currentTouches = new TouchCollection ();
			base.Initialize ();
		}

		/// <summary>
		/// Load your graphics content.
		/// </summary>
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be use to draw textures.
			spriteBatch = new SpriteBatch (graphics.GraphicsDevice);

			playerTexture = Content.Load<Texture2D> ("Fish-1");
			player.Initialize (playerTexture, new Vector2 (graphics.GraphicsDevice.Viewport.Width / 5, graphics.GraphicsDevice.Viewport.Height / 2));

			hookTexture = Content.Load<Texture2D> ("Fishhook");
			wormTexture = Content.Load<Texture2D> ("worm-1");
			energyTexture = Content.Load<Texture2D> ("HealthBar-1");

			insideEnergyTexture = new Texture2D (GraphicsDevice, energyTexture.Width, energyTexture.Height, false, SurfaceFormat.Color);
			Color[] colorData = new Color[energyTexture.Width * energyTexture.Height];

			for (int i = 0; i < energyTexture.Width * energyTexture.Height; i++) {
				colorData [i] = Color.LightGreen;
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

			doneIncreasingSpawn = false;
			player.Active = true;
			player.Health = 1;
			score = 0;
			energy = 1.0f;
			hooks.Clear ();
			worms.Clear ();

			var playerPosition = new Vector2 (graphics.GraphicsDevice.Viewport.Width / 5, graphics.GraphicsDevice.Viewport.Height / 2);
			player.Position = playerPosition;

			Color[] colorData = new Color[energyTexture.Width * energyTexture.Height];

			for (int i = 0; i < energyTexture.Width * energyTexture.Height; i++) {
				colorData [i] = Color.LightGreen;
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
			player.Update (gameTime, shouldSwim, hookHeight + 1, State == GameState.Menu);

			if (State != GameState.Score) {

			}

			if (State == GameState.Playing) {
				UpdateHooks (gameTime);
				UpdateWorms (gameTime);
				UpdateCollision ();
			} else if (State == GameState.Score) {
				UpdateGameOver (gameTime);
				if (gameOverAnimationDuration <= gameOverTimer && Toggled ()) {
					Reset ();
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
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);

			spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

			hooks.ForEach (x => x.Draw (spriteBatch));
			worms.ForEach (x => x.Draw (spriteBatch));

			Color energyColor = Color.LightGreen;
			if (energy < .25f) {
				energyColor = Color.Red;
				Color[] colorData = new Color[energyTexture.Width * energyTexture.Height];

				for (int i = 0; i < energyTexture.Width * energyTexture.Height; i++) {
					colorData [i] = Color.Red;
				}
				insideEnergyTexture.SetData<Color> (colorData);
			}

			spriteBatch.Draw (insideEnergyTexture, new Vector2 (GraphicsDevice.Viewport.Width - (energyTexture.Width - 30), GamePhysics.TopOffset - 10), null, 
				energyColor, 0, new Vector2 (0, 0), energy, SpriteEffects.None, 0);

			if (State == GameState.Menu) {
				// draw tap to swim memo before start
				spriteBatch.DrawString (font, GamePhysics.TapToSwimString,
					new Vector2 ((this.Window.ClientBounds.Width / 2) - GamePhysics.TapToSwimString.Length * 10, GamePhysics.TopOffset + 20), Color.White, 0,
					new Vector2 (0, 0), 1.8f, SpriteEffects.None, 0);
			}

			if (State == GameState.Playing) {
				// draw score if playing
				spriteBatch.DrawString (font, score.ToString (),
					new Vector2 (this.Window.ClientBounds.Width / 2 - score.ToString ().Length / 2, GamePhysics.TopOffset), Color.White, 0,
					new Vector2 (0, 0), 3.0f, SpriteEffects.None, 0);
			}

			if (State == GameState.Score) {

			}

			// draw the player
			player.Draw (spriteBatch);

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

		private void UpdateHooks(GameTime gameTime)
		{
			previousHookSpanTime += gameTime.ElapsedGameTime.TotalMilliseconds;
			if (previousHookSpanTime > hookSpanTime) {
				previousHookSpanTime = 0;
				//hookSpanTime -= 200;
				//hookSpanTime = Math.Max (hookSpanTime, GamePhysics.MinimumHookSpawnRate);
				// add hook
				AddHook ();
			}

			var deadHooks = new List<Hook> ();
			foreach (var hook in hooks) {
				hook.Update (gameTime);
				if (hook.Position.X < hook.Width / 2) {
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
				if (worm.Position.X < worm.Width / 2) {
					deadWorms.Add (worm);
				}
			}

			foreach (var worm in deadWorms) {
				worms.Remove (worm);
			}
		}

		public void UpdateCollision()
		{
			// determine if two objects are overlapping
			var rectangle1 = player.Rectangle;

			// if collision with any hook, dead
			foreach (var hook in hooks) {
				if (hook.Collides (rectangle1)) {
					gameOver ();
				}
			}

			// collision with worm, point and energy stuff
			bool wormEaten = false;
			var eatenWorms = new List<Worm> ();
			foreach (var worm in worms) {
				if (worm.Collides (rectangle1)) {
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
				energy = energy - .0003f;
			}

			if (energy <= 0) {
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
			State = GameState.Score;
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
		//testing testing
		#endregion
	}
}
