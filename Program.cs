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

/// On-Site Server Summary
/// Connects to Firstarter, Jumpstarter, and FCU through Serial Ports.
/// Connects to CSS through a wireless TCP-IP Connection.
namespace On_Site_Server_Ver3
{
    public class Control_Main
    {
        CSS_Communications CSS;
        Communication_Constants Constant;
        FCU_Communications FCU_Object;
        FS_Communications FS_Object;
        JS_Communications JS_Object;

        //Port Numbers: Contains the portname of three serial ports. 
        public string JS_PORT = "";
        public string FS_PORT = "";
        public string FCU_PORT = "";

        //File Writer
        int Writer_interval = 100;

        //ArrayList for Status updates sent to OSS. 
        public ArrayList CSS_Writer_List = new ArrayList();
        //ArrayList of OSS to Circuit Board Commands that are still needed to be processed. 
        public ArrayList JS_List = new ArrayList();
        public ArrayList FS_List = new ArrayList();
        public ArrayList FCU_List = new ArrayList();

        //FireStarter
        public double D_Pressurant_P = 0;
        public double D_Oxidizer_P = 0;
        public double D_Backfill_A = 0;
        public String Ignitor_Check = "";
        public String E_Regualator_Check = "";
        public String Chamber_Check = ""; 

        //FCU
        public double D_FuelN2O = 0;
        public double D_N2 = 0;
        public double D_RocketMass = 0;
        public double D_RocketThrust = 0;

