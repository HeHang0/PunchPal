using Microsoft.Win32;
using PicaPico;
using PunchPal.Core.ViewModels;
using PunchPal.Tools;
using PunchPal.WPF.Tools;
using System.ComponentModel;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PunchPal.WPF.ViewModels
{
    public class MainModel : Core.ViewModels.MainModel
    {
        private readonly ImageSource _appIcon = ImageTools.AppIcon;
        private ImageSource _whiteIcon => Properties.Resources.white.ToBitmapSource();
        private ImageSource _blackIcon => Properties.Resources.black.ToBitmapSource();
        public ImageSource AppIcon => _appIcon;
        public ImageSource TrayIcon => ThemeListener.IsDarkMode ? _whiteIcon : _blackIcon;

        public MainModel()
        {
            ThemeListener.ThemeChanged += ApplyTheme;
            SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
            ApplyTheme(ThemeListener.IsDarkMode);
            Setting.Personalize.PropertyChanged += OnPropertyChanged;
        }

        private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.General)
            {
                return;
            }
            var isDark = ThemeListener.IsDarkMode;
            if (Setting.Personalize.ThemeColorMode != SettingsPersonalize.ColorMode.System)
            {
                isDark = Setting.Personalize.IsColorModeDark;
            }
            if (isDark != _lastDark)
            {
                return;
            }

            ApplicationThemeManager.Apply(
                isDark ? ApplicationTheme.Dark : ApplicationTheme.Light,
                WindowBackdropType,
                true
            );
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsPersonalize.ThemeColorMode):
                    ApplyTheme(ThemeListener.IsDarkMode);
                    break;
            }
        }
        bool _lastDark = false;
        private void ApplyTheme(bool isDark)
        {
            if (Setting.Personalize.ThemeColorMode != SettingsPersonalize.ColorMode.System)
            {
                isDark = Setting.Personalize.IsColorModeDark;
            }
            _lastDark = isDark;

            ApplicationThemeManager.Apply(
                isDark ? ApplicationTheme.Dark : ApplicationTheme.Light,
                WindowBackdropType,
                true
            );
            CalendarItem.IsDarkMode = isDark;
            foreach (var item in Calendar.Items)
            {
                item.UpdateDayColor();
            }
            OnPropertyChanged(nameof(TrayIcon));
            InitAcrylicBackground();
            Overview.IsDarkMode = isDark;
        }

        public WindowBackdropType WindowBackdropType
        {
            get
            {
                var effect = Setting.Personalize.WindowEffectType;
                if (OSVersionTools.IsMicaTabbedSupported && effect == SettingsPersonalize.WindowEffect.Tabbed)
                {
                    return WindowBackdropType.Tabbed;
                }
                else if (OSVersionTools.IsMicaSupported && effect == SettingsPersonalize.WindowEffect.Mica)
                {
                    return WindowBackdropType.Mica;
                }
                else if (OSVersionTools.IsAcrylicSupported && effect == SettingsPersonalize.WindowEffect.Acrylic)
                {
                    return WindowBackdropType.Acrylic;
                }
                else
                {
                    return WindowBackdropType.Auto;
                }
            }
        }

        private void InitAcrylicBackground()
        {
            if (!OSVersionTools.IsAcrylicCustom || WindowBackdropType != WindowBackdropType.Acrylic) return;
            if (Setting.Personalize.IsColorModeLight)
            {
                AcrylicHelper.BackgroundLightA = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
                AcrylicHelper.BackgroundDarkA = AcrylicHelper.BackgroundLightA;
                AcrylicHelper.BackgroundLight = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                AcrylicHelper.BackgroundDark = AcrylicHelper.BackgroundLight;
            }
            else if (Setting.Personalize.IsColorModeDark)
            {
                AcrylicHelper.BackgroundDarkA = new SolidColorBrush(Color.FromArgb(0xAA, 0x1F, 0x1F, 0x1F));
                AcrylicHelper.BackgroundLightA = AcrylicHelper.BackgroundDarkA;
                AcrylicHelper.BackgroundDark = new SolidColorBrush(Color.FromRgb(0x1F, 0x1F, 0x1F));
                AcrylicHelper.BackgroundLight = AcrylicHelper.BackgroundDark;
            }
            else
            {
                AcrylicHelper.BackgroundDarkA = new SolidColorBrush(Color.FromArgb(0xAA, 0x1F, 0x1F, 0x1F));
                AcrylicHelper.BackgroundLightA = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
                AcrylicHelper.BackgroundDark = new SolidColorBrush(Color.FromRgb(0x1F, 0x1F, 0x1F));
                AcrylicHelper.BackgroundLight = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            }
        }
    }
}
