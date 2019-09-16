﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class selectInvoiceRequest
    {
    
        public DateTime? DocDueDate { get; set; }
        public string County { get; set; }
        public string DocNum { get; set; }
        public string Region { get; set; }
        public string CardName { get; set; }
        [Required]
        public int page { get; set; } = 1;
        [Required]
        public int size { get; set; } = 10;
    }
}
