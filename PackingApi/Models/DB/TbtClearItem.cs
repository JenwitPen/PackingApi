using System;
using System.Collections.Generic;

namespace PackingApi.Models.DB
{
    public partial class TbtClearItem
    {
        public string ItemCode { get; set; }
        public string DocNum { get; set; }
        public string TrackNumber { get; set; }
        public bool? FlagClear { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUser { get; set; }
    }
}
