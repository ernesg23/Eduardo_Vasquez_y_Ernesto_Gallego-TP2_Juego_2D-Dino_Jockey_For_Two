using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Settings;
// using MonoGameGum; // Habilitar cuando integres Gum Runtime explícitamente

namespace DinoJockey.Scenes;

public class SettingsMenuScene : Scene
{
    private SpriteFont _titleFont;
    private SpriteFont _font;
    private Texture2D _pixel;

    private Rectangle _btnBack;

    // Volumen
    private Rectangle _btnMusicMinus;
    private Rectangle _btnMusicPlus;
    private Rectangle _btnSfxMinus;
    private Rectangle _btnSfxPlus;

    // Resolución y fullscreen
    private Rectangle _btnResPrev;
    private Rectangle _btnResNext;
    private Rectangle _btnFullscreenToggle;
    private readonly Point[] _resolutions = new Point[]
    {
        new Point(640, 360),
        new Point(854, 480),
        new Point(960, 540),
        new Point(1024, 576),
        new Point(1152, 648),
        new Point(1280, 720),
        new Point(1366, 768)
    };
    private int _resIndex = 5; // 1280x720 por defecto

    // Rebinding
    private Rectangle _btnP1Key;
    private Rectangle _btnP1Pad;
    private Rectangle _btnP2Key;
    private Rectangle _btnP2Pad;

    private enum RebindState { None, P1Key, P1Pad, P2Key, P2Pad }
    private RebindState _rebindState = RebindState.None;

    private MouseInfo _mouse => Core.Input.Mouse;

    public override void LoadContent()
    {
        _titleFont = Core.Content.Load<SpriteFont>("fonts/titleFont");
        _font = Core.Content.Load<SpriteFont>("fonts/mainFont");
        _pixel = new Texture2D(Core.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        var vp = Core.GraphicsDevice.Viewport;
        int centerX = vp.Width / 2;
        int y = 120;

        _btnBack = new Rectangle(20, 20, 140, 50);

        // Volumen controles
        _btnMusicMinus = new Rectangle(centerX - 200, y + 60, 50, 50);
        _btnMusicPlus = new Rectangle(centerX + 150, y + 60, 50, 50);
        _btnSfxMinus = new Rectangle(centerX - 200, y + 140, 50, 50);
        _btnSfxPlus = new Rectangle(centerX + 150, y + 140, 50, 50);

        // Resolución y fullscreen
        _btnResPrev = new Rectangle(centerX - 200, y + 260, 50, 50);
        _btnResNext = new Rectangle(centerX + 150, y + 260, 50, 50);
        _btnFullscreenToggle = new Rectangle(centerX - 70, y + 330, 140, 50);

        // Rebinding
        _btnP1Key = new Rectangle(centerX - 300, y + 420, 220, 50);
        _btnP1Pad = new Rectangle(centerX + 80, y + 420, 220, 50);
        _btnP2Key = new Rectangle(centerX - 300, y + 500, 220, 50);
        _btnP2Pad = new Rectangle(centerX + 80, y + 500, 220, 50);

        // Establecer índice de resolución según ajustes actuales
        var current = new Point(GameSettings.BackBufferWidth, GameSettings.BackBufferHeight);
        int idx = Array.FindIndex(_resolutions, p => p == current);
        if (idx >= 0) _resIndex = idx;

        // Mostrar mouse
        Core.Instance.IsMouseVisible = true;
    }

    public override void Update(GameTime gameTime)
    {
        // Si estamos esperando input para rebind
        if (_rebindState != RebindState.None)
        {
            HandleRebindInput();
            return;
        }

        // Back
        if (WasClicked(_btnBack))
        {
            Core.ChangeScene(new StartMenuScene());
            return;
        }

        // Music volume
        if (WasClicked(_btnMusicMinus)) GameSettings.SetMusicVolume(GameSettings.MusicVolume - 0.05f);
        if (WasClicked(_btnMusicPlus)) GameSettings.SetMusicVolume(GameSettings.MusicVolume + 0.05f);

        // SFX volume
        if (WasClicked(_btnSfxMinus)) GameSettings.SetSfxVolume(GameSettings.SfxVolume - 0.05f);
        if (WasClicked(_btnSfxPlus)) GameSettings.SetSfxVolume(GameSettings.SfxVolume + 0.05f);

        // Resolution
        if (WasClicked(_btnResPrev))
        {
            _resIndex = Math.Max(0, _resIndex - 1);
            ApplyResolutionFromIndex();
        }
        if (WasClicked(_btnResNext))
        {
            _resIndex = Math.Min(_resolutions.Length - 1, _resIndex + 1);
            ApplyResolutionFromIndex();
        }
        if (WasClicked(_btnFullscreenToggle))
        {
            GameSettings.SetResolution(GameSettings.BackBufferWidth, GameSettings.BackBufferHeight, !GameSettings.IsFullScreen);
            GameSettings.ApplyResolution();
        }

        // Rebinding
        if (WasClicked(_btnP1Key)) _rebindState = RebindState.P1Key;
        if (WasClicked(_btnP1Pad)) _rebindState = RebindState.P1Pad;
        if (WasClicked(_btnP2Key)) _rebindState = RebindState.P2Key;
        if (WasClicked(_btnP2Pad)) _rebindState = RebindState.P2Pad;
    }

    private void ApplyResolutionFromIndex()
    {
        var res = _resolutions[_resIndex];
        GameSettings.SetResolution(res.X, res.Y, GameSettings.IsFullScreen);
        GameSettings.ApplyResolution();
    }

    private void HandleRebindInput()
    {
        // Detectar tecla
        var kb = Core.Input.Keyboard;
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            if (kb.WasKeyJustPressed(key))
            {
                // Evitar asignaciones usando mouse izquierdo/derecho no aplica aquí porque son Keys
                if (_rebindState == RebindState.P1Key)
                    GameSettings.SetPlayer1(key, GameSettings.Player1.GamePadIndex, GameSettings.Player1.GamePadButton);
                else if (_rebindState == RebindState.P2Key)
                    GameSettings.SetPlayer2(key, GameSettings.Player2.GamePadIndex, GameSettings.Player2.GamePadButton);
                _rebindState = RebindState.None;
                return;
            }
        }

        // Detectar botón de gamepad: tomamos del índice configurado de cada jugador
        Buttons? pressed = GetAnyPressedButton(Core.Input.GamePads[(int)GameSettings.Player1.GamePadIndex]);
        if (_rebindState == RebindState.P1Pad && pressed.HasValue)
        {
            GameSettings.SetPlayer1(GameSettings.Player1.KeyboardKey, GameSettings.Player1.GamePadIndex, pressed.Value);
            _rebindState = RebindState.None; return;
        }
        pressed = GetAnyPressedButton(Core.Input.GamePads[(int)GameSettings.Player2.GamePadIndex]);
        if (_rebindState == RebindState.P2Pad && pressed.HasValue)
        {
            GameSettings.SetPlayer2(GameSettings.Player2.KeyboardKey, GameSettings.Player2.GamePadIndex, pressed.Value);
            _rebindState = RebindState.None; return;
        }
    }

