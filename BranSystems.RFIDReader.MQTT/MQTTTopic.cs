using BranSystems.MQTT.Extend;

namespace BranSystems.MQTT.Device.RFIDReader
{
    public class MQTTTopic : Action
    {
        public static MQTTTopic Status = new MQTTTopic($"{nameof(Status)}");
        public static MQTTTopic Tag = new MQTTTopic($"{nameof(Tag)}");
        public static MQTTTopic Tags = new MQTTTopic($"{nameof(Tags)}");

        public MQTTTopic(string value) : base(value) { }

        public override string ToString()
        {
            return base.Value.ToLower();
        }
    }
}
