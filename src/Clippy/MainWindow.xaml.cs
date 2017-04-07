using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Clippy.Properties;
using Microsoft.Win32;
using MenuItem = System.Windows.Forms.MenuItem;

namespace Clippy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MenuItem _showMI;
        private MenuItem _exitMI;

        private NotifyIcon _notifyIcon;

        private bool _hiddenNotificationShown = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(Dispatcher);


            
            IsVisibleChanged += OnIsVisibleChanged;
            StateChanged += OnStateChanged;

            _rkApp = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            _clippyRegistryKey = _rkApp.GetValue("Clippy");

            if (_clippyRegistryKey == null && !Settings.Default.AutomaticAutorunCompleted)
            {
                CreateAutorun();
                Settings.Default.AutomaticAutorunCompleted = true;
                Settings.Default.Save();
            }


            CreateContextMenu();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            _showMI.Visible = !IsVisible;
            _hideMI.Visible = IsVisible;
        }


        private void CreateContextMenu()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Click += (sender, args) =>
            {
                if (_notifyIcon != null)
                {
                    try
                    {
                        MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                        mi.Invoke(_notifyIcon, null);
                    }
                    catch
                    {
                        
                    }
                    
                }
            };

            _notifyIcon.Icon = new System.Drawing.Icon("trayicon.ico");
            _notifyIcon.Visible = true;
            _notifyIcon.DoubleClick += delegate(object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };

            _showMI = new MenuItem
            {
                Text = "Show",
            };
            _showMI.Click += (sender, args) => Show();

            _hideMI = new MenuItem
            {
                Text = "Hide"
            };
            _hideMI.Click += (sender, args) => Close();
            

            _exitMI = new MenuItem
            {
                Text = "Exit"
            };

            _exitMI.Click += (sender, args) => 
            {
                _forceClose = true;
                Close();

            };

            _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();

            var sep1 = new MenuItem() { Text = "-" };
            var sep2 = new MenuItem() { Text = "-" };

            

            var autorunMI = new MenuItem()
            {
                Text = "Run on startup"
            };
            var removeAutorunMI = new MenuItem()
            {
                Text = "Don't run on startup"
            };

            autorunMI.Click += (sender, args) =>
            {
                CreateAutorun();

                SetMenuItems(_showMI, _hideMI, sep1, removeAutorunMI, sep2, _exitMI);
            };

            removeAutorunMI.Click += (sender, args) =>
            {
                _rkApp.DeleteValue("Clippy");
                SetMenuItems(_showMI, _hideMI, sep1, autorunMI, sep2, _exitMI);
            };

            if (_clippyRegistryKey == null)
            {
                SetMenuItems(_showMI, _hideMI, sep1, autorunMI, sep2, _exitMI);
            }
            else
            {
                SetMenuItems(_showMI, _hideMI, sep1, removeAutorunMI, sep2, _exitMI);
            }
            
        }

        private void CreateAutorun()
        {
            string startPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs)
                               + @"\Clippy Software Ltd\Clippy\Clippy.appref-ms";

            _rkApp.SetValue("Clippy", startPath);

            _clippyRegistryKey = _rkApp.GetValue("Clippy");
        }

        private void SetMenuItems(params MenuItem[] items)
        {
            _notifyIcon.ContextMenu.MenuItems.Clear();
            _notifyIcon.ContextMenu.MenuItems.AddRange(items);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_forceClose && IsVisible)
            {
                Hide();
                _lastNonMinimizedState = WindowState;

                if (!_hiddenNotificationShown)
                {
                    _notifyIcon.ShowBalloonTip(2500, "CLIPPY!", "Clippy is still running, you can close it from the system tray.", ToolTipIcon.None);
                    _hiddenNotificationShown = true;
                }

                
                e.Cancel = true;
            }

            base.OnClosing(e);
        }

        WindowState _lastNonMinimizedState = WindowState.Maximized;
        private MenuItem _hideMI;
        private bool _forceClose;
        private readonly RegistryKey _rkApp;
        private object _clippyRegistryKey;

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized)
            {
                _lastNonMinimizedState = WindowState;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ClipboardMonitor.Initialize(this);
            HotkeyManager.Initialize(this);

            HotkeyManager.RegisterHotKey(Key.C, () =>
            {
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = _lastNonMinimizedState;
                }

                if (!IsVisible)
                {
                    Show();
                }

                Topmost = true;

            }, altMod: true);

        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                var n = _notifyIcon;
                _notifyIcon = null;
                n.Dispose();
            }
            finally
            {
                ClipboardMonitor.Stop();
                base.OnClosed(e);
            }
            


            

            
        }
    }
}
