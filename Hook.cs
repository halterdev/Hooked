using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hooked
{
	public class Hook
	{
		// position of the hook, relative to top left of screen
		public Vector2 Position;

		// state of hook
		public bool Active;

		// get width of hook
		public int Width{
			get{ return Texture.Width; }
		}

		// get height of hook
		public		int Height{ get; set; }

		// speed that the hook moves
		float hookMoveSpeed;
		float hookSurfaceSpeed;

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
			if (GamePhysics.IsLargeScreen) {
				hookMoveSpeed = GamePhysics.HookSpeedLargeScreen;
			} else {
				hookMoveSpeed = GamePhysics.HookSpeed;
			}
			hookSurfaceSpeed = GamePhysics.HookSurfaceSpeed;
		}

		public void Update(GameTime gameTime)
		{
			// hook always moves to left, so decrement xposition
			Position.X -= hookMoveSpeed;
		}

		public void Update(bool collided)
		{
			Position.Y -= hookSurfaceSpeed;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (Active) {
				// draw the hook
				spriteBatch.Draw (Texture, Position, Color.White);
			}
		}

		int points{ get; set; }

		bool PointCollected{ get; set; }

		public bool Collides(Rectangle rect)
		{
			// if point hasnt already collected and hitbox isn't hit, Point time
			points = (!PointCollected && !HitBox.Intersects (rect)) ? 1 : 0;

			return rect.Intersects (Rectangle);
		}

		public int CollectPoints()
		{
			if (points > 0) {
				PointCollected = true;
			}
			return points;
		}

		// hitbox of hook
		public Rectangle Rectangle
		{
			get{ if (GamePhysics.IsLargeScreen) {
					return new Rectangle ((int)Position.X + 15, (int)Position.Y + 60, (int)(Width * .7), Height + 60);
				} else {
					return new Rectangle ((int)Position.X + 15, (int)Position.Y + 30, (int)(Width * .7), Height + 30);
				}}
		}
	}
}

