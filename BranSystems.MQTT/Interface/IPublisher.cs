namespace BranSystems.MQTT.Interface
{
    public interface IPublisher
    {
        IDetail Detail { get; set; }
        void Start();
        void Stop();
        void Publish(IMessage message);
    }
}
