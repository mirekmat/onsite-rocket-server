using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace On_Site_Server_Ver3
{
    class Communication_Constants
    {
        //Prefixes
        //CO: CSS --> OSS
        //OC: OSS --> CSS
        //OJ: OSS --> JumpStarter
        //JO: JumpStarter --> OSS
        //OF: OSS --> FireStarter
        //FO: FireStarter --> OSS
        //OFC: OSS --> Flow Control Unit
        //FCO: Flow Control Unit --> OSS

        //Global Variables
        //JumpStarter
        //Ematch Fire
        public const String CO_Ematch_1 = "FE1"; public const String OJ_Ematch_1 = "1"; public const String OC_Ematch_1_C = "EM1C"; public const String OC_Ematch_1_D = "EM1D";
        public const String CO_Ematch_2 = "FE2"; public const String OJ_Ematch_2 = "2"; public const String OC_Ematch_2_C = "EM2C"; public const String OC_Ematch_2_D = "EM2D";
        public const String CO_Ematch_3 = "FE3"; public const String OJ_Ematch_3 = "3"; public const String OC_Ematch_3_C = "EM3C"; public const String OC_Ematch_3_D = "EM3D";
        public const String CO_Ematch_4 = "FE4"; public const String OJ_Ematch_4 = "4"; public const String OC_Ematch_4_C = "EM4C"; public const String OC_Ematch_4_D = "EM4D";
        public const String OJ_Check_Status = "7";

        //FireStarter
        //Commands
        public const String CO_Arm_E = "ERA"; public const String OF_Arm_E = "AE\n"; public const String FO_Arm_EA = "AEA"; public const String FO_Arm_ED = "AED"; public const String OC_Arm_EA = "AEA"; public const String OC_Arm_ED = "AED";
        public const String CO_Dis_E = "ERD"; public const String OF_Dis_E = "DE\n"; public const String FO_Dis_EA = "DEA"; public const String FO_Dis_ED = "DED"; public const String OC_Dis_EA = "DEA"; public const String OC_Dis_ED = "DED";
        public const String CO_Arm_F = "FSA"; public const String OF_Arm_F = "AF\n"; public const String FO_Arm_FA = "AFA"; public const String FO_Arm_FD = "AFD"; public const String OC_Arm_FA = "AFA"; public const String OC_Arm_FD = "AFD";
        public const String CO_Dis_F = "FSD"; public const String OF_Dis_F = "DF\n"; public const String FO_Dis_FA = "DFA"; public const String FO_Dis_FD = "DFD"; public const String OC_Dis_FA = "DFA"; public const String OC_Dis_FD = "DFD";
        public const String CO_Blow_Pyro = "PVB"; public const String OF_Blow_Pyro = "qd\n"; public const String FO_Blow_PyroA = "PAA"; public const String FO_Blow_PyroD = "PAD"; public const String OC_Blow_PyroA = "PAA"; public const String OC_Blow_PyroD = "PAD";
        public const String CO_Start_BF = "SRTBF"; public const String OF_Start_BF = "bb\n";
        public const String CO_Stop_BF = "STPBF"; public const String OF_Stop_BF = "be\n";
        public const String CO_IA_BF = "IABF"; public const String OF_IA_BF = "bi\n";

        //Automatic Checks
        public const String OF_Ignite_C = "IC\n"; public const String FO_Ignite_C_A = "ICA"; public const String FO_Ignite_C_D = "ICD"; public const String OC_Ignite_C_C = "IGC"; public const String OC_Ignite_C_D = "IGD";
        public const String OF_E_Reg_Check = "ES\n"; public const String FO_E_Reg_Check_A = "ESA"; public const String FO_E_Reg_Check_D = "ESD"; public const String OC_E_Reg_Check_C = "ERC"; public const String OC_E_Reg_Check_D = "ERD";
        public const String OF_Chamber_Check = "CD\n"; public const String FO_Chamber_Check_A = "CDA"; public const String FO_Chamber_Check_D = "CDD"; public const String OC_Chamber_Check_C = "CSC"; public const String OC_Chamber_Check_D = "CSD";
        //Data Collection
        public const String OF_Pessurant_P = "PD\n"; public const String FO_Pessurant_P = "P"; public const String OC_Pessurant_P = "P";
        public const String OF_Oxidizer_P = "PF\n"; public const String FO_Oxidizer_P = "O"; public const String OC_Oxidizer_P = "O";
        public const String OF_Backfill_A = "bi\n"; public const String FO_Backfill_A = "B"; public const String OC_Backfill_A = "A";

        //Flow Control Unit
        //Valve Control
        public const String CO_OFV_O = "OFVO"; public const String OFC_OFV_O = "O1\n"; public const String FCO_OFC_O = "A"; public const String OC_OFC_O = "OFVO";
        public const String CO_OFV_C = "OFVC"; public const String OFC_OFV_C = "C1\n"; public const String FCO_OFC_C = "a"; public const String OC_OFC_C = "OFVC";
        public const String CO_OBV_O = "OBVO"; public const String OFC_OBV_O = "O2\n"; public const String FCO_OBC_O = "B"; public const String OC_OBC_O = "OBVO";
        public const String CO_OBV_C = "OBVC"; public const String OFC_OBV_C = "C2\n"; public const String FCO_OBC_C = "b"; public const String OC_OBC_C = "OBVC";
        public const String CO_OVV_O = "OVVO"; public const String OFC_OVV_O = "O3\n"; public const String FCO_OVC_O = "C"; public const String OC_OVC_O = "OVVO";
        public const String CO_OVV_C = "OVVC"; public const String OFC_OVV_C = "C3\n"; public const String FCO_OVC_C = "c"; public const String OC_OVC_C = "OVVC";
        public const String CO_PFV_O = "PFVO"; public const String OFC_PFV_O = "O4\n"; public const String FCO_PFC_O = "D"; public const String OC_PFC_O = "PFVO";
        public const String CO_PFV_C = "PFVC"; public const String OFC_PFV_C = "C4\n"; public const String FCO_PFC_C = "d"; public const String OC_PFC_C = "PFVC";
        public const String CO_PBV_O = "PBVO"; public const String OFC_PBV_O = "O5\n"; public const String FCO_PBC_O = "E"; public const String OC_PBC_O = "PBVO";
        public const String CO_PBV_C = "PBVC"; public const String OFC_PBV_C = "C5\n"; public const String FCO_PBC_C = "e"; public const String OC_PBC_C = "PBVC";
        //Data Collection
        public const String OFC_FuelN2O = "FMD\n"; public const String FCO_FuelN2O = "F"; public const String OC_FuelN2O = "F";
        public const String OFC_N2 = "PMD\n"; public const String FCO_N2 = "P"; public const String OC_N2 = "S";
        public const String OFC_RocketMass = "RMD\n"; public const String FCO_RocketMass = "R"; public const String OC_RocketMass = "T";
        public const String OFC_RocketThrust = "THD\n"; public const String FCO_RocketThrust = "T"; public const String OC_RocketThrust = "M";

        Control_Main main_controls;
        public Communication_Constants(Control_Main parent)
        {
            //Inherits the Control Main Function
            main_controls = parent;
        }

    }
}
