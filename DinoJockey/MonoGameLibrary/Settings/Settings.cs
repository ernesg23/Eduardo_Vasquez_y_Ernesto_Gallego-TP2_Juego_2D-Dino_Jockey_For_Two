using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameLibrary.Settings;

public static class GameSettings
{
    public struct PlayerBinding
    {
        public Keys KeyboardKey;
        public PlayerIndex GamePadIndex;
        public Buttons GamePadButton;
    }

    // Resolución y pantalla
    public static int BackBufferWidth { get; private set; } = 1280; // default 1280x720
    public static int BackBufferHeight { get; private set; } = 720;
    public static bool IsFullScreen { get; private set; } = true;

    // Audio
    public static float MusicVolume { get; private set; } = 1.0f;
    public static float SfxVolume { get; private set; } = 1.0f;

    // Bindings por jugador
    public static PlayerBinding Player1 { get; private set; } = new PlayerBinding
    {
        KeyboardKey = Keys.Up,
        GamePadIndex = PlayerIndex.One,
        GamePadButton = Buttons.A
    };

    public static PlayerBinding Player2 { get; private set; } = new PlayerBinding
    {
        KeyboardKey = Keys.W,
        GamePadIndex = PlayerIndex.Two,
        GamePadButton = Buttons.A
    };

    public static void SetResolution(int width, int height, bool fullscreen)
    {
        // Mantener máximo 1366x768 y aspecto 16:9 para abajo
        width = MathHelper.Clamp(width, 640, 1366);
        height = MathHelper.Clamp(height, 360, 768);
        IsFullScreen = fullscreen;
        BackBufferWidth = width;
        BackBufferHeight = height;
    }

    public static void ApplyResolution()
    {
        Core.Graphics.PreferredBackBufferWidth = BackBufferWidth;
        Core.Graphics.PreferredBackBufferHeight = BackBufferHeight;
        Core.Graphics.IsFullScreen = IsFullScreen;
        Core.Graphics.ApplyChanges();
    }

    public static void SetMusicVolume(float value)
    {
        MusicVolume = MathHelper.Clamp(value, 0f, 1f);
        Core.Audio.SongVolume = MusicVolume;
    }

    public static void SetSfxVolume(float value)
    {
        SfxVolume = MathHelper.Clamp(value, 0f, 1f);
        Core.Audio.SoundEffectVolume = SfxVolume;
    }

    public static void SetPlayer1(Keys keyboard, PlayerIndex padIndex, Buttons padButton)
    {
        if (IsMouseLeftRightForbidden(keyboard)) return;
        Player1 = new PlayerBinding { KeyboardKey = keyboard, GamePadIndex = padIndex, GamePadButton = padButton };
    }

    public static void SetPlayer2(Keys keyboard, PlayerIndex padIndex, Buttons padButton)
    {
        if (IsMouseLeftRightForbidden(keyboard)) return;
        Player2 = new PlayerBinding { KeyboardKey = keyboard, GamePadIndex = padIndex, GamePadButton = padButton };
    }

    // Nota: Se excluyen únicamente los botones de mouse izquierdo/derecho para asignaciones.
    // Como estamos asignando Keys/Buttons (teclado/gamepad), esta validación aplica cuando en el futuro
    // se extienda a mouse; se deja placeholder aquí por claridad.
    private static bool IsMouseLeftRightForbidden(Keys key)
    {
        // No aplica a Keys; los botones de mouse no son Keys. Retorna siempre false.
        return false;
    }
}
