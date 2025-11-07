using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class Sprite
{
    protected TextureRegion Region { get; set; }
    public Color Tint { get; set; } = Color.White;
    private float Rotation { get; set; } = 0.0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    private SpriteEffects Effects { get; set; } = SpriteEffects.None;
    public float LayerDepth { get; set; }
    public float Width => Region.Width * Scale.X;
    public float Height => Region.Height * Scale.Y;
    protected Sprite() { }
    public Sprite(TextureRegion region)
    {
        Region = region;
    }
    public void CenterOrigin()
    {
        Origin = new Vector2(Region.Width, Region.Height) * 0.5f;
    }
    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        Region.Draw(spriteBatch, position, Tint, Rotation, Origin, Scale, Effects, LayerDepth);
    }
}
