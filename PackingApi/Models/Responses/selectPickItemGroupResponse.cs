using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Responses
{
    public class selectPickItemGroupResponse
    {
        public string ItemGrpCode { get; set; }
        public string ItemGrpName { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }
        
    }
}
