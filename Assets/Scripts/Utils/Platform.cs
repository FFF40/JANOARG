using UnityEngine;

public static class Platform
{
    public static bool IsLinux() => Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer;
}