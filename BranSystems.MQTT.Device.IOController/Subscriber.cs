using BranSystems.MQTT.Interface;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using IOClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BranSystems.MQTT.Device.IOController
{
    public class Subscriber : ISubscriber
    {
        private static IMqttClient _client;
        private static IMqttClientOptions _options;

        private bool _initialized = false;

        #region Properties
        public bool ConnectionState { get; set; } = false;
        public IIo Io { get; set; } = null;
        public IDetail Detail { get; set; }
        public Publisher Publisher { get; set; } = null;
        #endregion

        public Subscriber(MqttClientOptionsBuilder options)
        {
            _options = options.WithClientId($"{RemovePubSub(options.Build().ClientId)}_Sub").Build();
        }

        private string RemovePubSub(string text)
        {
            text = text.Replace("_Pub", string.Empty);
            text = text.Replace("_Sub", string.Empty);
            return text;
        }

        public Subscriber(MqttClientOptionsBuilder options, ref Publisher publisher) : this(options)
        {
            Publisher = publisher;
        }

        #region Methods
        public void Start()
        {
            try
            {
                Helper.ConsoleWriteLine("Starting IO subscriber...");

                //create subscriber client
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();

                //Handlers
                _client.UseConnectedHandler(e =>
                {
                    Helper.ConsoleWriteLine("Subscriber connected successfully with MQTT Broker(s).");

                    //Subscribe to topic (# wild card for all topics)
                    _client.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{Detail.Route}#").Build()).Wait();

                    Helper.ConsoleWriteLine($"Client connected: {_client.IsConnected}");
                    Helper.ConsoleWriteLine($"Client ID: {_client.Options.ClientId}");
                    Helper.ConsoleWriteLine($"Client Options: {_client.Options}");
                });
                _client.UseDisconnectedHandler(e =>
                {
                    Helper.ConsoleWriteLine("Subscriber disconnected from MQTT Broker(s).");
                });
                _client.UseApplicationMessageReceivedHandler(e =>
                {
                    Helper.ConsoleWriteLine("### RECEIVED APPLICATION MESSAGE ###\n" +
                            $"+ Topic = {e.ApplicationMessage.Topic}\n" +
                            $"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}\n" +
                            $"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}\n" +
                            $"+ Retain = {e.ApplicationMessage.Retain}",
                            Helper.MsgType.Received
                        );

                    HandleSubscription(new MQTTMessage()
                    {
                        Topic = e.ApplicationMessage.Topic,
                        Payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload)
                    });
                });

                //actually connect
                _client.ConnectAsync(_options).Wait();

                _initialized = true;
            }
            catch (Exception e)
            {
                Helper.ConsoleWriteLine(e.ToString(), Helper.MsgType.Error);
                throw;
            }
        }

        public void Stop()
        {
            _client.DisconnectAsync().Wait();
        }

        private bool IsInitialized()
        {
            if (!_initialized)
                Helper.ConsoleWriteLine("IO publisher not initialized.", Helper.MsgType.Warning);
            return _initialized;
        }

        private async Task PushConnectionStateAsync()
        {
            while (_initialized)
            {
                Helper.ConsoleWriteLine($"Subscriber {(_client.IsConnected ? "connected" : "disconnected")}");
                await Task.Delay(5 * 1000);
            }
        }

        private string GetTopicOnly(string topic)
        {
            return topic.Replace(Detail.Route, string.Empty);
        }

        public void HandleSubscription(IMessage message)
        {
            if (IsInitialized())
            {
                Helper.ConsoleWriteLine($"received {message.Topic} at {DateTime.UtcNow.ToLocalTime()}", Helper.MsgType.Received);

                var topicOnly = GetTopicOnly(message.Topic);

                //connection state received
                if (topicOnly.Equals(MQTTTopic.Status.ToString()))
                {
                    Helper.ConsoleWriteLine($"Received connection state: {message.Payload}", Helper.MsgType.Received);
                    var state = false;
                    try
                    {
                        state = JsonSerializer.Deserialize<MQTTStatus>(message.Payload).Connection;
                    }
                    catch (Exception ex)
                    {
                        Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                        if (ex.GetType().Equals(typeof(JsonException)))
                            Helper.ConsoleWriteLine($"Invalid connection state payload received: {message.Payload}", Helper.MsgType.Error);
                    }
                    OnConnectionStateReceived(new ConnectionStateReceivedEventArgs()
                    {
                        ConnectionState = state
                    });
                }
                //Config received
                else if (topicOnly.Equals(MQTTTopic.Config.ToString()))
                {
                    Helper.ConsoleWriteLine($"Received configuration: {message.Payload}", Helper.MsgType.Received);
                    try
                    {
                        var json = JsonDocument.Parse(message.Payload);
                        Type type = typeof(IIo);

                        var deserializeOptions = new JsonSerializerOptions();
                        deserializeOptions.Converters.Add(new TypeMappingConverter<IPort, IoPort>());

                        switch ((IOs.IO)json.RootElement.GetProperty("IoType").GetInt32())
                        {
                            case IOs.IO.EAGLE:
                                OnConfigReceived(new ConfigReceivedEventArgs()
                                {
                                    Io = JsonSerializer.Deserialize<Eagle>(message.Payload, deserializeOptions)
                                });
                                break;
                            case IOs.IO.MOXA:
                                OnConfigReceived(new ConfigReceivedEventArgs()
                                {
                                    Io = JsonSerializer.Deserialize<Moxa>(message.Payload, deserializeOptions)
                                });
                                break;
                            default:
                                OnConfigReceived(new ConfigReceivedEventArgs()
                                {
                                    Io = JsonSerializer.Deserialize<IIo>(message.Payload, deserializeOptions)
                                });
                                break;
                        }

                        //OnConfigReceived(new ConfigReceivedEventArgs()
                        //{
                        //    Io = JsonSerializer.Deserialize<IIo>(message.Payload)
                        //});
                    }
                    catch (Exception ex)
                    {
                        Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                        if (ex.GetType().Equals(typeof(JsonException)))
                            Helper.ConsoleWriteLine($"Invalid IO configuration payload received: {message.Payload}", Helper.MsgType.Error);
                    }
                }
                //IO received
                else if (topicOnly.Equals(MQTTTopic.IO.ToString()))
                {
                    Helper.ConsoleWriteLine($"Received IO: {message.Payload}", Helper.MsgType.Received);
                    try
                    {
                        OnIoReceived(new IoReceivedEventArgs()
                        {
                            Ports = new List<IPort>(JsonSerializer.Deserialize<IoPort[]>(message.Payload))
                        });
                    }
                    catch (Exception ex)
                    {
                        Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                        if (ex.GetType().Equals(typeof(JsonException)))
                            Helper.ConsoleWriteLine($"Invalid IO payload received: {message.Payload}", Helper.MsgType.Error);
                    }
                }
                //IO port received
                else if (topicOnly.Equals(MQTTTopic.IOPort.ToString()))
                {
                    Helper.ConsoleWriteLine($"Received IO port: {message.Payload}", Helper.MsgType.Received);
                    try
                    {
                        OnIoPortReceived(new IoPortReceivedEventArgs()
                        {
                            Port = JsonSerializer.Deserialize<IoPort>(message.Payload)
                        });
                    }
                    catch (Exception ex)
                    {
                        Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                        if (ex.GetType().Equals(typeof(JsonException)))
                            Helper.ConsoleWriteLine($"Invalid IO port payload received: {message.Payload}", Helper.MsgType.Error);
                    }
                }
                //action received
                else if (topicOnly.Length == 0)
                {
                    if (Publisher is null)
                        Helper.ConsoleWriteLine($"Publisher not loaded to action requests");
                    else
                    {
                        try
                        {
                            var action = JsonSerializer.Deserialize<MQTTActionCall>(message.Payload);

                            switch (action.Call)
                            {
                                case nameof(MQTTAction.Config):
                                    Publisher.ProcessConfigRequest();
                                    break;
                                case nameof(MQTTAction.ReadIO):
                                    Publisher.ProcessIoRequest();
                                    break;
                                case nameof(MQTTAction.ReadIOPort):
                                    Publisher.ProcessIoPortRequest(JsonSerializer.Deserialize<IoPort>(action.Parameter.ToString()));
                                    break;
                                case nameof(MQTTAction.SwitchOutput):
                                    Publisher.ProcessSwitchOutputRequest(JsonSerializer.Deserialize<IoPort>(action.Parameter.ToString()));
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                            if (ex.GetType().Equals(typeof(JsonException)))
                                Helper.ConsoleWriteLine($"Invalid action payload received: {message.Payload}", Helper.MsgType.Error);
                        }
                    }
                }
                else
                {
                    Helper.ConsoleWriteLine($"Unrecognized topic received: {message.Topic}", Helper.MsgType.Error);
                }
            }
        }

        protected virtual void OnConfigReceived(ConfigReceivedEventArgs e)
        {
            EventHandler<ConfigReceivedEventArgs> handler = ConfigReceived;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<ConfigReceivedEventArgs> ConfigReceived;

        protected virtual void OnIoReceived(IoReceivedEventArgs e)
        {
            EventHandler<IoReceivedEventArgs> handler = IoReceived;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<IoReceivedEventArgs> IoReceived;

        protected virtual void OnIoPortReceived(IoPortReceivedEventArgs e)
        {
            EventHandler<IoPortReceivedEventArgs> handler = IoPortReceived;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<IoPortReceivedEventArgs> IoPortReceived;

        protected virtual void OnConnectionStateReceived(ConnectionStateReceivedEventArgs e)
        {
            EventHandler<ConnectionStateReceivedEventArgs> handler = ConnectionStateReceived;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<ConnectionStateReceivedEventArgs> ConnectionStateReceived;
        #endregion
    }

    public class ConfigReceivedEventArgs : EventArgs
    {
        public IIo Io { get; set; }
    }

    public class IoReceivedEventArgs : EventArgs
    {
        public List<IPort> Ports { get; set; }
    }

    public class IoPortReceivedEventArgs : EventArgs
    {
        public IPort Port { get; set; }
    }

    public class ConnectionStateReceivedEventArgs : EventArgs
    {
        public bool ConnectionState { get; set; }
    }
}
