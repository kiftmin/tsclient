using BranSystems.MQTT.Extend;
using BranSystems.MQTT.Interface;

namespace BranSystems.MQTT.Abstract
{
    public abstract class Detail : IDetail
    {
        private string _route = string.Empty;

        public string Route
        {
            get => _route;
            set
            {
                _route = value;
                //add ending forward slash if not available
                while (!_route.EndsWith("/"))
                    _route += '/';
            }
        }

        public Topic Topic { get; set; }

        public Action Action { get; set; }
    }
}
