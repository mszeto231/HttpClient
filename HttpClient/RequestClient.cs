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
        private int _timeGap;               // amount of time (milliseconds) between each request
        private int _threadAmount;          // number of threads used to send requests
        private string _responseBody;       // response body of the request made
        private List<ResponseInfo> _responseList;

        private HttpClient _client;         // client used to create http requests
        private List<Thread> _threadList;   // list of threads used to run http requests
        private CountdownEvent _countdown;   // records when each thread is done

        public HttpMethods HttpMethodType { get; set; }
        public RequestType RequestType { get; set; }
        public int RequestValue { get; set; }
        public List<ResponseInfo> ResponseList
        {
            get { return _responseList; }
            set { _responseList = value; }
        }

        // binded to tbUrlRequest
        public string UrlRequest
        {
            get { return _urlRequest; }
            set { _urlRequest = value; }
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
            set
            { _threadAmount = value;
                NotifyPropertyChanged("ThreadAmount");
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
            _responseList = new List<ResponseInfo>();
        }

        /// <summary> checks if event has happended to binded textbox </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> notify if a property is changed </summary>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<ResponseInfo> SendRequestTestAsync()
        {
            ResponseInfo res = await Request();
            _responseList.Add(res);
            Thread.Sleep(_timeGap);
            return res;
        }
        /// <summary>
        /// Creates and sends an HTTP request and sets ResponseBody message
        /// </summary>
        public void SendRequest()
        {
            _responseList.Clear();
            ResponseBody = string.Empty;
            _countdown = new CountdownEvent(_threadAmount);
            DateTime startTime = DateTime.UtcNow;
            if (RequestType == RequestType.Amount)
            {
                // sends _requestAmount of http requests
                int reqPerThread = RequestValue / _threadAmount;
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
        public async Task<ResponseInfo> Request()
        {
            HttpRequestMessage request;
            if (HttpMethodType == HttpMethods.GET)
            {
                request = new HttpRequestMessage(HttpMethod.Get, _urlRequest);
            }
            else
            {
                request = new HttpRequestMessage(HttpMethod.Post, _urlRequest);
            }

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
                _responseList.Add(res);
                Thread.Sleep(_timeGap);
            }
            _countdown.Signal();
        }

        private async void DurationRequests()
        {
            double time = 0;
            while (time < RequestValue)
            {
                ResponseInfo res = await Request();
                time += res.Time;
                _responseList.Add(res);
                Thread.Sleep(_timeGap);
            }
            _countdown.Signal();
        }

        /// <returns>
        /// return true if the request parameters are valid and false if not
        /// </returns>
        public bool IsValidRequest()
        {
            return (_urlRequest != null && _timeGap != 0 && _threadAmount != 0 && RequestValue != 0);
        }

        /// <returns>
        /// return true if there is a request url
        /// </retuns>
        /// <returns></returns>
        public bool IsValidSample()
        {
            return _urlRequest != null;
        }

        public void SetThreadAmount(int threadAmount)
        {
            ThreadAmount = threadAmount;
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
