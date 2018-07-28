﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HttpClientTester
{
    public enum HttpMethods { GET, POST }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum RequestType
    {
        [Description("Number of Requests")]
        Amount,
        [Description("Duration (milliseconds)")]
        Duration
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int REQUEST_TRYS = 100;
        public const int REQUEST_NUMBER = 100;
        private static readonly Regex _regex = new Regex("^[0-9]+$");  // text expression is a number

        private RequestClient _requestClient;
        private RequestTester _requestTester;

        public MainWindow()
        {
            InitializeComponent();
            _requestClient = new RequestClient();
            _requestTester = new RequestTester(_requestClient);
            mainGrid.DataContext = _requestClient;
        }

        /// <summary> allows only numbers to be inputed to textbox </summary>
        private void TextBox_PreviewNumbersOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumber(e.Text);
        }

        /// <summary> Sends http request when button is clicked. Input parameters must be valid </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _requestClient.RequestType = (RequestType)cmbRequestType.SelectedItem;
            _requestClient.HttpMethodType = (HttpMethods)cmbHttpMethod.SelectedIndex;
           // _requestClient.ResponseList.Clear();

            if (_requestClient.IsValidRequest())
            {
                PbStatusBar.Maximum = _requestClient.RequestValue;
                lvResponse.ItemsSource = _requestClient.ResponseList;

                if (_requestClient.RequestType == RequestType.Amount)
                {
                    _requestClient.SendAmountRequest(lvResponse, PbStatusBar);
                }
                else if (_requestClient.RequestType == RequestType.Duration)
                {
                    await _requestClient.SendDurationRequestAsync(lvResponse, PbStatusBar);
                }
            }
        }

        /// <param name="text"> any input string that client inputs into a textbook </param>
        /// <returns> returns true if input text is a number, otherwise returns false</returns>
        private bool IsNumber(string text)
        {
            return _regex.IsMatch(text);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_requestClient.IsValidSample())
            {
                await _requestTester.RunTestsAsync(REQUEST_TRYS);
                double time = _requestTester.AverageRequestTime();
                int threads = _requestTester.ComputeThreadCount(REQUEST_NUMBER);
                //_requestClient.SetThreadAmount(threads);
                //Console.WriteLine("Test: " + _requestClient.ThreadAmount);
            }
        }
    }
}
