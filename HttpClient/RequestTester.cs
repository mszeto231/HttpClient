using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientTester
{
    class RequestTester
    {
        public const int ONE_SECOND = 1000;

        private List<double> _timesList;
        private RequestClient _requestClient;
        private double _time;

        public RequestTester(RequestClient requestClient)
        {
            _timesList = new List<double>();
            _requestClient = requestClient;
        }

        /// <summary>
        /// Runs a number of requests in a row to get the data for an average request time
        /// </summary>
        /// <param name="testNums"> Number of requests used to test</param>
        public async Task RunTestsAsync(int testNums)
        {
            for (int i = 0; i < testNums; i++) {
                ResponseInfo response = await _requestClient.Request();
                _timesList.Add(response.Time);
            }
        }

        /// <returns> An average request time in milliseconds </returns>
        public double AverageRequestTime()
        {
            int cutoff = (int)(_timesList.Count * 0.1);
            double sum = 0;
            int count = 0;
            for (int i = cutoff; i < _timesList.Count - cutoff; i++) {
                sum += _timesList[i];
                count++;
            }
            _time = sum / count;
            return _time;
        }

        public int ComputeThreadCount(int requestsNum)
        {
            int num = (int)(ONE_SECOND / _time);
            return (int)Math.Ceiling((double)requestsNum / num);
        }
    }
}
