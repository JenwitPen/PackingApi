using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class printLabelRequest
    {
        public List<String> DocNums { get; set; }
    }
}
