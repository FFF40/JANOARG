using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class LinuxAPI
{
    private const string DlLib = "libdl.so.2"; // This is the library for dynamic linking on Linux.

    [DllImport(DlLib)]
    public static extern IntPtr dlopen(string filename, int flags);

    [DllImport(DlLib)]
    public static extern int dlclose(IntPtr handle);

    // RTLD_LAZY = 0x00001: Perform lazy binding.
    private const int RTLD_LAZY = 0x00001;

    public static bool IsX11LibAvailable()
    {
        IntPtr handle = dlopen("libX11.so.6", RTLD_LAZY);
        if (handle == IntPtr.Zero)
        {
            return false;
        }
        dlclose(handle);
        return true;
    }


    private static IntPtr display;
    private static IntPtr rootWindow;
    private static IntPtr currentWindow;

    // Struct for _MOTIF_WM_HINTS
    [StructLayout(LayoutKind.Sequential)]
    struct MotifWmHints
    {
        public ulong flags;
        public ulong functions;
        public ulong decorations;
        public long inputMode;
        public ulong status;
    }

    // Constants for _MOTIF_WM_HINTS
    private const ulong MWM_HINTS_FUNCTIONS = 1L << 0;
    private const ulong MWM_HINTS_DECORATIONS = 1L << 1;

    private const ulong MWM_DECOR_ALL = 1L << 0;
    private const ulong MWM_DECOR_BORDER = 1L << 1;
    private const ulong MWM_DECOR_RESIZEH = 1L << 2;
    private const ulong MWM_DECOR_TITLE = 1L << 3;
    private const ulong MWM_DECOR_MENU = 1L << 4;
    private const ulong MWM_DECOR_MINIMIZE = 1L << 5;
    private const ulong MWM_DECOR_MAXIMIZE = 1L << 6;

    private const int PropModeReplace = 0;

    private const string X11Lib = "libX11.so.6";
    [DllImport(X11Lib)]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport(X11Lib)]
    private static extern IntPtr XDefaultRootWindow(IntPtr display);

    [DllImport(X11Lib)]
    private static extern int XMapWindow(IntPtr display, IntPtr window);

    [DllImport(X11Lib)]
    private static extern int XMoveWindow(IntPtr display, IntPtr window, int x, int y);

    [DllImport(X11Lib)]
    private static extern int XResizeWindow(IntPtr display, IntPtr window, uint width, uint height);

    [DllImport(X11Lib)]
    private static extern int XFetchName(IntPtr display, IntPtr window, ref IntPtr windowName);

    [DllImport(X11Lib)]
    private static extern int XStoreName(IntPtr display, IntPtr window, string windowName);

    [DllImport(X11Lib)]
    private static extern int XDestroyWindow(IntPtr display, IntPtr window);

    [DllImport(X11Lib)]
    static extern int XMoveResizeWindow(IntPtr display, IntPtr w, int x, int y, uint width, uint height);

    [DllImport(X11Lib)]
    static extern int XUnmapWindow(IntPtr display, IntPtr w);

    [DllImport(X11Lib)]
    private static extern IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

    [DllImport(X11Lib)]
    private static extern void XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, int mode, ref MotifWmHints hints, int nelements);

    [DllImport(X11Lib)]
    private static extern int XCloseDisplay(IntPtr display);

    public static void InitializeAPI()
    {
        display = XOpenDisplay(IntPtr.Zero);
        rootWindow = XDefaultRootWindow(display);

        currentWindow = Process.GetCurrentProcess().MainWindowHandle;
        XMapWindow(display, currentWindow);
    }

    public static void MoveWindow(int x, int y)
    {
        XMoveWindow(display, currentWindow, x, y);
    }

    public static void ResizeWindow(int width, int height)
    {
        XResizeWindow(display, currentWindow, (uint)width, (uint)height);
    }

    public static void RenameWindow(string name)
    {
        XStoreName(display, currentWindow, name);
    }

    /// <summary>
    /// Sets the window as frameless, there's no going back yet, this whole API is still a WIP
    /// </summary>
    /// <param name="window"></param>
    public static void SetFrameless(IntPtr window)
    {
        IntPtr display = XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
        {
            Console.WriteLine("Cannot open display");
            return;
        }

        // Get _MOTIF_WM_HINTS atom
        IntPtr motifHintsAtom = XInternAtom(display, "_MOTIF_WM_HINTS", false);
        IntPtr propTypeAtom = XInternAtom(display, "ATOM", false);

        // Set hints for no decorations (frameless)
        MotifWmHints hints = new MotifWmHints
        {
            flags = MWM_HINTS_DECORATIONS,
            decorations = 0
        };

        XChangeProperty(display, window, motifHintsAtom, propTypeAtom, 32, PropModeReplace, ref hints, 5);
        XCloseDisplay(display);
    }

    public static void DestroyWindow()
    {
        XDestroyWindow(display, currentWindow);
    }

    public static string GetWindowTitle()
    {
        IntPtr windowNamePtr = IntPtr.Zero;
        XFetchName(display, currentWindow, ref windowNamePtr);
        string windowName = Marshal.PtrToStringAnsi(windowNamePtr);
        return windowName;
    }
}
