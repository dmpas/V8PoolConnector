using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

internal class NativeMethod
{
    [DllImport("kernel32.dll")]
    internal static extern uint GetCurrentThreadId();

    [DllImport("kernel32.dll")]
    internal static extern uint GetCurrentProcessId();

    [DllImport("user32.dll")]
    internal static extern bool GetMessage(
        out MSG lpMsg, 
        IntPtr hWnd, 
        uint wMsgFilterMin, 
        uint wMsgFilterMax);

    [DllImport("user32.dll")]
    internal static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    internal static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    internal static extern bool PostThreadMessage(
        uint idThread, 
        uint Msg, 
        UIntPtr wParam,
        IntPtr lParam);

    internal const Int32 WM_QUIT = 0x0012;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MSG
{
    public IntPtr hWnd;
    public uint message;
    public IntPtr wParam;
    public IntPtr lParam;
    public uint time;
    public POINT pt;
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    public int X;
    public int Y;

    public POINT(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}