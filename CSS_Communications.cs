using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Timers;
using System.IO.Ports;
using System.Diagnostics;
using System.ComponentModel; //For backgroundworker

namespace On_Site_Server_Ver3
{
    class CSS_Communications
    {
        //CSS Listener and Client 
        private TcpListener cssserver = null;
        private NetworkStream cssstream = null;
        private TcpClient cssclient = null;
        private BackgroundWorker CSS_Listener = new BackgroundWorker();

        private BackgroundWorker ExtraConnections = new BackgroundWorker();
        //private TcpListener cssserver2 = null;
        private ArrayList connection_list = new ArrayList();

        //IP and Port for TcpListener
        private Int32 portcss = 13000;
        private IPAddress localAddr = IPAddress.Parse("192.168.1.101");

        //Controls the frequency that the CSS_Reader_Event() method is called.
        //public System.Timers.Timer Reader_Timer;
        public int cssreaderint = 100;

        //Controls the frequency that the CSS_Writer() method is called.
        public System.Timers.Timer Writer_Timer;
        public int csswriterint = 100;

        //Extra Connection Thread
        Thread connection_listener;

        //Refers Back to the main Function
        Control_Main main_controls;

        //Connection Boolean 
        private Boolean hope = true;

        public CSS_Communications(Control_Main parent)
        {
            //Inherits the Control Main Function
            main_controls = parent;

            //Sets IP Address
            string ip = "";
            Console.Write("Enter IP (blank for default 192.168.1.101): ");
            ip = Console.ReadLine();
            if (ip == "")
                localAddr = IPAddress.Parse("192.168.1.101");
            else
                localAddr = IPAddress.Parse(ip);

            //create the cssserver side 
            this.CSS_Listener.DoWork += new System.ComponentModel.DoWorkEventHandler(this.CSS_Reader);
            cssserver = new TcpListener(localAddr, portcss);

            //Console Output and chatches the validity of the IP Address
            Console.WriteLine("Waiting for a Connection on IP: " + localAddr.ToString() + " Port: " + portcss.ToString());
            try { cssserver.Start(); }
            catch { Console.WriteLine("Invalid IP"); return; }

            CSS_Listener.RunWorkerAsync();

        }

