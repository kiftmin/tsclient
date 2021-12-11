namespace BranSystems.MQTT.Helper
{
    public abstract class Enumaration
    {
        public override string ToString()
        {
            return Value;
        }

        protected Enumaration(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }
    }
}
