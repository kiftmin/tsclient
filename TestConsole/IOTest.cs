using IOClasses;
using MQTTnet.Client.Options;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Security.Authentication;
using BranSystems.MQTT.Device.IOController;

namespace TestConsole
{
    class IOTest
    {
        private static string _clientName = "M020_Client";
        //private static string _host = "mosquitto-kiftmin-dev.apps.sandbox.x8i5.p1.openshiftapps.com";
        private static string _host = "localhost";
        //private static int _port = 443;
        private static int _port = 1883;
        private static string _user = "bud";
        private static string _pass = "%spencer%";
        private static string _route = "vw/za/68/m020/io";
        private static SslProtocols _sslproto = SslProtocols.Tls12;
        private static string _crt = "mosquitto_ca.crt";
        private static bool _anon = false;
        private static int _tmo = 10;

        private static bool _sendiotogether = false;
        private static bool _simulate = true;

        private static void PrintStartupOptions()
        {
            Helper.ConsoleWriteLine($"MQTT options:\nClientID: {_clientName}\nHost: {_host}\nPort: {_port}\nUser: {_user}\nPass: {_pass}\nRoute: {_route}\nSSL Protocol: {_sslproto}\nCertificate: {_crt}\nAnonymous: {_anon}\nSend IO together: {_sendiotogether}\nSimulate: {_simulate}");
        }

        private static void ProcessStartupParameters(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                PrintStartupOptions();
                return;
            }

            const string p_cid = "-cid";
            const string p_hst = "-hst";
            const string p_prt = "-prt";
            const string p_usr = "-u";
            const string p_pss = "-p";
            const string p_rt = "-rt";
            const string p_sslp = "-sslp";
            const string p_crt = "-crt";
            const string p_anon = "-anon";
            const string p_tmo = "-tmo";
            const string p_sendiotogether = "-sendiotogether";
            const string p_simulate = "-simumate";

            string[] val = new string[2];

            foreach (string param in parameters)
            {
                val = param.Split(':', 2, StringSplitOptions.TrimEntries);

                switch (val[0])
                {
                    case p_cid: _clientName = val[1]; break;
                    case p_hst: _host = val[1]; break;
                    case p_prt: int.TryParse(val[1], out _port); break;
                    case p_usr: _user = val[1]; break;
                    case p_pss: _pass = val[1]; break;
                    case p_rt: _route = val[1]; break;
                    case p_sslp: Enum.TryParse<SslProtocols>(val[1], true, out _sslproto); break;
                    case p_crt: _crt = val[1]; break;
                    case p_anon: bool.TryParse(val[1], out _anon); break;
                    case p_tmo: int.TryParse(val[1], out _tmo); break;
                    case p_sendiotogether: bool.TryParse(val[1], out _sendiotogether); break;
                    case p_simulate: bool.TryParse(val[1], out _simulate); break;
                    default:
                        Helper.ConsoleWriteLine($"Unsupported start-up parameter supplied: {param}", Helper.MsgType.Error);
                        break;
                }
            }

            PrintStartupOptions();
        }

