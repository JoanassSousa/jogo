using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace jogo
{
    public class ExperienceGem
    {
        public Vector2 WorldPosition { get; set; }
        public int VisualLevel { get; private set; } // 1 for XP nível 1 a 2, 2 for XP acima
        public bool IsCollected { get; set; }
        public float XpAmount { get; private set; }

        public Rectangle Bounds => new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, 10, 10);

        public ExperienceGem(Vector2 position, int visualLevel, float xpAmount)
        {
            WorldPosition = position;
            VisualLevel = visualLevel;
            XpAmount = xpAmount;
            IsCollected = false;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerScreenPosition, Vector2 playerWorldPosition)
        {
            if (IsCollected) return;

            Vector2 screenPosition = WorldPosition - playerWorldPosition + playerScreenPosition;

            Texture2D texture = new Texture2D(spriteBatch.GraphicsDevice, 10, 10);
            Color gemColor = VisualLevel == 1 ? new Color(35, 79, 215) : new Color(35, 79, 215); // "#234fd7" and "#234fd7l" might just be variants of blue

            if (VisualLevel > 1)
            {
                // slightly different tint for visual info
                gemColor = new Color(50, 100, 255);
            }

            Color[] data = new Color[10 * 10];
            for (int i = 0; i < data.Length; ++i) data[i] = gemColor;
            texture.SetData(data);

            spriteBatch.Draw(texture, screenPosition, Color.White);
        }
    }
}