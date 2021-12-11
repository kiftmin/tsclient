using IOClasses;

namespace BranSystems.MQTT.Device.IOController.Parameter
{
    public class IOPortParameter
    {
        public PortType.Physical Type { get; set; } = PortType.Physical.Unknown;
        public int Channel { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
    }
}
