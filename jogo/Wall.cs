using Microsoft.Xna.Framework;
using System.Diagnostics;

#nullable disable
namespace jogo;

public class Wall
{
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

    public Wall(Vector2 position, int width, int height)
    {
        this.Position = position;
        this.Width = width;
        this.Height = height;
    }
}
