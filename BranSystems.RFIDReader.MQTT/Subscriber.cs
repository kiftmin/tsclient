using BranSystems.MQTT.Interface;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using RFIDReader;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BranSystems.MQTT.Device.RFIDReader
{
    public class Subscriber : ISubscriber
    {
        private static IMqttClient _client;
        private static IMqttClientOptions _options;

        private bool _initialized = false;

        #region Properties
        public bool ConnectionState { get; set; } = false;
        public IRFID_Reader Reader { get; set; } = null;
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

        public Subscriber(MqttClientOptionsBuilder options, ref Publisher publisher) : this (options)
        {
            Publisher = publisher;
        }

        #region Methods
        public void Start()
        {
            try
            {
                Helper.ConsoleWriteLine("Starting RFID Reader subscriber...");

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

                    HandleSubscription(new MQTTMessage() {
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
                Helper.ConsoleWriteLine("RFID Reader publisher not initialized.", Helper.MsgType.Warning);
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
                //tag received
                else if (topicOnly.Equals(MQTTTopic.Tag.ToString()))
                {
                    Helper.ConsoleWriteLine($"Received tag: {message.Payload}", Helper.MsgType.Received);
                    try
                    {
                        OnTagReceived(new TagReceivedEventArgs()
                        {
                            Tag = JsonSerializer.Deserialize<RFID_Tag>(message.Payload)
                        });
                    }
                    catch (Exception ex)
                    {
                        Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                        if (ex.GetType().Equals(typeof(JsonException)))
                            Helper.ConsoleWriteLine($"Invalid RFID tag payload received: {message.Payload}", Helper.MsgType.Error);
                    }
                }
                //tags received
                else if (topicOnly.Equals(MQTTTopic.Tags.ToString()))
                {
                    Helper.ConsoleWriteLine($"Received tags: {message.Payload}", Helper.MsgType.Received);
                    try
                    {
                        OnTagsReceived(new TagsReceivedEventArgs()
                        {
                            Tags = new List<IRFID_Tag>(JsonSerializer.Deserialize<RFID_Tag[]>(message.Payload))
                        });
                    }
                    catch (Exception ex)
                    {
                        Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                        if (ex.GetType().Equals(typeof(JsonException)))
                            Helper.ConsoleWriteLine($"Invalid RFID tag payload received: {message.Payload}", Helper.MsgType.Error);
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

                            //TODO - replace with actual reader code
                            switch (action.Call)
                            {
                                case nameof(MQTTAction.ReadTag):
                                    Publisher.ProcessReadTagRequest();
                                    break;
                                case nameof(MQTTAction.ReadTags):
                                    Publisher.ProcessReadTagsRequest();
                                    break;
                                case nameof(MQTTAction.ReadUserData):
                                    Publisher.ProcessReadUserDataRequest(JsonSerializer.Deserialize<RFID_Tag>(action.Parameter.ToString())); //TODO - add tag to payload of ReadUserData
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

        protected virtual void OnTagReceived(TagReceivedEventArgs e)
        {
            EventHandler<TagReceivedEventArgs> handler = TagReceived;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<TagReceivedEventArgs> TagReceived;

        protected virtual void OnTagsReceived(TagsReceivedEventArgs e)
        {
            EventHandler<TagsReceivedEventArgs> handler = TagsReceived;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<TagsReceivedEventArgs> TagsReceived;

        protected virtual void OnConnectionStateReceived(ConnectionStateReceivedEventArgs e)
        {
            EventHandler<ConnectionStateReceivedEventArgs> handler = ConnectionStateReceived;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<ConnectionStateReceivedEventArgs> ConnectionStateReceived;
        #endregion
    }

    public class TagReceivedEventArgs : EventArgs
    {
        public IRFID_Tag Tag { get; set; }
    }

    public class TagsReceivedEventArgs : EventArgs
    {
        public List<IRFID_Tag> Tags { get; set; }
    }

    public class ConnectionStateReceivedEventArgs : EventArgs
    {
        public bool ConnectionState { get; set; }
    }
}
