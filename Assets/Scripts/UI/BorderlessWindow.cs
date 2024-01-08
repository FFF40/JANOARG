using System;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;
using System.Text;
using UnityEngine.Events;

public class BorderlessWindow
{
    public static bool IsFramed;
    public static bool IsMaximized;
    public static bool IsInTitleBar;

    public static UnityEvent OnWindowUpdate = new();

    static IntPtr CurrentWindow;
    
    static WinProc newWndProcDelegate = null;
    static IntPtr newWndProc = IntPtr.Zero;
    static IntPtr oldWndProc = IntPtr.Zero;
    
    delegate IntPtr WinProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")]
    static extern bool EnumThreadWindows(uint dwThreadId, EnumWinProc lpEnumFunc, IntPtr lParam);
    delegate bool EnumWinProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool SetWindowText(IntPtr hWnd, string lpString);
    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")]
    static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
    static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
    static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")]
    static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);
    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hwnd, out WinRect lpRect);
    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr hwnd, out WinRect lpRect);
    [DllImport("user32.dll")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    
    [DllImport("dwmapi.dll")]
    static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, WinMargin margin);

    [DllImport("user32.dll")]
    static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
	[DllImport("user32.dll")]
	private static extern IntPtr DefWindowProc(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr SetCursor(IntPtr hCursor);

    struct WinRect { public int left, top, right, bottom; }
    struct WinMargin { public int left, right, top, bottom; }
    struct WinPoint { public int x, y; }

    [StructLayout(LayoutKind.Sequential)]
    struct MinMaxInfo
    {
        public WinPoint Reserved, MaxSize, MaxPosition, MinTrackSize, MaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WindowPos
    {
        public IntPtr hWnd, hWndInsertAfter;
        public int x, y, cx, cy, flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct NCCalcSizeParams
    {
        public WinRect rect0, rect1, rect2;
        public WindowPos pos;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct StyleStruct
    {
        public uint oldStyle, newStyle;
    }


    const int GWL_WINPROC = -4;
    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;

    const int SW_MINIMIZE = 6;
    const int SW_MAXIMIZE = 3;
    const int SW_RESTORE = 9;

    const int WM_GETMINMAXINFO = 0x0024;
    const int WM_SIZING = 0x0214;
    const int WM_SIZE = 0x0005;
    const int WM_NCCALCSIZE = 0x0083;
    const int WM_STYLECHANGED = 0x007D;
    const int WM_SETCURSOR = 0x0020;
    const int WM_MOUSEMOVE = 0x0200;
    const int WM_NCHITTEST = 0x0084;

    const uint WS_VISIBLE = 0x10000000;    
    const uint WS_POPUP = 0x80000000;
    const uint WS_BORDER = 0x00800000;
    const uint WS_OVERLAPPED = 0x00000000;
    const uint WS_CAPTION = 0x00C00000;
    const uint WS_DLGFRAME = 0x00400000;
    const uint WS_SYSMENU = 0x00080000;
    const uint WS_THICKFRAME = 0x00040000; // WS_SIZEBOX
    const uint WS_MINIMIZEBOX = 0x00020000;
    const uint WS_MAXIMIZEBOX = 0x00010000;
    const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

    const uint WS_EX_WINDOWEDGE = 0x00000100;

    const int SWP_NOSIZE = 0x0001;
    const int SWP_NOMOVE = 0x0002;
    const int SWP_FRAMECHANGED = 0x0020;

    static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8) return SetWindowLong64(hWnd, nIndex, dwNewLong);
        else return SetWindowLong32(hWnd, nIndex, dwNewLong);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void InitializeWindow()
    {
        Vector2Int screenSize = new(Screen.width, Screen.height);
        
        Chartmaker.PreferencesStorage = new("cm_prefs");
        Chartmaker.Preferences.Load(Chartmaker.PreferencesStorage);

        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN 
            FindWindow();
            BorderlessWindow.HookWindowProc();
            RenameWindow("JANOARG Chartmaker");

            Cursor.SetCursor(new Texture2D(0, 0), Vector2.zero, CursorMode.ForceSoftware);
            CursorChanger.PushCursor(CursorType.Arrow);
            
            IsFramed = Chartmaker.Preferences.UseDefaultWindow;
            if (!IsFramed)
            {
                SetFramelessWindow();
                ResizeWindow(screenSize.x + 14, screenSize.y + 7);
            }
        #endif
    }

    public static void FindWindow () {
        const string TargetClassName = "UnityWndClass";
        EnumThreadWindows(GetCurrentThreadId(), (hWnd, lParam) =>
        {
            var classText = new StringBuilder(TargetClassName.Length + 1);
            GetClassName(hWnd, classText, classText.Capacity);

            if (classText.ToString() == TargetClassName)
            {
                CurrentWindow = hWnd;
                return false;
            }
            return true;
        }, IntPtr.Zero);
    }

    public static Rect GetWindowRect()
    {

        GetWindowRect(CurrentWindow, out WinRect winRect);

        return new Rect(winRect.left, winRect.top, winRect.right - winRect.left, winRect.bottom - winRect.top);
    }

    public static void HookWindowProc()
    {
        newWndProcDelegate = new WinProc(WindowProc);
        newWndProc = Marshal.GetFunctionPointerForDelegate(newWndProcDelegate);
        oldWndProc = SetWindowLong(CurrentWindow, GWL_WINPROC, newWndProc);
    }

    public static void UnhookWindowProc()
    {
        oldWndProc = SetWindowLong(CurrentWindow, GWL_WINPROC, oldWndProc);
    }

    static IntPtr WindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
    { 
        if (msg == WM_SIZING) 
        {
            if (IsFramed) return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);

            GetWindowRect(CurrentWindow, out WinRect winRect);
            MoveWindowDelta(Vector2.one, true);
            MoveWindowDelta(-Vector2.one, true);
            return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        }
        else if (msg == WM_STYLECHANGED) 
        {
            if ((int)wParam == GWL_STYLE)
            {
                var styleStruct = Marshal.PtrToStructure<StyleStruct>(lParam);
                IsFramed = (styleStruct.newStyle & WS_CAPTION) != 0;
                OnWindowUpdate.Invoke();
            }

            return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        }
        else if (msg == WM_SIZE) 
        {
            if ((int)wParam == 2)
            {
                if (!IsMaximized) 
                {
                    IsMaximized = true;
                    SetWindowPos(hWnd, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_FRAMECHANGED);
                }
                OnWindowUpdate.Invoke();
            }
            else if ((int)wParam == 0)
            {
                if (IsMaximized) 
                {
                    IsMaximized = false;
                    SetWindowPos(hWnd, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_FRAMECHANGED);
                    ResizeWindowDelta(0, -7);
                }
                OnWindowUpdate.Invoke();
            }

            return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        }
        else if (msg == WM_GETMINMAXINFO) 
        {
            var minMaxInfo = Marshal.PtrToStructure<MinMaxInfo>(lParam);

            minMaxInfo.MinTrackSize.x = 974;
            minMaxInfo.MinTrackSize.y = 607;

            Marshal.StructureToPtr(minMaxInfo, lParam, false);
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }
        else if (msg == WM_NCCALCSIZE) 
        {
            if (IsFramed) return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);

            if (wParam != IntPtr.Zero)
            {
                var size = Marshal.PtrToStructure<NCCalcSizeParams>(lParam);

                size.rect0.top += IsMaximized ? 7 : 0;
                size.rect0.bottom -= 7;
                size.rect0.left += 7;
                size.rect0.right -= 7;
                
                Marshal.StructureToPtr(size, lParam, true);
            }
            else
            {
                var size = Marshal.PtrToStructure<WinRect>(lParam);

                size.top += IsMaximized ? 7 : 0;
                size.bottom -= 7;
                size.left += 7;
                size.right -= 7;
                
                Marshal.StructureToPtr(size, lParam, true);
            }

            return IntPtr.Zero;
        }
        else if (msg == WM_SETCURSOR || msg == WM_MOUSEMOVE) 
        {
            var proc = CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
            if (CursorChanger.Cursors.Count <= 0) return proc;

            UpdateCursor();
            return (IntPtr)(-1);
        }
        else if (msg == WM_NCHITTEST) 
        {
            var proc = CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
            if (IsFramed) return proc;

            return IsInTitleBar ? (IntPtr)2 : proc;
        }
        else 
        {
            return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        }
    }

    public static void SetFramelessWindow()
    {
        SetWindowLong(CurrentWindow, GWL_STYLE, WS_OVERLAPPEDWINDOW | WS_VISIBLE);
        DwmExtendFrameIntoClientArea(CurrentWindow, new WinMargin { top = 0, left = 0, bottom = 0, right = 0 });
        IsFramed = false;
    }

    public static void SetFramedWindow()
    {
        SetWindowLong(CurrentWindow, GWL_STYLE, WS_OVERLAPPEDWINDOW | WS_VISIBLE);
        IsFramed = true;
    }

    public static void MinimizeWindow()
    {
        IsMaximized = false;
        ShowWindow(CurrentWindow, SW_MINIMIZE);
    }

    public static void MaximizeWindow()
    {
        IsMaximized = true;
        ShowWindow(CurrentWindow, SW_MAXIMIZE);
    }

    public static void RestoreWindow()
    {
        IsMaximized = false;
        ShowWindow(CurrentWindow, SW_RESTORE);
    }

    public static void MoveWindow(Vector2 pos, bool bRepaint = false)
    {

        GetWindowRect(CurrentWindow, out WinRect winRect);

        MoveWindow(CurrentWindow, (int)pos.x, (int)pos.y, winRect.right - winRect.left, winRect.bottom - winRect.top, bRepaint);
    }

    public static void MoveWindowDelta(Vector2 posDelta, bool bRepaint = false)
    {

        GetWindowRect(CurrentWindow, out WinRect winRect);

        var x = winRect.left + (int)posDelta.x;
        var y = winRect.top - (int)posDelta.y;
        MoveWindow(CurrentWindow, x, y, winRect.right - winRect.left, winRect.bottom - winRect.top, bRepaint);
    }

    public static void ResizeWindow(int width, int height)
    {

        GetWindowRect(CurrentWindow, out WinRect winRect);

        MoveWindow(CurrentWindow, winRect.left, winRect.top, width, height, false);
    }

    public static void ResizeWindowDelta(int dWidth, int dHeight)
    {

        GetWindowRect(CurrentWindow, out WinRect winRect);

        var w = winRect.right - winRect.left + dWidth;
        var h = winRect.bottom - winRect.top + dHeight;
        MoveWindow(CurrentWindow, winRect.left, winRect.top, w, h, false);
    }

    public static void RenameWindow (string title) 
    {
        SetWindowText(CurrentWindow, title);
    }

    public static void UpdateCursor ()
    {
        if (CursorChanger.Cursors.Count > ((
            Input.mousePosition.x >= 0 && Input.mousePosition.x < Screen.width &&
            Input.mousePosition.y >= 0 && Input.mousePosition.y < Screen.height
        ) ? 0 : 1)) SetCursor(CursorChanger.Cursors.Peek());
    }
}

