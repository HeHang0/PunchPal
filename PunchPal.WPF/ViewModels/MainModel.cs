using PicaPico;
using PunchPal.Core.ViewModels;
using PunchPal.Tools;
using PunchPal.WPF.Tools;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PunchPal.WPF.ViewModels
{
    public class MainModel : Core.ViewModels.MainModel
    {
        private ImageSource _appIcon = ImageTools.AppIcon;
        private ImageSource _whiteIcon => Properties.Resources.white.ToBitmapSource();
        private ImageSource _blackIcon => Properties.Resources.black.ToBitmapSource();
        public ImageSource AppIcon => _appIcon;
        public ImageSource TrayIcon => ThemeListener.IsDarkMode ? _whiteIcon : _blackIcon;
        public SettingsModel Setting => SettingsModel.Load();

        private void ApplyTheme(bool isDark)
        {
            //if (Setting.ThemeColorMode != Setting.ColorMode.System)
            //{
            //    isDark = Settings.Load().ThemeColorMode == Settings.ColorMode.Dark;
            //}

            ApplicationThemeManager.Apply(
                isDark ? ApplicationTheme.Dark : ApplicationTheme.Light,
                WindowBackdropType
            );
            OnPropertyChanged(nameof(TrayIcon));
            InitAcrylicBackground();
        }

        public WindowBackdropType WindowBackdropType
        {
            get
            {
                return WindowBackdropType.Mica;
                //var effect = Settings.Load().WindowEffectType;
                //if (Common.IsMicaTabbedSupported && effect == Settings.WindowEffect.Tabbed)
                //{
                //}
                //else if (Common.IsMicaSupported && effect == Settings.WindowEffect.Mica)
                //{
                //    return WindowBackdropType.Mica;
                //}
                //else if (Common.IsAcrylicSupported && effect == Settings.WindowEffect.Acrylic)
                //{
                //    return WindowBackdropType.Acrylic;
                //}
                //else
                //{
                //    return WindowBackdropType.Auto;
                //}
            }
        }

        private void InitAcrylicBackground()
        {
            if (!OSVersionTools.IsAcrylicCustom || WindowBackdropType != WindowBackdropType.Acrylic) return;
            //if (Setting.IsColorModeLight)
            //{
            //    AcrylicHelper.BackgroundLightA = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
            //    AcrylicHelper.BackgroundDarkA = AcrylicHelper.BackgroundLightA;
            //    AcrylicHelper.BackgroundLight = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            //    AcrylicHelper.BackgroundDark = AcrylicHelper.BackgroundLight;
            //}
            //else if (Setting.IsColorModeDark)
            //{
            //    AcrylicHelper.BackgroundDarkA = new SolidColorBrush(Color.FromArgb(0xAA, 0x1F, 0x1F, 0x1F));
            //    AcrylicHelper.BackgroundLightA = AcrylicHelper.BackgroundDarkA;
            //    AcrylicHelper.BackgroundDark = new SolidColorBrush(Color.FromRgb(0x1F, 0x1F, 0x1F));
            //    AcrylicHelper.BackgroundLight = AcrylicHelper.BackgroundDark;
            //}
            //else
            //{
            //    AcrylicHelper.BackgroundDarkA = new SolidColorBrush(Color.FromArgb(0xAA, 0x1F, 0x1F, 0x1F));
            //    AcrylicHelper.BackgroundLightA = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
            //    AcrylicHelper.BackgroundDark = new SolidColorBrush(Color.FromRgb(0x1F, 0x1F, 0x1F));
            //    AcrylicHelper.BackgroundLight = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            //}
        }
    }
}
