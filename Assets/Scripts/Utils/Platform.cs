using UnityEngine;

public static class Platform
{
    public static bool IsWin32APIApplicable() {
        return Application.platform == RuntimePlatform.WindowsPlayer;
    }
}