using PackingApi.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.PDF.Models
{
    public class LabelDocumentModel
    {
        public string CardName { get; set; }
        public string Address { get; set; }

        public string DocNum { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? DocDueDate { get; set; }
        public string Remark { get; set; }

        public List<LabelItem> labelItems { get; set; }
    }

    public class LabelItem
    {
        public string Dscription { get; set; }
        public double? Quantity { get; set; }
    }
}
