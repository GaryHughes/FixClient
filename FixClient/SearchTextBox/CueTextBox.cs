/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: CueTextBox.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FixClient;

class CueTextBox : ButtonTextBox
{
    static class NativeMethods
    {
        const uint ECM_FIRST = 0x1500;
        internal const uint EM_SETCUEBANNER = ECM_FIRST + 1;

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
    }

    string? _cue;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public string? Cue
    {
        get
        {
            return _cue;
        }
        set
        {
            _cue = value;
            UpdateCue();
        }
    }

    void UpdateCue()
    {
        if (IsHandleCreated && _cue != null)
        {
            NativeMethods.SendMessageW(Handle, NativeMethods.EM_SETCUEBANNER, (IntPtr)1, _cue);
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdateCue();
    }
}

