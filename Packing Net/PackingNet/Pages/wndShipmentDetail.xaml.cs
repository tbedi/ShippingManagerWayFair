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
using PackingClassLibrary;
using Packing_Net.Classes;
using System.Windows.Resources;
using PackingClassLibrary.CustomEntity;
using System.Collections;
using System.Windows.Threading;
using System.Globalization;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using PackingClassLibrary.BusinessLogic;
using PackingClassLibrary.CustomEntity.SMEntitys;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Runtime.InteropServices;
using PackingNet.Classes;
using PackingNet.Pages;

#endregion
namespace Packing_Net.Pages
{
    public partial class ShipmentScreen : Window
    {
        #region Declaration

        //Get model of shipment;
        model_Shipment _shipment = Global.controller.getModelShipment(Global.ShippingNumber);

        //Datatime Veriables for packing details Start and end date.
        DateTime StartTime = DateTime.UtcNow;
        DateTime LastEndTime = DateTime.UtcNow;
        DispatcherTimer timer;
        int OrderedQuantiy = 0;
        int PakedQuantitiy = 0;
        int BoxQuantity = 1;

        //Override mode 2 Flage maintainer is row scanning automatic or by user to save reocred or update record.
        Boolean ISRowAutoScaned = false;
        DateTime itemPackedTime = DateTime.UtcNow;

        //Work with gridview fill complete event.
        BackgroundWorker Worker = new BackgroundWorker();

        //String application location 
        String ApplicationLocation = Global.controller.ApplicationLocation();

        //Grid Data items
        List<cstShipment> Bindedshipment = new List<cstShipment>();

        //CartonNumber for save on add Box
        int CartonNumber = 0;

        List<KeyValuePair<int, Guid>> lsRowAndPackingDetailsiD = new List<KeyValuePair<int, Guid>>();
        Boolean IsFirstTime = true;

        Boolean IsBoxAdded = false;
        #endregion

