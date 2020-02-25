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

namespace On_Site_Server_Ver3
{
    class JS_Communications
    {
        //Port Threads
        public Thread j; //Thread for JumpStarter Reader and Writer

        //Serial Port Information
        public SerialPort JStarter;

        //Ematch 
        public string EM1 = " ";
        public string EM2 = " ";
        public string EM3 = " ";
        public string EM4 = " ";

        //Controls the frequency that the circuit read and write method of the serial port. 
        int JS_interval = 100;

        Control_Main main_controls;
        public JS_Communications(Control_Main parent)
        {
            //Inherits the Control Main Function
            main_controls = parent;
        }
        ///Creates Thread to Start JStarter
        public void Intialize_Reader_Thread(string portname)
        {
            JStarter = new SerialPort(portname, 57600, Parity.None, 8, StopBits.One);
            JStarter.Open();
            JStarter.WriteTimeout = 500;
            JStarter.ReadTimeout = 500;
            j = new Thread(new ThreadStart(JStarter_Reader));
            j.Start();
            Console.WriteLine("JumpStarter Connected on Port:" + portname);
        }
        public void Kill_SerialPort()
        {
            try { j.Abort(); }
            catch { }
            try { JStarter.Close(); }
            catch { }
            main_controls.JS_PORT = "";
            JStarter = null;
        }
        /// Continously loops and reads data from the JStarter serial port, and 
        /// then send status updates to CSS_Writer_List.
        private void JStarter_Reader()
        {
            Console.WriteLine("JumpStarter Reader Initialized");
            //Instead of a Timer, DateTime.UtcNow is utilized to read the serial port
            //at the specified interval, this method doesn't allow the serial port more 
            //than once at a time in the case that the loop takes more than the specified
            //time. 
            DateTime earlier = DateTime.UtcNow;
            while (true)
            {

                while (((DateTime.UtcNow - earlier).TotalMilliseconds) < JS_interval)
                {
                    //Acts as a timer that 
                }
                // Console.WriteLine((DateTime.UtcNow - earlier).TotalMilliseconds);
                earlier = DateTime.UtcNow;

                //Copies JS_List to list and erases JS_List
                ArrayList list = new ArrayList(main_controls.JS_List);
                main_controls.JS_List.Clear();

                String data = "";
                //If JS_List is empty than a status check of the ematches will be sent.
                //Otherwise the commands to fire the ematchs will be sent through the JStarter_Writer
                if (list.Count == 0)
                {
                    data = JStarter_Writer(Communication_Constants.OJ_Check_Status);
                }
                else
                {
                    foreach (String i in list)
                    {
                        data = JStarter_Writer(i);
                    }
                }

                if (data != "")
                {
                    data = data.ToUpper();
                    //Ematch Status are sent to the CSS_Writer_List
                    if (data[0] != EM1[0])
                    {
                        if (data[0] == 'D')
                            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ematch_1_D.Length + Communication_Constants.OC_Ematch_1_D);
                        else
                            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ematch_1_C.Length + Communication_Constants.OC_Ematch_1_C);
                        EM1 = data.Substring(0, 1);
                    }
                    if (data[1] != EM2[0])
                    {
                        if (data[1] == 'D')
                            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ematch_2_D.Length + Communication_Constants.OC_Ematch_2_D);
                        else
                            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ematch_2_C.Length + Communication_Constants.OC_Ematch_2_C);
                        EM2 = data.Substring(1, 1);
                    }
                    if (data[2] != EM3[0])
                    {
                        if (data[2] == 'D')
                            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ematch_3_D.Length + Communication_Constants.OC_Ematch_3_D);
                        else
                            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ematch_3_C.Length + Communication_Constants.OC_Ematch_3_C);
                        EM3 = data.Substring(2, 1);
                    }
                    if (data[3] != EM4[0])
                    {
                        if (data[3] == 'D')
                            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ematch_4_D.Length + Communication_Constants.OC_Ematch_4_D);
                        else
                            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ematch_4_C.Length + Communication_Constants.OC_Ematch_4_C);
                        EM4 = data.Substring(3, 1);
                    }
                }
            }
        }
        /// Converts the string given in the parameter to bytes and send it to 
        /// JStarter via Serial Port Connection. 
        private String JStarter_Writer(String msg)
        {
            String data = "";
            try
            {
                Byte[] dataq = System.Text.Encoding.ASCII.GetBytes(msg);
                JStarter.Write(dataq, 0, dataq.Length);
                data = JStarter.ReadLine();

            }
            catch (Exception e)
            {
                Console.WriteLine(e + "" + JStarter.IsOpen);
                if (!JStarter.IsOpen)
                {
                    main_controls.CSS_Writer_List.Add("S" + (char)"JSD".Length + "JSD");
                    JStarter.Close();
                    main_controls.JS_PORT = "";
                    JStarter = null;
                    j.Abort();
                }

            }
            return data;
        }
    }
}
