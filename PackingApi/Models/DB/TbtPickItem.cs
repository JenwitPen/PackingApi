using System;
using System.Collections.Generic;

namespace PackingApi.Models.DB
{
    public partial class TbtPickItem
    {
        public string ItemCode { get; set; }
        public string Isbn { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUser { get; set; }
    }
}
