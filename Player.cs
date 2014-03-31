using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hooked
{
	public class Player
	{
		public Vector2 StartLocation { get; set;}

		// representing the player
		public Texture2D PlayerTexture;

		// relative to top left of screen
		public Vector2 Position;

		public bool Active;
		public int Health, Energy;

		// get width of player texture
		public int Width{ get; set;}

		// get height of player texture
		public int Height{ get; set; }

		// animation representing the player
		public Texture2D Texture;
		Vector2 DrawOffset;
		Vector2 Origin;
		float Scale = 1f;

		// height we always want player to be
		const int DesiredHeight = 72;

		// initialize the player
		public void Initialize(Texture2D animation, Vector2 position)
		{
			Texture = animation;
			Scale = DesiredHeight / Texture.Height;
			Width = (int)(Texture.Width * Scale);
			Height = (int)(Texture.Height * Scale);

			// set starting position around middle of screen, towards the back
			StartLocation = Position = position;

			// set player active
			Active = true;

			// set players health and energy
			Health = 1;
			Energy = 50;

			Origin = new Vector2 (Texture.Width / 2, Texture.Height / 2);
			DrawOffset = new Vector2 (Width / 2, Height / 2);
		}

		// hit box around the player
		public Rectangle Rectangle
		{
			get{return new Rectangle((int)Position.X, (int)Position.Y, (int)(Width * .65), Height);}
		}

		double jumpTimer = GamePhysics.PlayerJumpLength;
		double fallTimer = 0;
		bool isSwimming = false;

		// update player animation
		public void Update(GameTime gameTime, bool shouldSwim, float maxHeight, bool autoSwim)
		{
			jumpTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

			if (Position.X < -Width || Health <= 0 || Energy <= 0) {
				// player is no longer active
				Active = false;
			}

			if (shouldSwim && !autoSwim) {
				isSwimming = true;
				jumpTimer = 0;
			}

			if (Active && isSwimming) {
				var sin = Math.Sin (jumpTimer * .5 * Math.PI / GamePhysics.PlayerJumpLength);
				var height = (int)(GamePhysics.PlayerJumpHeight - GamePhysics.PlayerJumpHeight * sin);
				Position.Y += height;
				fallTimer = 0;
			} else {
				Position.Y += Convert.ToInt32 (GamePhysics.PlayerFallSpeed * gameTime.ElapsedGameTime.TotalMilliseconds);
				fallTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
			}
			rotation = autoSwim ? 0 : TurnToFace (rotation, Rotation (), TurnSpeed);
			Position.Y = MathHelper.Clamp (Position.Y, 0, maxHeight - Height);

			if (jumpTimer > GamePhysics.PlayerJumpLength) {
				isSwimming = false;
			}
			if (autoSwim && Position.Y >= StartLocation.Y) {
				isSwimming = true;
				jumpTimer = 0;
			}
		}

		// update player when energy expires
		public void Update(bool died)
		{
			Position.Y -= GamePhysics.PlayerSurfaceSpeed;
		}

		private static float TurnToFace(float currentAngle, float targetRotation, float turnSpeed)
		{
			float difference = WrapAngle (targetRotation - currentAngle);
			difference = MathHelper.Clamp (difference, -turnSpeed, turnSpeed);
			return WrapAngle (currentAngle + difference);
		}

		// returns an angle in radians between -Pi and Pi
		private static float WrapAngle(float radians)
		{
			while (radians < -MathHelper.Pi) {
				radians += MathHelper.TwoPi;
			}

			while (radians > MathHelper.Pi) {
				radians -= MathHelper.TwoPi;
			}
			return radians;
		}

		// draw the player
		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw (Texture, Position + DrawOffset, null, Color.White, rotation, Origin, Scale, SpriteEffects.None, 0);
		}

		// draw player flipped
		public void Draw(SpriteBatch spriteBatch, bool flip)
		{
			spriteBatch.Draw (Texture, Position + DrawOffset, null, Color.White, rotation, Origin, Scale, SpriteEffects.FlipVertically, 0);
		}

		const float TurnSpeed = 0.50f;
		float rotation = 0f;

		public float Rotation()
		{
			const float topAngle = -30;
			const float bottomAngle = 90;

			if (isSwimming) {
				return MathHelper.ToRadians (topAngle);
			}
			if (!Active || fallTimer > GamePhysics.PlayerJumpLength) {
				return MathHelper.ToRadians (bottomAngle);
			}
			if (fallTimer < GamePhysics.PlayerJumpLength) {
				var sin = (float)Math.Sin (fallTimer * .5 * Math.PI / GamePhysics.PlayerJumpLength);
				return MathHelper.ToRadians (sin * bottomAngle);
			}
			return 0;
		}
	}
}

