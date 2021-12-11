using IOClasses;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestConsole;
using TrackingClient.Hubs;

namespace TrackingClient.Services
{
    public class Global
    {
        public static RFIDReaderTest MQTTReader { get; set; } = default;
        public static IOTest MQTT_IO { get; set; } = default;

        public static string ReaderTag { get; set; }
        public static List<IPort> IOPorts { get; set; } = new List<IPort>();
        private static bool _rdrConnStatus = false;
        public static bool ReaderConnectionStatus
        {
            get
            {
                return _rdrConnStatus;
            }
            set
            {
                _rdrConnStatus = value;
                Global.Hub.UpdateReaderConnectionState(_rdrConnStatus);
            }
        }
        private static bool _ioConnStatus = false;
        public static bool IOConnectionStatus
        {
            get
            {
                return _ioConnStatus;
            }
            set
            {
                _ioConnStatus = value;
                Global.Hub.UpdateIOConnectionState(_ioConnStatus);
            }
        }
        private static bool _rdrMQTTStatus = false;
        public static bool ReaderMQTTStatus
        {
            get
            {
                return _rdrMQTTStatus;
            }
            set
            {
                _rdrMQTTStatus = value;
                Global.Hub.UpdateReaderMQTTStatus(_rdrMQTTStatus);
            }
        }
        private static bool _ioMQTTStatus = false;
        public static bool IOMQTTStatus
        {
            get
            {
                return _ioMQTTStatus;
            }
            set
            {
                _ioMQTTStatus = value;
                Global.Hub.UpdateIOMQTTStatus(_ioMQTTStatus);
            }
        }
        public static DateTime ReaderConnectionStatusDate { get; set; } = DateTime.Now;
        public static DateTime IOConnectionStatusDate { get; set; } = DateTime.Now;
        public static PageHub Hub { get; set; } = new PageHub();

        private static Processing.State _processingState = Processing.State.WaitingForUnit;
        public static Processing.State ProcessingState 
        {
            get 
            { 
                return _processingState;
            }
            set 
            { 
                _processingState = value;
                Global.Hub.UpdateCurrentProcessingState(_processingState);
            }
        }

        private static string _transactionStatus = string.Empty;
        public static string TransactionStatus
        {
            get
            {
                return _transactionStatus;
            }
            set
            {
                _transactionStatus = value;
                Global.Hub.UpdateTransactionStatus(_transactionStatus);
            }
        }

        public static IHubContext<PageHub> HubContext { get; set; }

        public static async Task TestProcessingState()
        {
            while (true)
            {
                try
                {
                    await Global.Hub.UpdateCurrentProcessingState(Processing.State.WaitingForUnit);
                    await Task.Delay(3000);
                    ReaderConnectionStatus = true;
                    IOConnectionStatus = true;
                    await Global.Hub.UpdateCurrentProcessingState(Processing.State.UnitDetected);
                    await Task.Delay(3000);
                    ReaderConnectionStatus = false;
                    IOConnectionStatus = false;
                    await Global.Hub.UpdateCurrentProcessingState(Processing.State.UnitInStation);
                    await Task.Delay(3000);
                    ReaderConnectionStatus = true;
                    IOConnectionStatus = true;
                    await Global.Hub.UpdateCurrentProcessingState(Processing.State.ReadingTag);
                    await Task.Delay(3000);
                    ReaderConnectionStatus = false;
                    IOConnectionStatus = false;
                    await Global.Hub.UpdateCurrentProcessingState(Processing.State.TagRead);
                    await Task.Delay(3000);
                    await Global.Hub.UpdateCurrentProcessingState(Processing.State.ProcessingUnit);
                    await Task.Delay(3000);
                    await Global.Hub.UpdateCurrentProcessingState(Processing.State.UnitReleased);
                    await Task.Delay(3000);
                    await Global.Hub.UpdateCurrentProcessingState(Processing.State.TransactionFailed);
                    await Task.Delay(3000);
                }
                catch
                {
                    await Task.Delay(5000);
                }
            }
        }
    }
}
