using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class updatePickConfirmRequest
    {
        [Required]
        public string PickNo { get; set; }
        [Required]
        public string ItemGrpCode { get; set; }
    }
}
