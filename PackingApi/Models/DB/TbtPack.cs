using System;
using System.Collections.Generic;

namespace PackingApi.Models.DB
{
    public partial class TbtPack
    {
        public string PackNo { get; set; }
        public string Package { get; set; }
        public int? Unit { get; set; }
        public int? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUser { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
