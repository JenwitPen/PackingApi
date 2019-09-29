using System;
using System.Collections.Generic;

namespace PackingApi.Models.DB
{
    public partial class TbtPackItem
    {
        public string ItemCode { get; set; }
        public string DocNum { get; set; }
        public string PackNo { get; set; }
        public bool? FlagPack { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUser { get; set; }
        public string IsbnRecheck { get; set; }
    }
}
