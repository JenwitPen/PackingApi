using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class updatePackageStockRequest
    {
        public int Qty { get; set; }
        public String PackageName { get; set; }
        public int PackageID { get; set; }
        public bool Active { get; set; }
        public int UpdateUser { get; set; }
        public bool flagNew { get; set; }
    }
}
