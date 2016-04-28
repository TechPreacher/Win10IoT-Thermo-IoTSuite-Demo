using System;
using System.Collections.Generic;
using System.Linq;
using Sensors.Dht;
using Win10IoT_Thermo.Common;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Win10IoT_Thermo
{
    public sealed partial class MainPage : BindablePage
    {
        private const int LED_PIN = 16;
        private const int DHT11_PIN = 4;

        private DispatcherTimer _timer = new DispatcherTimer();
        private DispatcherTimer _timerLed = new DispatcherTimer();

        //private AzureIot _azureIot;
        private SensorData _sensorData;
        private DeviceInfo _deviceInfo = new DeviceInfo();

        // Get deviceKey for deviceName and iotHubUri by adding new device to IoTSuite monitoring solution.
        private string _deviceName = "[replace]";

        // Set to true to send data to the IoTSuite.
        bool _sendToCloud = true;

        private GpioController _gpio = GpioController.GetDefault();
        private GpioPin _pinDht = null;
        private GpioPin _pinLed = null;
        private IDht _dht11 = null;

        private List<int> _retryCount = new List<int>();
        private DateTimeOffset _startedAt;

        public MainPage()
        {
            this.InitializeComponent();

            // Init device info.
            _deviceInfo.ObjectType = "DeviceInfo";
            _deviceInfo.IsSimulatedDevice = false;
            _deviceInfo.Version = "1.0";

            // Init device info (Properties).
            _deviceInfo.DeviceProperties = new DeviceProperties();
            _deviceInfo.DeviceProperties.HubEnabledState = true;
            _deviceInfo.DeviceProperties.DeviceID = _deviceName;
            _deviceInfo.DeviceProperties.Manufacturer = "Your Company";
            _deviceInfo.DeviceProperties.ModelNumber = "Your Model";
            _deviceInfo.DeviceProperties.SerialNumber = "Your Serial";
            _deviceInfo.DeviceProperties.FirmwareVersion = "Your Firmware";
            _deviceInfo.DeviceProperties.Platform = "Your Platform";
            _deviceInfo.DeviceProperties.Processor = "Your Processor";
            _deviceInfo.DeviceProperties.InstalledRAM = "1 GB";
            _deviceInfo.DeviceProperties.Latitude = 47;
            _deviceInfo.DeviceProperties.Longitude = 8;

            // Init timer for sensor measurements.
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += _timer_Tick;

            // Init timer for blinking led.
            _timerLed.Interval = TimeSpan.FromMilliseconds(500);
            _timerLed.Tick += _timerLed_Tick;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _pinDht = _gpio.OpenPin(DHT11_PIN, GpioSharingMode.Exclusive);
            _dht11 = new Dht11(_pinDht, GpioPinDriveMode.Input);

            _pinLed = _gpio.OpenPin(LED_PIN);
            _pinLed.SetDriveMode(GpioPinDriveMode.Output);
            _pinLed.Write(GpioPinValue.Low);

            _timer.Start();

            _startedAt = DateTimeOffset.Now;

            // Send IoT device info (once).
            if (_sendToCloud) {
                try {
                    await AzureIoTHub.SendDeviceToCloudMessageAsync(JsonConvert.SerializeObject(_deviceInfo));
                    _gridOffline.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex) {
                    Debug.WriteLine("Problem sending to IoT Hub: " + ex.Message.ToString());
                    _gridOffline.Visibility = Visibility.Visible;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _timer.Stop();
            _timerLed.Stop();

            _pinDht.Dispose();
            _pinDht = null;

            _dht11 = null;

            base.OnNavigatedFrom(e);
        }

        private async void _timer_Tick(object sender, object e)
        {
            DhtReading reading = new DhtReading();
            int val = this.TotalAttempts;
            this.TotalAttempts++;

            reading = await _dht11.GetReadingAsync().AsTask();

            _retryCount.Add(reading.RetryCount);
            this.OnPropertyChanged(nameof(AverageRetriesDisplay));
            this.OnPropertyChanged(nameof(TotalAttempts));
            this.OnPropertyChanged(nameof(PercentSuccess));

            if (reading.IsValid)
            {
                this.TotalSuccess++;
                this.Temperature = Convert.ToSingle(reading.Temperature);
                this.Humidity = Convert.ToSingle(reading.Humidity);
                this.LastUpdated = DateTimeOffset.Now;
                
                // Set IoT data.
                _sensorData = new SensorData();
                _sensorData.DeviceId = _deviceName;
                _sensorData.Temperature = Convert.ToSingle(reading.Temperature);
                _sensorData.ExternalTemperature = 20;
                _sensorData.Humidity = Convert.ToSingle(reading.Humidity);

                // Blink led.
                this.BlinkLed();

                // Send IoT data.
                if (_sendToCloud) { 
                    try {
                        await AzureIoTHub.SendDeviceToCloudMessageAsync(JsonConvert.SerializeObject(_sensorData));
                        _gridOffline.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("Problem sending to IoT Hub: " + ex.Message.ToString());
                        _gridOffline.Visibility = Visibility.Visible;
                    }
                }

                // Update UI.
                this.OnPropertyChanged(nameof(SuccessRate));
            }

            this.OnPropertyChanged(nameof(LastUpdatedDisplay));
        }

        private void _timerLed_Tick(object sender, object e)
        {
            _pinLed.Write(GpioPinValue.Low);
            _timerLed.Stop();
        }

        public void BlinkLed()
        {
            _pinLed.Write(GpioPinValue.High);
            _timerLed.Start();
        }

        public string PercentSuccess
        {
            get
            {
                string returnValue = string.Empty;

                int attempts = this.TotalAttempts;

                if (attempts > 0)
                {
                    returnValue = string.Format("{0:0.0}%", 100f * (float)this.TotalSuccess / (float)attempts);
                }
                else
                {
                    returnValue = "0.0%";
                }

                return returnValue;
            }
        }

        private int _totalAttempts = 0;
        public int TotalAttempts
        {
            get
            {
                return _totalAttempts;
            }
            set
            {
                this.SetProperty(ref _totalAttempts, value);
                this.OnPropertyChanged(nameof(PercentSuccess));
            }
        }

        private int _totalSuccess = 0;
        public int TotalSuccess
        {
            get
            {
                return _totalSuccess;
            }
            set
            {
                this.SetProperty(ref _totalSuccess, value);
                this.OnPropertyChanged(nameof(PercentSuccess));
            }
        }

        private float _humidity = 0f;
        public float Humidity
        {
            get
            {
                return _humidity;
            }

            set
            {
                this.SetProperty(ref _humidity, value);
                this.OnPropertyChanged(nameof(HumidityDisplay));
            }
        }

        public string HumidityDisplay
        {
            get
            {
                return string.Format("{0:0}% RH", this.Humidity);
            }
        }

        private float _temperature = 0f;
        public float Temperature
        {
            get
            {
                return _temperature;
            }
            set
            {
                this.SetProperty(ref _temperature, value);
                this.OnPropertyChanged(nameof(TemperatureDisplay));
            }
        }

        public string TemperatureDisplay
        {
            get
            {
                return string.Format("{0:0} °C", this.Temperature);
            }
        }

        private DateTimeOffset _lastUpdated = DateTimeOffset.MinValue;
        public DateTimeOffset LastUpdated
        {
            get
            {
                return _lastUpdated;
            }
            set
            {
                this.SetProperty(ref _lastUpdated, value);
                this.OnPropertyChanged(nameof(LastUpdatedDisplay));
            }
        }

        public string LastUpdatedDisplay
        {
            get
            {
                string returnValue = string.Empty;

                TimeSpan elapsed = DateTimeOffset.Now.Subtract(this.LastUpdated);

                if (this.LastUpdated == DateTimeOffset.MinValue)
                {
                    returnValue = "never";
                }
                else if (elapsed.TotalSeconds < 60d)
                {
                    int seconds = (int)elapsed.TotalSeconds;

                    if (seconds < 2)
                    {
                        returnValue = "just now";
                    }
                    else
                    {
                        returnValue = string.Format("{0:0} {1} ago", seconds, seconds == 1 ? "second" : "seconds");
                    }
                }
                else if (elapsed.TotalMinutes < 60d)
                {
                    int minutes = (int)elapsed.TotalMinutes == 0 ? 1 : (int)elapsed.TotalMinutes;
                    returnValue = string.Format("{0:0} {1} ago", minutes, minutes == 1 ? "minute" : "minutes");
                }
                else if (elapsed.TotalHours < 24d)
                {
                    int hours = (int)elapsed.TotalHours == 0 ? 1 : (int)elapsed.TotalHours;
                    returnValue = string.Format("{0:0} {1} ago", hours, hours == 1 ? "hour" : "hours");
                }
                else
                {
                    returnValue = "a long time ago";
                }

                return returnValue;
            }
        }

        public int AverageRetries
        {
            get
            {
                int returnValue = 0;

                if (_retryCount.Count() > 0)
                {
                    returnValue = (int)_retryCount.Average();
                }

                return returnValue;
            }
        }

        public string AverageRetriesDisplay
        {
            get
            {
                return string.Format("{0:0}", this.AverageRetries);
            }
        }

        public string SuccessRate
        {
            get
            {
                string returnValue = string.Empty;

                double totalSeconds = DateTimeOffset.Now.Subtract(_startedAt).TotalSeconds;
                double rate = this.TotalSuccess / totalSeconds;

                if (rate < 1)
                {
                    returnValue = string.Format("{0:0.00} s/r", 1d / rate);
                }
                else
                {
                    returnValue = string.Format("{0:0.00} r/s", rate);
                }

                return returnValue;
            }
        }
    }
}
