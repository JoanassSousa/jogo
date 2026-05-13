using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace jogo
{
    public class BossEnemy : Enemy
    {
    private Texture2D[] bossFrames;
    private int currentBossFrame = 0;

    private float animationTimer = 0f;
    private float animationSpeed = 0.2f;

    public void LoadBossFrames(Texture2D[] frames)
    {
        bossFrames = frames;
    }

    public void UpdateAnimation(GameTime gameTime)
    {
        animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (animationTimer >= animationSpeed)
        {
            currentBossFrame++;

            if (currentBossFrame >= bossFrames.Length)
                currentBossFrame = 0;

            animationTimer = 0f;
        }
    }

    public BossEnemy(Vector2 initialPosition)
      : base(initialPosition)
    {
        this.Speed = 0.0f;
        this.Health = 750;
    }

    public new Rectangle Bounds
    {
        get => new Rectangle((int)this.WorldPosition.X, (int)this.WorldPosition.Y, 60, 60);
    }

        public new void Draw(
            SpriteBatch spriteBatch,
            Vector2 playerScreenPosition,
            Vector2 playerWorldPosition)
        {
        Vector2 position = this.WorldPosition - playerWorldPosition + playerScreenPosition;

        spriteBatch.Draw(
            bossFrames[currentBossFrame],
            new Rectangle((int)position.X, (int)position.Y, 60, 60),
            Color.White
        );

        if (this.Health <= 0)
            return;

        Texture2D texture1 = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        texture1.SetData<Color>(new Color[1] { Color.White });

        int width1 = 60;
        int height = 6;
        int num1 = -12;

        Rectangle destinationRectangle1 =
            new Rectangle((int)position.X, (int)position.Y + num1, width1, height);

        spriteBatch.Draw(texture1, destinationRectangle1, Color.Red);

        float num2 = (float)this.Health / 750f;

        int width2 = (int)(width1 * num2);

        Rectangle destinationRectangle2 =
            new Rectangle((int)position.X, (int)position.Y + num1, width2, height);

        spriteBatch.Draw(texture1, destinationRectangle2, Color.DarkGreen);
        }
    }
}
