using System;
using System.Collections.Generic;

namespace PackingApi.Models.DB
{
    public partial class TbmPackage
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public bool? Active { get; set; }
        public int? Qty { get; set; }
        public int? CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