        public ShipmentScreen()
        {
            InitializeComponent();
            try
            {
                txtScannSKu.Focus();

                //Hide the Error Label
                BErrorMsg.Visibility = System.Windows.Visibility.Hidden;

                //Show the satation name at the title
                lblStationName.Content = Global.StationName;

                //Define DispacherTimer to Show clock at the Top;
                timer = new DispatcherTimer();
                timer.Tick += timer_Tick;
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Start();

                DateTime Dt = Convert.ToDateTime(Global.LastLoginDateTime);
                lblLastLoginTime.Content = Dt.ToString("MMM dd, yyyy h:mm tt ").ToString();

                //Add event to the BackGround worker.
                Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - ShipmentScreen", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                lblTime.Content = DateTime.UtcNow.ToString("MMM dd, yyyy h:mm tt ").ToString();
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - timer_Tick", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        #region Page load

        /// <summary>
        /// Window Load functions Placed here.
        /// Data Binding to the Gridview is done on the window load from the appropriate Funcation call to 
        /// Dll included in the project.
        /// </summary>
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {
                //log 
                Thread t = new Thread(() => { SaveUserLogsTbl.logThis(csteActionType.ShipmentScreenLoaded.ToString(), Global.ShippingNumber.ToString()); });

                //Add Box Button hide
                btnAddNewBox.Visibility = Visibility.Hidden;

                //Clear recently packed packing ID;
                Global.RecentlyPackedID = Guid.Empty;

                //start sesseion start
                Thread t1 = new Thread(() => { SessionManager.StartTime(); });
                //Show Scroll Messages in message stack
                this.Dispatcher.Invoke(new Action(() => { _showListStrings(); }));
                //Hide and Show the Error Strip to animate the Error Label
                ScrollMsg("please Scan item", Colors.Green);

                // Load the shipment Grid view depending on conditions, set Mode.
                #region Load GridView
                try
                {
                    List<cstShipment> shipment = new List<cstShipment>();

                    //Manager Override.
                    #region Manager Override

                    //Hide top label.
                    lblOverride.Visibility = System.Windows.Visibility.Hidden;

                    if (Global.Mode == "Override")
                    {
                        shipment = Global.controller.GetShipment_SPCKD(Global.ShippingNumber);

                        lblOverride.Visibility = System.Windows.Visibility.Visible;
                        lblOverride.Content = Global.controller.ConvetLanguage("This Shipment is in override mode.( Manager: " + Global.ManagerName + ")", Global.LanguageFileName);
                        lblUserTop.Content = Global.WindowTopUserName;

                        //Save New Record To the BoxPackage table
                        cstBoxPackage _boxPackage = new cstBoxPackage();
                        _boxPackage.PackingID = Global.PackingID;
                        _boxPackage.BoxCreatedTime = DateTime.UtcNow;
                        List<cstBoxPackage> lsBox = new List<cstBoxPackage>();
                        lsBox.Add(_boxPackage);
                        Global.PrintBoxID = Global.controller.SetBox(lsBox);
                        //delete previous information
                        Global.controller.DeleteCartonInfo(Global.ShippingNumber);
                    }
                    else if (Global.Mode == "SameUser")
                    {
                        shipment = Global.controller.GetShipment_SPCKD(Global.ShippingNumber);

                        lblOverride.Visibility = System.Windows.Visibility.Visible;
                        lblOverride.Content = Global.controller.ConvetLanguage("This Shipment is in re-packing Mode.", Global.LanguageFileName);
                        lblUserTop.Content = Global.WindowTopUserName;

                        try
                        {
                            cstBoxPackage _boxPackage = new cstBoxPackage();
                            _boxPackage.PackingID = Global.PackingID;
                            _boxPackage.BoxCreatedTime = DateTime.UtcNow;
                            List<cstBoxPackage> lsBox = new List<cstBoxPackage>();
                            lsBox.Add(_boxPackage);
                            Global.PrintBoxID = Global.controller.SetBox(lsBox);
                             Global.PrintBoxID = Global.controller.GetBoxPackageByPackingID(Global.SameUserpackingID).Max(bx => bx.BoxID);
                        }
                        catch (Exception)
                        {
                            cstBoxPackage _boxPackage = new cstBoxPackage();
                        //    _boxPackage.PackingID = Global.PackingID;
                        //    _boxPackage.BoxCreatedTime = DateTime.UtcNow;
                        //    List<cstBoxPackage> lsBox = new List<cstBoxPackage>();
                        //    lsBox.Add(_boxPackage);
                        //    Global.PrintBoxID = Global.controller.SetBox(lsBox);
                        }

                    }
                    else
                    {
                        shipment = Global.controller.GetShipment_SPCKD(Global.ShippingNumber);

                        //Save New Record To the BoxPackage table
                        cstBoxPackage _boxPackage = new cstBoxPackage();
                        _boxPackage.PackingID = Global.PackingID;
                        _boxPackage.BoxCreatedTime = DateTime.UtcNow;
                        List<cstBoxPackage> lsBox = new List<cstBoxPackage>();
                        lsBox.Add(_boxPackage);
                        Global.PrintBoxID = Global.controller.SetBox(lsBox);

                    }

                    if (Global.ShippingNumber != "")
                    {
                        //Fetch Shipping nuber information..

                        if (shipment != null)
                        {
                            lblShipmentId.Content = Global.ShippingNumber.ToString();
                            
                            //add comboIdTO the shipment Information.
                            CobmoIDGenrator _generate = new CobmoIDGenrator();
                            shipment = _generate.SetComboNumbers(shipment);
                            this.Dispatcher.Invoke(new Action(() => { grdContent.ItemsSource = shipment; }));
                            Bindedshipment = shipment;
                        }
                    }
                    #endregion
                    //set packing Start date when this screen loaded.
                    StartTime = DateTime.UtcNow;

                    //Display the contern at the top;
                    lblTime.Content = DateTime.UtcNow.ToLongDateString();
                    if (Global.LoggedUserModel.UserInfo.UserFullName != "")
                    {
                        lblUserName.Content = Global.WindowTopUserName;
                        lblUserTop.Content = Global.WindowTopUserName;
                    }

                    // Code for total today
                    lblTotalToday.Content = Global.controller.GetTotalToday(Global.LoggedUserId)[0].Value;

                    // Code for average time
                    TimeSpan Tm = TimeSpan.FromSeconds(Global.controller.GetAverageTime(Global.LoggedUserId)[0].Value);
                    string min = Tm.Minutes.ToString();
                    string sec = Tm.Seconds.ToString();
                    sec = sec.TrimStart(new char[] { '0' }) + "";
                    if (sec != "")
                    {
                        sec = "" + sec + "sec";
                    }
                    min = min.TrimStart(new char[] { '0' }) + "";
                    if (min != "")
                    {
                        min = min + "min:";
                    }
                    lblAverageBoxTime.Content = min + sec;
                }
                catch (Exception Ex)
                {
                    //Log the Error to the Error Log table
                    ErrorLoger.save("wndShipmentDetailPage - Window_Loaded_1_Sub1", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
                }
                #endregion

                txtScannSKu.Focus();

                //Language Change:
                WindowLanguages.Convert(this);

            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - Window_Loaded_1", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }


       

        /// <summary>
        /// Set The upate mode shipment and call the key press event of txtSkuSacn to paint rows.
        /// </summary>
        private void _sameUserRepacking()
        {
            try
            {
                List<cstPackageDetails> _lsPackedDetailTableInfo = Global.controller.GetPackingDetailTbl(Global.SameUserpackingID);
                foreach (cstPackageDetails _PackItem in _lsPackedDetailTableInfo)
                {
                    int itemQuty = Convert.ToInt32(_PackItem.SKUQuantity);//Repacking Quantity
                    for (int i1 = 0; i1 < itemQuty; i1++)//Loop for quantity
                    {
                        //Set Auto Sacannig Flag true. 
                        ISRowAutoScaned = true;

                        //Set text of textbox to upc code.
                        txtScannSKu.Text = _shipment.ShipmentDetailSage.FirstOrDefault(i => i.SKU == _PackItem.SKUNumber).UPCCode;

                        //Set Previously packed time of item to show in the Grid.
                        itemPackedTime = _PackItem.PackingDetailStartDateTime;

                        //Set Previously packed time as packing time. for package table
                        StartTime = _shipment.PackingInfo[0].StartTime;

                        var key = Key.Enter;                    // Key to send
                        var target = txtScannSKu;   // Target element
                        var routedEvent = Keyboard.KeyDownEvent; // Event to send
                        target.RaiseEvent(new System.Windows.Input.KeyEventArgs(Keyboard.PrimaryDevice,
                         System.Windows.PresentationSource.FromVisual((Visual)target), 0, key) { RoutedEvent = routedEvent });
                    }
                }
                ISRowAutoScaned = false;
                Global.SameUserpackingID = Guid.Empty;
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - _sameUserRepacking", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }
        #endregion

        public bool IsLineType6(String SKU)
        {
            Boolean _return=false;
            foreach (var item in Bindedshipment)
            {
                if (item.SKU.ToString() == SKU.ToString() && item.LineType==6)
                {
                    _return = true;
                }
            }
            return _return;
        }

        /// <summary>
        ///Show items Packing Remaining and Items Packed label.
        /// </summary>
        public void showQuantityLabel()
        {
            try
            {
                OrderedQuantiy = 0;
                PakedQuantitiy = 0;
                //this forech for each Quantity Count.
                foreach (DataGridRow row in GetDataGridRows(grdContent))
                {
                    try
                    {
                        TextBlock txtSku = grdContent.Columns[1].GetCellContent(row) as TextBlock;
                        TextBlock txtOrderdQuantity = grdContent.Columns[3].GetCellContent(row) as TextBlock;
                        if(!IsLineType6(txtSku.Text))
                        OrderedQuantiy = Convert.ToInt32(txtOrderdQuantity.Text) + Convert.ToInt32(OrderedQuantiy);
                        TextBlock txtPakedQuantity = grdContent.Columns[4].GetCellContent(row) as TextBlock;
                        PakedQuantitiy = Convert.ToInt32(txtPakedQuantity.Text) + Convert.ToInt32(PakedQuantitiy);
                    }
                    catch (Exception Ex)
                    {
                        ErrorLoger.save("ShipmentScreen.showQuantityLabel()", Ex.Message.ToString());
                        PakedQuantitiy += 0;
                    }
                    tbkStatus.Text = "Status :- " + PakedQuantitiy + Global.controller.ConvetLanguage(" item(s) packed out of ", Global.LanguageFileName) + OrderedQuantiy + Global.controller.ConvetLanguage(" item(s).", Global.LanguageFileName);
                    if (PakedQuantitiy == OrderedQuantiy)
                    {
                        tbkStatus.Text = "All items of this shipment are packed. Please scan your badge to continue.";
                        bdrStatus.Background = new SolidColorBrush(Colors.Green);

                    }
                    else if (PakedQuantitiy >= OrderedQuantiy / 2)
                    {
                        bdrStatus.Background = new SolidColorBrush(Color.FromRgb(38, 148, 189));
                        tbkStatus.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        bdrStatus.Background = new SolidColorBrush(Color.FromRgb(38, 148, 189));
                        tbkStatus.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
                    }
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - showQuantityLabel", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        /// <summary>
        /// This is to all return DataGridRows Object
        /// </summary>
        /// <param name="grid"> Grid View object</param>
        /// <returns>DataGridRow</returns>
        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }

        /// <summary>
        /// Validation for each row is checked for complete or not
        /// </summary>
        /// <returns>Boolean true is one or more then one row not checked else false</returns>
        public Boolean Done_ButtonValidation_TocheckAllRowsCompleted()
        {
            Boolean _retuen = false;
            try
            {
                foreach (DataGridRow row in GetDataGridRows(grdContent))
                {
                    if (row.IsEnabled == true)
                    {
                        _retuen = true;
                    }
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - Done_ButtonValidation_TocheckAllRowsCompleted", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
            return _retuen;
        }

        /// <summary>
        /// Avinash:
        /// This Code is for for pluse button in Gridview tamplet field button with increments the value of textbox by 1.
        /// this coce is very IMP for next project.
        /// </summary>
        /// <param name="sender"> parameter of click event</param>
        /// <param name="e"> paramenter of click evet</param>
        private void gButton_onclick(object sender, RoutedEventArgs e)
        {


            btnAddNewBox.Visibility = Visibility.Hidden;

            //Increment Row number
            foreach (DataGridRow row in GetDataGridRows(grdContent))
            {
                if (row.IsEnabled && grdContent.Items.Count > 1)
                {
                    try
                    {
                        ContentPresenter cp = grdContent.Columns[6].GetCellContent(row) as ContentPresenter;
                        DataTemplate myDataTemplate = cp.ContentTemplate;
                        TextBox t = (TextBox)myDataTemplate.FindName("gtxtBox", cp);
                        t.Text = (Convert.ToInt32(t.Text) + 1).ToString();
                    }
                    catch (Exception)
                    { }
                }
            }
            txtScannSKu.Focus();//Set Focus on textbox of sku UPC Code Scanning.

            String BoxNumber = Global.controller.GetBoxPackageByBoxID(Global.PrintBoxID).BOXNUM;
            
                SaveToCartonInfo(BoxNumber,CartonNumber);

            //Print old Box Id slip 
                wndBoxSlip _boxSlip = new wndBoxSlip();
            _boxSlip.ShowDialog();
            this.Dispatcher.Invoke(new Action(() => { _boxSlip.Hide(); }));
           

            //save box and Replace Global BoxID with new One
            cstBoxPackage _boxPackage = new cstBoxPackage();
            _boxPackage.PackingID = Global.PackingID;
            _boxPackage.BoxCreatedTime = DateTime.UtcNow;
            List<cstBoxPackage> lsBox = new List<cstBoxPackage>();
            lsBox.Add(_boxPackage);
            Global.PrintBoxID = Global.controller.SetBox(lsBox);

            IsBoxAdded = true;


            //Set Error message on the Sctrip and Visible it to animate.
            ScrollMsg("Packing Slip: Packing Slip Printed For Packed Box", Colors.Green);


        }

        private void ImgSKU_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Worker.IsBusy)
                {
                    Worker.RunWorkerAsync();
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - ImgSKU_Loaded", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        /// <summary>
        /// Background worker method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //Show images on the screen.
                this.Dispatcher.Invoke(new Action(_fillImage));

                //Please wait screen abort.
                if (Global.newWindowThread.IsAlive)
                {
                    Global.newWindowThread.Abort();
                }

                //Barcode show hide 
                Global.ISBarcodeShow = Global.controller.ReadFromLocalFile("ISBarcodeShow");
                if (Global.ISBarcodeShow == "Yes")
                {
                    this.Dispatcher.Invoke(new Action(_showBarcode));
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(_hideBarcodes));
                }

                //Same User Packing Call Update row Event Key press textbox sku scan.
                if (Global.SameUserpackingID != Guid.Empty || Global.SameUserpackingID != null) //Already packed items mark to packed.(Update mode shipment open.)
                {
                    this.Dispatcher.Invoke(new Action(_sameUserRepacking));
                }

                //mark the Combo Product and hide its borcodes.
                this.Dispatcher.Invoke(new Action(() => { BoldFontandHideCombp(Bindedshipment); }));
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - Worker_DoWork", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        //Combo Product Bold and Barcode hide them
        public void BoldFontandHideCombp(List<cstShipment> lsShipment)
        {
            foreach (var item in lsShipment)
            {
                if (item.LineType == 6)
                {
                    foreach (DataGridRow row in GetDataGridRows(grdContent))
                    {
                        TextBlock txtSKUName = grdContent.Columns[1].GetCellContent(row) as TextBlock;
                        if (txtSKUName.Text.ToUpper() == item.SKU.ToUpper())
                        {
                            TextBlock txtproductName = grdContent.Columns[2].GetCellContent(row) as TextBlock;
                            txtproductName.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
                            txtproductName.FontWeight = FontWeight.FromOpenTypeWeight(400);
                            TextBlock txtQuantity = grdContent.Columns[3].GetCellContent(row) as TextBlock;
                            txtQuantity.Foreground = new SolidColorBrush(Colors.Gray);
                            txtQuantity.Text = "0";
                            txtSKUName.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
                            txtSKUName.FontWeight = FontWeight.FromOpenTypeWeight(400);
                            row.Background = new SolidColorBrush(Colors.Gray);
                            TextBlock txtPacked = grdContent.Columns[4].GetCellContent(row) as TextBlock;
                            txtPacked.Foreground = new SolidColorBrush(Colors.Gray);
                            ContentPresenter sp1 = grdContent.Columns[6].GetCellContent(row) as ContentPresenter;
                            DataTemplate myDataTemplate12 = sp1.ContentTemplate;
                            TextBox myTextBlock = (TextBox)myDataTemplate12.FindName("gtxtBox", sp1);
                            myTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
                            //Hode Quantity Equal Barcode
                            ContentPresenter _contentPar = grdContent.Columns[8].GetCellContent(row) as ContentPresenter;
                            DataTemplate _dataTemplate = _contentPar.ContentTemplate;
                            Image _imgBarcode = (Image)_dataTemplate.FindName("imgBarCode", _contentPar);
                            TextBlock txtComboNumber = (TextBlock)_dataTemplate.FindName("txtGroupID", _contentPar);
                            _imgBarcode.Visibility = Visibility.Hidden;
                            txtComboNumber.Text = "";
                            ContentPresenter sp = grdContent.Columns[5].GetCellContent(row) as ContentPresenter;
                            DataTemplate myDataTemplate2 = sp.ContentTemplate;
                            Button btn = (Button)myDataTemplate2.FindName("btnComplete", sp);
                            btn.Visibility = Visibility.Hidden;
                            row.IsEnabled = false;
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Fill images for the SKU in data grid view.
        /// </summary>
        private void _fillImage()
        {

            try
            {
                foreach (DataGridRow row in GetDataGridRows(grdContent))
                {
                    ContentPresenter sp = grdContent.Columns[0].GetCellContent(row) as ContentPresenter;
                    DataTemplate myDataTemplate = sp.ContentTemplate;
                    Image ImgSKUset = (Image)myDataTemplate.FindName("ImgSKU", sp);
                    ImgSKUset.Height = 50;
                    ImgSKUset.Width = 60;
                    TextBlock SKUNo = grdContent.Columns[1].GetCellContent(row) as TextBlock;
                    try
                    {
                        ImgSKUset.Source = BitmapFrame.Create(new Uri(System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "//Images//MEDIA//" + SKUNo.Text + ".jpg", UriKind.Relative));
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ImgSKUset.Source = BitmapFrame.Create(new Uri(System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "//Images//MEDIA//" + SKUNo.Text + "-1" + ".jpg", UriKind.Relative));
                        }
                        catch (Exception)
                        { }
                    }

                    try
                    {
                        txtScannSKu.Focus();
                    }
                    catch (Exception Ex)
                    {
                        Thread ti = new Thread(() =>
                        {
                            //Log the Error to the Error Log table
                            ErrorLoger.save("wndShipmentDetailPage - _fillImage_Sub1", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
                        });
                    }
                }
            }
            catch (Exception Ex)
            {
                Thread ti = new Thread(() =>
                         { //Log the Error to the Error Log table
                             ErrorLoger.save("wndShipmentDetailPage - _fillImage", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
                         });
            }
        }

        /// <summary>
        /// Barcode Show in Grid
        /// </summary>
        private void _showBarcode()
        {
            try
            {
                BarcodeLib.Barcode b = new BarcodeLib.Barcode();
                foreach (DataGridRow Row in GetDataGridRows(grdContent))
                {

                    DataGridRow row = (DataGridRow)Row;
                    TextBlock SKUNo = grdContent.Columns[1].GetCellContent(row) as TextBlock;

                    if (Global.controller.IsSKUShowBarcode(SKUNo.Text))
                    {
                        String SkuName = SKUNo.Text.ToString();

                        //Convert SKU name to UPC COde;
                        String UPC_Code = _shipment.ShipmentDetailSage.FirstOrDefault(i => i.SKU == SkuName).UPCCode;
                        if (UPC_Code.Trim() == "") UPC_Code = "00000000000";

                        //clGlobal.call.SKUnameToUPCCode(SKUNo.Text.ToString());
                        ContentPresenter sp = grdContent.Columns[8].GetCellContent(row) as ContentPresenter;
                        DataTemplate myDataTemplate = sp.ContentTemplate;
                        Image ImgbarcodSet = (Image)myDataTemplate.FindName("imgBarCode", sp);
                        System.Drawing.Image Barcodeimg = null;
                        try
                        {
                            Barcodeimg = b.Encode(BarcodeLib.TYPE.UPCA, UPC_Code, System.Drawing.Color.Black, System.Drawing.Color.White, 300, 60);
                        }
                        catch (Exception Ex)
                        {
                            //Log the Error to the Error Log table
                            ErrorLoger.save("wndShipmentDetailPage - _showBarcode_Sub1", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
                        }
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        MemoryStream ms = new MemoryStream();

                        // Save to a memory stream...
                        Barcodeimg.Save(ms, ImageFormat.Bmp);

                        // Rewind the stream...
                        ms.Seek(0, SeekOrigin.Begin);

                        // Tell the WPF image to use this stream...
                        bi.StreamSource = ms;
                        bi.EndInit();
                        ImgbarcodSet.Source = bi;

                        try
                        {
                            txtScannSKu.Focus();
                        }
                        catch (Exception Ex)
                        {
                            //Log the Error to the Error Log table
                            ErrorLoger.save("wndShipmentDetailPage - _showBarcode_Sub2", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - _showBarcode", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Global.MsgBoxMessage = "Are you sure want to logout? ";
                Global.MsgBoxTitle = "Logout";
                Global.MsgBoxType = "Error";
                Umsgbox msg = new Umsgbox();
                msg.ShowDialog();
                if (Global.MsgBoxResult == "Ok")
                {
                    HomeScreen _Home = new HomeScreen();
                    _Home.Show();
                    this.Close();
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - btnHome_Click", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        /// <summary>
        /// Save record code and roll Back code.
        /// </summary>
        private void _saveButtonClick(int PackingStatus, DataGridRow Row)
        {
            try
            {
                try
                {
                   int Rowindex= Row.GetIndex();
                   IsFirstTime = false;
                    Guid UserID = Guid.Empty;
                    UserID = Global.LoggedUserModel.UserInfo.UserID;
                    if (Global.Mode == "Override") UserID = Global.ManagerID;

                    List<cstPackageTbl> _lsPacking = new List<cstPackageTbl>();
                    cstPackageTbl _packingCustom = new cstPackageTbl();
                    _packingCustom.ShippingNum = lblShipmentId.Content.ToString();
                    _packingCustom.UserID = UserID;
                    _packingCustom.StartTime = StartTime;
                    _packingCustom.EndTime = DateTime.UtcNow;
                    _packingCustom.StationID = Global.controller.GetStationMasterByName(Global.StationName).StationID;
                    _packingCustom.ShippingID = Global.controller.GetShippingTbl(lblShipmentId.Content.ToString()).ShippingID;
                    _packingCustom.ShipmentLocation = ApplicationLocation;
                    if (Global.Mode == "Override")
                    {
                        _packingCustom.MangerOverride = 1;
                    }
                    else if (Global.Mode == "SameUser")
                    {
                        _packingCustom.MangerOverride = 2;
                    }

                    //Status: 1 - Under Packing Process.
                    //Status: 0 - Packing Complete
                    _packingCustom.PackingStatus = PackingStatus;
                    _lsPacking.Add(_packingCustom);

                    //Imp Code Avinash 7-May2013
                    //save in transaction table Packing Details.
                    String _result = "";//RollBack Transaction veriable.
                    try
                    {
                        List<cstPackageDetails> lsPackingCustom = new List<cstPackageDetails>();
                        DataGridRow row = Row;
                        TextBlock SKUNo = grdContent.Columns[1].GetCellContent(row) as TextBlock;
                        TextBlock ProductName = grdContent.Columns[2].GetCellContent(row) as TextBlock;
                        TextBlock QUantity = grdContent.Columns[4].GetCellContent(row) as TextBlock;
                        ContentPresenter sp = grdContent.Columns[6].GetCellContent(row) as ContentPresenter;
                        DataTemplate myDataTemplate = sp.ContentTemplate;
                        TextBox myTextBlock = (TextBox)myDataTemplate.FindName("gtxtBox", sp);
                        TextBlock Endtime = grdContent.Columns[7].GetCellContent(row) as TextBlock;
                        string[] _TempEndTimeUser = Endtime.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        String EndTimeUser = _TempEndTimeUser[1];

                        // Custom Entity Boject of Packing Details.
                        cstPackageDetails _packingC = new cstPackageDetails();
                        _packingC.PackingDetailID = Guid.NewGuid();
                        _packingC.PackingId = Global.PackingID;
                        _packingC.SKUNumber = SKUNo.Text;

                        //First Sku Items Paking DateTime is Shipment Allocation Datetime.
                        _packingC.PackingDetailStartDateTime = Convert.ToDateTime(EndTimeUser);
                        _packingC.SKUQuantity = Convert.ToInt32(QUantity.Text);

                        _packingC.BoxNumber = Global.controller.GetBoxPackageByBoxID(Global.PrintBoxID).BOXNUM;

                        _packingC.ShipmentLocation = ApplicationLocation;
                        _packingC.ProductName = ProductName.Text;

                        //Added Columns information in packing Details.
                        cstViewExtraColumns _view = Global.controller.GetViewColumnInfo(lblShipmentId.Content.ToString(), SKUNo.Text);

                        _packingC.ItemName = _view.ItemName;
                        _packingC.UnitOfMeasure = _view.UnitOfMeasure;
                        _packingC.CountryOfOrigin = _view.CountryOfOrigin;
                        _packingC.MAP_Price = _view.MAP_Price;
                        _packingC.TCLCOD_0 = _view.TCLCOD_0;
                        _packingC.TarrifCode = _view.TarrifCode;

                        lsPackingCustom.Add(_packingC);

                        //Convert message to te applition language
                        Global.MsgBoxTitle = Global.controller.ConvetLanguage(Global.controller.SetPackingTable(_lsPacking, Global.PackingID), Global.LanguageFileName);

                        //Save information inpacking detail table.
                        _result = Global.controller.SetPackingDetails(lsPackingCustom);

                        //Hide and Show the Error Strip to animate the Error Label
                        ScrollMsg("Ok. \'" + SKUNo.Text + "\' Item Packed.", Colors.Green);
                        btnAddNewBox.Visibility = Visibility.Visible;

                        //save to key value pair.
                       lsRowAndPackingDetailsiD.Add( new KeyValuePair<int, Guid>(Rowindex,_packingC.PackingDetailID));


                    }
                    catch (Exception E)
                    {
                        //Log the Error to the Error Log table
                        ErrorLoger.save("wndShipmentDetailPage - _saveButtonClick_Sub1", "[" + DateTime.UtcNow.ToString() + "]" + E.ToString(), DateTime.UtcNow, Global.LoggedUserId);

                        //RollBack Transaction
                        //RollBack Transaction function Call.
                        Boolean _Tranc = Global.controller.RollBackPakingMaster(lblShipmentId.Content.ToString());
                        Global.MsgBoxMessage = Global.controller.ConvetLanguage("Save Fail! Transaction rollback successful.", Global.LanguageFileName);
                        Global.MsgBoxTitle = Global.controller.ConvetLanguage("Warning", Global.LanguageFileName);
                    }
                    //Imp Code Avinash 7-May2013
                    if (Global.MsgBoxTitle == "Warning" || Global.MsgBoxTitle == null)
                    {
                        Global.MsgBoxTitle = Global.controller.ConvetLanguage("Warning", Global.LanguageFileName);
                        Global.MsgBoxType = "Warning";
                    }

                    

                }
                catch (Exception Ex)
                {
                    //Log the Error to the Error Log table
                    ErrorLoger.save("wndShipmentDetailPage - _saveButtonClick_Sub2", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);

                    //Set Error message on the Sctrip and Visible it to animate.
                    ScrollMsg("Warning: Unexpected Error! Force Closed.", Color.FromRgb(222, 87, 24));

                    MsgBox.Show("Warning", "Unexpected Error!", "Force Closed.");
                    MainWindow Shipmentwnd = new MainWindow();
                    Shipmentwnd.Show();
                    this.Close();
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - _saveButtonClick", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }


        private void savePackingDatilsOnly(DataGridRow Row)
        {
            try
            {
                int Rowindex = Row.GetIndex();

                List<cstPackageDetails> lsPackingCustom = new List<cstPackageDetails>();
                DataGridRow row = Row;
                TextBlock SKUNo = grdContent.Columns[1].GetCellContent(row) as TextBlock;
                TextBlock ProductName = grdContent.Columns[2].GetCellContent(row) as TextBlock;
                TextBlock QUantity = grdContent.Columns[4].GetCellContent(row) as TextBlock;
                ContentPresenter sp = grdContent.Columns[6].GetCellContent(row) as ContentPresenter;
                DataTemplate myDataTemplate = sp.ContentTemplate;
                TextBox myTextBlock = (TextBox)myDataTemplate.FindName("gtxtBox", sp);
                TextBlock Endtime = grdContent.Columns[7].GetCellContent(row) as TextBlock;
                string[] _TempEndTimeUser = Endtime.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                String EndTimeUser = _TempEndTimeUser[1];

                // Custom Entity Boject of Packing Details.
                cstPackageDetails _packingC = new cstPackageDetails();
                _packingC.PackingDetailID = Guid.NewGuid();
                _packingC.PackingId = Global.PackingID;
                _packingC.SKUNumber = SKUNo.Text;

                //First Sku Items Paking DateTime is Shipment Allocation Datetime.
                _packingC.PackingDetailStartDateTime = Convert.ToDateTime(EndTimeUser);
                _packingC.SKUQuantity = 1;//Convert.ToInt32(QUantity.Text);

                _packingC.BoxNumber = Global.controller.GetBoxPackageByBoxID(Global.PrintBoxID).BOXNUM;

                _packingC.ShipmentLocation = ApplicationLocation;
                _packingC.ProductName = ProductName.Text;

                //Added Columns information in packing Details.
                cstViewExtraColumns _view = Global.controller.GetViewColumnInfo(lblShipmentId.Content.ToString(), SKUNo.Text);

                _packingC.ItemName = _view.ItemName;
                _packingC.UnitOfMeasure = _view.UnitOfMeasure;
                _packingC.CountryOfOrigin = _view.CountryOfOrigin;
                _packingC.MAP_Price = _view.MAP_Price;
                _packingC.TCLCOD_0 = _view.TCLCOD_0;
                _packingC.TarrifCode = _view.TarrifCode;

                lsPackingCustom.Add(_packingC);

                ////Convert message to te applition language
                //Global.MsgBoxTitle = Global.controller.ConvetLanguage(Global.controller.SetPackingTable(_lsPacking, Global.PackingID), Global.LanguageFileName);

                //Save information inpacking detail table.
                Global.controller.SetPackingDetails(lsPackingCustom);

                //Hide and Show the Error Strip to animate the Error Label
                ScrollMsg("Ok. \'" + SKUNo.Text + "\' Item Packed.", Colors.Green);
                btnAddNewBox.Visibility = Visibility.Visible;

                //save to key value pair.
                lsRowAndPackingDetailsiD.Add(new KeyValuePair<int, Guid>(Rowindex, _packingC.PackingDetailID));


            }
            catch (Exception )
            { }
        }


        private void UpdatePackingDatilsOnly(DataGridRow Row , Guid PackingDetailsID)
        {
            try
            {
                int Rowindex = Row.GetIndex();

                List<cstPackageDetails> lsPackingCustom = new List<cstPackageDetails>();
                DataGridRow row = Row;
                TextBlock SKUNo = grdContent.Columns[1].GetCellContent(row) as TextBlock;
                TextBlock ProductName = grdContent.Columns[2].GetCellContent(row) as TextBlock;
                TextBlock QUantity = grdContent.Columns[4].GetCellContent(row) as TextBlock;
                ContentPresenter sp = grdContent.Columns[6].GetCellContent(row) as ContentPresenter;
                DataTemplate myDataTemplate = sp.ContentTemplate;
                TextBox myTextBlock = (TextBox)myDataTemplate.FindName("gtxtBox", sp);
                TextBlock Endtime = grdContent.Columns[7].GetCellContent(row) as TextBlock;
                string[] _TempEndTimeUser = Endtime.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                String EndTimeUser = _TempEndTimeUser[1];

                // Custom Entity Boject of Packing Details.
                cstPackageDetails _packingC = new cstPackageDetails();
                _packingC.PackingDetailID = PackingDetailsID;
               _packingC.PackingId = Global.PackingID;
                _packingC.SKUNumber = SKUNo.Text;

                //First Sku Items Paking DateTime is Shipment Allocation Datetime.
                _packingC.PackingDetailStartDateTime = Convert.ToDateTime(EndTimeUser);
                _packingC.SKUQuantity = Convert.ToInt32(Global.skuNamefor); //Convert.ToInt32(QUantity.Text);
                Global.skuNamefor = "";

                _packingC.BoxNumber = Global.controller.GetBoxPackageByBoxID(Global.PrintBoxID).BOXNUM;

                _packingC.ShipmentLocation = ApplicationLocation;
                _packingC.ProductName = ProductName.Text;

                //Added Columns information in packing Details.
                cstViewExtraColumns _view = Global.controller.GetViewColumnInfo(lblShipmentId.Content.ToString(), SKUNo.Text);

                _packingC.ItemName = _view.ItemName;
                _packingC.UnitOfMeasure = _view.UnitOfMeasure;
                _packingC.CountryOfOrigin = _view.CountryOfOrigin;
                _packingC.MAP_Price = _view.MAP_Price;
                _packingC.TCLCOD_0 = _view.TCLCOD_0;
                _packingC.TarrifCode = _view.TarrifCode;

                lsPackingCustom.Add(_packingC);

                ////Convert message to te applition language
                //Global.MsgBoxTitle = Global.controller.ConvetLanguage(Global.controller.SetPackingTable(_lsPacking, Global.PackingID), Global.LanguageFileName);

                //Save information inpacking detail table.
                Global.controller.UpdatePackingDetails(lsPackingCustom);

                //Hide and Show the Error Strip to animate the Error Label
                ScrollMsg("Ok. \'" + SKUNo.Text + "\' Item Packed.", Colors.Green);
                btnAddNewBox.Visibility = Visibility.Visible;
            }
            catch (Exception )
            { }
        }

        private void txtScannSKu_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                //Avinash: Detect the Key Pres that is scanned Code and enter at the last.
                if (e.Key == Key.Enter && txtScannSKu.Text.Trim() != "")
                {
                    if (txtScannSKu.Text.Contains("#"))
                    {
                       if (txtScannSKu.Text == "#addbox" || txtScannSKu.Text == "#ADDBOX")
                        {
                            txtScannSKu.Text = "";

                            if (btnAddNewBox.IsVisible)
                            {
                                ButtonAutomationPeer peer = new ButtonAutomationPeer(btnAddNewBox);
                                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                                invokeProv.Invoke();
                            }
                            else
                            {
                                ScrollMsg("You can not add new box.", Color.FromRgb(222, 87, 24));
                            }
                        }
                    }
                    else
                    {
                        //Hide the Combo text 
                        ComboWarningText.Visibility = Visibility.Hidden;

                        String _tempUPCStore = txtScannSKu.Text;
                        //Logout expire timer RE-start
                        SessionManager.StartTime();

                        if (!(tbkStatus.Text == "All items of this shipment are packed. Please scan your badge to continue."))
                        {
                            //Log
                            SaveUserLogsTbl.logThis(csteActionType.Shipment_RowScan.ToString(), _tempUPCStore);

                            if (txtScannSKu.Text != "" || txtScannSKu.Text == null)
                            {
                                //Convert the UPC Code to the Sku Name and assign it to the text Box.
                                string Str = txtScannSKu.Text.TrimStart('0').ToString();
                                txtScannSKu.Text = _shipment.ShipmentDetailSage.FirstOrDefault(i => i.UPCCode == Str).SKU.ToString();
                               int LineType = _shipment.ShipmentDetailSage.FirstOrDefault(i=> i.UPCCode == Str).LineType;
                                //---------

                                //Hide and Show the Error Strip to animate the Error Label
                                ScrollMsg("New item Scanned. \'" + txtScannSKu.Text + "\'", Colors.WhiteSmoke);

                                Boolean _SkuInShipment = false;
                                int _AllRowCount = grdContent.Items.Count;
                                int _EnableRowCount = 0;
                                int _SKUEnableFalseCount = 0;

                                foreach (DataGridRow row in GetDataGridRows(grdContent))
                                {
                                    TextBlock txtSKUName = grdContent.Columns[1].GetCellContent(row) as TextBlock;
                                    Global.skuNamefor = txtSKUName.Text; 
                                    TextBlock txtPckDate = grdContent.Columns[7].GetCellContent(row) as TextBlock;
                                    TextBlock txtPacked = grdContent.Columns[4].GetCellContent(row) as TextBlock;
                                    TextBlock txtQuantity = grdContent.Columns[3].GetCellContent(row) as TextBlock;
                                    ContentPresenter _contentPar = grdContent.Columns[8].GetCellContent(row) as ContentPresenter;
                                    DataTemplate _dataTemplate = _contentPar.ContentTemplate;
                                    TextBlock txtComboNumber = (TextBlock)_dataTemplate.FindName("txtGroupID", _contentPar);

                                    




                                    if (row.IsEnabled == true)//Row color wihte for enabled true rows
                                    {
                                        this.Dispatcher.Invoke(new Action(() => { row.Background = new SolidColorBrush(Colors.White); }));
                                        //Scroll item to view of grid view..
                                        //  this.Dispatcher.Invoke(new Action(() => { grdContent.ScrollIntoView(row.Item); }));
                                    }
                                    if (txtScannSKu.Text.ToUpper() == txtSKUName.Text.ToUpper())//check SKU is Present in Shiment
                                    {
                                        btnAddNewBox.Visibility = Visibility.Visible;
                                        // this cosider as Sku is present
                                        _SkuInShipment = true;
                                        if (row.IsEnabled == true )
                                        {
                                            //Scroll item to view of grid view..
                                            //this.Dispatcher.Invoke(new Action(() => { grdContent.ScrollIntoView(row.Item); }));
                                            //grdContent.ScrollIntoView(row);

                                            //set caton number
                                            CartonNumber = Convert.ToInt32(txtComboNumber.Text);
                                            

                                            #region Enable true
                                            if (Convert.ToInt32(txtPacked.Text) < Convert.ToInt32(txtQuantity.Text))
                                            {
                                                grdContent.ScrollIntoView(row.Item);
                                                //if scanned item is not combo
                                                    txtPacked.Text = Convert.ToString(Convert.ToInt32(txtPacked.Text) + 1);
                                                    this.Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        row.Background = new SolidColorBrush(Color.FromRgb(106, 188, 236));//Change Back Color  blck
                                                        if (!ISRowAutoScaned)//Update mode Flag check.
                                                        {
                                                            txtPckDate.Text = "";
                                                            txtPckDate.Text = lblUserName.Content + "-" + Environment.NewLine + (DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm:ss tt"));
                                                        }
                                                        else
                                                        {
                                                            txtPckDate.Text = "";
                                                            txtPckDate.Text = lblUserName.Content + "-" + Environment.NewLine + (itemPackedTime.ToString("MMM dd, yyyy hh:mm:ss tt"));
                                                        }

                                                    }));
                                                ////Save Recored to Database.
                                                    if (LineType != 6 )
                                                    {
                                                        if (!ISRowAutoScaned)
                                                        {
                                                            int _rowindex = row.GetIndex();
                                                            if (IsFirstTime)
                                                            { this.Dispatcher.Invoke(new Action(() => { _saveButtonClick(1, row); })); }
                                                            else if (IsRowPresentInColumn(lsRowAndPackingDetailsiD, _rowindex))
                                                            {
                                                                //if (IsBoxAdded)
                                                                ////    savePackingDatilsOnly(row);
                                                                ////else
                                                                ////    UpdatePackingDatilsOnly(row, GetGuidOfRow(lsRowAndPackingDetailsiD, _rowindex));
                                                                //{
                                                                    string _boxNUmber = Global.controller.GetBoxPackageByBoxID(Global.PrintBoxID).BOXNUM;


                                                                    if (Global.controller.checkinsertornot(txtSKUName.Text, _boxNUmber))
                                                                    {
                                                                       // Global.numbersku = 1; 

                                                                        string quantity = Global.controller.getskuquantity(txtSKUName.Text, _boxNUmber);

                                                                        Global.skuNamefor = (Convert.ToInt16(quantity) + 1).ToString();

                                                                        UpdatePackingDatilsOnly(row, GetGuidOfRow(lsRowAndPackingDetailsiD, _rowindex));
                                                                    }
                                                                    else
                                                                    {
                                                                        if (Global.controller.checkinsertornotsku(txtSKUName.Text))
                                                                        {
                                                                            //if (!Global.controller.checkinsertornotBoxnumber(_boxNUmber))
                                                                            //{
                                                                                savePackingDatilsOnly(row);
                                                                           // }
                                                                        }
                                                                        
                                                                    }
                                                                //}
                                                            }
                                                            else
                                                            {
                                                                savePackingDatilsOnly(row);
                                                            }
                                                        }
                                                            
                                                    }
                                                //ComboWarningText.Visibility = Visibility.Hidden;
                                            }
                                            if (txtPacked.Text == txtQuantity.Text && row.IsEnabled == true)
                                            {
                                                grdContent.ScrollIntoView(row.Item);

                                                ContentPresenter sp = grdContent.Columns[5].GetCellContent(row) as ContentPresenter;
                                                DataTemplate myDataTemplate = sp.ContentTemplate;
                                                Button btn = (Button)myDataTemplate.FindName("btnComplete", sp);
                                                this.Dispatcher.Invoke(new Action(() =>
                                                {
                                                    btn.Visibility = System.Windows.Visibility.Hidden;
                                                    row.Background = new SolidColorBrush(Color.FromRgb(192, 192, 192));
                                                    row.IsEnabled = false;

                                                }));
                                                if (!ISRowAutoScaned)//Update mode Flag check.
                                                {
                                                    txtPckDate.Text = lblUserName.Content + "-" + Environment.NewLine + (DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm:ss tt"));
                                                }
                                                else
                                                {
                                                    txtPckDate.Text = lblUserName.Content + "-" + Environment.NewLine + (itemPackedTime.ToString("MMM dd, yyyy hh:mm:ss tt"));
                                                }

                                                //Save Row to the Database
                                                if (!ISRowAutoScaned)//Update mode flage check
                                                {
                                                    //Add Box Button Show
                                                    btnAddNewBox.Visibility = Visibility.Visible;

                                                    //Hode Quantity Equal Barcode
                                                    ContentPresenter _contentPar1 = grdContent.Columns[8].GetCellContent(row) as ContentPresenter;
                                                    DataTemplate _dataTemplate2 = _contentPar1.ContentTemplate;
                                                    Image _imgBarcode = (Image)_dataTemplate2.FindName("imgBarCode", _contentPar1);

                                                    _imgBarcode.Visibility = Visibility.Hidden;

                                                    ////Save Recored to Database.
                                                    //if (LineType != 6)
                                                    //{
                                                    //    this.Dispatcher.Invoke(new Action(() => { _saveButtonClick(1, row); }));
                                                    //}
                                                }
                                            }
                                            txtScannSKu.Text = "";
                                            #endregion Enable true
                                        }
                                        else
                                        {
                                            _SKUEnableFalseCount = _SKUEnableFalseCount + 1;
                                        }
                                    }
                                    if (row.IsEnabled == false)
                                    {
                                        _EnableRowCount = _EnableRowCount + 1;//this count used to save function call.

                                    }
                                }//foreach end.

                                //Save call..
                                if (_EnableRowCount == _AllRowCount)
                                {
                                    btnAddNewBox.Visibility = System.Windows.Visibility.Hidden;

                                    //Avinash:4May - Display How many items remain to Pack.
                                    this.Dispatcher.Invoke(new Action(() => { showQuantityLabel(); }));
                                }
                                if (_SKUEnableFalseCount == SkuRowsInGridCount(txtScannSKu.Text.ToUpper()) && SkuRowsInGridCount(txtScannSKu.Text.ToUpper()) != 0)
                                {
                                    //Log
                                    SaveUserLogsTbl.logThis(csteActionType.ExtraProductScan__00.ToString(), _tempUPCStore);

                                    ScrollMsg("Warning: Extra Packing! Required quantity of this product is packed. Please do not pack this product. ", Color.FromRgb(222, 87, 24));

                                    txtScannSKu.Text = "";
                                }
                                //if sku not found in any row 
                                if (_SkuInShipment == false)
                                {
                                    //Log
                                    SaveUserLogsTbl.logThis(csteActionType.WrongProductScan_00.ToString(), _tempUPCStore);

                                    ScrollMsg("Warning: Wrong Product! This product is not belongs to current shipment. Please do not pack this product.", Color.FromRgb(222, 87, 24));
                                    Global.SKUName = "";
                                    txtScannSKu.Text = "";
                                }
                            }
                            else
                            {
                                //Log
                                SaveUserLogsTbl.logThis(csteActionType.WrongProductScan_00.ToString(), _tempUPCStore);

                                //Strip Error Show...
                                ScrollMsg("Warning: Product not found! \"" + _tempUPCStore + "\"  Wrong UPC Code. Please check the code again.", Color.FromRgb(222, 87, 24));
                                txtScannSKu.Clear();
                            }
                        }
                        //Badge Scan Request..
                        else if (tbkStatus.Text == "All items of this shipment are packed. Please scan your badge to continue.")
                        {
                            //Add Box Button disable
                            btnAddNewBox.Visibility = System.Windows.Visibility.Hidden;
                            //badge Scan request...
                            List<cstUserMasterTbl> lsUserInfo = Global.controller.GetSelcetedUserMaster(txtScannSKu.Text);
                            if (lsUserInfo.Count > 0 && lsUserInfo[0].UserID == Global.LoggedUserId)
                            {
                                // save If packed Quantity equal to the order Quantity.
                                //Save package Detail End time HERE==================

                                //set Packing Status 0 From 1 in package table
                                cstPackageTbl packing = Global.controller.GetPackingList(Global.PackingID, true);
                                packing.PackingStatus = 0;
                                List<cstPackageTbl> _lsNewPacking = new List<cstPackageTbl>();
                                _lsNewPacking.Add(packing);
                                
                                Global.controller.SetPackingTable(_lsNewPacking, Global.PackingID);

                                //Print packing Slip
                                Global.RecentlyPackedID = Global.PackingID;

                                // PrintSlip _printslip = new PrintSlip();
                                //_printslip.PrintPckingSlip(Global.PrintBoxID);
                                wndBoxSlip _boxSlip = new wndBoxSlip();
                                _boxSlip.ShowDialog();
                                this.Dispatcher.Invoke(new Action(() => { _boxSlip.Hide(); }));

                                #region Save to Carton Info table

                                String BoxNumber = Global.controller.GetBoxPackageByBoxID(Global.PrintBoxID).BOXNUM;

                                ///save to carton number table
                                SaveToCartonInfo(BoxNumber, CartonNumber);
                                #endregion

                                ScrollMsg("Shipment Packed. Shipment ID = " + Global.ShippingNumber, Color.FromRgb(38, 148, 189));

                                #region Clear Global Veriables
                                //clear all Global Veriabels
                                // Global.RecentlyPackedID = Guid.Empty;
                                Global.Mode = "";
                                Global.ShippingNumber = "";
                                Global.ManagerID = Guid.Empty;
                                Global.ManagerName = "";
                                Global.PackingID = Guid.Empty;
                                Global.SameUserpackingID = Guid.Empty;

                                #endregion


                                //save shipment Number fot WayFair.
                                Global.ShippingNumber = lblShipmentId.Content.ToString();

                                MainWindow Shipmentwnd = new MainWindow();
                                Shipmentwnd.Show();
                                this.Close();

                                if (Global.LoginType=="LTL")
                                {
                                    wndBoxInfo _wayfairBox = new wndBoxInfo();
                                    _wayfairBox.ShowDialog();
                                }

                              
                            }
                            else
                            {
                                txtScannSKu.Text = "";
                                //Strip Error Show...
                                //Log
                                SaveUserLogsTbl.logThis(csteActionType.Login_Invalid_User_00.ToString(), _tempUPCStore);
                                ScrollMsg("Warning: Incorrect User Scan! Please scan your badge again.", Color.FromRgb(222, 87, 24));
                            }
                        }
                    }
                    //Avinash:4May - Display How many items remain to Pack.
                    // This Function must be execute after all operation.
                    showQuantityLabel();
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - txtScannSKu_KeyDown", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);

                //Strip Error Show...
                //Log
                SaveUserLogsTbl.logThis(csteActionType.WrongProductScan_00.ToString(), txtScannSKu.Text);
                ScrollMsg("Warning: Product not found! Wrong UPC Code. Please check the code again.", Color.FromRgb(222, 87, 24));
                txtScannSKu.Clear();
            }
        }

        /// <summary>
        /// Count number of rows repeted in DataGrid 
        /// </summary>
        /// <param name="SKUName">Sku Name To be check</param>
        /// <returns>int value count</returns>
        public int SkuRowsInGridCount(String SKUName)
        {
            int Count = 0;
            try
            {
                foreach (DataGridRow row in GetDataGridRows(grdContent))
                {
                    TextBlock txtSkuName = grdContent.Columns[1].GetCellContent(row) as TextBlock;
                    if (SKUName == txtSkuName.Text.ToUpper())
                    {
                        Count = Count + 1;
                    }
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - txtScannSKu_KeyDown", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
            return Count;
        }

        private void btnExitShipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Log
                SaveUserLogsTbl.logThis(csteActionType.Shipment_ForceExit__00.ToString(), lblShipmentId.Content.ToString());

                MsgBox.Show("Error", "Exit", "Exiting shipment will clear this shipment information." + Environment.NewLine + "Are you sure want to exit shipment?");
                if (Global.MsgBoxResult == "Ok")
                {
                    Global.Mode = "";
                    Global.ShippingNumber = "";
                    Global.ManagerID = Guid.Empty;
                    Global.ManagerName = "";
                    Global.PackingID = Guid.Empty;
                    MainWindow _main = new MainWindow();
                    _main.Show();
                    this.Close();
                }
                else
                {
                    txtScannSKu.Focus();
                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - btnExitShipment_Click", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        /// <summary>
        /// Check shipment is allocated for multiple locations 
        /// </summary>
        /// <param name="ShipmentID">String ShipmentID</param>
        /// <returns>int value of Box respect to how may locations already packed this shipment</returns>
        public int MultilocationShipmentPacked(String ShipmentID)
        {
            int _returnBox = 1;
            try
            {
                if (Global.controller.ISShipMultiLocationExist(Global.ShippingNumber.ToString()) == true)
                {
                    List<cstShipmentLocationWise> lsShipmentLocation = Global.controller.ShipmentLocationList(Global.ShippingNumber.ToString());
                    foreach (var _Lovationitem in lsShipmentLocation)
                    {
                        List<cstPackageTbl> _lsPackig = Global.controller.GetPackingList(Global.ShippingNumber.ToString(), _Lovationitem.LocationName.ToString());
                        if (_lsPackig.Count > 0 && _lsPackig[0].ShipmentLocation != ApplicationLocation)
                        {
                            _returnBox++;
                        }
                    }

                }
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - MultilocationShipmentPacked", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
            return _returnBox;
        }

        private void gtxtBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox txtBox = (TextBox)e.Source;
                txtBox.Text = BoxQuantity.ToString();
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - gtxtBox_Loaded", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        #region Scroll message set
        public void ScrollMsg(string Message, Color clr)
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                   {
                       BErrorMsg.Visibility = System.Windows.Visibility.Hidden;
                       BErrorMsg.Visibility = System.Windows.Visibility.Visible;
                       lblErrorMsg.Foreground = new SolidColorBrush(clr);
                       lblErrorMsg.Text = "SKU Scan -[" + (DateTime.UtcNow.ToString("hh:mm:ss tt")) + "]▶ " + Message.ToString();
                       txtblStack.Inlines.Add(new Run { Text = lblErrorMsg.Text + Environment.NewLine, Foreground = new SolidColorBrush(clr) });
                       Global.lsScroll.Add(new Run { Text = lblErrorMsg.Text + Environment.NewLine, Foreground = new SolidColorBrush(clr) });
                       svStack.ScrollToBottom();
                   }));
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - ScrollMsg", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
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
                ErrorLoger.save("wndShipmentDetailPage - _showListStrings", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        #endregion

        private void grdContent_GotFocus(object sender, RoutedEventArgs e)
        {
            txtScannSKu.Focus();
        }

        /// <summary>
        /// Hide Barcode column from the Grid
        /// </summary>
        private void _hideBarcodes()
        {
            try
            {
                grdContent.Columns[8].Width = 0;
                grdContent.Columns[8].Header = "";
                grdContent.Columns[7].Width = 490;
            }
            catch (Exception Ex)
            {
                //Log the Error to the Error Log table
                ErrorLoger.save("wndShipmentDetailPage - _hideBarcodes", "[" + DateTime.UtcNow.ToString() + "]" + Ex.ToString(), DateTime.UtcNow, Global.LoggedUserId);
            }
        }

        private void svStack_GotFocus(object sender, RoutedEventArgs e)
        {
            txtScannSKu.Focus();
            txtScannSKu.Focusable = true;
        }

        /// <summary>
        /// save carton information with default parameter.
        /// </summary>
        /// <param name="BoxNumber"></param>
        /// <param name="CartonNumber"></param>
        /// <returns></returns>
        private Guid SaveToCartonInfo(String BoxNumber,int CartonNumber = 0)
        {
            cstCartonInfo _Carton = new cstCartonInfo();
            _Carton.CartonID = Guid.NewGuid();
            _Carton.BOXNumber = BoxNumber;
            _Carton.ShipmentNumber = lblShipmentId.Content.ToString();
            _Carton.CartonNumber = CartonNumber;
            _Carton.Printed = 0;

           return Global.controller.SaveCartonInfo(_Carton);
        }


        /// <summary>
        /// Check that row is previously save in database.
        /// </summary>
        /// <param name="lsKayVal"></param>
        /// <param name="RowIndex"></param>
        /// <returns></returns>
        private bool IsRowPresentInColumn(List<KeyValuePair<int, Guid>> lsKayVal, int RowIndex)
        {
            Boolean _return = false;
            foreach (var item in lsRowAndPackingDetailsiD)
            {
                if (item.Key == RowIndex)
                    _return = true;
            }
            return _return;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsKayVal"></param>
        /// <param name="RowIndex"></param>
        /// <returns></returns>
        private Guid GetGuidOfRow(List<KeyValuePair<int, Guid>> lsKayVal, int RowIndex)
        {
            Guid _return = Guid.Empty;
            foreach (var item in lsRowAndPackingDetailsiD)
            {
                if (item.Key == RowIndex)
                    _return = item.Value;
            }
            return _return;
        }
    }
}