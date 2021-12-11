using BranSystems.MQTT.Interface;
using ErrorLog;
using IOClasses;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BranSystems.MQTT.Device.IOController
{
    public class Publisher : IPublisher
    {
        private static IMqttClient _client;
        private static IMqttClientOptions _options;

        private bool _initialized = false;

        private static IProgress<string> IoStatus = new Progress<string>(value => { Helper.ConsoleWriteLine($"IO status: {value}"); });

        private List<IPort> _ports;

        #region Properties
        public IDetail Detail { get; set; }
        public IIo Io { get; set; }
        public int CheckReaderConnInterval { get; set; } = 5;

        public bool TestMode { get; set; } = false;
        public bool SendIOSimultaneous { get; set; } = false;
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
            Helper.ConsoleWriteLine("Starting IO publisher...");
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

                Helper.ConsoleWriteLine("IO publisher initialized");

                _initialized = true;

                SetupIo();
            }
            catch (Exception e)
            {
                Helper.ConsoleWriteLine(e.ToString(), Helper.MsgType.Error);
                throw;
            }
        }

        private void SetupIo()
        {
            if (Io is not null)
            {
                Io.InputStateChanged += OnIoStateChange;
                Io.OutputStateChanged += OnIoStateChange;
                Io.Connect();
                Helper.ConsoleWriteLine("IO loaded");
                Task.Run( async () =>
                {
                    await Task.Delay(2000);
                    Io.ForceDigitalInputPoll();
                }).Wait();
            }
            else
                Helper.ConsoleWriteLine("No IO loaded", Helper.MsgType.Warning);
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

        private void SendIoConfig(IIo io)
        {
            var settings = new JsonSerializerOptions();
            //settings.Converters.Add(new JsonIoConverter());
            //settings.WriteIndented = true;
            //settings.ReferenceHandler = ReferenceHandler.Preserve;
            //settings.Converters.Add(new JsonIoConverter());

            Publish(new MQTTMessage()
            {
                Topic = Detail.Route + MQTTTopic.Config,
                Payload = JsonSerializer.Serialize(io, settings)
            });
        }

        private void SendIo(List<IPort> ports)
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route + MQTTTopic.IO,
                Payload = JsonSerializer.Serialize(ports)
            });
        }

        private void SendIoPort(IPort port)
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route + MQTTTopic.IOPort,
                Payload = JsonSerializer.Serialize(port)
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

        public async Task PushIoConnectionStateAsync()
        {
            while (_initialized)
            {
                while (_client.IsConnected)
                {
                    if (Io is not null)
                        SendConnectionState(await Io.CheckConnection());
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

        public void RequestConfig()
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route,
                Payload = JsonSerializer.Serialize(new MQTTActionCall() { Call = nameof(MQTTAction.Config) })
            });
        }

        public void RequestReadIO()
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route,
                Payload = JsonSerializer.Serialize(new MQTTActionCall() { Call = nameof(MQTTAction.ReadIO) })
            });
        }

        public void RequestReadIOPort(IPort port)
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route,
                Payload = JsonSerializer.Serialize(new MQTTActionCall() { Call = nameof(MQTTAction.ReadIOPort), Parameter = port })
            });
        }

        public void RequestSwitchOutput(IPort port)
        {
            Publish(new MQTTMessage()
            {
                Topic = Detail.Route,
                Payload = JsonSerializer.Serialize(new MQTTActionCall() { Call = nameof(MQTTAction.SwitchOutput), Parameter = port })
            });
        }

        public void ProcessConfigRequest()
        {
            if (TestMode)
                SendIoConfig(new Moxa("127.0.0.1", "", new Channels(8, 8, 0, 0, 8, 0, 8, 0, 0, 0), new TextLog("IO_MQTT")));
            else if (Io is not null)
                SendIoConfig(Io);
        }

        public void ProcessIoRequest()
        {
            if (TestMode)
                SendIo(new List<IPort>() {
                    new IoPort() { Name = "DI0", Description = "Digital IN", Position = 0, Value = true.ToString() },
                    new IoPort() { Name = "DIO0", Description = "Digital OUT", Position = 0, Value = false.ToString() }
                });
            else if (Io is not null)
                SendIo(Io.Ports);
        }

        public void ProcessIoPortRequest(Port port)
        {
            if (TestMode)
                SendIoPort(new IoPort() { Name = port.Name, Description = port.Description, Position = port.Position, Value = (!bool.Parse(port.Value)).ToString() });
            else if (Io is not null)
                SendIoPort(Io.Ports.First(a => a.Name.Equals(port.Name) && a.Position.Equals(port.Position) && a.PhysicalType.Equals(port.PhysicalType)));
        }

        public void ProcessSwitchOutputRequest(Port port)
        {
            if (TestMode)
                SendIo(new List<IPort>() {
                    new IoPort() { Name = "DI0", Description = "Digital IN", Position = 0, Value = true.ToString(), PhysicalType = PortType.Physical.DI, ChannelType = PortType.Channel.DI },
                    new IoPort() { Name = "DIO0", Description = "Digital OUT", Position = 0, Value = true.ToString(), PhysicalType = PortType.Physical.DIO, ChannelType = PortType.Channel.DO }
                });
            else if (Io is not null)
            {
                switch (port.ChannelType)
                {
                    case PortType.Channel.DO:
                        var state = false;
                        bool.TryParse(port.Value, out state);
                        Io.WriteDigitalOutput(port.Position, state);
                        break;
                    case PortType.Channel.AO:
                        ushort val = 0;
                        ushort.TryParse(port.Value, out val);
                        Io.WriteAnalogueOutput(port.Position, val);
                        break;
                    default:
                        //do nothing
                        break;
                }
            }
        }

        private void OnIoStateChange(IIo io)
        {
            if (io is null)
                return;

            //for sending all IO simultaneous
            if (SendIOSimultaneous)
            {
                SendIo(io.Ports);
                return;
            }

            //send each IO independently
            //properly copy each item in ports to temp
            List<IPort> temp = new();
            foreach (var port in io.Ports)
            {
                temp.Add(
                    new IoPort()
                    {
                        Name = port.Name,
                        Description = port.Description,
                        PhysicalType = port.PhysicalType,
                        ChannelType = port.ChannelType,
                        Position = port.Position,
                        Value = port.Value
                    }
                );
            };
            if (_ports is null)
                //send all ports since first startup
                foreach (var port in temp)
                    SendIoPort(port);
            else
            {
                //send only ports that changed
                foreach (var port in temp)
                {
                    try
                    {
                        var val = _ports.First(a => a.PhysicalType.Equals(port.PhysicalType) && a.ChannelType.Equals(port.ChannelType) && a.Position.Equals(port.Position)).Value;
                        if (!port.Value.Equals(val))
                            SendIoPort(port);
                    }
                    catch (Exception ex)
                    {
                        Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                    }
                }
            }
            _ports = temp;
        }
        #endregion
    }
}
