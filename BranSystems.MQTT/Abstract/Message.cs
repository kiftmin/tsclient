using System;

namespace BranSystems.MQTT.Abstract
{
    public abstract class Message : IMessage
    {
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}
