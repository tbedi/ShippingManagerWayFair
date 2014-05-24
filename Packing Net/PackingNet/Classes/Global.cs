using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PackingClassLibrary;
using PackingClassLibrary.CustomEntity;
using System.Windows.Threading;
using System.Globalization;
using System.Threading;
using System.Windows.Documents;
using PackingClassLibrary.Models;

namespace Packing_Net.Classes
{
    /// <summary>
    /// Use Global Class to store Application Level variables.
    /// </summary>
    public class Global
    {
        public static Thread newWindowThread;
    /// <summary>
    /// Controller object.
    /// </summary>
        public static smController controller = new smController();

        #region Model Objects
        /// <summary>
        /// UserModel Object for global access.
        /// </summary>
        public static model_User LoggedUserModel;
        /// <summary>
        /// Station Model information for global Access.
        /// </summary>
        public static model_Station StationModel;
        #endregion

        //Print Wayfair Box Lables
        public String ShipmentNumber;
        public static string BoxNumberScanned;
        public static int counter = 0;
        public static string WH;
        public static string skuNamefor;
        public static int numbersku;
        public static string LoginType;
        public static string LTLLogin;
     


        public static string ShippingNumber;
        public static int primaryKey;
        public static String WindowTopUserName;
        
        //message box veriables.
        public static string MsgBoxTitle;
        public static string MsgBoxMessage;
        public static string MsgBoxType;
        public static String MsgBoxResult;
        public static String SKUName;
        
        //User Control Veriables.
        public static Guid  LoggedUserId;
        public static string StationName;//For Station Name Dispaly..

        //LastLoginTime
        public static DateTime LastLoginDateTime;
        
        //Manager Override Scan ID
        public static Guid ManagerID;
        public static string ManagerName;
        public static string Mode;
        
        //Same User Repacking Mode Packing ID
        public static Guid SameUserpackingID;
        
        //Timer Flag
        public static Boolean ISTimerRaised ;
        public static Boolean ISTimerTickInitialised = false;
        public static String TimeOutUserName="";
       
        //language File name for messageBox Translator
        public static String LanguageFileName;

        //StationDenied
        public static Boolean ISStateTimer=false;

        //Recently Packed Shipment ID
        public static Guid RecentlyPackedID;

        //Packing shipment GUID
        public static Guid PackingID;
        
        //Show Barcode in GridView radio button Status;
        public static String ISBarcodeShow = "Yes";
        
        //List scroll messages in Application
        public static List<Run> lsScroll = new List<Run>();

        //PrintBoxID
        public static Guid PrintBoxID = Guid.Empty;

        public static string formatDateTime(DateTime date, string culture)
        {
            return DateTime.UtcNow.ToString("MMM, dd yyyy hh:mm tt", CultureInfo.CreateSpecificCulture(culture));
        }

    }    
}