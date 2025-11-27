using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace DinoJockey.Scenes;

public class GameOverScene : Scene
{
    private readonly string _winnerText;

    private SpriteFont _titleFont;
    private SpriteFont _font;
    private Texture2D _pixel;

    private Rectangle _btnRetry;
    private Rectangle _btnMenu;

    private MouseInfo _mouse => Core.Input.Mouse;

    public GameOverScene(string winnerText)
    {
        _winnerText = string.IsNullOrWhiteSpace(winnerText) ? "Juego terminado" : winnerText;
    }

    public override void LoadContent()
    {
        _titleFont = Core.Content.Load<SpriteFont>("fonts/titleFont");
        _font = Core.Content.Load<SpriteFont>("fonts/mainFont");
        //if(_font == null) Core.Instance.Exit();
        _pixel = new Texture2D(Core.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        var vp = Core.GraphicsDevice.Viewport;
        int centerX = vp.Width / 2;
        int centerY = vp.Height / 2;

        _btnRetry = new Rectangle(centerX - 320, centerY + 60, 260, 60);
        _btnMenu  = new Rectangle(centerX + 60,  centerY + 60, 260, 60);

        Core.Instance.IsMouseVisible = true;
    }

    public override void Update(GameTime gameTime)
    {
        if (WasClicked(_btnRetry))
        {
            Core.ChangeScene(new GameScene());
            return;
        }
        if (WasClicked(_btnMenu))
        {
            Core.ChangeScene(new StartMenuScene());
            return;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.GraphicsDevice.Clear(Color.Black);
        var sb = Core.SpriteBatch;
        sb.Begin();

        var vp = Core.GraphicsDevice.Viewport;

        string title = "Fin del juego";
        Vector2 tsize = _titleFont.MeasureString(title);
        sb.DrawString(_titleFont, title, new Vector2(vp.Width/2f - tsize.X/2f, 100), Color.White);

        Vector2 wsize = _font.MeasureString(_winnerText);
        sb.DrawString(_font, _winnerText, new Vector2(vp.Width/2f - wsize.X/2f, vp.Height/2f - 40), Color.White);

        DrawButton(_btnRetry, "Reintentar");
        DrawButton(_btnMenu,  "Menu");

        sb.End();
    }

    private void DrawButton(Rectangle rect, string text)
    {
        DrawBox(rect, IsHover(rect) ? new Color(60,60,60) : new Color(40,40,40));
        Vector2 size = _font.MeasureString(text);
        Vector2 pos = new Vector2(rect.X + (rect.Width - size.X)/2f, rect.Y + (rect.Height - size.Y)/2f);
        Core.SpriteBatch.DrawString(_font, text, pos, Color.White);
    }

    private void DrawBox(Rectangle rect, Color color)
    {
        Core.SpriteBatch.Draw(_pixel, rect, color);
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), Color.White);
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom-2, rect.Width, 2), Color.White);
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), Color.White);
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.Right-2, rect.Y, 2, rect.Height), Color.White);
    }

    private bool IsHover(Rectangle rect) => rect.Contains(_mouse.Position);
    private bool WasClicked(Rectangle rect) => rect.Contains(_mouse.Position) && _mouse.WasButtonJustPressed(MouseButton.Left);
}
