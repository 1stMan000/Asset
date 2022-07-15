using UnityEngine;

public static class CursorManager
{
    public static void SetCursor(bool on)
    {
        Cursor_Lock_State(on);
        Cursor.visible = on;
    }

    static void Cursor_Lock_State(bool on_Off_Switch)
    {
        if (on_Off_Switch)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
