using BranSystems.MQTT.Abstract;
using BranSystems.MQTT.Interface;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using RFIDReader;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BranSystems.MQTT.Device.RFIDReader
{
    public class Publisher : IPublisher
    {
        private static IMqttClient _client;
        private static IMqttClientOptions _options;

        private bool _initialized = false;

        private static IProgress<string> ReaderStatus = new Progress<string>(value => { Helper.ConsoleWriteLine($"Reader status: {value}"); });

        #region Properties
        public IDetail Detail { get; set; }
        public IRFID_Reader Reader { get; set; }
        public int CheckReaderConnInterval { get; set; } = 5;

        public bool TestMode { get; set; } = false;
        #endregion

        public Publisher(MqttClientOptionsBuilder options)
        {
            _options = options.WithClientId($"{RemovePubSub(options.Build().ClientId)}_Pub").Build();
        }

        private string RemovePubSub(string text)
        {
            text = text.Replace("_Pub", string.Empty);
            text = text.Replace("_Sub", string.Empty);
            return text;
        }

        #region Methods
        public void Start()
        {
            Helper.ConsoleWriteLine("Starting RFID Reader publisher...");
            try
            {
                // Create a new MQTT client.
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();

                //handlers
                _client.UseConnectedHandler(e =>
                {
                    Helper.ConsoleWriteLine("Publisher connected successfully with MQTT Broker(s).");
                });
                _client.UseDisconnectedHandler(e =>
                {
                    Helper.ConsoleWriteLine("Publisher disconnected from MQTT Broker(s).");
                });
                _client.UseApplicationMessageReceivedHandler(e =>
                {
                    try
                    {
                        string topic = e.ApplicationMessage.Topic;
                        if (string.IsNullOrWhiteSpace(topic) == false)
                        {
                            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                            Helper.ConsoleWriteLine($"Topic: {topic}. Message Received: {payload}", Helper.MsgType.Received);
                        }
                    }
                    catch (Exception ex)
                    {
                        Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                    }
                });


                //connect
                _client.ConnectAsync(_options).Wait();

                Helper.ConsoleWriteLine("RFID Reader publisher initialized");

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

        public void Publish(IMessage message)
        {
            if (IsInitialized())
            {
                var testMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(message.Topic)
                    .WithPayload(message.Payload)
                    .WithExactlyOnceQoS()
                    //.WithRetainFlag()
                    .Build();

                if (_client.IsConnected)
                {
                    Helper.ConsoleWriteLine($"publishing {message.Topic} at {DateTime.UtcNow.ToLocalTime()}", Helper.MsgType.Sent);
                    _client.PublishAsync(testMessage);
                }
            }
        }

        private void SendTag(IRFID_Tag tag)
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route + MQTTTopic.Tag,
                Payload = JsonSerializer.Serialize(tag)
            });
        }

        private void SendTags(List<IRFID_Tag> tags)
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route + MQTTTopic.Tags,
                Payload = JsonSerializer.Serialize(tags)
            });
        }

        private void SendConnectionState(bool state)
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route + MQTTTopic.Status,
                Payload = JsonSerializer.Serialize(new MQTTStatus() { Connection = state })
            });
        }

        public async Task PushReaderConnectionStateAsync()
        {
            while (_initialized)
            {
                while (_client.IsConnected)
                {
                    if (Reader is not null)
                        SendConnectionState(Reader.CheckConnection());
                    else
                        SendConnectionState(false);
                    await Task.Delay(CheckReaderConnInterval * 1000);
                }
            }
        }

        public async Task PushConnectionStateAsync()
        {
            while (_initialized)
            {
                Helper.ConsoleWriteLine($"Publisher {(_client.IsConnected ? "connected" : "disconnected")}");
                await Task.Delay(5 * 1000);
            }
        }

        public void RequestReadTag()
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route,
                Payload = JsonSerializer.Serialize(new MQTTActionCall() { Call = MQTTAction.ReadTag.Value })
            });
        }

        public void RequestReadTags()
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route,
                Payload = JsonSerializer.Serialize(new MQTTActionCall() { Call = MQTTAction.ReadTags.Value })
            });
        }

        public void RequestReadUserData(IRFID_Tag tag)
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route,
                Payload = JsonSerializer.Serialize(new MQTTActionCall() { Call = MQTTAction.ReadUserData.Value, Parameter = (IRFID_Tag)tag })
            });
        }

        public void ProcessReadTagRequest()
        {
            if (TestMode)
                SendTag(new RFID_Tag("2Y68201955045148^4Y51118", -56));
            else if (Reader is not null)
            {
                var tags = Reader.ReadTags(ReaderStatus).Result;
                if (tags.Count == 0)
                    SendTag(new RFID_Tag(string.Empty, 0));
                else
                    SendTag(tags[0]);
            }
        }

        public void ProcessReadTagsRequest()
        {
            if (TestMode)
                SendTags(new List<IRFID_Tag>() { new RFID_Tag("2Y68201955045148^4Y51118", -56), new RFID_Tag("2Y68201955045159^4Y51118", -56) });
            else if (Reader is not null)
            {
                var tags = Reader.ReadTags(ReaderStatus).Result;
                if (tags.Count == 0)
                    SendTags(new List<IRFID_Tag>() { new RFID_Tag(string.Empty, 0) });
                else
                    SendTags(new List<IRFID_Tag>(tags));
            }
        }

        public void ProcessReadUserDataRequest(IRFID_Tag tag)
        {
            if (TestMode)
                SendTag(new RFID_Tag("2Y68201955045148^4Y51118", -56) { UserData = "682020024152156RS2L3" });
            else if (Reader is not null)
            {
                var updtag = (RFID_Tag)tag;
                var result = Reader.ReadData(ReaderStatus, updtag, new CancellationToken()).Result;
                if (!result)
                    SendTag(tag);
                else
                    SendTag(updtag);
            }
        }
        #endregion
    }
}
