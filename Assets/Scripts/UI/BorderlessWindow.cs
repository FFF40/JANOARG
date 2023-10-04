using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Diagnostics;
using UnityEngine.Rendering;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

public class BorderlessWindow
{
    public static bool framed = true;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out WinRect lpRect);
    
    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, WinMargin margin);

    private struct WinRect { public int left, top, right, bottom; }
    private struct WinMargin { public int left, right, top, bottom; }

    private const int GWL_STYLE = -16;

    private const int SW_MINIMIZE = 6;
    private const int SW_MAXIMIZE = 3;
    private const int SW_RESTORE = 9;

    private const uint WS_VISIBLE = 0x10000000;    
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_OVERLAPPED = 0x00000000;
    private const uint WS_CAPTION = 0x00C00000;
    private const uint WS_DLGFRAME = 0x00400000;
    private const uint WS_SYSMENU = 0x00080000;
    private const uint WS_THICKFRAME = 0x00040000; // WS_SIZEBOX
    private const uint WS_MINIMIZEBOX = 0x00020000;
    private const uint WS_MAXIMIZEBOX = 0x00010000;
    private const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void InitializeWindow()
    {
        Vector2Int screenSize = new(Screen.width, Screen.height);
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN 
            SetFramelessWindow();
            ResizeWindow(screenSize.x + 14, screenSize.y + 14);
        #endif
    }

    public static IntPtr GetMainWindow () {
        return FindWindowA(null, "JANOARG");
    }

    public static Rect GetWindowRect()
    {
        var hwnd = GetMainWindow();

        GetWindowRect(hwnd, out WinRect winRect);

        return new Rect(winRect.left, winRect.top, winRect.right - winRect.left, winRect.bottom - winRect.top);
    }

    public static void SetFramelessWindow()
    {
        var hwnd = GetMainWindow();
        SetWindowLong(hwnd, GWL_STYLE, WS_THICKFRAME | WS_VISIBLE);
        framed = false;
    }

    public static void SetFramedWindow()
    {
        var hwnd = GetMainWindow();
        SetWindowLong(hwnd, GWL_STYLE, WS_OVERLAPPEDWINDOW | WS_VISIBLE);
        framed = true;
    }

    public static void MinimizeWindow()
    {
        var hwnd = GetMainWindow();
        ShowWindow(hwnd, SW_MINIMIZE);
    }

    public static void MaximizeWindow()
    {
        var hwnd = GetMainWindow();
        ShowWindow(hwnd, SW_MAXIMIZE);
    }

    public static void RestoreWindow()
    {
        var hwnd = GetMainWindow();
        ShowWindow(hwnd, SW_RESTORE);
    }

    public static void MoveWindow(Vector2 pos)
    {
        var hwnd = GetMainWindow();

        GetWindowRect(hwnd, out WinRect winRect);

        MoveWindow(hwnd, (int)pos.x, (int)pos.y, winRect.right - winRect.left, winRect.bottom - winRect.top, false);
    }

    public static void MoveWindowDelta(Vector2 posDelta)
    {
        var hwnd = GetMainWindow();

        GetWindowRect(hwnd, out WinRect winRect);

        var x = winRect.left + (int)posDelta.x;
        var y = winRect.top - (int)posDelta.y;
        MoveWindow(hwnd, x, y, winRect.right - winRect.left, winRect.bottom - winRect.top, false);
    }

    public static void ResizeWindow(int width, int height)
    {
        var hwnd = GetMainWindow();

        GetWindowRect(hwnd, out WinRect winRect);

        MoveWindow(hwnd, winRect.left, winRect.top, width, height, false);
    }

    public static void ResizeWindowDelta(int dWidth, int dHeight)
    {
        var hwnd = GetMainWindow();

        GetWindowRect(hwnd, out WinRect winRect);

        var w = winRect.right - winRect.left + dWidth;
        var h = winRect.bottom - winRect.top + dHeight;
        MoveWindow(hwnd, winRect.left, winRect.top, w, h, false);
    }
}

