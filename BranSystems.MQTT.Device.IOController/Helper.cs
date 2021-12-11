using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranSystems.MQTT.Device.IOController
{
    public static class Helper
    {
        public enum MsgType
        {
            Info,
            Warning,
            Error,
            Received,
            Sent,
            Handle
        }

        public static void ConsoleWriteLine(string msg, MsgType type = MsgType.Info)
        {
            switch (type)
            {
                case MsgType.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case MsgType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case MsgType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case MsgType.Received:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case MsgType.Sent:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case MsgType.Handle:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            Console.WriteLine(msg);
        }
    }
}
