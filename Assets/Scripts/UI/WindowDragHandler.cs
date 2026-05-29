using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;

public class WindowDragHandler : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    // --- Windows API Imports ---
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    // --- Windows Messages Constants ---
    private const int WM_NCLBUTTONDOWN = 0xA1; // Left mouse button down on non-client area
    private const int HT_CAPTION = 0x2;        // Title bar/Caption code

    private IntPtr _windowHandle;

    private void Awake()
    {
        // Cache the active OS window handle when the script initializes
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        _windowHandle = GetActiveWindow();
#endif
    }

    /// <summary>
    /// Fires the moment the user clicks down on the drag handle.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Only drag the OS window in a built standalone executable on Windows
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Release mouse capture from the Unity engine control
            ReleaseCapture();
            
            // Send a message directly to Windows telling it that the user clicked 
            // the "invisible title bar," hijacking the OS native dragging behavior!
            SendMessage(_windowHandle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
#endif
    }

    /// <summary>
    /// Interface requirement for IDragHandler. Keeps the pointer tracked.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // leave empty because SendMessage hands dragging control 
        // entirely over to the Windows OS framework, but interface 
        // MUST be present on the script EventSystem to detect dragging.
    }
}

