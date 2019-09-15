using System;
using System.Collections.Generic;

namespace PackingApi.Models.DB
{
    public partial class TbmRunNo
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int? RunNo { get; set; }
    }
}
