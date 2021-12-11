using BranSystems.MQTT.Device.RFIDReader;
using ErrorLog;
using MQTTnet.Client.Options;
using RFIDReader;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace BranSystems.Container.RFIDReader
{
    class Program
    {
        private static string _clientName = "M020_SICK_Reader";
        //private static string _host = "mosquitto-kiftmin-dev.apps.sandbox.x8i5.p1.openshiftapps.com";
        private static string _host = "localhost";
        //private static int _port = 443;
        private static int _port = 1883;
        private static string _user = "bud";
        private static string _pass = "%spencer%";
        private static string _route = "vw/za/68/m020/rfid-reader";
        private static SslProtocols _sslproto = SslProtocols.Tls12;
        private static string _crt = "mosquitto_ca.crt";
        private static bool _anon = false;
        private static int _tmo = 10;

        private static bool _simulate = false;

        private static string _rdrip = "127.0.0.1";
        private static int _rdrprt = 2112;

        private static IErrorLog _log = new TextLog("Reader");

        private static void PrintStartupOptions()
        {
            Console.WriteLine($"MQTT options:\nClientID: {_clientName}\nHost: {_host}\nPort: {_port}\nUser: {_user}\nPass: {_pass}\nRoute: {_route}\nSSL Protocol: {_sslproto}\nCertificate: {_crt}");
            Console.WriteLine($"Reader options:\nIP: {_rdrip}\nPort: {_rdrprt}");
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
            const string p_simulate = "-simulate";

            const string p_rdrip = "-rdrip";
            const string p_rdrprt = "-rdrprt";

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
                    case p_simulate: bool.TryParse(val[1], out _simulate); break;
                    case p_rdrip: _rdrip = val[1]; break;
                    case p_rdrprt: int.TryParse(val[1], out _rdrprt); break;
                    default:
                        Console.WriteLine($"Unsupported start-up parameter supplied: {param}");
                        break;
                }
            }

            PrintStartupOptions();
        }

        static void Main(string[] args)
        {
            ProcessStartupParameters(args);

            Console.Title = _clientName;

            Publisher pub = null;
            Subscriber sub = null;
            try
            {
                IRFID_Reader reader = new SICKREADER(_rdrip, _rdrprt, _log);
                reader.Connect();

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
                pub.Detail = new MQTTDetail() { Route = "vw/za/68/m020/rfid-reader" };
                pub.CheckReaderConnInterval = 10;
                pub.Reader = reader;
                pub.TestMode = _simulate;
                pub.Start();

                sub = new Subscriber(options, ref pub);
                sub.Detail = pub.Detail;
                sub.Start();

                //sub.Publisher = pub;

                pub.PushReaderConnectionStateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex);
            }

            Console.WriteLine("Press any key to stop.");
            Console.ReadKey();
            
            if (pub is not null)
                pub.Stop();
            if (sub is not null)
                sub.Stop();
        }
    }
}
