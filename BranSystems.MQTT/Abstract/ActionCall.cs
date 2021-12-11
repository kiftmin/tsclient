using BranSystems.MQTT.Extend;

namespace BranSystems.MQTT.Abstract
{
    public abstract class ActionCall
    {
        private Action Action { get; set; }
        public string Call { get => Action.Value; set => Action = new Action(value); }
        public object Parameter { get; set; } = null;
    }
}
