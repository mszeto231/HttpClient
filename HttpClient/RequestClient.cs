using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientTester
{
    // Handles http requests based off of user input
    class RequestClient : INotifyPropertyChanged
    {
        private string _urlRequest;         // url of http request
        private int _requestAmount;         // number of requests to be sent
        private int _timeGap;               // amount of time (milliseconds) between each request
        private int _threadAmount;          // number of threads used to send requests
        private int _duration;              // amount of time (milliseconds) for requests to be sent
        private string _responseBody;       // response body of the request made

        private HttpClient _client;         // client used to create http requests
        private List<Thread> _threadList;   // list of threads used to run http requests
        private CountdownEvent _countdown;   // records when each thread is done

        // binded to tbUrlRequest
        public string UrlRequest
        {
            get { return _urlRequest; }
            set {_urlRequest = value; }
        }

        // binded to tbRequestAmount
        // if !0, sets Duration to 0
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
            }
        }

        // binded to tbTimeGap
        public int TimeGap
        {
            get { return _timeGap; }
            set { _timeGap = value;}
        }

        // binded to tbThreadAmount
        public int ThreadAmount
        {
            get { return _threadAmount; }
            set { _threadAmount = value; }
        }

        // binded to tbDuration
        // if !0, setRequestAmount to 0
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
            }
        }

        // binded to tbResponseBody
        public string ResponseBody
        {
            get { return _responseBody; }
            set
            {
                _responseBody = value;
                NotifyPropertyChanged("ResponseBody");
            }
        }

        public RequestClient()
        {
            _client = new HttpClient();
            _threadList = new List<Thread>();
        }

        /// <summary> checks if event has happended to binded textbox </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> notify if a property is changed </summary>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Creates and sends an HTTP request and sets ResponseBody message
        /// </summary>
        public void SendRequest()
        {
            ResponseBody = string.Empty;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _urlRequest);
            _countdown = new CountdownEvent(_threadAmount);
            DateTime startTime = DateTime.UtcNow;
            if (_requestAmount != 0)
            {
                // sends _requestAmount of http requests
                int reqPerThread = _requestAmount / _threadAmount;
                for (int i = 0; i < _threadAmount; i++)
                {
                    Thread thread = new Thread(() => AmountRequests(reqPerThread));
                    _threadList.Add(thread);
                    thread.Start();
                }
            }
            else
            {
                // sends Http requests for _duration milliseconds
                for (int i = 0; i < _threadAmount; i++)
                {
                    Thread thread = new Thread(() => DurationRequests());
                    _threadList.Add(thread);
                    thread.Start();
                }
            }

            _countdown.Wait();
            double milliseconds = (DateTime.UtcNow - startTime).TotalMilliseconds;

            Console.WriteLine("Time: " + milliseconds);
            _threadList.Clear();
        }

        /// <summary> Creates and sends an HTTP request  </summary>
        /// <returns> Information about the HTTP response </returns>
        private async Task<ResponseInfo> Request()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _urlRequest);
            try
            {
                DateTime startTime = DateTime.UtcNow;
                HttpResponseMessage response = await _client.SendAsync(request);
                double milliseconds = (DateTime.UtcNow - startTime).TotalMilliseconds;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string message = await response.Content.ReadAsStringAsync();
                    return new ResponseInfo(message, milliseconds, HttpStatusCode.OK);
                }
                else
                {
                    return new ResponseInfo(response.StatusCode.ToString(), milliseconds, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return new ResponseInfo(ex.Message);
            }
        }

        private async void AmountRequests(int reqPerThread)
        {
            for (int i = 0; i < reqPerThread; i++)
            {
                ResponseInfo res = await Request();
                Console.WriteLine(res.Time);
                if (res.Time == 0)
                {
                    Console.WriteLine(res.Message);
                }
                ResponseBody += res.Message + "\n";
                Thread.Sleep(_timeGap);
            }
            _countdown.Signal();
        }

        private async void DurationRequests()
        {
            double time = 0;
            while (time < _duration)
            {
                ResponseInfo res = await Request();
                Console.WriteLine(res.Time);
                time += res.Time;
                ResponseBody += res.Message + "\n";
                Thread.Sleep(_timeGap);
            }
            _countdown.Signal();
        }

        /// <returns>
        /// return true if the request parameters are valid and false if not
        /// </returns>
        public bool IsValidRequest()
        {
            return (_urlRequest != null && _timeGap != 0 && _threadAmount != 0) && (_duration != 0 || _requestAmount != 0);
        }

        /// <returns>
        /// returns string format of object in form of  {Property: value, Property: value, ect}
        /// </returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            PropertyInfo[] properties = this.GetType().GetProperties();

            foreach (PropertyInfo info in properties)
            {
                var value = info.GetValue(this, null) ?? "null";
                result.Append("\t" + info.Name + ": " + value.ToString() + "\n");
            }
            return "{\n" + result + "}";
        }
    }
}
