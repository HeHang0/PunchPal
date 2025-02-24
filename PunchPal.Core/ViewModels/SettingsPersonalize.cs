using PunchPal.Core.Events;
using System.IO;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class SettingsPersonalize : NotifyPropertyBase
    {
        public enum ColorMode
        {
            System,
            Dark,
            Light
        }

        private ColorMode _themeColorMode = ColorMode.System;

        public ColorMode ThemeColorMode
        {
            get => _themeColorMode;
            set
            {
                _themeColorMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsColorModeSystem));
                OnPropertyChanged(nameof(IsColorModeDark));
                OnPropertyChanged(nameof(IsColorModeLight));
            }
        }

        public bool IsColorModeSystem
        {
            get => _themeColorMode == ColorMode.System;
            set
            {
                if (value)
                {
                    ThemeColorMode = ColorMode.System;
                }
            }
        }

        public bool IsColorModeDark
        {
            get => _themeColorMode == ColorMode.Dark;
            set
            {
                if (value)
                {
                    ThemeColorMode = ColorMode.Dark;
                }
            }
        }

        public bool IsColorModeLight
        {
            get => _themeColorMode == ColorMode.Light;
            set
            {
                if (value)
                {
                    ThemeColorMode = ColorMode.Light;
                }
            }
        }

        public enum WindowEffect
        {
            Default,
            Tabbed,
            Mica,
            Acrylic
        }

        private WindowEffect _windowEffect = Tools.OSVersionTools.IsMicaTabbedSupported
            ? WindowEffect.Mica
            : (Tools.OSVersionTools.IsMicaSupported
                ? WindowEffect.Mica
                : (Tools.OSVersionTools.IsAcrylicSupported ? WindowEffect.Acrylic : WindowEffect.Default));

        public WindowEffect WindowEffectType
        {
            get => _windowEffect;
            set
            {
                _windowEffect = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWindowEffectDefault));
                OnPropertyChanged(nameof(IsWindowEffectTabbed));
                OnPropertyChanged(nameof(IsWindowEffectMica));
                OnPropertyChanged(nameof(IsWindowEffectAcrylic));
            }
        }

        public bool IsWindowEffectDefault
        {
            get => _windowEffect == WindowEffect.Default;
            set
            {
                if (value)
                {
                    WindowEffectType = WindowEffect.Default;
                }
            }
        }

        public bool IsWindowEffectTabbed
        {
            get => _windowEffect == WindowEffect.Tabbed;
            set
            {
                if (value)
                {
                    WindowEffectType = WindowEffect.Tabbed;
                }
            }
        }

        public bool IsWindowEffectMica
        {
            get => _windowEffect == WindowEffect.Mica;
            set
            {
                if (value)
                {
                    WindowEffectType = WindowEffect.Mica;
                }
            }
        }

        public bool IsWindowEffectAcrylic
        {
            get => _windowEffect == WindowEffect.Acrylic;
            set
            {
                if (value)
                {
                    WindowEffectType = WindowEffect.Acrylic;
                }
            }
        }

        private string _backgroundImage = string.Empty;
        public string BackgroundImage
        {
            get => _backgroundImage;
            set
            {
                _backgroundImage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BackgroundImageVisible));
                OnPropertyChanged(nameof(BackgroundImageExists));
            }
        }

        private bool _backgroundImageEnabled;
        public bool BackgroundImageEnabled
        {
            get => _backgroundImageEnabled;
            set
            {
                _backgroundImageEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BackgroundImageVisible));
            }
        }

        public bool BackgroundImageVisible => _backgroundImageEnabled &&
                                              !string.IsNullOrWhiteSpace(_backgroundImage) && File.Exists(_backgroundImage);
        public bool BackgroundImageExists => !string.IsNullOrWhiteSpace(_backgroundImage) && File.Exists(_backgroundImage);

        private float _backgroundImageOpacity = 1;
        public float BackgroundImageOpacity
        {
            get => _backgroundImageOpacity;
            set
            {
                _backgroundImageOpacity = value;
                OnPropertyChanged();
            }
        }

        private int _backgroundImageBlur = 0;
        public int BackgroundImageBlur
        {
            get => _backgroundImageBlur;
            set
            {
                _backgroundImageBlur = value;
                OnPropertyChanged();
            }
        }

        public bool IsTabbedSupported => Tools.OSVersionTools.IsMicaTabbedSupported;
        public bool IsMicaSupported => Tools.OSVersionTools.IsMicaSupported;
        public bool IsAcrylicSupported => Tools.OSVersionTools.IsAcrylicSupported;

        public ICommand OpenSystemThemeCommand => new ActionCommand(OnOpenSystemTheme);

        public ICommand BackgroundImageSelectCommand => new ActionCommand(OnBackgroundImageSelect);

        private void OnOpenSystemTheme()
        {
            if (!Tools.OSVersionTools.IsWindowsNT)
            {
                return;
            }
            if (Tools.OSVersionTools.IsWindows7OrLess)
            {
                Tools.FileTools.ProcessStart("control.exe", "/name Microsoft.Personalization");
            }
            else
            {
                Tools.FileTools.ProcessStart("ms-settings:personalization-colors");
            }
        }

        private static readonly string ImageFilter = "图片文件 (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        private void OnBackgroundImageSelect()
        {
            var option = new EventManager.FileDialogOption()
            {
                Filter = ImageFilter
            };
            var fileNames = EventManager.ShowFileDialog(option);

            if (fileNames != null && fileNames.Length > 0)
            {
                BackgroundImage = fileNames[0];
            }
        }
    }
}
