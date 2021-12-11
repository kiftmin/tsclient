using BranSystems.MQTT.Extend;

namespace BranSystems.MQTT.Interface
{
    public interface IDetail
    {
        string Route { get; set; }
        Topic Topic { get; }
        Action Action { get; }
    }
}
