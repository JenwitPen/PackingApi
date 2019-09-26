using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class selectPickItemByGroupRequest
    {
        [Required]
        public string PickNo { get; set; }
        [Required]
        public string ItemGrpCode { get; set; }
        [Required]
        public int page { get; set; } = 1;
        [Required]
        public int size { get; set; } = 10;

    }
}
