using IOClasses;
using MQTTnet.Client.Options;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Security.Authentication;
using BranSystems.MQTT.Device.IOController;
using TrackingClient;
using TrackingClient.Services;
using System.Linq;

namespace TestConsole
{
    public class IOTest
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

        private static void PrintStartupOptions()
        {
            System.Diagnostics.Debug.WriteLine($"MQTT options:\nClientID: {_clientName}\nHost: {_host}\nPort: {_port}\nUser: {_user}\nPass: {_pass}\nRoute: {_route}\nSSL Protocol: {_sslproto}\nCertificate: {_crt}\nAnonymous: {_anon}");
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
                    default:
                        System.Diagnostics.Debug.WriteLine($"Unsupported start-up parameter supplied: {param}");
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

                Begin:
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

                    Global.IOMQTTStatus = true;

                    string input = string.Empty;
                    //while (true)
                    //{
                    //    Console.Write("i for ReadIO, c for Config, so for SwitchOutput, or e to stop: ");
                    //    input = Console.ReadLine();
                    //    if (input.Equals("e"))
                    //        break;
                    //    else if (input.Equals("c"))
                    //        pub.RequestConfig();
                    //    else if (input.Equals("i"))
                    //        pub.RequestReadIO();
                    //    else if (input.Equals("so"))
                    //        pub.RequestSwitchOutput(new IoPort() { Name = "DIO0", Description = "OUT", Type = PortType.Type.DIO, Value = true.ToString(), Position = 0 });
                    //}
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message, ex);
                    Global.IOMQTTStatus = false;
                    goto Begin;
                }

                //if (pub is not null)
                //    pub.Stop();
                //if (sub is not null)
                //    sub.Stop();

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message, e);
            }

            Global.IOConnectionStatus = false;
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
                    System.Diagnostics.Debug.WriteLine("Unsupported IO config received");
                    return;
            }
            System.Diagnostics.Debug.WriteLine($"Handled config received event from {sender.GetType()} for:\n{args.Io}");
        }

        private static void HandleIoReceivedEvent(object sender, IoReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"Handled IO received event from {sender.GetType()} for:");
            foreach (var port in args.Ports)
                System.Diagnostics.Debug.WriteLine(port);

            //if (Global.IOPorts.Any(a => a.ChannelType == PortType.Channel.DI && a.Position == args.Ports))
        }

        private static void HandleIoPortReceivedEvent(object sender, IoPortReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"Handled IO received event from {sender.GetType()} for:");
            System.Diagnostics.Debug.WriteLine(args.Port);

            if (!Global.IOPorts.Any(a => a.ChannelType == args.Port.ChannelType && a.Position == args.Port.Position))
                Global.IOPorts.Add(args.Port);
            //Global.IOPorts.Where(a => a.ChannelType == args.Port.ChannelType && a.Position == args.Port.Position).ToList().ForEach(a => a = args.Port);
            else
            {
                var customListItem2 = Global.IOPorts.Where(a => a.ChannelType == args.Port.ChannelType && a.Position == args.Port.Position).First();
                var index = Global.IOPorts.IndexOf(customListItem2);
                if (index != -1)
                    Global.IOPorts[index] = args.Port;
            }
            

            Global.Hub.UpdateIOChannels(Global.IOPorts);

            bool unitDetect = false;
            bool unitInStation = false;
            if (Global.IOPorts.Any(a => a.Description.Equals("Unit Detect")))
                bool.TryParse(Global.IOPorts.First(a => a.Description.Equals("Unit Detect")).Value, out unitDetect);
            if (Global.IOPorts.Any(a => a.Description.Equals("Unit in Station")))
                bool.TryParse(Global.IOPorts.First(a => a.Description.Equals("Unit in Station")).Value, out unitInStation);

            if (!unitDetect & !unitInStation)
            {
                Global.TransactionStatus = string.Empty;
                Global.ProcessingState = Processing.State.WaitingForUnit;
            }
            else if (unitDetect & !unitInStation)
                Global.ProcessingState = Processing.State.UnitDetected;
            else if (unitDetect & unitInStation)
            {
                Global.ProcessingState = Processing.State.UnitInStation;
                Global.MQTTReader.RequestReadTag();
            }
        }

        private static void HandleConnectionStateReceivedEvent(object sender, ConnectionStateReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"Handled connection state received event from {sender.GetType()} as: Connected = {args.ConnectionState}");
            Global.IOConnectionStatus = args.ConnectionState;
            Global.IOConnectionStatusDate = DateTime.Now;
        }
    }
}
