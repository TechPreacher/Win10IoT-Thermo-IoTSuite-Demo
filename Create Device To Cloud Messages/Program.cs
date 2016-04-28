using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

namespace Create_Device_To_Cloud_Messages
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "[replace].azure-devices.net";
        static string deviceName = "[replace]";
        static string deviceKey = "[replace]";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, deviceKey));

            // Send device ID to cloud
            SendDeviceToCloudIdAsync();

            // Send sensor data to cloud (continuously)
            SendDeviceToCloudMessagesAsync();

            Console.ReadLine();
        }

        private static async void SendDeviceToCloudIdAsync()
        {
            DeviceInfo deviceInfo = new DeviceInfo();
            deviceInfo.DeviceProperties = new DeviceProperties();

            deviceInfo.ObjectType = "DeviceInfo";
            deviceInfo.IsSimulatedDevice = false;
            deviceInfo.Version = "1.0";

            deviceInfo.DeviceProperties.HubEnabledState = true;
            deviceInfo.DeviceProperties.DeviceID = "[replace]";
            deviceInfo.DeviceProperties.Manufacturer = "Your Name";
            deviceInfo.DeviceProperties.ModelNumber = "Your Model";
            deviceInfo.DeviceProperties.SerialNumber = "Your Serial";
            deviceInfo.DeviceProperties.FirmwareVersion = "1.00";
            deviceInfo.DeviceProperties.Platform = "Your Platform";
            deviceInfo.DeviceProperties.Processor = "Your Processor";
            deviceInfo.DeviceProperties.InstalledRAM = "16 MB";
            deviceInfo.DeviceProperties.Latitude = 47;
            deviceInfo.DeviceProperties.Longitude = 8;

            var messageString = JsonConvert.SerializeObject(deviceInfo);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
            Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double avgTemp = 20; // °C
            double avgHumidity = 30; // %
            Random randTemp = new Random();
            Random randHumidity = new Random();

            while (true)
            {
                double currentTemp = avgTemp + randTemp.NextDouble() * 4 - 2;
                double currentExternalTemp = avgTemp + randTemp.NextDouble() * 4 - 2;
                double currentHumidity = avgHumidity + randHumidity.NextDouble() * 4 - 2;

                SensorData sensorData = new SensorData();
                sensorData.DeviceId = "[replace]";
                sensorData.Temperature = currentTemp;
                sensorData.Humidity = currentHumidity;
                sensorData.ExternalTemperature = currentExternalTemp;

                var messageString = JsonConvert.SerializeObject(sensorData);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                Thread.Sleep(2500);
            }
        }
    }
}
