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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HttpClientTester
{
    // Handles http requests based off of user input
    class RequestClient : INotifyPropertyChanged
    {
        private string _urlRequest;         // url of http request
        private int _timeGap;               // amount of time (milliseconds) between each request
        private List<ResponseInfo> _responseList;

        private HttpClient _client;         // client used to create http requests

        public HttpMethods HttpMethodType { get; set; }
        public RequestType RequestType { get; set; }
        public int RequestValue { get; set; }
        public string PostBody { get; set; }
        public int ConcurrentUsers { get; set; }
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


        public RequestClient()
        {
            _client = new HttpClient();
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

        public async Task SendAmountRequest(ListView listView, ProgressBar progressBar)
        {
            var task = Task.Run(() =>
            {
                int numRequests = RequestValue / ConcurrentUsers;
                int count = 0;
                Parallel.For(0, ConcurrentUsers, i =>
                {
                    while (count < RequestValue)
                    {
                        ResponseInfo res = Request().Result;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (_responseList.Count < RequestValue)
                            {
                                Update(res, listView);
                                progressBar.Value = _responseList.Count;
                            }
                            count++;
                        });
                        Thread.Sleep(_timeGap);
                    }
                });
            });
            await task;
        }
        
        public async Task SendDurationRequestAsync(ListView listView, ProgressBar progressBar)
        {
            var task = Task.Run(() => 
            {
                double time = 0;
                Parallel.For(0, ConcurrentUsers, i =>
                {
                    while (time < RequestValue)
                    {
                        ResponseInfo res = Request().Result;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            double timeCount = 0;
                            for (int j = 0; i < _responseList.Count; i++)
                            {
                                timeCount += _responseList[j].Time;
                            }
                            if (timeCount < RequestValue)
                            {
                                time += res.Time;
                                Update(res, listView);
                                progressBar.Value = time;
                            }
                        });
                        Thread.Sleep(_timeGap);
                    }
                });
            });
            await task;
        }

        private void Update(ResponseInfo res, ListView listView)
        {
            _responseList.Add(res);
            listView.Items.Refresh();
            var selectedIndex = listView.Items.Count - 1;
            if (selectedIndex < 0)
                return;

            listView.SelectedIndex = selectedIndex;
            listView.ScrollIntoView(listView.SelectedItem);
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
                request = new HttpRequestMessage(HttpMethod.Post, _urlRequest)
                {
                    Content = new StringContent(PostBody, Encoding.UTF8, "application/json")
                };
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
                return new ResponseInfo(ex.Message, 0, HttpStatusCode.NotFound);
            }
        }

        /// <returns>
        /// return true if the request parameters are valid and false if not
        /// </returns>
        public bool IsValidRequest()
        {
            return (_urlRequest != null && _timeGap != 0 && ConcurrentUsers != 0 && RequestValue != 0);
        }

        /// <returns>
        /// return true if there is a request url
        /// </retuns>
        /// <returns></returns>
        public bool IsValidSample()
        {
            return _urlRequest != null;
        }

        public int AverageRequestTime()
        {
            int cutoff = 0;
            if (ResponseList.Count >= 10)
            {
                cutoff = (int)(ResponseList.Count * 0.1);
            }
            double sum = 0;
            int count = 0;

            for (int i = cutoff; i < ResponseList.Count - cutoff; i++)
            {
                sum += ResponseList[i].Time;
                count++;
            }
            return (int) (sum / count);
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
