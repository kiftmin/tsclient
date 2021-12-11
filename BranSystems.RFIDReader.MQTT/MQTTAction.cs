using BranSystems.MQTT.Extend;

namespace BranSystems.MQTT.Device.RFIDReader
{
    public class MQTTAction : Action
    {
        public static MQTTAction ReadTag = new MQTTAction($"{nameof(ReadTag)}");
        public static MQTTAction ReadTags = new MQTTAction($"{nameof(ReadTags)}");
        public static MQTTAction ReadUserData = new MQTTAction($"{nameof(ReadUserData)}");

        public MQTTAction(string value) : base(value) { }

        public override string ToString()
        {
            return base.Value.ToLower();
        }
    }
}
