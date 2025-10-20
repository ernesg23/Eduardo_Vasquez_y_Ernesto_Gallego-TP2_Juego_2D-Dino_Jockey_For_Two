using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace MonoGameLibrary.Collider;

public class Box
{
    public Rectangle Bounds;

    public Box(Vector2 position, int width, int height)
    {
        this.Bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
    }

    public void MoveCentered(Vector2 position)
    {
        Bounds.X = (int)(position.X - Bounds.Width * 0.5f);
        Bounds.Y = (int)(position.Y - Bounds.Height * 0.5f);
    }


    public bool CollidesWith(Box other)
    {
        return Bounds.Intersects(other.Bounds);
    }
}
