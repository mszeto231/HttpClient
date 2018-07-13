using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientTester
{
    class ResponseInfo
    {
        private double _time;                     // http response time in milliseconds
        private HttpStatusCode _statusCode;     // http response status code
        private string _message;                // message from http response

        public double Time
        {
            get { return _time; }
            set { _time = value; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
            set { _statusCode = value; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// Initalizes the ResponseInfo object
        /// </summary>
        /// <param name="message"> message that is returned by the response</param>
        /// <param name="time"> amount of time in milliseconds response takes</param>
        /// <param name="message"> message that is returned by the response</param>
        public ResponseInfo(string message = null, double time = 0, HttpStatusCode code = 0)
        {
            _message = message;
            _time = time;
            _statusCode = code;
        }
    }
}
