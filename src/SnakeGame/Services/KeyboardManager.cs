using Microsoft.Xna.Framework.Input;

namespace SnakeGame.Services;

public sealed class KeyboardManager
{
    public KeyboardState PreviousState { get; private set; }
    public KeyboardState CurrentState { get; private set; }

    public void Update()
    {
        PreviousState = CurrentState;
        CurrentState = Keyboard.GetState();
    }

    public bool IsKeyPressed(params Keys[] keys)
    {
        foreach (var key in keys)
            if (CurrentState.IsKeyUp(key) || PreviousState.IsKeyDown(key))
                return false;
        return true;
    }

    public bool IsAnyKeyPressed(params Keys[] keys)
    {
        foreach (var key in keys)
            if (CurrentState.IsKeyDown(key) && PreviousState.IsKeyUp(key))
                return true;
        return false;
    }

    public bool IsKeyReleased(params Keys[] keys)
    {
        foreach (var key in keys)
            if (PreviousState.IsKeyUp(key) || CurrentState.IsKeyDown(key))
                return false;
        return true;
    }

    public bool IsAnyKeyReleased(params Keys[] keys)
    {
        foreach (var key in keys)
            if (PreviousState.IsKeyDown(key) && CurrentState.IsKeyUp(key))
                return true;
        return false;
    }

    public bool IsKeyDown(params Keys[] keys)
    {
        foreach (var key in keys)
            if (CurrentState.IsKeyUp(key))
                return false;
        return true;
    }

    public bool IsAnyKeyDown(params Keys[] keys)
    {
        foreach (var key in keys)
            if (CurrentState.IsKeyDown(key))
                return true;
        return false;
    }

    public bool IsKeyUp(params Keys[] keys)
    {
        foreach (var key in keys)
            if (CurrentState.IsKeyDown(key))
                return false;
        return true;
    }

    public bool IsAnyKeyUp(params Keys[] keys)
    {
        foreach (var key in keys)
            if (CurrentState.IsKeyUp(key))
                return true;
        return false;
    }

    public bool IsShiftPressed => IsAnyKeyPressed(Keys.LeftShift, Keys.RightShift);
    public bool IsCtrlPressed => IsAnyKeyPressed(Keys.LeftControl, Keys.RightControl);
    public bool IsAltPressed => IsAnyKeyPressed(Keys.LeftAlt, Keys.RightAlt);
    public bool IsWindowsPressed => IsAnyKeyPressed(Keys.LeftWindows, Keys.RightWindows);
    public bool IsNumLockStateEntered => CurrentState.NumLock && !PreviousState.NumLock;
    public bool IsCapsLockStateEntered => CurrentState.CapsLock && !PreviousState.CapsLock;

    public bool IsShiftReleased => IsKeyReleased(Keys.LeftShift, Keys.RightShift);
    public bool IsCtrlReleased => IsKeyReleased(Keys.LeftControl, Keys.RightControl);
    public bool IsAltReleased => IsKeyReleased(Keys.LeftAlt, Keys.RightAlt);
    public bool IsWindowsReleased => IsKeyReleased(Keys.LeftWindows, Keys.RightWindows);
    public bool IsNumLockStateExited => !CurrentState.NumLock && PreviousState.NumLock;
    public bool IsCapsLockStateExited => !CurrentState.CapsLock && PreviousState.CapsLock;

    public bool IsShiftDown => IsAnyKeyDown(Keys.LeftShift, Keys.RightShift);
    public bool IsCtrlDown => IsAnyKeyDown(Keys.LeftControl, Keys.RightControl);
    public bool IsAltDown => IsAnyKeyDown(Keys.LeftAlt, Keys.RightAlt);
    public bool IsWindowsDown => IsAnyKeyDown(Keys.LeftWindows, Keys.RightWindows);

    public bool IsShiftUp => IsKeyUp(Keys.LeftShift, Keys.RightShift);
    public bool IsCtrlUp => IsKeyUp(Keys.LeftControl, Keys.RightControl);
    public bool IsAltUp => IsKeyUp(Keys.LeftAlt, Keys.RightAlt);
    public bool IsWindowsUp => IsKeyUp(Keys.LeftWindows, Keys.RightWindows);

    public bool IsNumLockStateOn => CurrentState.NumLock;
    public bool IsCapsLockStateOn => CurrentState.CapsLock;
    public bool IsNumLockStateOff => !CurrentState.NumLock;
    public bool IsCapsLockStateOff => !CurrentState.CapsLock;
}