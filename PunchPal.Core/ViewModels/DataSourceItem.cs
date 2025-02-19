using PunchPal.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public enum DataSourceRequestType
    {
        Get, Post, Put, Browser
    }
    public class DataSourceItem : NotifyPropertyBase
    {
        public enum DataSourceType
        {
            Authenticate, UserInfo, PunchTime, Attendance
        }

        public DataSourceItem(DataSourceType dataSourceType, DataSourceRequestType requestType = DataSourceRequestType.Post)
        {
            Type = dataSourceType;
            RequestMethod = requestType;
        }

        public void ResetMappings()
        {
            for (int i = 0; i < RequestMappings.Count;)
            {
                if (string.IsNullOrWhiteSpace(RequestMappings[i].Key))
                {
                    RequestMappings.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            switch (Type)
            {
                case DataSourceType.UserInfo:
                    OnAddRequestMapping(nameof(User.Name));
                    OnAddRequestMapping(nameof(User.Avator));
                    OnAddRequestMapping(nameof(User.UserId));
                    break;
                case DataSourceType.PunchTime:
                    OnAddRequestMapping(nameof(PunchRecord.PunchTime));
                    OnAddRequestMapping(nameof(PunchRecord.PunchType));
                    OnAddRequestMapping(nameof(PunchRecord.Remark));
                    break;
                case DataSourceType.Attendance:
                    OnAddRequestMapping(nameof(AttendanceRecord.AttendanceId));
                    OnAddRequestMapping(nameof(AttendanceRecord.AttendanceTypeId));
                    OnAddRequestMapping(nameof(AttendanceRecord.StartTime));
                    OnAddRequestMapping(nameof(AttendanceRecord.EndTime));
                    OnAddRequestMapping(nameof(AttendanceRecord.AttendanceTime));
                    OnAddRequestMapping(nameof(AttendanceRecord.Remark));
                    break;
            }
        }

        public class RequestMapping : NotifyPropertyBase
        {
            private string _key = string.Empty;
            public string Key
            {
                get => _key;
                set
                {
                    _key = value;
                    OnPropertyChanged();
                }
            }
            private string _value = string.Empty;
            public string Value
            {
                get => _value;
                set
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
            private string _scripts = string.Empty;
            public string Scripts
            {
                get => _scripts;
                set
                {
                    _scripts = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Name
        {
            get
            {
                switch (Type)
                {
                    case DataSourceType.Authenticate:
                        return "认证";
                    case DataSourceType.UserInfo:
                        return "用户信息";
                    case DataSourceType.PunchTime:
                        return "打卡时间";
                    case DataSourceType.Attendance:
                        return "考勤";
                    default:
                        return string.Empty;
                }
            }
        }
        public DataSourceType Type;
        private string _requestUrl = string.Empty;
        public string RequestUrl
        {
            get => _requestUrl;
            set
            {
                _requestUrl = value;
                OnPropertyChanged();
            }
        }
        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
        private DataSourceRequestType _requestMethod = DataSourceRequestType.Get;
        public DataSourceRequestType RequestMethod
        {
            get => _requestMethod;
            set
            {
                _requestMethod = value;
                OnPropertyChanged();
            }
        }
        private string _requestBody = string.Empty;
        public string RequestBody
        {
            get => _requestBody;
            set
            {
                _requestBody = value;
                OnPropertyChanged();
            }
        }
        private string _responseValue = string.Empty;
        public string ResponseValue
        {
            get => _responseValue;
            set
            {
                _responseValue = value;
                OnPropertyChanged();
            }
        }
        public bool CanAddMapping => Type == DataSourceType.Authenticate;
        public bool RequestMappingVisible => CanAddMapping || RequestMappings.Any();
        public ObservableCollection<RequestMapping> RequestHeaders { get; set; } = new ObservableCollection<RequestMapping>();
        public ObservableCollection<RequestMapping> RequestFilters { get; set; } = new ObservableCollection<RequestMapping>();
        public ObservableCollection<RequestMapping> RequestMappings { get; set; } = new ObservableCollection<RequestMapping>();

        public ICommand AddRequestHeader => new ActionCommand(OnAddRequestHeader);
        public ICommand AddRequestFilter => new ActionCommand(OnAddRequestFilter);
        public ICommand AddRequestMapping => new ActionCommand(OnAddRequestMapping);

        private void OnAddRequestHeader()
        {
            if (RequestHeaders.Any(m => string.IsNullOrEmpty(m.Key) && string.IsNullOrEmpty(m.Value)))
            {
                return;
            }
            RequestHeaders.Add(new RequestMapping());
        }

        private void OnAddRequestFilter()
        {
            if (RequestFilters.Any(m => string.IsNullOrEmpty(m.Key) && string.IsNullOrEmpty(m.Value)))
            {
                return;
            }
            RequestFilters.Add(new RequestMapping());
        }

        public void OnAddRequestMapping()
        {
            OnAddRequestMapping(string.Empty);
        }

        public void OnAddRequestMapping(string name)
        {
            if (RequestMappings.Any(m => m.Key == name))
            {
                return;
            }
            RequestMappings.Add(new RequestMapping { Key = name });
        }
    }
}
