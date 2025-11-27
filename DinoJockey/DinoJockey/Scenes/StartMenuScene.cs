using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Settings;

namespace DinoJockey.Scenes;

public class StartMenuScene : Scene
{
    private SpriteFont _titleFont;
    private SpriteFont _font;
    private Texture2D _pixel;

    private Rectangle _btnPlay;
    private Rectangle _btnExit;
    private Rectangle _btnSettings;

    private MouseInfo _mouse => Core.Input.Mouse;

    public override void LoadContent()
    {
        _titleFont = Core.Content.Load<SpriteFont>("fonts/titleFont");
        _font = Core.Content.Load<SpriteFont>("fonts/mainFont");
        _pixel = new Texture2D(Core.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        var vp = Core.GraphicsDevice.Viewport;
        int centerX = vp.Width / 2;
        int centerY = vp.Height / 2;

        _btnPlay = new Rectangle(centerX - 150, centerY - 40, 300, 80);
        // Cruz roja a la izquierda (salir)
        _btnExit = new Rectangle(20, 20, 60, 60);
        // Engranaje arriba a la derecha (configuración)
        _btnSettings = new Rectangle(vp.Width - 80, 20, 60, 60);

        // Mostrar mouse en los menús
        Core.Instance.IsMouseVisible = true;

        // Aplicar resolución/volúmenes por si cambiaron en Settings
        GameSettings.ApplyResolution();
        GameSettings.SetMusicVolume(GameSettings.MusicVolume);
        GameSettings.SetSfxVolume(GameSettings.SfxVolume);
    }

    public override void Update(GameTime gameTime)
    {
        if (WasClicked(_btnPlay))
        {
            Core.ChangeScene(new GameScene());
            return;
        }
        if (WasClicked(_btnExit))
        {
            Core.Instance.Exit();
            return;
        }
        if (WasClicked(_btnSettings))
        {
            Core.ChangeScene(new SettingsMenuScene());
            return;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.GraphicsDevice.Clear(Color.Black);
        var sb = Core.SpriteBatch;
        sb.Begin();

        var vp = Core.GraphicsDevice.Viewport;
        string title = "Dino Jockey For Two";
        Vector2 tsize = _titleFont.MeasureString(title);
        sb.DrawString(_titleFont, title, new Vector2(vp.Width/2f - tsize.X/2f, 100), Color.White);

        DrawButton(_btnPlay, "Jugar");

        // Dibujar cruz roja (placeholder). Reemplazar con imagen:
        // Comentario: Para usar imagen de cruz, cargá un Texture2D y dibújalo dentro de _btnExit.
        DrawBox(_btnExit, Color.DarkRed);
        sb.DrawString(_font, "X", new Vector2(_btnExit.X + 20, _btnExit.Y + 10), Color.White);

        // Dibujar engranaje (placeholder). Reemplazar con imagen:
        // Comentario: Para usar imagen de engranaje, cargá un Texture2D y dibújalo dentro de _btnSettings.
        DrawBox(_btnSettings, Color.DarkSlateGray);
        sb.DrawString(_font, "A", new Vector2(_btnSettings.X + 18, _btnSettings.Y + 10), Color.White);

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
        // borde
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), Color.White);
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom-2, rect.Width, 2), Color.White);
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), Color.White);
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.Right-2, rect.Y, 2, rect.Height), Color.White);
    }

    private bool IsHover(Rectangle rect)
    {
        return rect.Contains(_mouse.Position);
    }

    private bool WasClicked(Rectangle rect)
    {
        return rect.Contains(_mouse.Position) && _mouse.WasButtonJustPressed(MouseButton.Left);
    }
}
