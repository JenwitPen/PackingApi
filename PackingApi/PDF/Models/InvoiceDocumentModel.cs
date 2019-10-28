using PackingApi.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.PDF.Models
{
    public class InvoiceDocumentModel
    {
        public string CardName { get; set; }
        public string Address { get; set; }
        public string CardCode { get; set; }
        public string DocNum { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? DocDueDate { get; set; }
        public string Remark { get; set; }
        public string County { get; set; }
        public List<InvoiceItem> invoiceitems { get; set; }
    }

    public class InvoiceItem
    {
        public string ItemGrpCode { get; set; }
        public string ItemGrpName { get; set; }
        public string ItemCode { get; set; }
        public string Dscription { get; set; }
        public double Price { get; set; }
        public double Qty { get; set; }
    }
}