        /// Accepts the Client and Creates the networkstream. Then it goes 
        /// into a continous while loop reading the data.
        private void CSS_Reader(object sender, DoWorkEventArgs e)
        {

            // Perform a blocking call to accept requests. 
            cssclient = cssserver.AcceptTcpClient();
            Console.WriteLine("CSSClient Connected");

            // Get a stream object for reading and writing.
            cssstream = cssclient.GetStream();

            connection_listener = new Thread(new ThreadStart(ExtraConnectionListener));
            connection_listener.Start();
            //Starts the CSS_Writer Thread which loops and read the OSS_Writer ArrayList
            //and sends it to CSS at 10 Hertz
            Writer_Timer = new System.Timers.Timer();
            Writer_Timer.Elapsed += new ElapsedEventHandler(CSS_Writer);
            Writer_Timer.Interval = csswriterint;
            Writer_Timer.Enabled = true;

            Byte[] cssreadbytes = new Byte[256];
            String data = null;
            int i;

            DateTime earlier = DateTime.UtcNow;
            while (true)
            {
                while (((DateTime.UtcNow - earlier).TotalMilliseconds) < cssreaderint) { }
                earlier = DateTime.UtcNow;

                // Loop to receive all the data sent by the client. 
                try
                {
                    while ((i = cssstream.Read(cssreadbytes, 0, cssreadbytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(cssreadbytes, 0, i);
                        //  Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        data = data.ToUpper();
                        Console.WriteLine("Received:" + data);

                        //Send Data to Be Parsed and Relayed to Serial Ports
                        Proccess_CSS(data);
                    }
                }
                catch (Exception) { }
            }
        }
        private void Proccess_CSS(String data)
        {
            //Parsing Data
            //Returns because data string is too short.
            if (data.Length < 3)
                return;
            //Removes the "C" character if needed. 
            char first = data[0];
            if (first == 'C')
            {
                String c = data.Substring(1, (data.Length - 1));
                data = c;
            }
            //Finds the message length for the individual command and returns if it is larger
            //than data.length
            int msglength = (Int16)data[0];
            if (data.Length < msglength + 1)
                return;
            //Retrieves the individual command
            String msgdata = data.Substring(1, msglength);
            //Takes out invididual command from the data String
            data = data.Substring((msglength + 1), data.Length - msglength - 1);
            //Switch statement that compares the command to existing commands in the list of global
            //variables and then adds the command sent to the correct serial port in the appropiate
            //ArrayList. 
            switch (msgdata)
            {
                //FCU
                //Open & Close Valves
                case Communication_Constants.CO_OFV_O: main_controls.FCU_List.Add(Communication_Constants.OFC_OFV_O); break;
                case Communication_Constants.CO_OFV_C: main_controls.FCU_List.Add(Communication_Constants.OFC_OFV_C); break;
                case Communication_Constants.CO_OBV_O: main_controls.FCU_List.Add(Communication_Constants.OFC_OBV_O); break;
                case Communication_Constants.CO_OBV_C: main_controls.FCU_List.Add(Communication_Constants.OFC_OBV_C); break;
                case Communication_Constants.CO_OVV_O: main_controls.FCU_List.Add(Communication_Constants.OFC_OVV_O); break;
                case Communication_Constants.CO_OVV_C: main_controls.FCU_List.Add(Communication_Constants.OFC_OVV_C); break;
                case Communication_Constants.CO_PFV_O: main_controls.FCU_List.Add(Communication_Constants.OFC_PFV_O); break;
                case Communication_Constants.CO_PFV_C: main_controls.FCU_List.Add(Communication_Constants.OFC_PFV_C); break;
                case Communication_Constants.CO_PBV_O: main_controls.FCU_List.Add(Communication_Constants.OFC_PBV_O); break;
                case Communication_Constants.CO_PBV_C: main_controls.FCU_List.Add(Communication_Constants.OFC_PBV_C); break;

                //FireStarter
                //Commands
                case Communication_Constants.CO_Arm_E: main_controls.FS_List.Add(Communication_Constants.OF_Arm_E); break;
                case Communication_Constants.CO_Dis_E: main_controls.FS_List.Add(Communication_Constants.OF_Dis_E); break;
                case Communication_Constants.CO_Arm_F: main_controls.FS_List.Add(Communication_Constants.OF_Arm_F); break;
                case Communication_Constants.CO_Dis_F: main_controls.FS_List.Add(Communication_Constants.OF_Dis_F); break;
                case Communication_Constants.CO_Blow_Pyro: main_controls.FS_List.Add(Communication_Constants.OF_Blow_Pyro); break;
                case Communication_Constants.CO_Start_BF: main_controls.FS_List.Add(Communication_Constants.OF_Start_BF); break;
                case Communication_Constants.CO_Stop_BF: main_controls.FS_List.Add(Communication_Constants.OF_Stop_BF); break;
                case Communication_Constants.CO_IA_BF: main_controls.FS_List.Add(Communication_Constants.OF_IA_BF); break;

                //JumpStarter
                //Fire Ematch 1-4
                case Communication_Constants.CO_Ematch_1: main_controls.JS_List.Add(Communication_Constants.OJ_Ematch_1); break;
                case Communication_Constants.CO_Ematch_2: main_controls.JS_List.Add(Communication_Constants.OJ_Ematch_2); break;
                case Communication_Constants.CO_Ematch_3: main_controls.JS_List.Add(Communication_Constants.OJ_Ematch_3); break;
                case Communication_Constants.CO_Ematch_4: main_controls.JS_List.Add(Communication_Constants.OJ_Ematch_4); break;

                //Abort
                case "ABT": break;
                //Connect Serial Port JumpStarter
                case "JPS": main_controls.Port_Finder(1); break;
                //Connect Serial Port FireStarter
                case "FRS": main_controls.Port_Finder(2); break;
                //Connect Serial Port FCU
                case "FCU": main_controls.Port_Finder(3); break;
                //Connect to All Serial Ports
                case "ALL": main_controls.Port_Finder(1); main_controls.Port_Finder(2); main_controls.Port_Finder(3); break;
                //Insert code to cut off the first character of the String data incase of meaningless code.
                default: break;
            }
            //If data is not null the data is sent into a recursive loops till all commands have been processed. 
            if (data != null)
                Proccess_CSS(data);

        }
        /// Reads from OSS_Writer ArrayList, and clears the ArrayList, and then converts
        /// commands and data into a string and then into bytes and sent it to CSS via
        /// TCP-IP Connection
        public void CSS_Writer(object source, ElapsedEventArgs e)
        {
            String message = "";
            ArrayList list = main_controls.CSS_Writer_List;
            main_controls.CSS_Writer_List.Clear();

            //converts CSS_Writer_Array List to a string
            if (list.Count != 0)
            {
                foreach (String i in list)
                {
                    message += i;
                }
            }

            //Accesses global variables and adds data to the messages. 
            if (main_controls.FS_PORT != "")
            {
                double b = main_controls.D_Oxidizer_P;
                message += "D" + (char)(b.ToString().Length + 1) + Communication_Constants.OC_Oxidizer_P + b;
                double k = main_controls.D_Pressurant_P;
                message += "D" + (char)(k.ToString().Length + 1) + Communication_Constants.OC_Pessurant_P + k;
                double bfa = main_controls.D_Backfill_A;
                message += "D" + (char)(bfa.ToString().Length + 1) + Communication_Constants.OC_Backfill_A + bfa;
            }
            if (main_controls.FCU_PORT != "")
            {
                double w = main_controls.D_N2;
                message += "D" + (char)(w.ToString().Length + 1) + Communication_Constants.OC_N2 + w;
                double f = main_controls.D_FuelN2O;
                message += "D" + (char)(f.ToString().Length + 1) + Communication_Constants.OC_FuelN2O + f;
                double x = main_controls.D_RocketMass;
                message += "D" + (char)(x.ToString().Length + 1) + Communication_Constants.OC_RocketMass + x;
                double z = main_controls.D_RocketThrust;
                message += "D" + (char)(z.ToString().Length + 1) + Communication_Constants.OC_RocketThrust + z;
            }
            //Console.WriteLine(message);

            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] send = System.Text.Encoding.ASCII.GetBytes(message);
                // Send the message to the connected Client. 
                cssstream.Write(send, 0, send.Length);
                for (int i = 0; i < connection_list.Count; i++)
                {
                    if (((TcpClient)connection_list[i]).Connected == false)
                    {
                        connection_list.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        ((TcpClient)connection_list[i]).GetStream().Write(send, 0, send.Length);
                    }
                }
            }
            catch (Exception v)
            {
                if (!cssclient.Connected) //Tries to reconnect if CSS becomes disconnected.
                {
                    hope = false; 
                    Writer_Timer.Enabled = false; //Stop the timer from calling the method. 
                    Console.WriteLine("CSS Client is Unable to Read:");
                    Console.WriteLine("Connected Value is {0}", cssclient.Connected);
                    Console.WriteLine("Try to Reconnect:");
                    Close_Valves_Last_Hope();
                    cssclient = cssserver.AcceptTcpClient();
                    cssstream = cssclient.GetStream();
                    hope = true; 
                    Console.WriteLine("CSSClient Reconnected");
                    Writer_Timer.Enabled = true; //Resumes once connection is restored. 
                    main_controls.CSS_Writer_List.Clear();
                }
                else
                {
                    Console.WriteLine(v);
                }
            }
        }
        private void ExtraConnectionListener()
        {
            while (true)
            {
                while (cssclient == null) ;
                TcpClient temp = cssserver.AcceptTcpClient();
                connection_list.Add(temp);
                Console.WriteLine("CSS Spectator Connected");
            }
        }

        private void Close_Valves_Last_Hope()
        {
            main_controls.FCU_List.Add(Communication_Constants.OFC_OBV_C);
            main_controls.FCU_List.Add(Communication_Constants.OFC_OFV_C);
            main_controls.FCU_List.Add(Communication_Constants.OFC_OVV_C);
            main_controls.FCU_List.Add(Communication_Constants.OFC_PBV_C);
            main_controls.FCU_List.Add(Communication_Constants.OFC_PFV_C);
            Thread.Sleep(600000);
            //if(!hope)
            //{
            //    main_controls.FS_List.Add(Communication_Constants.OF_Blow_Pyro);
            //}
        }
    }
}
