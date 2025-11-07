using Microsoft.Xna.Framework.Input;

namespace MonoGameLibrary.Input;

public class KeyboardInfo
{
    private KeyboardState PreviousState { get; set; }

    private KeyboardState CurrentState { get; set; } = Keyboard.GetState();

    public void Update()
    {
        PreviousState = CurrentState;
        CurrentState = Keyboard.GetState();
    }
    public bool IsKeyDown(Keys key)
    {
        return CurrentState.IsKeyDown(key);
    }

    public bool IsKeyUp(Keys key)
    {
        return CurrentState.IsKeyUp(key);
    }

    public bool WasKeyJustPressed(Keys key)
    {
        return CurrentState.IsKeyDown(key) && PreviousState.IsKeyUp(key);
    }
    
    public bool WasKeyJustReleased(Keys key)
    {
        return CurrentState.IsKeyUp(key) && PreviousState.IsKeyDown(key);
    }

}
