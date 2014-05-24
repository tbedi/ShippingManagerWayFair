#region Assemblies
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Packing_Net.Classes;
using PackingClassLibrary;
using PackingClassLibrary.CustomEntity;
using System.Windows.Threading;
using System.Globalization;
using System.Threading;
using PackingClassLibrary.BusinessLogic;
using PackingClassLibrary.CustomEntity.SMEntitys;
#endregion

namespace Packing_Net.Pages
{
    public partial class MainWindow : Window
    {
        #region Declaration section
        //Global Dispacher Timer 
        DispatcherTimer timer = new DispatcherTimer();
        #endregion

        #region Events
        /// <summary>
        /// Shipment Scan Initialize
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                //Hode the Error Strip
                BErrorMsg.Visibility = System.Windows.Visibility.Hidden;

                //time on top of window
                if (Global.ISStateTimer == false)
                {
                    timer.Tick += timer_Tick;
                    timer.Interval = new TimeSpan(0, 0, 1);
                    Global.ISStateTimer = true;
                    timer.Start();
                }

                //Session Timer if Tick event is not initiolised.
                if (Global.ISTimerTickInitialised == false)
                {
                    SessionManager.Autotimer.Tick += Autotimer_Tick;
                    Global.ISTimerTickInitialised = true;
                }

                //Reset the sesstion timer.
                SessionManager.StartTime();
                //---

