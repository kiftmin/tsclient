using BranSystems.MQTT.Extend;

namespace BranSystems.MQTT.Device.IOController
{
    public class MQTTTopic : Action
    {
        public static MQTTTopic Status = new MQTTTopic($"{nameof(Status)}");
        public static MQTTTopic Config = new MQTTTopic($"{nameof(Config)}");
        public static MQTTTopic IO = new MQTTTopic($"{nameof(IO)}");
        public static MQTTTopic IOPort = new MQTTTopic($"{nameof(IOPort)}");
        public static MQTTTopic Error = new MQTTTopic($"{nameof(Error)}");

        public MQTTTopic(string value) : base(value) { }

        public override string ToString()
        {
            return base.Value.ToLower();
        }
    }
}
