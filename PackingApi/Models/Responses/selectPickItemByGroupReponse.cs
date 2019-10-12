using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Responses
{
    public class selectPickItemByGroupReponse
    {
        public DateTime? DocDueDate { get; set; }
        public string PickNo { get; set; }
        public string ItemGrpName { get; set; }
        public string ItemGrpCode { get; set; }
        public List<selectPickItem> selectPickItems { get; set; }

    }
    public class selectPickItem
    {
        public string BinCode { get; set; }
        public string ItemCode { get; set; }
        public string Dscription { get; set; }
        public string Isbn { get; set; }
        public int Quantity { get; set; }
        public bool FlagPick { get; set; }
        public double Price { get; set; }

    }
}
