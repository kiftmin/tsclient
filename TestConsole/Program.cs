using System;
using System.Collections.Generic;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> lst = new List<string>(args);
            if (lst.Count == 0 || lst[0].Equals("-testRFID"))
            {
                lst.RemoveAt(0);
                Console.WriteLine("Starting test client for RFID Reader");
                new RFIDReaderTest(lst.ToArray());
            }
            else if (lst[0].Equals("-testIO"))
            {
                lst.RemoveAt(0);
                Console.WriteLine("Starting test client for IO");
                new IOTest(lst.ToArray());
            }
            else
                Console.WriteLine("No test parameter supplied");
        }
    }
}
