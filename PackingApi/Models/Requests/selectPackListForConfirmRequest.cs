﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class selectPackListForConfirmRequest
    {
        public string PackNo { get; set; }
        [Required]
        public int page { get; set; } = 1;
        [Required]
        public int size { get; set; } = 10;
    }
}