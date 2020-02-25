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
    class FCU_Communications
    {
        //Serial FCU
        public SerialPort FCU;

        //Thread for FCU Reader and Writer
        public Thread c; 

        //Interval for FCU Reader
        int FCU_interval = 10;
        Control_Main main_controls;
        public FCU_Communications(Control_Main parent)
        {
            //Inherits the Control Main Function
            main_controls = parent;
        }

        public void Intialize_Reader_Thread(string portname)
        {
            FCU = new SerialPort(portname, 57600, Parity.None, 8, StopBits.One);
            FCU.Open();
            FCU.ReadTimeout = 500;
            FCU.WriteTimeout = 500;
            c = new Thread(new ThreadStart(FCU_Reader));
            c.Start();
            Console.WriteLine("FCU Connected on Port:" + portname);
        }

        public void Kill_SerialPort()
        {
            try { c.Abort(); }
            catch { }
            try { FCU.Close(); }
            catch { }
            main_controls.FCU_PORT = "";
            FCU = null;
        }

        /// Continously loops and reads data from the FCU serial port, and 
        /// then send status and data updates CSS_Writer_List.
        private void FCU_Reader()
        {
            Console.WriteLine("FCU Reader Initialized");

            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_OFC_C.Length + Communication_Constants.OC_OFC_C);
            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_OBC_C.Length + Communication_Constants.OC_OBC_C);
            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_OVC_C.Length + Communication_Constants.OC_OVC_C);
            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_PFC_C.Length + Communication_Constants.OC_PFC_C);
            main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_PBC_C.Length + Communication_Constants.OC_PBC_C);

            DateTime earlier = DateTime.UtcNow;
            while (true)
            {

                while (((DateTime.UtcNow - earlier).TotalMilliseconds) < FCU_interval) { }
                earlier = DateTime.UtcNow;

                ArrayList list = new ArrayList(main_controls.FCU_List);
                main_controls.FCU_List.Clear();

                if (list.Count != 0)
                {
                    foreach (String i in list)
                    {
                        String FCU_status = FCU_Writer(i);
                        Process_FCU(FCU_status);
                    }
                }

                try
                {
                    //Reads N2O Fuel Data
                    Byte[] s = System.Text.Encoding.ASCII.GetBytes(Communication_Constants.OFC_FuelN2O);
                    FCU.Write(s, 0, s.Length);
                    int count = FCU.BytesToRead;
                    while (count < 4 && (DateTime.UtcNow - earlier).TotalMilliseconds < 1000)
                    {
                        count = FCU.BytesToRead;
                    }
                    byte[] ByteArray = new byte[count];
                    FCU.Read(ByteArray, 0, count);
                    if (ByteArray[0] == Communication_Constants.FCO_FuelN2O[0] && count >= 4)
                    {
                        int b = (ByteArray[1] << 16) + (ByteArray[2] << 8) + (ByteArray[3]);
                        main_controls.D_FuelN2O = main_controls.RawData_Converter(b, 20);
                    }
                    else
                        Console.WriteLine("Lack Of Bytes for N20 Fuel Data");

                    ////Reads N2 Ground Tank Data
                    Byte[] z = System.Text.Encoding.ASCII.GetBytes(Communication_Constants.OFC_N2);
                    FCU.Write(z, 0, z.Length);

                    count = FCU.BytesToRead;
                    while (count < 4 && (DateTime.UtcNow - earlier).TotalMilliseconds < 1000)
                    {
                        count = FCU.BytesToRead;
                    }
                    byte[] ByteArray1 = new byte[count];
                    FCU.Read(ByteArray1, 0, count);
                    if (ByteArray1[0] == Communication_Constants.FCO_N2[0] && count >= 4)
                    {
                        int b = (ByteArray1[1] << 16) + (ByteArray1[2] << 8) + (ByteArray1[3]);
                        main_controls.D_N2 = main_controls.RawData_Converter(b, 21);
                    }
                    else
                        Console.WriteLine("Lack Of Bytes for N2 Ground Tank Data");


                    ////Reads Rocket Mass
                    Byte[] x = System.Text.Encoding.ASCII.GetBytes(Communication_Constants.OFC_RocketMass);
                    FCU.Write(x, 0, x.Length);
                    count = FCU.BytesToRead;
                    while (count < 4 && (DateTime.UtcNow - earlier).TotalMilliseconds < 1000)
                    {
                        count = FCU.BytesToRead;
                    }
                    byte[] ByteArray2 = new byte[count];
                    FCU.Read(ByteArray2, 0, count);
                    if (ByteArray2[0] == Communication_Constants.FCO_RocketMass[0] && count >= 4)
                    {
                        int b = (ByteArray2[1] << 16) + (ByteArray2[2] << 8) + (ByteArray2[3]);
                        main_controls.D_RocketMass = main_controls.RawData_Converter(b, 22);
                    }
                    else
                        Console.WriteLine("Lack Of Bytes for Rocket Mass");

                    ////Reads Rocket Thrust
                    Byte[] c = System.Text.Encoding.ASCII.GetBytes(Communication_Constants.OFC_RocketThrust);
                    FCU.Write(c, 0, c.Length);
                    count = FCU.BytesToRead;
                    while (count < 4 && (DateTime.UtcNow - earlier).TotalMilliseconds < 1000)
                    {
                        count = FCU.BytesToRead;
                    }
                    byte[] ByteArray3 = new byte[count];
                    FCU.Read(ByteArray3, 0, count);
                    if (ByteArray3[0] == Communication_Constants.FCO_RocketThrust[0] && count >= 4)
                    {
                        int b = (ByteArray3[1] << 16) + (ByteArray3[2] << 8) + (ByteArray3[3]);
                        main_controls.D_RocketThrust = main_controls.RawData_Converter(b, 23);
                    }
                    else
                        Console.WriteLine("Lack Of Bytes for Rocket Thrust");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e + "" + FCU.IsOpen);
                    if (FCU.IsOpen == false)
                    {
                        main_controls.CSS_Writer_List.Add("S" + (char)"FCUD".Length + "FCUD");
                        FCU.Close();
                        main_controls.FCU_PORT = "";
                        FCU = null;
                        c.Abort();
                    }
                }
            }
        }
        /// Uses a switch statement to add the appropriate status code to the 
        /// CSS_Writer_List ArrayList
        private void Process_FCU(String data)
        {
            switch (data)
            {
                case Communication_Constants.FCO_OFC_O:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_OFC_O.Length + Communication_Constants.OC_OFC_O); break; }
                case Communication_Constants.FCO_OFC_C:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_OFC_C.Length + Communication_Constants.OC_OFC_C); break; }
                case Communication_Constants.FCO_OBC_O:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_OBC_O.Length + Communication_Constants.OC_OBC_O); break; }
                case Communication_Constants.FCO_OBC_C:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_OBC_C.Length + Communication_Constants.OC_OBC_C); break; }
                case Communication_Constants.FCO_OVC_O:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_OVC_O.Length + Communication_Constants.OC_OVC_O); break; }
                case Communication_Constants.FCO_OVC_C:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_OVC_C.Length + Communication_Constants.OC_OVC_C); break; }
                case Communication_Constants.FCO_PFC_O:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_PFC_O.Length + Communication_Constants.OC_PFC_O); break; }
                case Communication_Constants.FCO_PFC_C:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_PFC_C.Length + Communication_Constants.OC_PFC_C); break; }
                case Communication_Constants.FCO_PBC_O:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_PBC_O.Length + Communication_Constants.OC_PBC_O); break; }
                case Communication_Constants.FCO_PBC_C:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_PBC_C.Length + Communication_Constants.OC_PBC_C); break; }
            }

        }
        /// Converts the string given in the parameter to bytes and send it to 
        /// FCU via Serial Port Connection. 
        private String FCU_Writer(String msg)
        {
            String data = "";
            try
            {
                Byte[] dataq = System.Text.Encoding.ASCII.GetBytes(msg);
                FCU.Write(dataq, 0, dataq.Length);
                data = FCU.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "" + FCU.IsOpen);
                if (!FCU.IsOpen)
                {
                    main_controls.CSS_Writer_List.Add("S" + (char)"FCUD".Length + "FCUD");
                    FCU.Close();
                    main_controls.FCU_PORT = "";
                    FCU = null;
                    c.Abort();
                }
            }
            return data;
        }
 
    }
}
