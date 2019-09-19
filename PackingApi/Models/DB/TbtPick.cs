﻿using System;
using System.Collections.Generic;

namespace PackingApi.Models.DB
{
    public partial class TbtPick
    {
        public string PickNo { get; set; }
        public string Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUser { get; set; }
        public bool? Active { get; set; }
    }
}
