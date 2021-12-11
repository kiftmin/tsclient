using RFIDReader;
using MQTTnet.Client.Options;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Security.Authentication;
using BranSystems.MQTT.Device.RFIDReader;
using TrackingClient;
using System.Data.SqlClient;
using TrackingClient.Services;
using BranSystems.MQTT.Device.RFIDReader;

namespace TestConsole
{
    public class RFIDReaderTest
    {
      

        private static string _clientName = "M020_Client";
        //private static string _host = "mosquitto-kiftmin-dev.apps.sandbox.x8i5.p1.openshiftapps.com";
        private static string _host = "BRANSYSTEMS-EL";
        private static RFID_Tag _TagID = new RFID_Tag();
        //private static int _port = 443;
        private static int _port = 1883;
        private static string _user = "bud";
        private static string _pass = "%spencer%";
        private static string _route = "vw/za/68/m020/rfid-reader";
        private static SslProtocols _sslproto = SslProtocols.Tls12;
        private static string _crt = "mosquitto_ca.crt";
        private static bool _anon = false;
        private static int _tmo = 10;
        public string ClientName { get =>  _clientName; }

        private Publisher pub = null;
        private Subscriber sub = null;

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

        public RFIDReaderTest(string[] args)
        {
            try
            {
                ProcessStartupParameters(args);

                //System.Diagnostics.Debug.Title = _clientName;

                Begin:
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

                    Global.ReaderMQTTStatus = true;

                    sub.Publisher = pub;

                    string input = string.Empty;
                    //while (true)
                    //{
                    //    System.Diagnostics.Debug.Write("r for ReadTag, rs for ReadTags, u for ReadUserData, or e to stop: ");
                    //    input = System.Diagnostics.Debug.ReadLine();
                    //    if (input.Equals("e"))
                    //        break;
                    //    else if (input.Equals("r"))
                    //        pub.RequestReadTag();
                    //    else if (input.Equals("rs"))
                    //        pub.RequestReadTags();
                    //    else if (input.Equals("u"))
                    //        pub.RequestReadUserData(new RFID_Tag("2Y68201955045148^4Y51118", -56));
                    //}
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message, ex);
                    Global.ReaderMQTTStatus = false;
                    goto Begin;
                }

                //System.Diagnostics.Debug.WriteLine("Press any key to stop.");
                //System.Diagnostics.Debug.ReadKey();

                //if (pub is not null)
                //    pub.Stop();
                //if (sub is not null)
                //    sub.Stop();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message, e);
            }
        }

        private static void HandleTagReceivedEvent(object sender, TagReceivedEventArgs args)
          {

            System.Diagnostics.Debug.WriteLine($"Handled tag received event from {sender.GetType()} for:\n{(RFID_Tag)args.Tag}");
            _TagID = new RFID_Tag(args.Tag.TagID, args.Tag.RSSI);

            Global.ProcessingState = Processing.State.TagRead;
            Global.Hub.UpdateRFIDTag(_TagID);

            //string tagCode = "8827272772";
            SaveToDB(_TagID);
        }

        private static void SaveToDB(RFID_Tag tag)
        {
            Global.ProcessingState = Processing.State.ProcessingUnit;
            Global.TransactionStatus = "Saving RFID tag to database";

            var datasource = @"ts-db1.cqwemjrxk7xc.eu-west-1.rds.amazonaws.com";
            var database = "TS_DB1";
            var username = "admin";
            var password = "BRANsystems2009";

            string connString = @"Data Source=" + datasource + ";Initial Catalog="
                        + database + ";Persist Security Info=False;User ID=" + username + ";Password=" + password;

            DateTime dates = Global.ReaderConnectionStatusDate = DateTime.Now;

            try
            {

                using (SqlConnection connection = new SqlConnection(connString))
                {
                    String query = "INSERT INTO dbo.TagTest (TagID,ClientName,Reader,Time) VALUES (@TagID,@ClientName,@Reader,@Time)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TagID", tag.TagID);
                        command.Parameters.AddWithValue("@Time", tag.TransponderReadTime);
                        command.Parameters.AddWithValue("@ClientName", "M20");
                        command.Parameters.AddWithValue("@Reader", DBNull.Value);
                        Console.WriteLine(command.CommandText);
                        
                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            Console.WriteLine("Successfully inserted data into Database!");
                            Global.TransactionStatus = "RFID tag saved to database";
                            Global.ProcessingState = Processing.State.UnitReleased;
                        }

                        // Check Error
                        if (result < 0)
                        {
                            Console.WriteLine("Error inserting data into Database!");
                            Global.TransactionStatus = "Failed to save RFID tag to database";
                            Global.ProcessingState = Processing.State.TransactionFailed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.TransactionStatus = "Exception while saving RFID tag to database";
                Global.ProcessingState = Processing.State.TransactionFailed;
            }
        }

        private static void HandleTagsReceivedEvent(object sender, TagsReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"Handled tags received event from {sender.GetType()} for:");
            foreach (var tag in args.Tags)
                System.Diagnostics.Debug.WriteLine((RFID_Tag)tag);
        }

        private static void HandleConnectionStateReceivedEvent(object sender, ConnectionStateReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"Handled connection state received event from {sender.GetType()} as: Connected = {args.ConnectionState}");
            Global.ReaderConnectionStatus = args.ConnectionState;
            Global.ReaderConnectionStatusDate = DateTime.Now;
        }

        public void RequestReadTag()
        {
            Global.ProcessingState = Processing.State.ReadingTag;
            pub.RequestReadTag();
        }
    }
}
