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
    class FS_Communications
    {
        //Serial Ports
        public SerialPort FStarter;

        //Thread for FireStarter Reader and Writer
        public Thread f; 

        //Interval for FireStarter Reader
        int FS_interval = 10;


        Control_Main main_controls;
        public FS_Communications(Control_Main parent)
        {
            //Inherits the Control Main Function
            main_controls = parent;
        }

        public void Intialize_Reader_Thread(string portname)
        {
            FStarter = new SerialPort(portname, 57600, Parity.None, 8, StopBits.One);
            FStarter.Open();
            FStarter.WriteTimeout = 500;
            FStarter.ReadTimeout = 500;
            f = new Thread(new ThreadStart(FStarter_Reader));
            f.Start();
            Console.WriteLine("FireStarter Connected on Port:" + portname);
        }

        public void Kill_SerialPort()
        {
            try { f.Abort(); }
            catch { }
            try { FStarter.Close(); }
            catch { }
            main_controls.FS_PORT = "";
            FStarter = null;
        }

        /// Continously loops and reads data from the FStarter serial port, and 
        /// then send status and data updates to CSS_Writer_List.
        private void FStarter_Reader()
        {

            Console.WriteLine("FireStarter Reader Initialized");

            DateTime earlier = DateTime.UtcNow;
            while (true)
            {

                while (((DateTime.UtcNow - earlier).TotalMilliseconds) < FS_interval)
                {

                }
                //Console.WriteLine((DateTime.UtcNow - earlier).TotalMilliseconds);
                earlier = DateTime.UtcNow;

                ArrayList list = new ArrayList(main_controls.FS_List);
                main_controls.FS_List.Clear();

                //Processes the ArrayList and uses the FStarter_Writer to send the command
                //and receive a status, which will be then be processed by Process_FS.
                if (list.Count != 0)
                {
                    foreach (String i in list)
                    {

                        if (Communication_Constants.OF_Stop_BF == i || Communication_Constants.OF_Start_BF == i)
                        {
                            try
                            {
                                Byte[] bfa = System.Text.Encoding.ASCII.GetBytes(i);
                                FStarter.Write(bfa, 0, bfa.Length);
                            }
                            catch { }
                        }
                        else
                        {
                            String FO_status = FStarter_Writer(i);
                            Process_FS(FO_status);
                        }
                    }
                }
                #region "Automatic Status Checks"
                //Automatic status checks. 
                //String FO_status1 = FStarter_Writer(OF_Chamber_Check);
                //if (Chamber_Check != FO_status1)
                //{
                //   Process_FS(FO_status1);
                //   Chamber_Check = FO_status1;
                //}

                //String FO_status2 = FStarter_Writer(OF_Ignite_C);
                //if (Ignitor_Check != FO_status2)
                //{
                //    Process_FS(FO_status2);
                //    Ignitor_Check = FO_status2;
                //}

                //String FO_status3 = FStarter_Writer(OF_E_Reg_Check);
                //if (E_Regualator_Check != FO_status3)
                //{
                //    Process_FS(FO_status3);
                //    E_Regualator_Check = FO_status3;
                //}
                #endregion
                try
                {

                    //Reads Oxidizer Pressure Data
                    Byte[] x = System.Text.Encoding.ASCII.GetBytes(Communication_Constants.OF_Oxidizer_P);
                    FStarter.Write(x, 0, x.Length);
                    int count = FStarter.BytesToRead;
                    while (count < 4 && (DateTime.UtcNow - earlier).TotalMilliseconds < 500)
                    {
                        count = FStarter.BytesToRead;
                    }
                    byte[] ByteArray = new byte[count];
                    FStarter.Read(ByteArray, 0, count);
                    //If the first byte is equal to O, then next three bytes are converted and then converted to
                    //PSI using RawData_Converter. 
                    if (ByteArray[0] == Communication_Constants.FO_Oxidizer_P[0] && count >= 4)
                    {
                        int b = (ByteArray[1] << 16) + (ByteArray[2] << 8) + (ByteArray[3]);

                        main_controls.D_Oxidizer_P = main_controls.RawData_Converter(b, 10);

                    }
                    else
                    {
                        Console.WriteLine("Lack Of Bytes for Oxidizer Pressure");
                    }

                    //Reads Presurant Pressure Data
                    Byte[] z = System.Text.Encoding.ASCII.GetBytes(Communication_Constants.OF_Pessurant_P);
                    FStarter.Write(z, 0, z.Length);
                    count = FStarter.BytesToRead;
                    while (count < 4 && (DateTime.UtcNow - earlier).TotalMilliseconds < 500)
                    {
                        count = FStarter.BytesToRead;
                    }
                    byte[] ByteArray1 = new byte[count];
                    FStarter.Read(ByteArray1, 0, count);
                    if (ByteArray1[0] == Communication_Constants.FO_Pessurant_P[0] && count >= 4)
                    {
                        int b = (ByteArray1[1] << 16) + (ByteArray1[2] << 8) + (ByteArray1[3]);

                        main_controls.D_Pressurant_P = main_controls.RawData_Converter(b, 11);
                    }
                    else
                    {
                        Console.WriteLine("Lack Of Bytes for Pressurant Pressure");
                    }
                }
                catch (Exception e)
                {
                    //If the serial port loses connection, the port is closed and then the thread. 
                    Console.WriteLine(e + "" + FStarter.IsOpen);
                    if (FStarter.IsOpen == false)
                    {
                        main_controls.CSS_Writer_List.Add("S" + (char)"FSD".Length + "FSD");
                        FStarter.Close();
                        main_controls.FS_PORT = "";
                        FStarter = null;
                        f.Abort();
                    }
                }
            }
        }

        /// Uses a switch statement to add the appropriate status code to the 
        /// CSS_Writer_List ArrayList
        private void Process_FS(String data)
        {
            switch (data)
            {
                case Communication_Constants.FO_Arm_EA:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Arm_EA.Length + Communication_Constants.OC_Arm_EA); break; }
                case Communication_Constants.FO_Arm_ED:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Arm_ED.Length + Communication_Constants.OC_Arm_ED); break; }
                case Communication_Constants.FO_Arm_FA:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Arm_FA.Length + Communication_Constants.OC_Arm_FA); break; }
                case Communication_Constants.FO_Arm_FD:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Arm_FD.Length + Communication_Constants.OC_Arm_FD); break; }
                case Communication_Constants.FO_Dis_EA:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Dis_EA.Length + Communication_Constants.OC_Dis_EA); break; }
                case Communication_Constants.FO_Dis_ED:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Dis_ED.Length + Communication_Constants.OC_Dis_ED); break; }
                case Communication_Constants.FO_Dis_FA:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Dis_FA.Length + Communication_Constants.OC_Dis_FA); break; }
                case Communication_Constants.FO_Dis_FD:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Dis_FD.Length + Communication_Constants.OC_Dis_FD); break; }
                case Communication_Constants.FO_Blow_PyroA:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Blow_PyroA.Length + Communication_Constants.OC_Blow_PyroA); break; }
                case Communication_Constants.FO_Blow_PyroD:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Blow_PyroD.Length + Communication_Constants.OC_Blow_PyroD); break; }

                case Communication_Constants.FO_Ignite_C_A:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ignite_C_C.Length + Communication_Constants.OC_Ignite_C_C); break; }
                case Communication_Constants.FO_Ignite_C_D:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Ignite_C_D.Length + Communication_Constants.OC_Ignite_C_D); break; }
                case Communication_Constants.FO_E_Reg_Check_A:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_E_Reg_Check_C.Length + Communication_Constants.OC_E_Reg_Check_C); break; }
                case Communication_Constants.FO_E_Reg_Check_D:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_E_Reg_Check_D.Length + Communication_Constants.OC_E_Reg_Check_D); break; }
                case Communication_Constants.FO_Chamber_Check_A:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Chamber_Check_C.Length + Communication_Constants.OC_Chamber_Check_C); break; }
                case Communication_Constants.FO_Chamber_Check_D:
                    { main_controls.CSS_Writer_List.Add("S" + (char)Communication_Constants.OC_Chamber_Check_D.Length + Communication_Constants.OC_Chamber_Check_D); break; }
            }
        }
        /// Converts the string given in the parameter to bytes and send it to 
        /// FStarter via Serial Port Connection. 
        private String FStarter_Writer(String msg)
        {
            String data = "";
            try
            {
                Byte[] dataq = System.Text.Encoding.ASCII.GetBytes(msg);
                FStarter.Write(dataq, 0, dataq.Length);
                data = FStarter.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "" + FStarter.IsOpen);
                if (!FStarter.IsOpen)
                {
                    main_controls.CSS_Writer_List.Add("S" + (char)"FSD".Length + "FSD");
                    FStarter.Close();
                    main_controls.FS_PORT = "";
                    FStarter = null;
                    f.Abort();
                }
            }
            return data;
        }

    }
}
