using System;
using System.Collections.Generic;

namespace PackingApi.Models.DB
{
    public partial class TbmPackage
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public bool? Active { get; set; }
    }
}
