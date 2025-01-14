using Microsoft.Win32;
using PicaPico;
using PunchPal.Core.Events;
using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Core.ViewModels;
using PunchPal.Tools;
using PunchPal.WPF.Controls;
using PunchPal.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shell;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Interop;
using ControlAppearance = Wpf.Ui.Controls.ControlAppearance;
using MainModel = PunchPal.WPF.ViewModels.MainModel;

namespace PunchPal.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private readonly MainModel _mainModel;
        public MainWindow()
        {
            InitializeComponent();
            _mainModel = new MainModel();
            DataContext = _mainModel;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            _mainModel.ConfirmDialog += OnConfirmDialog;
            _mainModel.Tips += OnTips;
            _mainModel.AddRecord += OnAddRecord;
            _mainModel.WorkingHours.TextCoping += OnTextCoping;
            _mainModel.Setting.Personalize.PropertyChanged += OnPersonalizeChanged;
            _mainModel.Setting.Personalize.FileSelecting += OnFileSelecting;
            _mainModel.ShowWindow += OnShowWindow;
            InitWindowBackdropType();
        }

        private void OnFileSelecting(object sender, SelectFileEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = e.Filter,
                Multiselect = e.Multiselect
            };
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            e.FileName = openFileDialog.FileName;
            e.FileNames = openFileDialog.FileNames;
        }

        private void OnShowWindow(object sender, EventArgs e)
        {
            Dispatcher.Invoke(ShowWindow);
        }

        private void OnPersonalizeChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(SettingsPersonalize.WindowEffectType))
            {
                return;
            }
            Restart();
        }

        private void OnTextCoping(object sender, string text)
        {
            try
            {
                Clipboard.SetText(text);
                ShowTips(new TipsOption("提示", $"已复制到剪贴板", Core.Models.ControlAppearance.Success));
            }
            catch (Exception ex)
            {
                ShowTips(new TipsOption("提示", $"复制到剪贴板出错", Core.Models.ControlAppearance.Danger));
            }
        }

        private void OnAddRecord(object sender, EventArgs e)
        {
            if (_mainModel.IsPunchRecord)
            {
                AddPunchRecord();
            }else if(_mainModel.IsAttendanceRecord)
            {
                AddAttendanceRecord();
            }
        }

        private async void AddAttendanceRecord()
        {
            var content = new AddAttendanceRecordControl()
            {
                Width = 380,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var contentDialog = new ContentDialog()
            {
                Title = "添加考勤记录",
                CloseButtonText = "取消",
                PrimaryButtonText = "确认",
                MinHeight = 0,
                DialogHost = DialogPresenter,
                Content = content
            };
            var result = await contentDialog.ShowAsync();
            var records = new List<AttendanceRecord>();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    {
                        var startTime = content.StartDateTime.TimestampUnix();
                        var endTime = content.EndDateTime.TimestampUnix();
                        var record = new AttendanceRecord
                        {
                            AttendanceId = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                            AttendanceTypeId = content.AttendanceTypeId,
                            UserId = _mainModel.Setting.Common.CurrentUser?.UserId,
                            StartTime = startTime,
                            EndTime = endTime,
                            AttendanceTime = DateTime.Now.TimestampUnix(),
                            Remark = content.RecordRemark
                        };
                        records.Add(record);
                        break;
                    }
                default:
                    return;
            }
            _mainModel.Loading = true;
            var len = await AttendanceRecordService.Instance.Add(records);
            _mainModel.Loading = false;
            if (len > 0)
            {
                _mainModel.InitItems();
                ShowTips(new TipsOption("提示", $"添加{len}条数据成功", Core.Models.ControlAppearance.Success));
            }
        }

        private async void AddPunchRecord()
        {
            var content = new AddPunchRecordControl()
            {
                Width = 380,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var contentDialog = new ContentDialog()
            {
                Title = "添加打卡记录",
                CloseButtonText = "取消",
                PrimaryButtonText = "导入",
                PrimaryButtonAppearance = ControlAppearance.Success,
                SecondaryButtonText = "确认",
                SecondaryButtonAppearance = ControlAppearance.Primary,
                MinHeight = 0,
                DialogHost = DialogPresenter,
                Content = content
            };

            var result = await contentDialog.ShowAsync();
            var records = new List<PunchRecord>();
            switch (result)
            {
                case ContentDialogResult.Secondary:
                    {
                        var timestamp = content.RecordDateTime.TimestampUnix();
                        var record = new PunchRecord
                        {
                            PunchTime = timestamp,
                            PunchType = PunchRecord.PunchTypeManual,
                            UserId = _mainModel.Setting.Common.CurrentUser?.UserId,
                            Remark = content.RecordRemark
                        };
                        records.Add(record);
                        break;
                    }
                case ContentDialogResult.Primary:
                    _mainModel.Loading = true;
                    await PunchRecordService.Instance.ImportFromFile(SelectFile(), _mainModel.Setting.Common.CurrentUser?.UserId);
                    _mainModel.Loading = false;
                    return;
                default:
                    break;
            }
            _mainModel.Loading = true;
            var len = await PunchRecordService.Instance.Add(records);
            _mainModel.Loading = false;
            if (len > 0)
            {
                _mainModel.InitItems();
                ShowTips(new TipsOption("提示", $"添加{len}条数据成功", Core.Models.ControlAppearance.Success));
            }
        }

        private string SelectFile()
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Text|*.txt|Sqlite|*.sqlite|All|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() != true) return string.Empty;
            return openFileDialog.FileName;
        }

        private void OnTips(object sender, TipsOption option)
        {
            ShowTips(option);
        }

        private SnackbarService _snackbarService;

        private void ShowTips(TipsOption option)
        {
            if (SnackbarPresenter == null)
            {
                return;
            }

            if (_snackbarService == null)
            {
                _snackbarService = new SnackbarService();
                _snackbarService.SetSnackbarPresenter(SnackbarPresenter);
            }

            _snackbarService.Show(
                option.Title,
                option.Message,
                (ControlAppearance)option.Appearance,
                new SymbolIcon(SymbolRegular.Fluent24),
                option.Duration
            );
        }

        private void OnConfirmDialog(object sender, ConfirmDialogEventArgs e)
        {
            var contentDialog = new ContentDialog()
            {
                Title = e.Title,
                Content = e.Message,
                CloseButtonText = "取消",
                PrimaryButtonText = "确认",
                PrimaryButtonAppearance = (ControlAppearance)e.Appearance,
                MinHeight = 0,
                DialogHost = DialogPresenter,
            };
            var resultTask = contentDialog.ShowAsync();
            e.Result = resultTask.ContinueWith(t =>
            {
                return t.Result == ContentDialogResult.Primary;
            });
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PunchNavigationView.Navigate("记录");
            ShowWindow();
        }

        private void InitWindowBackdropType()
        {
            if (!OSVersionTools.IsAcrylicCustom || _mainModel.WindowBackdropType != WindowBackdropType.Acrylic) return;
            AcrylicHelper.Apply(this, DragHelper);
        }

        private void PunchNavigationView_Navigating(NavigationView sender, NavigatingCancelEventArgs args)
        {
            if (!(args.Page is Page page))
            {
                return;
            }
            switch (page.GetType())
            {
                case var type when type == typeof(Pages.SettingCommonPage):
                    if (page.DataContext == null) page.DataContext = _mainModel.Setting.Common;
                    _mainModel.Setting.CurrentSettingPage = SettingsModel.PageType.Common;
                    break;
                case var type when type == typeof(Pages.SettingPersonalizePage):
                    if (page.DataContext == null) page.DataContext = _mainModel.Setting.Personalize;
                    _mainModel.Setting.CurrentSettingPage = SettingsModel.PageType.Personalize;
                    break;
                case var type when type == typeof(Pages.SettingNetworkPage):
                    if (page.DataContext == null) page.DataContext = _mainModel.Setting.Network;
                    _mainModel.Setting.CurrentSettingPage = SettingsModel.PageType.Network;
                    break;
                case var type when type == typeof(Pages.PunchRecordPage):
                    _mainModel.CurrentPage = Core.ViewModels.MainModel.PageType.PunchRecord;
                    if (page.DataContext == null) page.DataContext = _mainModel.PunchRecord;
                    break;
                case var type when type == typeof(Pages.AttendanceRecordPage):
                    _mainModel.CurrentPage = Core.ViewModels.MainModel.PageType.AttendanceRecord;
                    if (page.DataContext == null) page.DataContext = _mainModel.AttendanceRecord;
                    break;
                case var type when type == typeof(Pages.WorkingHoursPage):
                    _mainModel.CurrentPage = Core.ViewModels.MainModel.PageType.WorkingHours;
                    if (page.DataContext == null) page.DataContext = _mainModel.WorkingHours;
                    break;
                case var type when type == typeof(Pages.CalendarPage):
                    _mainModel.CurrentPage = Core.ViewModels.MainModel.PageType.Calendar;
                    if (page.DataContext == null) page.DataContext = _mainModel.Calendar;
                    break;
                case var type when type == typeof(Pages.OverviewPage):
                    _mainModel.CurrentPage = Core.ViewModels.MainModel.PageType.Overview;
                    break;
                case var type when type == typeof(Pages.SettingsPage):
                    _mainModel.CurrentPage = Core.ViewModels.MainModel.PageType.Settings;
                    if (page.DataContext == null) page.DataContext = _mainModel.Setting;
                    break;
                default:
                    _mainModel.CurrentPage = Core.ViewModels.MainModel.PageType.None;
                    break;
            }
        }

        private bool _exiting;

        private void Exit(object sender, RoutedEventArgs e)
        {
            _exiting = true;
            _ = _mainModel.Setting.SaveReal();
            Application.Current.Shutdown();
        }

        private async void Restart()
        {
            var option = new ConfirmDialogEventArgs()
            {
                Title = "提示",
                Message = "设置已更改，是否重启应用？",
                Appearance = Core.Models.ControlAppearance.Caution
            };
            _ = _mainModel.Setting.SaveReal();
            OnConfirmDialog(null, option);
            if (option.Result == null)
            {
                return;
            }
            var result = await option.Result;
            if(!result)
            {
                return;
            }
            _exiting = true;
            Process.Start(Application.ResourceAssembly.Location, "--restarted");
            Application.Current.Shutdown();
        }

        private bool _toastShown;

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            if (_toastShown)
            {
                return;
            }

            Trace.WriteLine("已最小化到托盘");
            _toastShown = true;
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            var handle = new WindowInteropHelper(this).Handle;
            if (handle != IntPtr.Zero)
            {
                SetForegroundWindow(handle);
            }
        }

        private void OnShowWindowClick(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void OnShowSettingsClick(object sender, RoutedEventArgs e)
        {
            ShowWindow();
            PunchNavigationView.Navigate("设置");
        }

        protected override void OnExtendsContentIntoTitleBarChanged(bool oldValue, bool newValue)
        {
            SetCurrentValue(WindowStyleProperty, WindowStyle);

            WindowChrome.SetWindowChrome(
                this,
                new WindowChrome
                {
                    CaptionHeight = 0,
                    CornerRadius = default,
                    GlassFrameThickness = new Thickness(-1),
                    ResizeBorderThickness = ResizeMode == ResizeMode.NoResize ? default : new Thickness(4),
                    UseAeroCaptionButtons = false,
                }
            );
            _ = UnsafeNativeMethods.RemoveWindowTitlebarContents(this);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private void OnWindowMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FocusHelper.Focus();
        }
    }
}