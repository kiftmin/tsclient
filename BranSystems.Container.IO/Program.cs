using BranSystems.MQTT.Device.IOController;
using ErrorLog;
using MQTTnet.Client.Options;
using IOClasses;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Linq;

namespace BranSystems.Container.IO
{
    class Program
    {
        private static readonly string _ps = " ";

        private static string _clientName = "M020_IO";
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
        private static bool _simulate = false;

        private static string _ioip = "127.0.0.1";
        private static int _ioprt = 502;
        private static string _iopss = string.Empty;
        private static List<IPort> _iochannels = new ();

        private static IErrorLog _log = new TextLog("IO");

        private static void PrintStartupOptions()
        {
            Helper.ConsoleWriteLine($"MQTT options:\nClientID: {_clientName}\nHost: {_host}\nPort: {_port}\nUser: {_user}\nPass: {_pass}\nRoute: {_route}\nSSL Protocol: {_sslproto}\nCertificate: {_crt}\nAnonymous: {_anon}\nSend IO together: {_sendiotogether}\nSimulate: {_simulate}");
            Helper.ConsoleWriteLine($"IO options:\nIP: {_ioip}\nPort: {_ioprt}\nChannels:\n");
            foreach (var port in _iochannels)
                Helper.ConsoleWriteLine($"\t{port}");
        }

        private static void ProcessStartupParameters(string[] parameters)
        {
            parameters = CorrectParameterHTMLCharacters(parameters);

            string test = JsonSerializer.Serialize(new List<IPort>() { new IoPort() { Name = "DI0", Description = "Unit in Station", Position = 0, PhysicalType = PortType.Physical.DI, ChannelType = PortType.Channel.DI, Value = "0" } });

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
            const string p_simulate = "-simulate";

            const string p_ioip = "-ioip";
            const string p_ioprt = "-ioprt";
            const string p_iopss = "-iopss";
            const string p_iochannels = "-iochannels";

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
                    case p_ioip: _ioip = val[1]; break;
                    case p_iopss: _iopss = val[1]; break;
                    case p_ioprt: int.TryParse(val[1], out _ioprt); break;
                    case p_iochannels: _iochannels = GetPortListFromParameterValue(val[1]); break;
                    default:
                        Helper.ConsoleWriteLine($"Unsupported start-up parameter supplied: {param}", Helper.MsgType.Error);
                        break;
                }
            }

            PrintStartupOptions();
        }

        private static List<IPort> GetPortListFromParameterValue(string value)
        {
            var result = new List<IPort>();
            foreach (var port in value.Split("|"))
            {
                var p = port.Split(",");
                result.Add(new IoPort()
                {
                    Name = p[0],
                    Description = p[1],
                    PhysicalType = Enum.Parse<PortType.Physical>(p[2], true),
                    ChannelType = Enum.Parse<PortType.Channel>(p[3], true),
                    Position = byte.Parse(p[4]),
                    Value = p[5],
                });
            }
            return result;
        }

        private static Channels GetChannelsFromSuppliedParameter()
        {
            byte numDI = (byte)_iochannels.Where( a => a.ChannelType.Equals(PortType.Channel.DI)).Count();
            byte numDO = (byte)_iochannels.Where( a => a.ChannelType.Equals(PortType.Channel.DO)).Count();
            byte numAI = (byte)_iochannels.Where( a => a.ChannelType.Equals(PortType.Channel.AI)).Count();
            byte numAO = (byte)_iochannels.Where( a => a.ChannelType.Equals(PortType.Channel.AO)).Count();
            byte numPhysicalDI = (byte)_iochannels.Where( a => a.PhysicalType.Equals(PortType.Physical.DI)).Count();
            byte numPhysicalDO = (byte)_iochannels.Where( a => a.PhysicalType.Equals(PortType.Physical.DO)).Count();
            byte numPhysicalDIO = (byte)_iochannels.Where( a => a.PhysicalType.Equals(PortType.Physical.DIO)).Count();
            byte numPhysicalAI = (byte)_iochannels.Where( a => a.PhysicalType.Equals(PortType.Physical.AI)).Count();
            byte numPhysicalAO = (byte)_iochannels.Where( a => a.PhysicalType.Equals(PortType.Physical.AO)).Count();
            byte numPhysicalAIO = (byte)_iochannels.Where( a => a.PhysicalType.Equals(PortType.Physical.AIO)).Count();
            
            return new Channels(numDI,numDO,numAI,numAO,numPhysicalDI,numPhysicalDO,numPhysicalDIO,numPhysicalAI,numPhysicalAO,numPhysicalAIO);
        }

        private static string[] CorrectParameterHTMLCharacters(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
                args[i] = System.Web.HttpUtility.UrlDecode(args[i]);
            return args;
        }

        static void Main(string[] args)
        {
            ProcessStartupParameters(args);

            Console.Title = _clientName;

            Publisher pub = null;
            Subscriber sub = null;
            try
            {
                IIo io = new Moxa(_ioip, _iopss, GetChannelsFromSuppliedParameter(), _log);  //TODO - add channels as param
                io.Ports = _iochannels;
                //io.Connect();

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

                pub = new Publisher(options);
                pub.Detail = new MQTTDetail() { Route = _route };
                pub.CheckReaderConnInterval = 10;
                pub.Io = io;
                pub.TestMode = _simulate;
                pub.SendIOSimultaneous = _sendiotogether;
                pub.Start();

                sub = new Subscriber(options, ref pub);
                sub.Detail = pub.Detail;
                sub.Start();

                //sub.Publisher = pub;

                pub.PushIoConnectionStateAsync();
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
    }
}
