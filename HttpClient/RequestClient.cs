using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HttpClient
{
    class RequestClient : INotifyPropertyChanged
    {
        private string _urlRequest;
        private int _requestAmount;
        private int _timeGap;
        private int _threadAmount;
        private int _duration;

        public string UrlRequest
        {
            get { return _urlRequest; }
            set
            {
                if (value != _urlRequest)
                {
                    _urlRequest = value;
                    NotifyPropertyChanged("UrlRequest");
                }
            }
        }

        public int RequestAmount
        {
            get { return _requestAmount; }
            set
            {
                if (value != 0)
                {
                    Duration = 0;
                    NotifyPropertyChanged("Duration");
                }
                _requestAmount = value;
                NotifyPropertyChanged("RequestAmount");
            }
        }

        public int TimeGap
        {
            get { return _timeGap; }
            set
            {
                if (value != _timeGap)
                {
                    _timeGap = value;
                    NotifyPropertyChanged("TimeGap");
                }
            }
        }

        public int ThreadAmount
        {
            get { return _threadAmount; }
            set
            {
                if (value != _threadAmount)
                {
                    _threadAmount = value;
                    NotifyPropertyChanged("ThreadAmount");
                }
            }
        }

        public int Duration
        {
            get { return _duration; }
            set
            {
                if (value != 0)
                {
                    RequestAmount = 0;
                    NotifyPropertyChanged("RequestAmount");
                }
                _duration = value;
                NotifyPropertyChanged("Duration");
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }
        public event PropertyChangedEventHandler PropertyChanged;

    }
}
