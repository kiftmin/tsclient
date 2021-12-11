namespace BranSystems.MQTT
{
    public interface IMessage
    {
        string Topic { get; set; }
        string Payload { get; set; }
    }
}
