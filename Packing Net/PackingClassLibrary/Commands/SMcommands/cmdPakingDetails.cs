using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PackingClassLibrary;
using PackingClassLibrary.CustomEntity;
namespace PackingClassLibrary.Commands
{
    /// <summary>
    /// Avinash.
    /// Packing Detail class operations.
    /// </summary>
   public  class cmdPakingDetails
    {
       
      // x3v6 local entity database Object created.
       local_x3v6Entities entx3v6 = new local_x3v6Entities();

       #region Set Package Details Functions.
       /// <summary>
       /// Save the list values to the packing Detail table.
       /// </summary>
       /// <param name="lsPackingOb">list of values of packing Detail </param>
       /// <returns>Success if transaction Success else Fail.</returns>
             public string savePackageDetails(List< cstPackageDetails> lsPackingOb)
             {
                 string Retuen = "Fail";
                 try
                 {
                     foreach (var _PakingDetails in lsPackingOb)
                     {
                         PackageDetail _Packing = new PackageDetail();
                         _Packing.PackingDetailID = _PakingDetails.PackingDetailID;
                         _Packing.PackingId = _PakingDetails.PackingId;
                         _Packing.SKUNumber = _PakingDetails.SKUNumber;
                         _Packing.SKUQuantity = _PakingDetails.SKUQuantity;
                         _Packing.SKUScanDateTime = Convert.ToDateTime(_PakingDetails.PackingDetailStartDateTime);
                         _Packing.BoxNumber = _PakingDetails.BoxNumber;
                         _Packing.ShipmentLocation = _PakingDetails.ShipmentLocation;

                         //view added extra
                         _Packing.ItemName = _PakingDetails.ItemName;
                         _Packing.ProductName = _PakingDetails.ProductName;
                         _Packing.UnitOfMeasure = _PakingDetails.UnitOfMeasure;
                         _Packing.CountryOfOrigin = _PakingDetails.CountryOfOrigin;
                         _Packing.MAP_Price = _PakingDetails.MAP_Price;
                         _Packing.TCLCOD_0 = _PakingDetails.TCLCOD_0;
                         _Packing.TarrifCode = _PakingDetails.TarrifCode;
                         //created Time set
                         _Packing.CreatedDateTime = DateTime.UtcNow;
                         _Packing.CreatedBy = GlobalClasses.ClGlobal.UserID;
                         entx3v6.AddToPackageDetails(_Packing);
                     }
                     entx3v6.SaveChanges();
                     Retuen = "Success";
                 }
                 catch (Exception Ex)
                 {
                     Error_Loger.elAction.save("SetPakingDetailsCommand.Execute()", Ex.Message.ToString());
                 }
                 return Retuen;
             }

             public string UpdatePackageDetails(List<cstPackageDetails> lsPackingOb)
             {
                 string Retuen = "Fail";
                 try
                 {
                     foreach (var _PakingDetails in lsPackingOb)
                     {
                         PackageDetail _Packing = entx3v6.PackageDetails.SingleOrDefault(i => i.PackingDetailID == _PakingDetails.PackingDetailID);
                         _Packing.PackingId = _PakingDetails.PackingId;
                         _Packing.SKUNumber = _PakingDetails.SKUNumber;
                         _Packing.SKUQuantity = _PakingDetails.SKUQuantity;
                         _Packing.SKUScanDateTime = Convert.ToDateTime(_PakingDetails.PackingDetailStartDateTime);
                         _Packing.BoxNumber = _PakingDetails.BoxNumber;
                         _Packing.ShipmentLocation = _PakingDetails.ShipmentLocation;

                         //view added extra
                         _Packing.ItemName = _PakingDetails.ItemName;
                         _Packing.ProductName = _PakingDetails.ProductName;
                         _Packing.UnitOfMeasure = _PakingDetails.UnitOfMeasure;
                         _Packing.CountryOfOrigin = _PakingDetails.CountryOfOrigin;
                         _Packing.MAP_Price = _PakingDetails.MAP_Price;
                         _Packing.TCLCOD_0 = _PakingDetails.TCLCOD_0;
                         _Packing.TarrifCode = _PakingDetails.TarrifCode;
                         //created Time set
                         _Packing.CreatedDateTime = DateTime.UtcNow;
                         _Packing.CreatedBy = GlobalClasses.ClGlobal.UserID;
                     }
                     entx3v6.SaveChanges();
                     Retuen = "Success";
                 }
                 catch (Exception Ex)
                 {
                     Error_Loger.elAction.save("SetPakingDetailsCommand.Execute()", Ex.Message.ToString());
                 }
                 return Retuen;
             }
       #endregion