    private static readonly Buttons[] ButtonScanOrder = new[]
    {
        Buttons.A, Buttons.B, Buttons.X, Buttons.Y,
        Buttons.LeftShoulder, Buttons.RightShoulder,
        Buttons.LeftStick, Buttons.RightStick,
        Buttons.Start, Buttons.Back,
        Buttons.DPadUp, Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight
    };

    private Buttons? GetAnyPressedButton(GamePadInfo pad)
    {
        if (pad == null || !pad.IsConnected) return null;
        foreach (var b in ButtonScanOrder)
        {
            if (pad.WasButtonJustPressed(b)) return b;
        }
        return null;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.GraphicsDevice.Clear(Color.Black);
        var sb = Core.SpriteBatch;
        sb.Begin();

        var vp = Core.GraphicsDevice.Viewport;
        //string title = "Configuración";
        //Vector2 tsize = _titleFont.MeasureString(title);
        //sb.DrawString(_titleFont, title, new Vector2(vp.Width/2f - tsize.X/2f, 40), Color.White);

        DrawButton(_btnBack, "Volver");

        int centerX = vp.Width / 2;
        int y = 120;

        // Música
        sb.DrawString(_font, $"Musica: {(int)(GameSettings.MusicVolume*100)}%", new Vector2(centerX - 120, y + 60), Color.White);
        DrawButton(_btnMusicMinus, "-");
        DrawButton(_btnMusicPlus, "+");

        // SFX
        sb.DrawString(_font, $"SFX: {(int)(GameSettings.SfxVolume*100)}%", new Vector2(centerX - 120, y + 140), Color.White);
        DrawButton(_btnSfxMinus, "-");
        DrawButton(_btnSfxPlus, "+");

        // Resolución
        var res = _resolutions[_resIndex];
        sb.DrawString(_font, $"Resolucion: {res.X}x{res.Y}", new Vector2(centerX - 120, y + 260), Color.White);
        DrawButton(_btnResPrev, "<");
        DrawButton(_btnResNext, ">");

        // Fullscreen
        DrawButton(_btnFullscreenToggle, GameSettings.IsFullScreen ? "Pantalla Completa: ON" : "Pantalla Completa: OFF");

        // Rebinding
        sb.DrawString(_font, $"P1 Tecla: {GameSettings.Player1.KeyboardKey}", new Vector2(centerX - 300, y + 390), Color.White);
        DrawButton(_btnP1Key, _rebindState == RebindState.P1Key ? "Presione una tecla..." : "Cambiar Tecla P1");
        sb.DrawString(_font, $"P1 Gamepad: {GameSettings.Player1.GamePadButton}", new Vector2(centerX + 80, y + 390), Color.White);
        DrawButton(_btnP1Pad, _rebindState == RebindState.P1Pad ? "Presione un boton..." : "Cambiar Boton P1");

        sb.DrawString(_font, $"P2 Tecla: {GameSettings.Player2.KeyboardKey}", new Vector2(centerX - 300, y + 470), Color.White);
        DrawButton(_btnP2Key, _rebindState == RebindState.P2Key ? "Presione una tecla..." : "Cambiar Tecla P2");
        sb.DrawString(_font, $"P2 Gamepad: {GameSettings.Player2.GamePadButton}", new Vector2(centerX + 80, y + 470), Color.White);
        DrawButton(_btnP2Pad, _rebindState == RebindState.P2Pad ? "Presione un boton..." : "Cambiar Boton P2");

        // Notas para íconos e integración Gum
        // Para reemplazar los botones por imágenes (cruz/engranaje/sliders), cargá Texture2D con Core.Content.Load<Texture2D>("images/tuimagen")
        // y dibujalo dentro de los rectángulos de botón. Con MonoGameGum (FlatRedBall.Gum) ya referenciado, podés crear una pantalla Gum
        // y, en lugar de Draw/Update manuales, instanciar el runtime y enlazar los eventos de click. 

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
