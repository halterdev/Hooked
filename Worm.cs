using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hooked
{
	public class Worm
	{
		// location of worm
		public Vector2 Position;

		public bool Active;

		// represents worm
		Texture2D Texture;

		// width of worm
		public int Width
		{
			get { return Texture.Width; }
		}

		// height of hook
		public int Height
		{
			get { return Texture.Height; }
		}

		float wormMoveSpeed;

		public void Initialize(Texture2D texture, Rectangle rect)
		{
			Position = new Vector2 (rect.X + 100, rect.Y);
			Texture = texture;

			// worm is active
			Active = true;

			// set speed worm moves
			if (GamePhysics.IsLargeScreen) {
				wormMoveSpeed = GamePhysics.WormSpeedLargeScreen;
			} else {
				wormMoveSpeed = GamePhysics.WormSpeed;
			}
		}

		public void Update(GameTime gameTime)
		{
			// worm always moves to left
			Position.X -= wormMoveSpeed;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (Active) {
				spriteBatch.Draw (Texture, Position, Color.White);
			}
		}

		public bool Collides(Rectangle rect)
		{
			return rect.Intersects (Rectangle);
		}

		// hitbox of worm
		public Rectangle Rectangle
		{
			//get{return new Rectangle((int)Position.X, (int)Position.Y, (int)(Width * .9), Height);}
			get { return new Rectangle ((int)Position.X, (int)Position.Y, Width, Height); }
		}
	}
}

