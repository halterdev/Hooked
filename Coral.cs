using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hooked
{
	public class Coral
	{
		// position of the coral, relative to top left of screen
		public Vector2 Position;

		// state of coral
		public bool Active;

		// get width of coral
		public int Width{
			get{ return Texture.Width; }
		}

		// get height of coral
		public		int Height{ get; set; }

		// speed that the hook moves
		float coralMoveSpeed;

		Texture2D Texture;
		Rectangle HitBox;

		public void Initialize(Texture2D texture, Rectangle hitBox)
		{
			Position = new Vector2 (hitBox.X, hitBox.Y);
			HitBox = hitBox;

			Texture = texture;

			// hook is active
			Active = true;

			// set how fast the hook moves
			coralMoveSpeed = GamePhysics.CoralSpeed;
		}

		public void Update(GameTime gameTime)
		{
			// hook always moves to left, so decrement xposition
			Position.X -= coralMoveSpeed;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (Active) {
				// draw the hook
				spriteBatch.Draw (Texture, Position, Color.White);
			}
		}
	}
}