        /// Start Control_Main
        static void Main(string[] args)
        {
            new Control_Main();
        }
        /// Updates IP and Port Information, creates the TCP listener
        /// and starts the CSS_Reader method. 
        public Control_Main()
        {
           
            Console.WriteLine("Boston University\nRocket Propulsion Group\nOn-Site Server\nVersion: 5.0\nLast Edited: 2/17/14\n\n");

            //Intializes Objects
            Constant = new Communication_Constants(this);
            CSS = new CSS_Communications(this);
            JS_Object = new JS_Communications(this);
            FS_Object = new FS_Communications(this);
            FCU_Object = new FCU_Communications(this);

            Port_Finder(1);

            //Document Writer
            Thread document = new Thread(new ThreadStart(File_Writer));
            document.Start();

            //Keeps method alive.
            while (true) ;
        }
        #region "Port Finder"
        /// Locates Port, establishes connection, and starts the reading thread
        /// for the appropriate serial connection. 
        /// Parameter (int a): 1 = JStarter; 2 = FStarter; 3 = FCU; 
        public void Port_Finder(int a)
        {
            //These three if stataments delete any serial port data and thread operations for the appropriate 
            //serial port in case Port_Finder() was called on to reconnect to a serial port. 
            if (a == 1)
                JS_Object.Kill_SerialPort();
            if (a == 2)
                FS_Object.Kill_SerialPort();
            if (a == 3)
                FCU_Object.Kill_SerialPort();

            //Scans for available serial ports.
            foreach (string portname in SerialPort.GetPortNames())
            {
                //Creates temporary port.
                SerialPort sp = new SerialPort(portname, 57600, Parity.None, 8, StopBits.One);
                Console.WriteLine("Trying to Connect on port: " + portname);
                sp.ReadTimeout = 500;
                sp.WriteTimeout = 500;
                //If port is already used by another serial port, the code will skip to the next port
                //in the foreach statement. 
                if (portname == JS_PORT || portname == FS_PORT || portname == FCU_PORT)
                   continue;
                try
                {
                    sp.Open();

                    if (a == 2)//tries to find port for Firstarter
                    {
                        //Converts ASCII into to bytes.
                        Byte[] data;
                        data = System.Text.Encoding.ASCII.GetBytes("\n\n\n");
                        sp.Write(data, 0, data.Length);
                        data = System.Text.Encoding.ASCII.GetBytes("SN\n");
                        sp.Write(data, 0, data.Length);
                        Thread.Sleep(50);
                        String received = sp.ReadLine();
                        Console.WriteLine(received);
                        //If the recieved string matches the specified serial number the sp port will 
                        //be closed and FStarter will be assigned to that specific port. Then FStarter_Reader
                        //thread will be opened. 
                        if (received == "FS-01" || "FS-01\r" == received)
                        {
                            sp.Close();
                            FS_Object.Intialize_Reader_Thread(portname);
                            FS_PORT = portname;
                            CSS_Writer_List.Add("S" + (char)"FSC".Length + "FSC");
                            return;
                        }
                       

                    }
                    else if (a == 1)//tries to find port for Jumpstarter
                    {
                        Byte[] data;
                        data = System.Text.Encoding.ASCII.GetBytes("s");
                        sp.Write(data, 0, data.Length);
                        Thread.Sleep(300);
                        
                        String received = sp.ReadLine();
                        Console.WriteLine(received);
                        if (received == "JS-02" || received == "JS-02\r")
                        {
                            sp.Close();
                            JS_Object.Intialize_Reader_Thread(portname);
                            JS_PORT = portname;
                            CSS_Writer_List.Add("S" + (char)"JSC".Length + "JSC");
                            return;

                        }
                    }
                    else if (a == 3)//tries to find port for FCU
                    {
                        Byte[] data;

                        data = System.Text.Encoding.ASCII.GetBytes("\n\n\n");
                        sp.Write(data, 0, data.Length);
                        data = System.Text.Encoding.ASCII.GetBytes("SN\n");
                        sp.Write(data, 0, data.Length);
                        String received;
                        received = sp.ReadLine();
                        if (received == "FCU-01" || "FCU-01\r" == received)
                        {
                            sp.Close();
                            JS_Object.Intialize_Reader_Thread(portname);
                            FCU_PORT = portname;
                            CSS_Writer_List.Add("S" + (char)"FCUC".Length + "FCUC");
                            return;
                            
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    if (a == 1)
                        CSS_Writer_List.Add("S" + (char)"JSD".Length + "JSD");
                    if (a == 2)
                        CSS_Writer_List.Add("S" + (char)"FSD".Length + "FSD");
                    if (a == 3)
                        CSS_Writer_List.Add("S" + (char)"FCUD".Length + "FCUD");
                }

                try { sp.Close(); }
                catch { }
            }
            if (a == 1)
            {
                CSS_Writer_List.Add("S" + (char)"JSD".Length + "JSD");
            }
            if (a == 2)
            {
                CSS_Writer_List.Add("S" + (char)"FSD".Length + "FSD");
            }
            if (a == 3)
            {
                CSS_Writer_List.Add("S" + (char)"FCUD".Length + "FCUD");
            }
        }
        #endregion
        #region "File Writer"
        /// Write CSV Files for data through the durration of the test.
        /// Is called everytime data global variables are updated. 
        private void File_Writer()
        {
            //Finds path to document folder. 
            string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //mydocpath += @"\Data_Log";
            //If the file already exists the integer at the end of the file name will increase until
            //a file name is found that is not being used. 
            Boolean used_file = true;
            int i = 1;
            while (used_file)
            {
                if (File.Exists(mydocpath + @"\Server_Data_Log_" + i.ToString() + ".csv"))
                    i++;
                else
                    used_file = false;
            }
            //Creates file pathway.
            String document = mydocpath + @"\Server_Data_Log_" + i.ToString() + ".csv";
            Console.WriteLine("File Location: " + document);
            //Creates .csv file.
            using (StreamWriter outfile = new StreamWriter(document))
            {
                outfile.WriteLine("");
                outfile.WriteLine("Boston University");
                outfile.WriteLine("Rocket Propulsion Group");
                outfile.WriteLine("On-Site Server");
                outfile.WriteLine("Version: 5.0");
                outfile.WriteLine("Last Edited: 1/31/2014");
                outfile.WriteLine("Created: " + DateTime.UtcNow.ToString());
                outfile.WriteLine("");
                outfile.WriteLine("FireStarter" + "," + " " + "," + "Flow Control Unit");
                outfile.WriteLine("Pressurant Pressure" + "," + "Oxidizer Pressure" + "," + "N2O Fuel Pressure" + "," + "N2 Ground Tank" + "," + "Rocket Mass" + "," + "Rocket Thrust" + "," + "Time");
                outfile.WriteLine("(PSI)" + "," + "(PSI)" + "," + "(PSI)" + "," + "(PSI)" + "," + "(lbs)" + "," + "(Force Unit)" + "," + "(seconds)");
            }
            //Updates file with data from the global variables at a rate of 100 Hertz.
                DateTime earlier = DateTime.UtcNow;
                DateTime zero = DateTime.UtcNow;
                while (true)
                {
                    while (((DateTime.UtcNow - earlier).TotalMilliseconds) < Writer_interval)
                    {
                        //Acts as a timer that 
                    }
                    earlier = DateTime.UtcNow;
                    String P = D_Pressurant_P.ToString();
                    String O = D_Oxidizer_P.ToString();
                    String F = D_FuelN2O.ToString();
                    String N = D_N2.ToString();
                    String M = D_RocketMass.ToString();
                    String T = D_RocketThrust.ToString();
                    String Time = ((DateTime.UtcNow - zero).TotalSeconds).ToString();
                    File.AppendAllText(document, P + "," + O + "," + F + "," + N + "," + M + "," + T + "," + Time);
                    File.AppendAllText(document, string.Format("{0}{1}", "", Environment.NewLine));
                }
        }
        #endregion
        #region "Raw Data Converter"
        //Converts raw data, still needs to be defined. 
        long tempAvg = 0;
        long tempcnt = 0;
        long tempAvg2 = 0;
        long tempcnt2 = 0;

        public double RawData_Converter (int i, int a)
        {
            
            double OX_K = 0.0000076;
            double PR_K = 0.0000200821;
            double RM_K = 1.454E-6; // 0.0000015;
            double TH_K = 1.4915988219911E-6;
            double PP_K = .0000199;
            double OP_K = .0000938;
            double Vex = 5;
            double gain = 32;

            double OS1 = 0;
            double OS2 = -0.0001108798;
            double OS3 = -0.00001208;
            double OS4 = -.00003181394668;
            double OS5 = -.0000103546023;

            i = i - 0x800000;
            double volt = i / gain / (int)Math.Pow(2, 23) *Vex;
            switch (a)
            {
                case 10: //Oxidizer Pressure
                    {
                        return (volt - OS4) / OP_K;
                    }
                case 11: //Pressurant Pressure
                    {
                        return (volt - OS5) / PP_K;
                    }
                case 20: //N2O Fuel Data
                    {
                        return ((volt - OS1) / OX_K);
                    }
                case 21: //N2 Ground Tank
                    {
                        return ((volt - OS2) / PR_K);
                    }
                case 22: // Rocket Mass
                    {
                        double temp = ((volt - OS3) / RM_K);
                        if (tempAvg != 0)
                        {
                            tempAvg += i;
                            tempcnt++;
                        }
                        else
                        {
                            tempAvg = i;
                            tempcnt = 1;
                        }
                        Console.WriteLine("Mass: 0x" + i.ToString("X6") + " ==> " + temp+" tempAvg: 0x"+(tempAvg/tempcnt).ToString("X6"));
                        return ((volt - OS3) / RM_K);
                    }
                case 23: // Rocket Thrust
                    {
                        double temp = ((volt - OS3) / TH_K);
                        if (tempAvg2 != 0)
                        {
                            tempAvg2 += i;
                            tempcnt2++;
                        }
                        else
                        {
                            tempAvg2 = i;
                            tempcnt2 = 1;
                        }
                        Console.WriteLine("Thrust: 0x" + i.ToString("X6") + " ==> " + temp + " tempAvg: 0x" + (tempAvg2 / tempcnt2).ToString("X6"));

                        return temp;
                    }
            }
            return i;
        }
        #endregion
    }
}