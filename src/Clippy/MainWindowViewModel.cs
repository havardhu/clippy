using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;

namespace Clippy
{
    

    public class MainWindowViewModel
    {
        private readonly Dispatcher _dispatcher;
        public ClipboardContentListViewModel ClipboardContentListViewModel { get; private set; }

        private readonly Throttler<ClipboardContent> _throttler;

        public MainWindowViewModel(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _throttler = new Throttler<ClipboardContent>(c =>
            {
                _dispatcher.Invoke(() =>
                {
                    ClipboardContentListViewModel.Add(c);
                });

            });
            ClipboardContentListViewModel = new ClipboardContentListViewModel();
            ClipboardMonitor.OnClipboardChange += ClipboardMonitorOnOnClipboardChange;
            
        }

        private void ClipboardMonitorOnOnClipboardChange(ClipboardFormat format, object data)
        {
            _throttler.Trigger(new ClipboardContent
            {
                Format = format,
                Data = data
            });
        }
    }
}