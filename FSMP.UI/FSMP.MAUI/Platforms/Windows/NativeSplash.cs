using System.Runtime.InteropServices;

namespace FSMP.MAUI.WinUI;

/// <summary>
/// Lightweight Win32 splash window shown before MAUI/WinUI finishes bootstrapping.
/// Uses P/Invoke only — no WinUI or MAUI dependencies.
/// </summary>
internal static class NativeSplash
{
    private static IntPtr _hwnd;
    private static IntPtr _wndClass;
    private const string ClassName = "FSMPSplash";
    private const int Width = 400;
    private const int Height = 250;

    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_TOOLWINDOW = 0x00000080;

    private const uint WM_DESTROY = 0x0002;
    private const uint WM_PAINT = 0x000F;
    private const uint WM_CLOSE = 0x0010;
    private const uint WM_ERASEBKGND = 0x0014;

    private const int TRANSPARENT = 1;

    private static readonly uint BgColor = RGB(26, 35, 126); // #1A237E
    private static readonly uint TextColor = 0x00FFFFFF;

    // prevent delegate from being collected by GC
    private static readonly WndProcDelegate s_wndProc = WndProcHandler;

    public static void Show()
    {
        try
        {
            var wc = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
                lpfnWndProc = s_wndProc,
                hInstance = GetModuleHandle(null),
                lpszClassName = ClassName,
                hbrBackground = CreateSolidBrush(BgColor),
                hCursor = LoadCursor(IntPtr.Zero, 32512)
            };

            _wndClass = (IntPtr)RegisterClassEx(ref wc);
            if (_wndClass == IntPtr.Zero) return;

            int screenW = GetSystemMetrics(0);
            int screenH = GetSystemMetrics(1);
            int x = (screenW - Width) / 2;
            int y = (screenH - Height) / 2;

            _hwnd = CreateWindowEx(
                WS_EX_TOPMOST | WS_EX_TOOLWINDOW,
                ClassName, "FSMP",
                WS_POPUP | WS_VISIBLE,
                x, y, Width, Height,
                IntPtr.Zero, IntPtr.Zero, wc.hInstance, IntPtr.Zero);

            if (_hwnd != IntPtr.Zero)
            {
                ShowWindow(_hwnd, 5);
                UpdateWindow(_hwnd);
            }
        }
        catch
        {
            // Best-effort — never crash the app
        }
    }

    public static void Close()
    {
        try
        {
            if (_hwnd != IntPtr.Zero)
            {
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }
            if (_wndClass != IntPtr.Zero)
            {
                UnregisterClass(ClassName, GetModuleHandle(null));
                _wndClass = IntPtr.Zero;
            }
        }
        catch { }
    }

    private static IntPtr WndProcHandler(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_ERASEBKGND:
                return (IntPtr)1;

            case WM_PAINT:
                var ps = new PAINTSTRUCT();
                IntPtr hdc = BeginPaint(hwnd, ref ps);

                var rect = new RECT { left = 0, top = 0, right = Width, bottom = Height };
                IntPtr brush = CreateSolidBrush(BgColor);
                FillRect(hdc, ref rect, brush);
                DeleteObject(brush);

                SetBkMode(hdc, TRANSPARENT);
                SetTextColor(hdc, TextColor);

                IntPtr titleFont = CreateFont(36, 0, 0, 0, 700, 0, 0, 0, 1, 0, 0, 4, 0, "Segoe UI");
                IntPtr oldFont = SelectObject(hdc, titleFont);
                var titleRect = new RECT { left = 0, top = 60, right = Width, bottom = 120 };
                DrawText(hdc, "FSMP Music Player", -1, ref titleRect, 0x01 | 0x20);
                SelectObject(hdc, oldFont);
                DeleteObject(titleFont);

                IntPtr subFont = CreateFont(18, 0, 0, 0, 400, 0, 0, 0, 1, 0, 0, 4, 0, "Segoe UI");
                oldFont = SelectObject(hdc, subFont);
                var subRect = new RECT { left = 0, top = 140, right = Width, bottom = 180 };
                DrawText(hdc, "Loading...", -1, ref subRect, 0x01 | 0x20);
                SelectObject(hdc, oldFont);
                DeleteObject(subFont);

                EndPaint(hwnd, ref ps);
                return IntPtr.Zero;

            case WM_CLOSE:
                DestroyWindow(hwnd);
                return IntPtr.Zero;

            case WM_DESTROY:
                return IntPtr.Zero;

            default:
                return DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }

    private static uint RGB(byte r, byte g, byte b) => (uint)(r | (g << 8) | (b << 16));

    #region P/Invoke

    private delegate IntPtr WndProcDelegate(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public uint cbSize;
        public uint style;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left, top, right, bottom;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool UpdateWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr BeginPaint(IntPtr hwnd, ref PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    private static extern bool EndPaint(IntPtr hwnd, ref PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    private static extern int FillRect(IntPtr hdc, ref RECT lprc, IntPtr hbr);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int DrawText(IntPtr hdc, string lpchText, int cchText, ref RECT lprc, uint format);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateSolidBrush(uint crColor);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr ho);

    [DllImport("gdi32.dll")]
    private static extern int SetBkMode(IntPtr hdc, int mode);

    [DllImport("gdi32.dll")]
    private static extern uint SetTextColor(IntPtr hdc, uint color);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFont(
        int cHeight, int cWidth, int cEscapement, int cOrientation, int cWeight,
        uint bItalic, uint bUnderline, uint bStrikeOut, uint iCharSet,
        uint iOutPrecision, uint iClipPrecision, uint iQuality, uint iPitchAndFamily,
        string pszFaceName);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    #endregion
}
