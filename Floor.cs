using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hooked
{
	public class Floor
	{
		// position of floor
		public Vector2 Position;

		// represents the floor
		public Texture2D Texture;

		public int Width{ get; set; }
		public int Height{ get; set; }

		// initialize floor
		public void Initialize(Texture2D texture, Rectangle hitBox)
		{
			Position = new Vector2 (hitBox.X, hitBox.Y);
			Texture = texture;
		}

		// draw floor
		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw (Texture, Position, Color.White);
		}

		public bool Collides(Rectangle rect)
		{
			return rect.Intersects (Rectangle);
		}

		// hitbox of floor
		public Rectangle Rectangle
		{
			get{ return new Rectangle ((int)Position.X, (int)Position.Y, Width, Height); }
		}
	}
}

