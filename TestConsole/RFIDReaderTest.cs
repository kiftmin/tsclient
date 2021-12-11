using RFIDReader;
using MQTTnet.Client.Options;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Security.Authentication;
using BranSystems.MQTT.Device.RFIDReader;
using System.Data.SqlClient;

namespace TestConsole
{
    class RFIDReaderTest
    {
        private static string _clientName = "M020_Client";
        //private static string _host = "mosquitto-kiftmin-dev.apps.sandbox.x8i5.p1.openshiftapps.com";
        private static string _host = "localhost";
        private static string _TagID = "";
        //private static int _port = 443;
        private static int _port = 1883;
        private static string _user = "bud";
        private static string _pass = "%spencer%";
        private static string _route = "vw/za/68/m020/rfid-reader";
        private static SslProtocols _sslproto = SslProtocols.Tls12;
        private static string _crt = "mosquitto_ca.crt";
        private static bool _anon = false;
        private static int _tmo = 10;

        private static void PrintStartupOptions()
        {
            Console.WriteLine($"MQTT options:\nClientID: {_clientName}\nHost: {_host}\nPort: {_port}\nUser: {_user}\nPass: {_pass}\nRoute: {_route}\nSSL Protocol: {_sslproto}\nCertificate: {_crt}\nAnonymous: {_anon}");
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
                        Console.WriteLine($"Unsupported start-up parameter supplied: {param}");
                        break;
                }
            }

            PrintStartupOptions();
        }

        public RFIDReaderTest(string[] args)
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
                    sub.TagReceived += HandleTagReceivedEvent;
                    sub.TagsReceived += HandleTagsReceivedEvent;
                    sub.ConnectionStateReceived += HandleConnectionStateReceivedEvent;
                    sub.Start();

                    pub = new Publisher(options);
                    pub.Detail = sub.Detail;
                    pub.Start();

                    string input = string.Empty;
                    while (true)
                    {
                        Console.Write("r for ReadTag, rs for ReadTags, u for ReadUserData, or e to stop: ");
                        input = Console.ReadLine();
                        if (input.Equals("e"))
                            break;
                        else if (input.Equals("r"))
                            pub.RequestReadTag();
                        else if (input.Equals("rs"))
                            pub.RequestReadTags();
                        else if (input.Equals("u"))
                            pub.RequestReadUserData(new RFID_Tag("2Y68201955045148^4Y51118", -56));
                    }
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);
            }
        }

        private static void HandleTagReceivedEvent(object sender, TagReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"Handled tag received event from {sender.GetType()} for:\n{(RFID_Tag)args.Tag}");
            string tagCode = ((RFID_Tag)args.Tag).TagID;
            _TagID = tagCode;

          //  Console.WriteLine($"Handled tag received event from {sender.GetType()} for:\n{(RFID_Tag)args.Tag}");
        //    SaveToDB();
        }

        private static void SaveToDB()
        {
            var datasource = @"ts-db1.cqwemjrxk7xc.eu-west-1.rds.amazonaws.com";
            var database = "TS_DB1";
            var username = "admin";
            var password = "BRANsystems2009";

            string connString = @"Data Source=" + datasource + ";Initial Catalog="
                        + database + ";Persist Security Info=False;User ID=" + username + ";Password=" + password;

            string tagCode = _TagID;

            using (SqlConnection connection = new SqlConnection(connString))
            {
                String query = "INSERT INTO dbo.TagTest VALUES (@TagID)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TagID", tagCode);
                    //
                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                        Console.WriteLine("Successfully inserted data into Database!");

                    // Check Error
                    if (result < 0)
                        Console.WriteLine("Error inserting data into Database!");
                }
            }
        }


        private static void HandleTagsReceivedEvent(object sender, TagsReceivedEventArgs args)
        {
            Console.WriteLine($"Handled tags received event from {sender.GetType()} for:");
            foreach (var tag in args.Tags)
                Console.WriteLine((RFID_Tag)tag);
        }

        private static void HandleConnectionStateReceivedEvent(object sender, ConnectionStateReceivedEventArgs args)
        {
            Console.WriteLine($"Handled connection state received event from {sender.GetType()} as: Connected = {args.ConnectionState}");
        }
    }
}
