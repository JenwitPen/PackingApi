using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class selectPickForPackResponse
    {
        public string DocNum { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? DocDueDate { get; set; }
        public double? CardCode { get; set; }
        public string CardName { get; set; }
        public string County { get; set; }
        public string Descript { get; set; }
        public double? Price { get; set; }
        public string ShipToCode { get; set; }
        public string Transporter { get; set; }
        public string Address { get; set; }
        public string Remark { get; set; }

    }
}
