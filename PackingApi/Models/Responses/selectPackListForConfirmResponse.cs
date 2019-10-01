using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Responses
{
    public class selectPackListForConfirmResponse
    {

        public string ItemCode { get; set; }
        public string Dscription { get; set; }
        public int Quantity { get; set; }
        public string Isbn { get; set; }
        public string IsbnRecheck { get; set; }
        public int Unit { get; set; }
        public string Package { get; set; }
    }
}
