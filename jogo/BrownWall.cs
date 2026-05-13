using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

#nullable disable
namespace jogo;

public class BrownWall
{
    private Texture2D _texture;

    [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Vector2 Position { get; private set; }

    [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Width { get; private set; }

    [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Height { get; private set; }

    public Rectangle Bounds
    {
        get => new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);
    }

    public BrownWall(Vector2 position, int width, int height)
    {
        this.Position = position;
        this.Width = width;
        this.Height = height;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        this._texture = new Texture2D(graphicsDevice, 1, 1);
        this._texture.SetData<Color>(new Color[1]
        {
      Color.SaddleBrown
        });
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 cameraOffset)
    {
        if (this._texture == null)
            return;
        Rectangle destinationRectangle = new Rectangle((int)((double)this.Position.X - (double)cameraOffset.X), (int)((double)this.Position.Y - (double)cameraOffset.Y), this.Width, this.Height);
        spriteBatch.Draw(this._texture, destinationRectangle, Color.White);
    }
}
