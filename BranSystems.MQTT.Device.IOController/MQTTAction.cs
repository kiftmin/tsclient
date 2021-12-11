
using BranSystems.MQTT.Extend;

namespace BranSystems.MQTT.Device.IOController
{
    public class MQTTAction : Action
    {
        public static MQTTAction Config = new MQTTAction($"{nameof(Config)}");
        public static MQTTAction ReadIOPort = new MQTTAction($"{nameof(ReadIOPort)}");
        public static MQTTAction ReadIO = new MQTTAction($"{nameof(ReadIO)}");
        public static MQTTAction SwitchOutput = new MQTTAction($"{nameof(SwitchOutput)}");

        public MQTTAction(string value) : base(value) { }

        public override string ToString()
        {
            return base.Value.ToLower();
        }
    }
}
