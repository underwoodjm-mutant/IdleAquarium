using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class TransparentWindow : Singleton<TransparentWindow>
{
    // --- Windows API Imports ---
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // This is the library that handles Windows Desktop Composition (Alpha transparency)
    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS { public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight; }

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    // --- Constants ---
    const int GWL_STYLE = -16;
    const uint WS_POPUP = 0x80000000; // Borderless window
    const uint WS_VISIBLE = 0x10000000;

    // Window Pos Flags (Keeps the window floating on top)
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOMOVE = 0x0002;

    private void Start()
    {
        Application.runInBackground = true;

        // This code will only run in the actual built .exe, NOT the Unity Editor!
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        SetupTransparentWindow();
#endif
    }

    /// <summary>
    /// Commands the Windows OS to pin this application above all other windows, or release it.
    /// </summary>
    public void SetAlwaysOnTopState(bool isAlwaysOnTop)
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        IntPtr hWnd = GetActiveWindow();
        IntPtr zOrder = isAlwaysOnTop ? HWND_TOPMOST : HWND_NOTOPMOST;
        
        SetWindowPos(hWnd, zOrder, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
#endif
    }

    private void SetupTransparentWindow()
    {
        //Get  game window
        IntPtr hWnd = GetActiveWindow();

        //Tell Windows to extend the window frame to the entire client area (making it transparent)
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        //Remove the window borders, minimize/maximize buttons, and title bar
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        //Force the window to always stay on top of other applications
        //SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
    }
}
