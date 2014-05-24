﻿using PackingClassLibrary.CustomEntity;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackingClassLibrary.Commands.SMcommands
{
    public class cmdPackage
    {
        local_x3v6Entities entx3v6 = new local_x3v6Entities();
        Sage_x3v6Entities Sage = new Sage_x3v6Entities();

        #region Get Package
        /// <summary>
        /// get information from packag table
        /// </summary>
        /// <param name="PackingID">Guid PackingID</param>
        /// <returns>String ShippingNum</returns>
        public static String GetShippingNum(Guid PackingID)
        {
            local_x3v6Entities ent = new local_x3v6Entities();
            string lsShippingNumbers = "0";
            try
            {
                lsShippingNumbers = ent.Packages.SingleOrDefault(i => i.PackingId == PackingID).ShippingNum;
            }
            catch (Exception)
            { }
            return lsShippingNumbers;
        }

        /// <summary>
        /// get information from packag table
        /// </summary>
        /// <param name="ShippingNumber">String Shippingn Number</param>
        /// <returns>List of Guid That indicates the PackingID</returns>
        public static List<Guid> GetPackingNum(String ShippingNumber)
        {
            local_x3v6Entities ent = new local_x3v6Entities();
            List<Guid> _PackingID = new List<Guid>();
            try
            {
                var PackingNum = from Id in ent.Packages
                                 where Id.ShippingNum == ShippingNumber
                                 select Id;
                foreach (var PakingNumItem in PackingNum)
                {
                    Guid _PackingNum = new Guid();
                    _PackingNum = PakingNumItem.PackingId;
                    _PackingID.Add(_PackingNum);
                }
            }
            catch (Exception)
            { }
            return _PackingID;
        }

        public static Boolean Getbolforinsertorupdate(string _skuName, string Boxnumber)
        {
             local_x3v6Entities ent = new local_x3v6Entities();

            Boolean flag = false;
            try
            {
                var detail = from id in ent.PackageDetails
                             where id.SKUNumber == _skuName && id.BoxNumber == Boxnumber
                             select id;

                if (detail.Count()>0)
                {
                    flag = true;
                }
            }
            catch (Exception)
            {
            }
            return flag;
        
        }

        public static Boolean Getbolforsku(string _skuName)
        {
            local_x3v6Entities ent = new local_x3v6Entities();

            Boolean flag = false;
            try
            {
                var detail = from id in ent.PackageDetails
                             where id.SKUNumber == _skuName 
                             select id;

                if (detail.Count() > 0)
                {
                    flag = true;
                }
            }
            catch (Exception)
            {
            }
            return flag;

        }
        public static Boolean GetbolforBoxnumber(string _boxNumber)
        {
            local_x3v6Entities ent = new local_x3v6Entities();

            Boolean flag = false;
            try
            {
                var detail = from id in ent.PackageDetails
                             where id.BoxNumber==_boxNumber
                             select id;

                if (detail.Count() > 0)
                {
                    flag = true;
                }
            }
            catch (Exception)
            {
            }
            return flag;

        }

        public static string GetQuantityfrompackagedetail(string _skuName, string Boxnumber)
        {
            local_x3v6Entities ent = new local_x3v6Entities();

            string flag = "";
            try
            {
                var detail = from id in ent.PackageDetails
                             where id.SKUNumber == _skuName && id.BoxNumber == Boxnumber
                             select id.SKUQuantity;

                foreach (var item in detail)
                {
                    flag = item.ToString();
                }
               
            }
            catch (Exception)
            {
            }
            return flag;
        }




        /// <summary>
        /// Get Infromation from Package table 
        /// </summary>
        /// <param name="ShippingNumber">String Shipment Number</param>
        /// <param name="OverrideMode">int manager Override</param>
        /// <param name="Location">String Location</param>
        /// <returns>Guid as a packing</returns>
        public static Guid GetPackageDI(String ShippingNumber, int OverrideMode, String Location)
        {
            local_x3v6Entities ent = new local_x3v6Entities();
            Guid _PackingID = Guid.Empty;
            try
            {
                _PackingID = ent.Packages.SingleOrDefault(i => i.ShippingNum == ShippingNumber && i.ManagerOverride == OverrideMode && i.ShipmentLocation == Location).PackingId;
            }
            catch (Exception)
            { }
            return _PackingID;
        }

        /// <summary>
        /// All packing table 
        /// </summary>
        /// <returns>list < cstPackingtbl></returns>
        public List<cstPackageTbl> Execute()
        {
            List<cstPackageTbl> _lsreturn = new List<cstPackageTbl>();
            try
            {
                var listPacking = from packingtbl in entx3v6.Packages select packingtbl;
                foreach (var listitem in listPacking)
                {
                    cstPackageTbl _pack = new cstPackageTbl();
                    _pack.PackingId = listitem.PackingId;
                    _pack.ShippingID = (Guid)listitem.ShippingID;
                    _pack.ShippingNum = listitem.ShippingNum;
                    _pack.PackingStatus = Convert.ToInt32(listitem.PackingStatus);
                    _pack.UserID = listitem.UserId;
                    _pack.ShipmentLocation = listitem.ShipmentLocation;
                    _pack.StartTime = Convert.ToDateTime(listitem.StartTime);
                    _pack.EndTime = Convert.ToDateTime(listitem.EndTime);
                    _pack.StationID = (Guid)listitem.StationID;
                    _pack.MangerOverride = Convert.ToInt32(listitem.ManagerOverride);
                    _pack.PCKROWID = listitem.PCKROWID;
                    _lsreturn.Add(_pack);
                }
            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("GetPackingListSelcted.Execute(1)", Ex.Message.ToString());
            }
            return _lsreturn;
        }
        /// <summary>
        /// All packing table 
        /// </summary>
        /// <param name="UserID">long UserID</param>
        /// <returns>list < cstPackingtbl></returns>
        public List<cstPackageTbl> Execute(Guid UserID)
        {
            List<cstPackageTbl> _lsreturn = new List<cstPackageTbl>();
            try
            {
                
                var listPacking = from packingtbl in entx3v6.Packages
                                  where packingtbl.UserId == UserID
                                  select packingtbl;
                foreach (var listitem in listPacking)
                {
                    cstPackageTbl _pack = new cstPackageTbl();
                    _pack.PackingId = listitem.PackingId;
                    _pack.ShippingID = (Guid)listitem.ShippingID;
                    _pack.ShippingNum = listitem.ShippingNum;
                    _pack.PackingStatus = Convert.ToInt32(listitem.PackingStatus);
                    _pack.UserID = listitem.UserId;
                    _pack.ShipmentLocation = listitem.ShipmentLocation;
                    _pack.StartTime = Convert.ToDateTime(listitem.StartTime);
                    _pack.EndTime = Convert.ToDateTime(listitem.EndTime);
                    _pack.StationID = (Guid)listitem.StationID;
                    _pack.MangerOverride = Convert.ToInt32(listitem.ManagerOverride);
                    _pack.PCKROWID = listitem.PCKROWID;
                    _lsreturn.Add(_pack);
                }
            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("GetPackingListSelcted.Execute(2)", Ex.Message.ToString());
            }
            return _lsreturn;
        }

        /// <summary>
        /// All packing table 
        /// </summary>
        /// <param name="UserID">long UserID</param>
        /// <param name="Date">DateTime Date</param>
        /// <returns>list < cstPackingtbl></returns>
        public List<cstPackageTbl> Execute(Guid UserID, DateTime Date)
        {
            List<cstPackageTbl> _lsreturn = new List<cstPackageTbl>();
            try
            {
                var listPacking = from packingtbl in entx3v6.Packages
                                  where packingtbl.UserId == UserID &&
                                 EntityFunctions.TruncateTime(packingtbl.EndTime.Value) == EntityFunctions.TruncateTime(Date.Date)
                                  select packingtbl;

                foreach (var listitem in listPacking)
                {
                    cstPackageTbl _pack = new cstPackageTbl();
                    _pack.PackingId = listitem.PackingId;
                    _pack.ShippingID = (Guid)listitem.ShippingID;
                    _pack.ShippingNum = _pack.ShippingNum;
                    _pack.PackingStatus = Convert.ToInt32(listitem.PackingStatus);
                    _pack.UserID = listitem.UserId;
                    _pack.ShipmentLocation = listitem.ShipmentLocation;
                    _pack.StartTime = Convert.ToDateTime(listitem.StartTime);
                    _pack.EndTime = Convert.ToDateTime(listitem.EndTime);
                    _pack.StationID = (Guid)listitem.StationID;
                    _pack.MangerOverride = Convert.ToInt32(listitem.ManagerOverride);
                    _pack.PCKROWID = listitem.PCKROWID;
                    _lsreturn.Add(_pack);
                }
            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("GetPackingListSelcted.Execute(3)", Ex.Message.ToString());
            }
            return _lsreturn;
        }

        /// <summary>
        /// Packing Tbl Packing Id wiht manger Override =0
        /// </summary>
        /// <param name="ShippingNum"></param>
        /// <param name="Location"></param>
        /// <returns></returns>
        public List<cstPackageTbl> Execute(String ShippingNum, String Location)
        {
            List<cstPackageTbl> _lsPacking = new List<cstPackageTbl>();
            try
            {
                var _Packing1 = from v in entx3v6.Packages
                                where v.ShippingNum == ShippingNum && v.ShipmentLocation == Location
                                select v;
                foreach (var _Packing in _Packing1)
                {
                    cstPackageTbl _PC = new cstPackageTbl();
                    _PC.PackingId = _Packing.PackingId;
                    _PC.ShippingID = (Guid)_Packing.ShippingID;
                    _PC.ShippingNum = _Packing.ShippingNum;
                    _PC.UserID = _Packing.UserId;
                    _PC.StartTime = Convert.ToDateTime(_Packing.StartTime);
                    _PC.EndTime = Convert.ToDateTime(_Packing.EndTime);
                    _PC.StationID = (Guid)_Packing.StationID;
                    _PC.PackingStatus = Convert.ToInt32(_Packing.PackingStatus);
                    _PC.ShipmentLocation = _Packing.ShipmentLocation;
                    _PC.MangerOverride = Convert.ToInt32(_Packing.ManagerOverride);
                    _PC.PCKROWID = _Packing.PCKROWID;
                    _lsPacking.Add(_PC);
                }

            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("GetPackingListSelcted.Execute()", Ex.Message.ToString());
            }
            return _lsPacking;
        }

        public List<cstPackageTbl> Execute(String ShippingNum, String Location, int managerOvrride)
        {
            List<cstPackageTbl> _lsPacking = new List<cstPackageTbl>();
            try
            {
                local_x3v6Entities _Localx3v6 = new local_x3v6Entities();
                Package _Packing = _Localx3v6.Packages.SingleOrDefault(i => i.ShippingNum == ShippingNum && i.ShipmentLocation == Location && i.ManagerOverride == managerOvrride);
                cstPackageTbl _PC = new cstPackageTbl();
                _PC.PackingId = _Packing.PackingId;
                _PC.ShippingID = (Guid)_Packing.ShippingID;
                _PC.ShippingNum = _Packing.ShippingNum;
                _PC.UserID = _Packing.UserId;
                _PC.StartTime = Convert.ToDateTime(_Packing.StartTime);
                _PC.EndTime = Convert.ToDateTime(_Packing.EndTime);
                _PC.StationID = (Guid)_Packing.StationID;
                _PC.PackingStatus = Convert.ToInt32(_Packing.PackingStatus);
                _PC.ShipmentLocation = _Packing.ShipmentLocation;
                _PC.MangerOverride = Convert.ToInt32(_Packing.ManagerOverride);
                _PC.PCKROWID = _Packing.PCKROWID;
                _lsPacking.Add(_PC);
            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("GetPackingListSelcted.Execute()", Ex.Message.ToString());
            }
            return _lsPacking;
        }

        /// <summary>
        /// get the packing id from PCKROWID
        /// </summary>
        /// <param name="PCKROWID">
        /// String PCKROWID
        /// </param>
        /// <returns>
        /// Guid PackingID
        /// </returns>
        public Guid GetPackingID(string PCKROWID)
        {
            Guid _return = Guid.Empty;
            try
            {
                _return = entx3v6.Packages.SingleOrDefault(i => i.PCKROWID == PCKROWID).PackingId;
            }
            catch (Exception)
            {}

            return _return;
        }

        public cstPackageTbl ExecutePacking(Guid PackingID)
        {
            cstPackageTbl _PC = new cstPackageTbl();
            try
            {
                local_x3v6Entities _Localx3v6 = new local_x3v6Entities();
                Package _Packing = _Localx3v6.Packages.SingleOrDefault(i => i.PackingId == PackingID);
                
                _PC.PackingId = _Packing.PackingId;
                _PC.ShippingID = (Guid)_Packing.ShippingID;
                _PC.ShippingNum = _Packing.ShippingNum;
                _PC.UserID = _Packing.UserId;
                _PC.StartTime = Convert.ToDateTime(_Packing.StartTime);
                _PC.EndTime = Convert.ToDateTime(_Packing.EndTime);
                _PC.StationID = (Guid)_Packing.StationID;
                _PC.PackingStatus = Convert.ToInt32(_Packing.PackingStatus);
                _PC.ShipmentLocation = _Packing.ShipmentLocation;
                _PC.MangerOverride = Convert.ToInt32(_Packing.ManagerOverride);
                _PC.PCKROWID = _Packing.PCKROWID;
            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("GetPackingListSelcted.Execute()", Ex.Message.ToString());
            }
            return _PC;
        }

        public List<cstPackageTbl> ExecuteNoLocation(String ShippingNum)
        {
            List<cstPackageTbl> _lsPacking = new List<cstPackageTbl>();
            try
            {
                local_x3v6Entities _Localx3v6 = new local_x3v6Entities();
               // Package _Packing = _Localx3v6.Packages.SingleOrDefault(i => i.ShippingNum == ShippingNum);
                var _packinglist = from _Pack in _Localx3v6.Packages
                                   where _Pack.ShippingNum == ShippingNum
                                   select _Pack;
                foreach (var _Packing in _packinglist)
                {
                    cstPackageTbl _PC = new cstPackageTbl();
                    _PC.PackingId = _Packing.PackingId;
                    _PC.ShippingID = (Guid)_Packing.ShippingID;
                    _PC.ShippingNum = _Packing.ShippingNum;
                    _PC.UserID = _Packing.UserId;
                    _PC.StartTime = Convert.ToDateTime(_Packing.StartTime);
                    _PC.EndTime = Convert.ToDateTime(_Packing.EndTime);
                    _PC.StationID = (Guid)_Packing.StationID;
                    _PC.PackingStatus = Convert.ToInt32(_Packing.PackingStatus);
                    _PC.ShipmentLocation = _Packing.ShipmentLocation;
                    _PC.MangerOverride = Convert.ToInt32(_Packing.ManagerOverride);
                    _PC.PCKROWID = _Packing.PCKROWID;
                    _lsPacking.Add(_PC);   
                }

                
            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("GetPackingListSelcted.ExecuteNoLocation()", Ex.Message.ToString());
            }
            return _lsPacking;
        }

        public string getMaxPackageID()
        {
            string MaxID = "";
            try
            {
                Guid MaxGUiID = entx3v6.Packages.Max(i => i.PackingId);
                MaxID = entx3v6.Packages.SingleOrDefault(i => i.PackingId == MaxGUiID).ShippingNum;

            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("GetMaxShipmentIDCommmand.Execute()", Ex.Message.ToString());
            }
            return MaxID;
        }
        #endregion


        #region set Packing
        /// <summary>
        /// Save data to the paking table.
        /// </summary>
        /// <param name="lsPackingObj">list of values for the packing table.</param>
        /// <returns>New Guid</returns>
        public Guid setPacking(List<cstPackageTbl> lsPackingObj)
        {
            Guid Retuen = Guid.Empty;
            try
            {
                foreach (var Pckitems in lsPackingObj)
                {
                    Package _packing = new Package();
                    _packing.PackingId = Guid.NewGuid();
                    _packing.ShippingID = entx3v6.Shippings.SingleOrDefault(i => i.ShippingNum == Pckitems.ShippingNum).ShippingID;
                    _packing.UserId = Pckitems.UserID;
                    _packing.ShippingNum = Pckitems.ShippingNum;
                    _packing.StartTime = Pckitems.StartTime;
                    _packing.EndTime = Pckitems.EndTime;
                    _packing.StationID = Pckitems.StationID;
                    _packing.PackingStatus = Pckitems.PackingStatus;
                    _packing.ShipmentLocation = Pckitems.ShipmentLocation;
                    _packing.CreatedBy = GlobalClasses.ClGlobal.UserID;
                    _packing.CreatedDateTime = DateTime.UtcNow;
                    _packing.ManagerOverride = Pckitems.MangerOverride;
                    entx3v6.AddToPackages(_packing);
                    Retuen = _packing.PackingId;
                }
                entx3v6.SaveChanges();

            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("UpdatePackingCommand.Execute()", Ex.Message.ToString());
            }
            return Retuen;
        }
        /// <summary>
        /// Save data to the paking table.
        /// </summary>
        /// <param name="lsPackingObj">list of values for the packing table.</param>
        /// <returns>fail if trasaction fail else Success.</returns>
        public string setPacking(List<cstPackageTbl> lsPackingObj, Guid PackingID)
        {
            string Retuen = "Fail";
            try
            {
                foreach (var Pckitems in lsPackingObj)
                {
                    Package _packing = entx3v6.Packages.SingleOrDefault(i => i.PackingId == PackingID);
                    _packing.UserId = Pckitems.UserID;
                    _packing.ShippingID = Pckitems.ShippingID;
                    _packing.ShippingNum = Pckitems.ShippingNum;
                    _packing.StartTime = Pckitems.StartTime;
                    _packing.EndTime = Pckitems.EndTime;
                    _packing.StationID = Pckitems.StationID;
                    _packing.PackingStatus = Pckitems.PackingStatus;
                    _packing.ShipmentLocation = Pckitems.ShipmentLocation;
                    _packing.Updatedby = GlobalClasses.ClGlobal.UserID;
                    _packing.UpdatedDateTime = DateTime.UtcNow;
                    _packing.ManagerOverride = Pckitems.MangerOverride;
                }
                entx3v6.SaveChanges();
                Retuen = "Success";
            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("UpdatePackingCommand.UpdatePacking()", Ex.Message.ToString());
            }
            return Retuen;
        }
        #endregion


        #region Delete Packing
        /// <summary>
        /// Roll Back Transaction Operations that delete the entry from the table Paking.
        /// For shipment ID
        /// </summary>
        /// <param name="ShipmentID">Roll back entry from Shipment</param>
        /// <returns>Boolean if Transacetion Seccess else False</returns>
        public Boolean RollBack(String ShipmentID)
        {
            Boolean _return = false;
            try
            {
                Package _Packing = entx3v6.Packages.SingleOrDefault(i => i.ShippingNum == ShipmentID);
                entx3v6.DeleteObject(_Packing);
                entx3v6.SaveChanges();
                _return = true;
            }
            catch (Exception Ex)
            {
                Error_Loger.elAction.save("RollBackPakingMaster.Execute()", Ex.Message.ToString());
            }
            return _return;
        }
        #endregion
    }
}
