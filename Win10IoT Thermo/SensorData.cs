using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win10IoT_Thermo
{
    public class SensorData
    {
        public string DeviceId { get; set; }
        public double Temperature { get; set; }
        public double ExternalTemperature { get; set; }
        public double Humidity { get; set; }

    }

    public class DeviceInfo
    {
        public string ObjectType { get; set; }
        public bool IsSimulatedDevice { get; set; }
        public string Version { get; set; }
        public DeviceProperties DeviceProperties { get; set; }
        public string Commands { get; set; }
    }

    public class DeviceProperties
    {
        public string DeviceID { get; set; }
        public bool HubEnabledState { get; set; }
        public string Manufacturer { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string Platform { get; set; }
        public string Processor { get; set; }
        public string InstalledRAM { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
