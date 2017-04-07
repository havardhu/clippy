using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Clippy
{
    public static class ClipboardMonitor
    {
        public delegate void OnClipboardChangeEventHandler(ClipboardFormat format, object data);

        public static event OnClipboardChangeEventHandler OnClipboardChange;

        static IntPtr _nextClipboardViewer;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        
        // defined in winuser.h
        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CHANGECBCHAIN = 0x030D;


        static readonly string[] formats = Enum.GetNames(typeof(ClipboardFormat));

        private static bool _isSuspended = false;
        public static void Suspend()
        {
            _isSuspended = true;
        }

        public static void Resume()
        {
            _isSuspended = false;
        }

        static Throttler<object> _resumeThrottler = new Throttler<object>(o => Resume(), 1000);

        private static void ClipChanged()
        {

            if (_isSuspended)
            {
                _resumeThrottler.Trigger(null);
                return;
            }

            IDataObject iData = Clipboard.GetDataObject();

            ClipboardFormat? format = null;

            foreach (var f in formats)
            {
                if (iData.GetDataPresent(f))
                {
                    format = (ClipboardFormat)Enum.Parse(typeof(ClipboardFormat), f);
                    break;
                }
            }

            if (format != null)
            {
                try
                {
                    object data = iData.GetData(format.ToString());

                    if (data == null || format == null)
                        return;

                    if (OnClipboardChange != null)
                        OnClipboardChange((ClipboardFormat)format, data);
                }
                catch (COMException)
                {
                    
                }
                
            }

            
        }

        private static HwndSource _source;

        public static void Initialize(Visual visual)
        {
            _source = PresentationSource.FromVisual(visual) as HwndSource;
            _source.AddHook(WndProc);
            _nextClipboardViewer = SetClipboardViewer(_source.Handle);
        }

        private static bool _isSending = false;

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DRAWCLIPBOARD:
                    ClipChanged();
                    SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    handled = true;
                break;

                case WM_CHANGECBCHAIN:
                    if (wParam == _nextClipboardViewer)
                        _nextClipboardViewer = lParam;

                    else
                        SendMessage(_nextClipboardViewer, msg, wParam, lParam);

                    handled = true;
                    break;

            }
            return IntPtr.Zero;
        }
        
        public static void Stop()
        {

            ChangeClipboardChain(_source.Handle, _nextClipboardViewer);
        }
    }
}