﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackingClassLibrary.CustomEntity.ReportEntitys
{
   public class cstUserCurrentStationAndDeviceID
    {
       public Guid UserID { get; set; }
        public String UserName{ get; set; }
        public String StationName { get; set; }
        public String DeviceID { get; set; }
        public String Datetime { get; set; }
    }
}
