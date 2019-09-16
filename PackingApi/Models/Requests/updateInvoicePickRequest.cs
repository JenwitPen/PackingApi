using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class updateInvoicePickRequest
    {
        [Required]
        public List<String> DocNums { get; set; }
        [Required]
        public int UserId { get; set; }
    
    }
}
