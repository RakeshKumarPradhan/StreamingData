using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PortChat
{
    class Program
    {
        static bool _continue;
        static SerialPort _serialPort;
        static Queue<byte> recievedData = new Queue<byte>();
        static List<byte> test = new List<byte>(); 
        static void Main(string[] args)
        {
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);
            int count = 0; 

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = SetPortName(_serialPort.PortName); // "COM4"; //
            _serialPort.BaudRate = SetPortBaudRate(64); // 64; // _serialPort.BaudRate
            _serialPort.Parity = SetPortParity(_serialPort.Parity); // Parity.Odd; // 
            _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits); // 8; // 
            _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits); // StopBits.One; // 
            _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake); // Handshake.None; // 

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            _continue = true;

            Console.WriteLine("Enter command {0} :");
            int command = Convert.ToInt32(Console.ReadLine()); 

            byte[] data = new byte[] { 255, 255, 255, 255, 255, 02, 00, 00, 00, 02, 255 }; // StringToByteArray("ff ff ff ff ff 02 00 00 00 02 ff"); //     ff ff ff ff ff 02 00 00 00 02 ff                  ÿÿÿÿÿ.....ÿ 
            _serialPort.Write(data, 0, data.Length);

            // readThread.Start();
            // Thread.Sleep(300); 
            // int iByts = _serialPort.Read(bReply, 0, bReply.Length);

            //_serialPort.Read(bReply, 0, bReply.Length);
            //data.ToList().ForEach(b => recievedData.Enqueue(b));


            _serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                // Show all the incoming data in the port's buffer
                byte[] data1 = new byte[_serialPort.BytesToRead];
                //Console.WriteLine(_serialPort.BytesToRead.ToString());
                _serialPort.Read(data1, 0, data1.Length);
                data1.ToList().ForEach(b => recievedData.Enqueue(b));
                processData();
                //foreach (var test in data1)
                //{
                //    Console.WriteLine(test);
                //}
                HandleSerialData(data1); 
            }
            //Application.Run();

            void processData()
            {
                // Determine if we have a "packet" in the queue
                if (recievedData.Count > 50)
                {
                    var packet = Enumerable.Range(0, 50).Select(i => recievedData.Dequeue());
                }
                count = recievedData.Count(); 
            }

            // code to consolidate the response and then decipher. 
            // truncate the offset values from the response. and then decipher the response based on the specific types utype of float, which are fixed as per the firmware. 
            Console.WriteLine("processing !!! ... ");

            // introduce delay. Time to receive all the streaming response. 
            Thread.Sleep(1800);
            Console.WriteLine("\n\nConsolidated List of the response !!! ");
            Console.WriteLine("HART Command 0 Output.");
            Console.WriteLine("Count : {0}", test.Count);

            Console.WriteLine("Dummy : {0} ", test[11].ToString());
            Console.WriteLine("MfgId : {0} ", test[12].ToString());
            Console.WriteLine("Device Type: {0} ", test[13].ToString());
            Console.WriteLine("Preambles: {0} ", test[14].ToString());
            Console.WriteLine("CmdRev : {0} ", test[15].ToString());
            Console.WriteLine("TXRev : {0} ", test[16].ToString());
            Console.WriteLine("SWRev : {0} ", test[17].ToString());
            Console.WriteLine("HWrev : {0} ", test[18].ToString());
            Console.WriteLine("Flags: {0} ", test[19].ToString());
            //Console.WriteLine("Convert : {0:X2}{1:X2}{2:X2}", test[20], test[21], test[22]);
            StringBuilder sb = new StringBuilder();
            string deviceId = sb.AppendFormat("{0:X2}{1:X2}{2:X2}", test[20], test[21], test[22]).ToString();
            //Console.WriteLine(deviceId);
            Console.WriteLine("Device Id : {0} (In hex: {1})", Int64.Parse(deviceId, System.Globalization.NumberStyles.HexNumber), deviceId);

            //Console.Write("Name: ");            
            //name = Console.ReadLine();
            //Console.WriteLine("Type QUIT to exit");
            //while (_continue)
            //{
            //    message = Console.ReadLine();

            //    if (stringComparer.Equals("quit", message))
            //    {
            //        _continue = false;
            //    }
            //    else
            //    {
            //        _serialPort.WriteLine(
            //            String.Format("<{0}>: {1}", name, message));
            //    }
            //}

            Console.WriteLine("Press enter/key to quit!");
            Console.ReadLine(); 

            // readThread.Join();
            _serialPort.Close();

        }

        private static void HandleSerialData(byte[] data1)
        {
            //foreach(var test in data1)
            //{
            //    Console.WriteLine("Decimal: " + test);
            //}
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data1.Length; i++)
                sb.AppendFormat("{0:X2} ", data1[i]);

            var x = sb.ToString();
            //Console.WriteLine(x);
            test.AddRange(data1); 
            //HandleString(sb); 
        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    Console.WriteLine(message);
                }
                catch (TimeoutException) { }
            }
        }

        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
                defaultPortName = s;
            }

            Console.Write("COM port({0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "")
            {
                portName = defaultPortName;
            }
            return portName;
        }

        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate({0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Parity({0}):", defaultPortParity.ToString());
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity);
        }

        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Data Bits({0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits);
        }

        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available Stop Bits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Stop Bits({0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits);
        }

        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Handshake({0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake);
        }

    }
}
