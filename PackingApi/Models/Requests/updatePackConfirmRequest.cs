using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class updatePackConfirmRequest
    {
        public string ItemCode { get; set; }
        public string DocNum { get; set; }
        public string PackNo { get; set; }
        public string Package { get; set; }
        public int Unit { get; set; }
        public int UserId { get; set; }
        public string ISBN_Recheck { get; set; }

    }
}
