using System;
using System.Collections.Generic;

namespace PackingApi.Models.DB
{
    public partial class TbtInvoice
    {
        public int Id { get; set; }
        public string DocNum { get; set; }
        public string ItemCode { get; set; }
        public double? CardCode { get; set; }
        public string CardName { get; set; }
        public string County { get; set; }
        public string Descript { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? DocDueDate { get; set; }
        public string Dscription { get; set; }
        public double? Quantity { get; set; }
        public double? Price { get; set; }
        public string WhsCode { get; set; }
        public string BinCode { get; set; }
        public string ShipToCode { get; set; }
        public string Address { get; set; }
        public string Transporter { get; set; }
        public string Remark { get; set; }
        public bool? FlagPick { get; set; }
        public string Isbn { get; set; }
    }
}
