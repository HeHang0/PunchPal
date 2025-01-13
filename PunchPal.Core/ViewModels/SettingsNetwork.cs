using PunchPal.Core.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PunchPal.Core.ViewModels
{
    public class SettingsNetwork : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ProxyType _requestProxyType = ProxyType.System;

        public ProxyType RequestProxyType
        {
            get => _requestProxyType;
            set
            {
                _requestProxyType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNetProxyCustom));
                OnPropertyChanged(nameof(IsNetProxySystem));
                OnPropertyChanged(nameof(IsNetProxyNone));
                OnPropertyChanged(nameof(IsProxyServerAuthVisible));
            }
        }

        public bool IsNetProxySystem
        {
            get => _requestProxyType == ProxyType.System;
            set
            {
                if (value)
                {
                    RequestProxyType = ProxyType.System;
                }
            }
        }

        public bool IsNetProxyNone
        {
            get => _requestProxyType == ProxyType.None;
            set
            {
                if (value)
                {
                    RequestProxyType = ProxyType.None;
                }
            }
        }

        public bool IsNetProxyCustom
        {
            get => _requestProxyType == ProxyType.Custom;
            set
            {
                if (value)
                {
                    RequestProxyType = ProxyType.Custom;
                }
            }
        }

        private string _proxyServerAddress = string.Empty;
        public string ProxyServerAddress
        {
            get => _proxyServerAddress;
            set
            {
                _proxyServerAddress = value;
                OnPropertyChanged();
            }
        }

        private int _proxyServerPort = 8080;
        public int ProxyServerPort
        {
            get => _proxyServerPort;
            set
            {
                if(value > 0 && value < 65535)
                {
                    _proxyServerPort = value;
                }
                OnPropertyChanged();
            }
        }

        private bool _isProxyServerAuth = false;
        public bool IsProxyServerAuth
        {
            get => _isProxyServerAuth;
            set
            {
                _isProxyServerAuth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProxyServerAuthVisible));
            }
        }

        public bool IsProxyServerAuthVisible => IsNetProxyCustom && IsProxyServerAuth;

        private string _proxyUserName = string.Empty;
        public string ProxyUserName
        {
            get => _proxyUserName;
            set
            {
                _proxyUserName = value;
                OnPropertyChanged();
            }
        }

        private string _proxyPassword = string.Empty;
        public string ProxyPassword
        {
            get => _proxyPassword;
            set
            {
                _proxyPassword = value;
                OnPropertyChanged();
            }
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