        #region get Package Details functions.
             /// <summary>
             /// packing detail table information with all rows.
             /// </summary>
             /// <returns>List<cstPackageDetails> information</returns>
             public List<cstPackageDetails> GetPackingDetails()
             {
                 //Return list.
                 List<cstPackageDetails> _lsReturn = new List<cstPackageDetails>();

                 try
                 {
                     var packingDeatils = from Pack in entx3v6.PackageDetails select Pack;
                     //fill list object cstPackageDetails and add to return list.
                     //Foreach loop for all recoreds in the packageDetail table
                     foreach (var lsitem in packingDeatils)
                     {
                         cstPackageDetails _pd = new cstPackageDetails();
                         _pd.PackingDetailID = lsitem.PackingDetailID;
                         _pd.PackingId = lsitem.PackingId;
                         _pd.SKUNumber = lsitem.SKUNumber;
                         _pd.SKUQuantity = Convert.ToInt32(lsitem.SKUQuantity);
                         _pd.PackingDetailStartDateTime = Convert.ToDateTime(lsitem.SKUScanDateTime);
                         _pd.BoxNumber = lsitem.BoxNumber;
                         _pd.ShipmentLocation = lsitem.ShipmentLocation;
                         _pd.ItemName = lsitem.ItemName;
                         _pd.ProductName = lsitem.ProductName;
                         _pd.UnitOfMeasure = lsitem.UnitOfMeasure;
                         _pd.CountryOfOrigin = lsitem.CountryOfOrigin;
                         _pd.MAP_Price =Convert.ToDecimal( lsitem.MAP_Price);
                         _pd.TCLCOD_0 = lsitem.TCLCOD_0;
                         _pd.TarrifCode = lsitem.TarrifCode;
                         _lsReturn.Add(_pd);
                     }
                 }
                 catch (Exception)
                 { }
                 return _lsReturn;
             }
             /// <summary>
             /// Packing detail information list with Packing ID filter.
             /// </summary>
             /// <param name="PackingID">Guid packingID of the shipment from packing table</param>
             /// <returns>List<cstPackageDetails> Information list</returns>
             public List<cstPackageDetails> GetPackingDetails(Guid PackingID)
             {
                 //return list.
                 List<cstPackageDetails> _lsReturn = new List<cstPackageDetails>();
                 try
                 {
                     //Filtring condition.
                     var packingDeatils = from Pack in entx3v6.PackageDetails
                                          where Pack.PackingId == PackingID
                                          select Pack;

                     foreach (var lsitem in packingDeatils)
                     {
                         cstPackageDetails _pd = new cstPackageDetails();
                         _pd.PackingDetailID = lsitem.PackingDetailID;
                         _pd.PackingId = lsitem.PackingId;
                         _pd.SKUNumber = lsitem.SKUNumber;
                         _pd.SKUQuantity = Convert.ToInt32(lsitem.SKUQuantity);
                         _pd.PackingDetailStartDateTime = Convert.ToDateTime(lsitem.SKUScanDateTime);
                         _pd.BoxNumber = lsitem.BoxNumber;
                         _pd.ShipmentLocation = lsitem.ShipmentLocation;
                         _pd.ItemName = lsitem.ItemName;
                         _pd.ProductName = lsitem.ProductName;
                         _pd.UnitOfMeasure = lsitem.UnitOfMeasure;
                         _pd.CountryOfOrigin = lsitem.CountryOfOrigin;
                         _pd.MAP_Price = Convert.ToDecimal(lsitem.MAP_Price);
                         _pd.TCLCOD_0 = lsitem.TCLCOD_0;
                         _pd.TarrifCode = lsitem.TarrifCode;
                         _lsReturn.Add(_pd);
                     }
                 }
                 catch (Exception)
                 { }
                 return _lsReturn;
             }

       /// <summary>
       /// Filter Packing Detail Table by Box Number
       /// </summary>
       /// <param name="BoxNum">String Box Number</param>
       /// <returns>List of Packing Detail table Information.</returns>
             public List<cstPackageDetails> GetPackingDetailsByBoxNum(String BoxNum)
             {
                 //return list.
                 List<cstPackageDetails> _lsReturn = new List<cstPackageDetails>();
                 try
                 {
                     //Filtring condition.
                     var packingDeatils = from Pack in entx3v6.PackageDetails
                                          where Pack.BoxNumber == BoxNum
                                          select Pack;

                     foreach (var lsitem in packingDeatils)
                     {
                         cstPackageDetails _pd = new cstPackageDetails();
                         _pd.PackingDetailID = lsitem.PackingDetailID;
                         _pd.PackingId = lsitem.PackingId;
                         _pd.SKUNumber = lsitem.SKUNumber;
                         _pd.SKUQuantity = Convert.ToInt32(lsitem.SKUQuantity);
                         _pd.PackingDetailStartDateTime = Convert.ToDateTime(lsitem.SKUScanDateTime);
                         _pd.BoxNumber = lsitem.BoxNumber;
                         _pd.ShipmentLocation = lsitem.ShipmentLocation;
                         _pd.ItemName = lsitem.ItemName;
                         _pd.ProductName = lsitem.ProductName;
                         _pd.UnitOfMeasure = lsitem.UnitOfMeasure;
                         _pd.CountryOfOrigin = lsitem.CountryOfOrigin;
                         _pd.MAP_Price = Convert.ToDecimal(lsitem.MAP_Price);
                         _pd.TCLCOD_0 = lsitem.TCLCOD_0;
                         _pd.TarrifCode = lsitem.TarrifCode;
                         _lsReturn.Add(_pd);
                     }
                 }
                 catch (Exception)
                 { }
                 return _lsReturn;
             }
        #endregion
    }
}
