namespace BranSystems.MQTT.Interface
{
    public interface ISubscriber
    {
        IDetail Detail { get; set; }
        void Start();
        void Stop();
        void HandleSubscription(IMessage message);
    }
}