                //Show Error Stack
                _showListStrings();
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - MainWindow", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }

        }
        /// <summary>
        /// Timer for refresh and lagel clock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                lblTime.Content = DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm:ss tt");
                string sec = DateTime.UtcNow.Second.ToString();
                if (sec == "30")
                {
                    if (CheckStationRegistred.IsRegistered() == false)
                    {
                        wndStationMaster _Station = new wndStationMaster();
                        _Station.ShowDialog();
                    }
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - timer_Tick", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        private void Autotimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Global.ISTimerRaised == false)
                {
                    //log.
                    SaveUserLogsTbl.logThis(csteActionType.SessionTimeOut.ToString());
                    //Set Timer rased flag on
                    Global.ISTimerRaised = true;
                    //Open login Form
                    Login _login = new Login();
                    //Set it as top most form.
                    _login.Topmost = true;
                    _login.ShowDialog();
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - Autotimer_Tick", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {

                if ( Global.LoginType=="UPSandFEDEX")
                {
                    rbtnltl.Visibility = Visibility.Hidden;
                    rbtnother.Visibility = Visibility.Hidden;
                }

                //log.
                SaveUserLogsTbl.logThis(csteActionType.MainWindowLoaded.ToString());
                if (Global.controller.IsSuperUser(Global.LoggedUserId))
                    cvsHomeBtn.Visibility = System.Windows.Visibility.Visible;

                lblTime.Content = DateTime.UtcNow.ToLongDateString();
                txtShipmentId.Focus();
                lblUserName.Content = Global.WindowTopUserName;

                // txtShipmentId.Text = "SH0002XXX";
                lblStationName.Content = Global.StationName;
                //Display label Last Login time Of The  user;
                DateTime Dt = Convert.ToDateTime(Global.LastLoginDateTime);
                lblLastLoginTime.Content = Dt.ToString("MMM dd, yyyy h:mm tt ").ToString();

                //Show Error message on the Strip;
                _scrollMsg("Please Scan shipment ID.", Colors.Green);

                //Convert All Form labels to Language.
                WindowLanguages.Convert(this);
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - Window_Loaded_1", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
            
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                _saveClick();
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - Button_Click_4", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        private void txtShipmentId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                //Logout expire timer RE-start
                SessionManager.StartTime();
               
                if (e.Key == System.Windows.Input.Key.Enter && txtShipmentId.Text.Trim() != "")
                {
                    // Set Shipment Properties;
                    _saveClick();
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - txtShipmentId_KeyDown", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }

        }
       
        /// <summary>
        /// Logout Confirmation msg
        /// </summary>
        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //log.
                SaveUserLogsTbl.logThis(csteActionType.Logout.ToString());

                MsgBox.Show("Error", "Logout", Environment.NewLine + "Are you sure want to logout? ");
                if (Global.MsgBoxResult == "Ok")
                {
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - btnHome_Click", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        private void btnAdminHome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //log.
                SaveUserLogsTbl.logThis(csteActionType.MainWindow_Button_Home.ToString());
                HomeScreen _AdminHome = new HomeScreen();
                _AdminHome.Show();
                this.Close();
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - btnAdminHome_Click", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                wndReports report = new wndReports();
                this.Close();
                report.ShowDialog();
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - btnReports_Click", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }
        #endregion

        #region Methods
       
        #region click event
       
        /// <summary>
        /// Check for shipment Packed or not exist in the table .
        /// </summary>
        private void _saveClick()
        {
            BErrorMsg.Visibility = System.Windows.Visibility.Hidden;
            try
            {
                Global.PackingID = Guid.Empty;
                Global.ShippingNumber = txtShipmentId.Text.ToUpper();

                //Get model of shipment;
                model_Shipment modelshipment = Global.controller.getModelShipment(Global.ShippingNumber);
                String ApplicationLocation = Global.controller.ApplicationLocation();
                Boolean ISSameLocationPacked = modelshipment.ShipmentFucntions.IsShipmentPackedAtSameLocation(modelshipment.PackedLocations, ApplicationLocation, modelshipment.IsAlreadyPacked);
                if (modelshipment.IsShipmentValidated)
                {
                    if (modelshipment.IsAlreadyPacked && ISSameLocationPacked)
                    {
                        List<cstPackageTbl> _lsPck = modelshipment.PackingInfo;
                        int PackingStatus = modelshipment.PackingInfo.FirstOrDefault(i => i.ShipmentLocation == ApplicationLocation).PackingStatus;
                        Guid UserId = modelshipment.PackingInfo.FirstOrDefault(i => i.ShipmentLocation == ApplicationLocation).UserID;
                        String UserName = modelshipment.UserInfo_ShipmentPackedBy.FirstOrDefault(i => i.UserID == UserId).UserFullName;
                        String StationName = Global.controller.GetStationMaster(modelshipment.PackingInfo[0].StationID).StationName;
                        DateTime PakedDate = modelshipment.PackingInfo[0].EndTime;

                        #region single location packed
                        if (modelshipment.IsMultiLocation == false)
                        {
                           // if (PackingStatus == 1)
                           // {
                                //log.
                                SaveUserLogsTbl.logThis(csteActionType.ShipmentID_UnderPacking_Scan_00.ToString(), _lsPck[0].PackingId.ToString());

                                //if (UserId == Global.LoggedUserId)
                                if (true)
                                {
                                    //if (modelshipment.CanOverride)
                                    //{
                                    //    //set mod to the SameUser packing the Shipment;
                                        Global.Mode = "SameUser";
                                        Global.SameUserpackingID = modelshipment.PackingInfo[0].PackingId;
                                        this.Dispatcher.Invoke(new Action(() => { _show_Shipment(); }));
                                //    }
                                //    else
                                //    {
                                //        //Show Error message on the Strip;
                                //        _scrollMsg("Warning:\"" + txtShipmentId.Text + "\" is already overridden by you. Can not override shipment more than one.", Color.FromRgb(222, 87, 24));
                                //    }
                                //}
                                //else
                                //{
                                //    _scrollMsg("Warning: " + "'" + txtShipmentId.Text.ToUpper() + "' Shipment is under packing. user " + UserName + " is packing The Shipment at station  " + StationName, Color.FromRgb(222, 87, 24));
                                //    MsgBox.Show("Shipment", "Warning", "'" + txtShipmentId.Text.ToUpper() + "'" + Environment.NewLine + " Shipment is under packing." + Environment.NewLine + "user " + UserName + " is packing The Shipment at station  " + StationName);

                                //}
                                //_actMessageResult();
                                txtShipmentId.Text = "";
                            }
                            else if (PackingStatus == 0)
                            {
                                //log.
                                SaveUserLogsTbl.logThis(csteActionType.ShipmentID_AlreadyPacked_Scan_00.ToString(), _lsPck[0].PackingId.ToString());
                                //Distroy Shipment Object;
                                modelshipment = new model_Shipment();
                                _scrollMsg("Warning: " + "'" + txtShipmentId.Text + "'" + " Shipment is already packed  by  " + UserName + " on " + PakedDate.ToString("dddd,MMM dd, yyyy") + " at " + PakedDate.ToString("hh:mm:ss tt"), Color.FromRgb(222, 87, 24));
                                MsgBox.Show("Shipment", "Warning", "'" + txtShipmentId.Text + "'" + " Shipment is already packed " + Environment.NewLine + " by  " + UserName + Environment.NewLine + " on " + PakedDate.ToString("dddd,MMM dd, yyyy") + Environment.NewLine + " at " + PakedDate.ToString("hh:mm:ss tt"));
                                _actMessageResult();
                                txtShipmentId.Text = "";
                            }
                        }
                        #endregion
                        else if (modelshipment.IsMultiLocation)
                        {
                            //log.
                            SaveUserLogsTbl.logThis(csteActionType.MultiLocation_ShipmentIDScan.ToString());
                            Boolean LocationFound = false;
                            foreach (cstPackageTbl _packed in modelshipment.PackingInfo)
                            {
                                if (_packed.ShipmentLocation == ApplicationLocation)
                                {
                                    LocationFound = true;
                                   // if (_packed.PackingStatus == 1)
                                  //  {
                                        //log.
                                        SaveUserLogsTbl.logThis(csteActionType.ShipmentID_UnderPacking_Scan_00.ToString(), _lsPck[0].PackingId.ToString());

                                        //same User Override 
                                       // if (UserId == Global.LoggedUserId)
                                       // {
                                            //set mod to the SameUser packing the Shipment;
                                            Global.Mode = "SameUser";
                                            Global.SameUserpackingID = _packed.PackingId;
                                            _show_Shipment();
                                      //  }
                                      //  else
                                      //  {
                                      //      _scrollMsg("Warning: " + "'" + txtShipmentId.Text.ToUpper() + "' Shipment is under packing. user " + UserName + " is packing The Shipment at station  " + StationName, Color.FromRgb(222, 87, 24));
                                      //      MsgBox.Show("Shipment", "Warning", "'" + txtShipmentId.Text.ToUpper() + "'" + Environment.NewLine + " Shipment is under packing." + Environment.NewLine + "user " + UserName + " is packing The Shipment at station  " + StationName);
                                      //  }
                                      //  _actMessageResult();
                                        txtShipmentId.Text = "";
                                       // break;
                                   // }
                                    if (_packed.PackingStatus == 0)
                                    {
                                        //log.
                                        SaveUserLogsTbl.logThis(csteActionType.ShipmentID_AlreadyPacked_Scan_00.ToString(), _lsPck[0].PackingId.ToString());
                                        //Distroy Shipment Object;
                                        modelshipment = new model_Shipment();
                                        MsgBox.Show("Shipment", "Warning", "'" + txtShipmentId.Text + "'" + " Shipment is already packed " + Environment.NewLine + " by  " + UserName + Environment.NewLine + " on " + PakedDate.ToString("dddd,MMM dd, yyyy") + Environment.NewLine + " at " + PakedDate.ToString("hh:mm:ss tt"));
                                        _scrollMsg("Warning: " + "'" + txtShipmentId.Text + "'" + " Shipment is already packed  by  " + UserName + " on " + PakedDate.ToString("dddd,MMM dd, yyyy") + " at " + PakedDate.ToString("hh:mm:ss tt"), Color.FromRgb(222, 87, 24));
                                        _actMessageResult();
                                        txtShipmentId.Text = "";
                                        break;
                                    }
                                }
                            }
                            if (LocationFound == false)
                            {
                                //Open Shipment Screen Directly.
                                _show_Shipment();
                            }
                        }
                    }
                    else if (modelshipment.ShipmentDetailSage == null || modelshipment.ShipmentDetailSage.Count <= 0)
                    {
                        //log.
                        SaveUserLogsTbl.logThis(csteActionType.Invalid_ShipmentScan__00.ToString(), txtShipmentId.Text);
                        //show error
                        _catchErrorInCondition();
                    }
                    else
                    {
                        Boolean Shown = false;
                        foreach (var sage in modelshipment.ShipmentDetailSage)
                        {
                            if (sage.Location == ApplicationLocation)
                            {
                                Shown = true;
                                    _show_Shipment();
                                break;
                            }
                        }
                        if (Shown == false)
                        {
                            //Distroy Shipment Object;
                            modelshipment = new model_Shipment();

                            //Show Error message on the Strip;
                            _scrollMsg("Warning: Please check the shipment ID! - \"" + txtShipmentId.Text + "\" is not belongs to this location.", Color.FromRgb(222, 87, 24));

                            //clear the shipment textbox
                            txtShipmentId.Text = "";
                        }
                    }
                }
                else if (!modelshipment.IsShipmentValidated && modelshipment.ShipmentDetailSage.Count>0)
                {
                    //Show error message.
                    _scrollMsg("Warning: Shipment Number: "+txtShipmentId.Text + " is already Shipped.", Color.FromRgb(222, 87, 24));
                    //clear the shipment textbox
                    txtShipmentId.Text = "";
                    //Distroy Shipment Object;
                    model_Shipment _shipment = new model_Shipment();
                }
                else
                {
                    //Show error message.
                    _scrollMsg("Warning: Shipment Number: " + txtShipmentId.Text + " is not valid. Please check Shipment Number.", Color.FromRgb(222, 87, 24));
                    //clear the shipment textbox
                    txtShipmentId.Text = "";
                    //Distroy Shipment Object;
                    model_Shipment _shipment = new model_Shipment();
                }
            }
            catch (Exception Ex)
            {
                //Show error
                _catchErrorInCondition();
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - _saveClick", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }
       
        #endregion

        /// <summary>
        /// catch Error Exception and show error on the main screen.
        /// </summary>
        private void _catchErrorInCondition()
        {
            try
            {
                //Distroy Shipment Object;
                model_Shipment _shipment = new model_Shipment();

                //Show Error message on the Strip;
                _scrollMsg("Warning: Please check the shipment ID! - \"" + txtShipmentId.Text + "\" is invalid.", Color.FromRgb(222, 87, 24));

                //clear the shipment textbox
                txtShipmentId.Text = "";
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - _catchErrorInCondition", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        /// <summary>
        ///After Message Box button clicked. how to act on that message result is informed in this function.
        /// </summary>
        private void _actMessageResult()
        {
            try
            {
                if (Global.MsgBoxResult != "")
                {
                    if (Global.MsgBoxResult == "Cancel")
                    {
                        //log.
                        SaveUserLogsTbl.logThis(csteActionType.Manager_Override_Request.ToString(), txtShipmentId.Text);
                        MsgBox.Show("Continue", "Manager Override", Environment.NewLine + Environment.NewLine + "   Manager override required." + Environment.NewLine + "   Please scan your badge.");
                    }
                    if (Global.MsgBoxResult == "Ok")
                    { }
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - _actMessageResult", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        /// <summary>
        /// Save shipping Information to shipping table 
        /// </summary>
        /// <param name="ShippingNumber">String shipping Number</param>
        private Boolean _saveShippingInformation(String ShippingNumber)
        {
            Boolean _return = false;
            try
            {
                //Get Shipping Information from sage.

                List<cstShippingTbl> lsSageInfo = new List<cstShippingTbl>();

                if (Global.LTLLogin=="LTL")
                {
                    lsSageInfo = Global.controller.GetShippingInfoFromSage(ShippingNumber,Global.LTLLogin);
                }
                else if (Global.LTLLogin == "UPSandFeDex")
                {
                    lsSageInfo = Global.controller.GetShippingInfoFromSage(ShippingNumber,Global.LTLLogin);
                }


                
                if (lsSageInfo.Count>0)
                {
                    //Save to local Database.
                    Global.controller.SetShippingTbl(lsSageInfo);
                    _return = true;
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - _saveShippingInformation", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
            return _return;
        }

        /// <summary>
        /// Shipment Lock is add value to the package information to the package table. with its override mode.
        /// </summary>
        /// <param name="OverrideMode">Mode of override if 1: Manager Override. 2: Self override. 0: No override.</param>
        /// <returns>Boolean tru or false.</returns>
        private Boolean _shipmentLock(int OverrideMode)
        {
            Boolean _return = false;
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    //save Shipping Infromtaion.
                    _return = _saveShippingInformation(txtShipmentId.Text);

                    List<cstPackageTbl> lsPacking = new List<cstPackageTbl>();
                    cstPackageTbl _packingCustom = new cstPackageTbl();
                    _packingCustom.ShippingNum = txtShipmentId.Text;
                    _packingCustom.UserID = Global.LoggedUserModel.UserInfo.UserID;
                    _packingCustom.StartTime = DateTime.UtcNow;
                    _packingCustom.EndTime = DateTime.UtcNow;
                    _packingCustom.StationID = Global.controller.GetStationMasterByName(Global.StationName).StationID;
                    _packingCustom.ShippingID = Global.controller.GetShippingTbl(txtShipmentId.Text).ShippingID;
                    _packingCustom.ShipmentLocation = Global.controller.ApplicationLocation();

                    //Status: 1 - Under Packing Process.
                    //Status: 0 - Packing Complete
                    _packingCustom.PackingStatus = 1;

                    //Status: 0 - Not Manger override packing
                    //Status: 1 - Manger Override packing
                    //Status: 2 - Same User Repacking
                    _packingCustom.MangerOverride = OverrideMode;

                    lsPacking.Add(_packingCustom);
                    if (OverrideMode == 2)
                    {
                        //No save No update just pass the old Packing Id to next operations.
                        Global.PackingID = Global.SameUserpackingID;
                    }
                    else
                    {
                        Global.PackingID = Global.controller.SetPackingTable(lsPacking);
                    }

                }
                catch (Exception Ex)
                {
                    //Log the Error to the Error Log table
                    ErrorLoger.save("wndShipmentScanPage - _shipmentLock", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
                }
            }));
            return _return;
        }

        /// <summary>
        /// Lock the Screen and show shipment window.
        /// </summary>
        private void _show_Shipment()
        {
            try
            {
                Boolean _return = false;
                //lock the shipment that is under packing process.
                if ( Global.Mode == "SameUser")
                {
                    _return = _shipmentLock(2);
                }
                else if (Global.Mode == "Override")
                {
                    _return = _shipmentLock(1);
                }
                else
                {
                    _return = _shipmentLock(0);
                }
                if (_return)
                {
                    //Start please wait screen in saprate thread.
                    WindowThread.start();

                    //Set the Global Shiment Number
                    Global.ShippingNumber = txtShipmentId.Text.ToUpper();

                    ShipmentScreen shipmentScreen = new ShipmentScreen();

                    //loger add log.
                    SaveUserLogsTbl.logThis(csteActionType.ShipmentID_Scan.ToString(), Global.ShippingNumber.ToString());
                    _scrollMsg("Valid Shipment Scanned. Shipment ID =" + Global.ShippingNumber, Color.FromRgb(38, 148, 189));

                    shipmentScreen.Show();

                    //close thi screen.
                    this.Close();
                }
                else
                {
                    _scrollMsg("Warning: shipping information not available. Please scan another shipment.", Color.FromRgb(222, 87, 24));
                    txtShipmentId.Text = "";
                }
                
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - _show_Shipment", " [" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }
        #endregion
        
        #region Scroll message set

        /// <summary>
        /// Scroll Message show.
        /// </summary>
        /// <param name="Message">String message.</param>
        /// <param name="clr">Color of the message.</param>
        protected void _scrollMsg(string Message, Color clr)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        BErrorMsg.Visibility = System.Windows.Visibility.Hidden;
                        BErrorMsg.Visibility = System.Windows.Visibility.Visible;
                        lblErrorMsg.Foreground = new SolidColorBrush(clr);
                        // lblErrorMsg.Text = Message.ToString(); ;
                        lblErrorMsg.Text = "Shipment -[" + (DateTime.UtcNow.ToString("hh:mm:ss tt")) + "]▶ " + Message.ToString();
                        txtblStack.Inlines.Add(new Run { Text = lblErrorMsg.Text + Environment.NewLine, Foreground = new SolidColorBrush(clr) });
                        //add to list.
                        Global.lsScroll.Add(new Run { Text = lblErrorMsg.Text + Environment.NewLine, Foreground = new SolidColorBrush(clr) });
                        svStack.ScrollToBottom();
                    }
                    catch (Exception Ex)
                    {
                        //Log the Error to the Error Log table
                        ErrorLoger.save("wndShipmentScanPage - _scrollMsg", " [" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
                    }
                }));

        }

        //Load Previous Scroll messages in Sroll messgae stack of this page.
        private void _showListStrings()
        {
            try
            {
                foreach (Run r in Global.lsScroll)
                {
                    txtblStack.Inlines.Add(r);
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentScanPage - _showListStrings", " [" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }
        #endregion

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Global.RecentlyPackedID != Guid.Empty)
                {
                    //Save to Audit table
                    SaveUserLogsTbl.logThis(Global.LoggedUserId.ToString(), csteActionType.Re_Print_PackingSlip.ToString(), DateTime.UtcNow.ToString(), Global.controller.GetShippingNumber(Global.RecentlyPackedID));
                    
                    //Get Box number Packed Per Packing ID
                    List<cstBoxPackage> _lsPackageDetail = Global.controller.GetBoxPackageByPackingID(Global.RecentlyPackedID);
                    
                    //For each these Box Print Packing slip.
                    foreach (cstBoxPackage _boxitem in _lsPackageDetail)
                    {
                        //New Object Of Print window
                        wndBoxSlip _printSlip = new wndBoxSlip();

                        //assing To Pring BOXID
                        Global.PrintBoxID = _boxitem.BoxID;

                        //Open New Form
                        _printSlip.ShowDialog();
                    }

                    //Show message to user
                    _scrollMsg("Re-Printing: Packing Slip for Shipment Number " + Global.controller.GetShippingNumber(Global.RecentlyPackedID) + " printed.", Color.FromRgb(222, 87, 24));
                    txtShipmentId.Focus();

                }
                else
                {
                    _scrollMsg("Warning: Recently packed shipment information not available.", Color.FromRgb(222, 87, 24));
                }
            }
            catch (Exception)
            {}
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Global.LTLLogin = "UPSandFeDex";
            txtShipmentId.Focus();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            Global.LTLLogin = "LTL";
            txtShipmentId.Focus();
        }
    }
}
