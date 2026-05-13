using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace jogo
{
    public class RangedEnemyBullet
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public int Damage { get; set; } = 5;
        public bool IsActive { get; set; } = true;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, 10, 10);

        public RangedEnemyBullet(Vector2 position, Vector2 direction)
        {
            Position = position;
            if (direction != Vector2.Zero)
                direction.Normalize();
            Velocity = direction * 4f; // velocidade da bala
        }

        public void Update()
        {
            Position += Velocity;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (texture != null)
            {
                spriteBatch.Draw(texture, Position, Color.White);
            }
            else
            {
                Texture2D debug = new Texture2D(spriteBatch.GraphicsDevice, 10, 10);
                Color[] data = new Color[10 * 10];
                for (int i = 0; i < data.Length; ++i) data[i] = Color.Purple;
                debug.SetData(data);
                spriteBatch.Draw(debug, Position, Color.White);
            }
        }
    }
}