        public IOTest(string[] args)
        {
            try
            {
                ProcessStartupParameters(args);

                Console.Title = _clientName;

                Publisher pub = null;
                Subscriber sub = null;
                try
                {
                    MqttClientOptionsBuilder options = null;

                    if (!_anon)
                        options = new MqttClientOptionsBuilder()
                            .WithClientId(_clientName)
                            .WithTcpServer(_host, _port)
                            .WithCredentials(_user, _pass)
                            .WithTls(new MqttClientOptionsBuilderTlsParameters()
                            {
                                AllowUntrustedCertificates = false,
                                UseTls = true,
                                Certificates = new List<X509Certificate> { new X509Certificate(_crt) },
                                CertificateValidationHandler = delegate { return true; },
                                IgnoreCertificateChainErrors = false,
                                IgnoreCertificateRevocationErrors = false,
                                SslProtocol = _sslproto
                            })
                            .WithCommunicationTimeout(new TimeSpan(0, 0, _tmo))
                            .WithCleanSession();
                    else
                        options = new MqttClientOptionsBuilder()
                            .WithClientId(_clientName)
                            .WithTcpServer(_host, _port)
                            .WithCredentials(_user, _pass)
                            .WithCommunicationTimeout(new TimeSpan(0, 0, _tmo))
                            .WithCleanSession();

                    sub = new Subscriber(options);
                    sub.Detail = new MQTTDetail() { Route = _route };
                    sub.ConfigReceived += HandleConfigReceivedEvent;
                    sub.IoReceived += HandleIoReceivedEvent;
                    sub.IoPortReceived += HandleIoPortReceivedEvent;
                    sub.ConnectionStateReceived += HandleConnectionStateReceivedEvent;
                    sub.Start();

                    pub = new Publisher(options);
                    pub.Detail = sub.Detail;
                    pub.Start();

                    string input = string.Empty;
                    while (true)
                    {
                        Helper.ConsoleWriteLine("i for IO, ip for IOPort, c for Config, so for SwitchOutput, or e to stop: ");
                        input = Console.ReadLine();
                        if (input.Equals("e"))
                            break;
                        else if (input.Equals("c"))
                            pub.RequestConfig();
                        else if (input.Equals("i"))
                            pub.RequestReadIO();
                        else if (input.StartsWith("ip"))
                        {
                            try
                            {
                                input = input.Substring(input.IndexOf(" ") + 1);
                                string[] prt = input.Split(",");
                                pub.RequestReadIOPort(new IoPort() { Name = prt[0], Description = prt[1], PhysicalType = (PortType.Physical)Enum.Parse(typeof(PortType.Physical), prt[2]), ChannelType = (PortType.Channel)Enum.Parse(typeof(PortType.Channel), prt[3]), Position = byte.Parse(prt[4]), Value = prt[5] });
                            }
                            catch (Exception ex)
                            {
                                Helper.ConsoleWriteLine($"Invalid format supplied for IOPort. Use the following example:\nip DIO0,Digital OUT,DIO,DO,0,false", Helper.MsgType.Warning);
                            }
                        }
                        else if (input.StartsWith("so"))
                            pub.RequestSwitchOutput(new IoPort() { Name = "DIO0", Description = "OUT", PhysicalType = PortType.Physical.DIO, ChannelType = PortType.Channel.DO, Position = 0, Value = true.ToString() });
                    }
                }
                catch (Exception ex)
                {
                    Helper.ConsoleWriteLine(ex.Message, Helper.MsgType.Error);
                }

                Helper.ConsoleWriteLine("Press any key to stop.");
                Console.ReadKey();

                if (pub is not null)
                    pub.Stop();
                if (sub is not null)
                    sub.Stop();
            }
            catch (Exception e)
            {
                Helper.ConsoleWriteLine(e.Message, Helper.MsgType.Error);
            }
        }

        private static void HandleConfigReceivedEvent(object sender, ConfigReceivedEventArgs args)
        {
            switch (args.Io.IoType)
            {
                case IOs.IO.EAGLE:
                    args.Io = (Eagle)args.Io;
                    break;
                case IOs.IO.MOXA:
                    args.Io = (Moxa)args.Io;
                    break;
                default:
                    Helper.ConsoleWriteLine("Unsupported IO config received", Helper.MsgType.Error);
                    return;
            }
            Helper.ConsoleWriteLine($"Handled config received event from {sender.GetType()} for:\n{args.Io}", Helper.MsgType.Received);
        }

        private static void HandleIoReceivedEvent(object sender, IoReceivedEventArgs args)
        {
            Helper.ConsoleWriteLine($"Handled IO received event from {sender.GetType()} for:", Helper.MsgType.Handle);
            foreach (var port in args.Ports)
                Helper.ConsoleWriteLine(port.ToString(), Helper.MsgType.Handle);
        }

        private static void HandleIoPortReceivedEvent(object sender, IoPortReceivedEventArgs args)
        {
            Helper.ConsoleWriteLine($"Handled IO port received event from {sender.GetType()} for: {args.Port}", Helper.MsgType.Handle);
        }

        private static void HandleConnectionStateReceivedEvent(object sender, ConnectionStateReceivedEventArgs args)
        {
            Helper.ConsoleWriteLine($"Handled connection state received event from {sender.GetType()} as: Connected = {args.ConnectionState}", Helper.MsgType.Handle);
        }
    }
}
