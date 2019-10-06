using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Models.Requests
{
    public class updatePostConfirmRequest
    {
      
        public string DocNum { get; set; }
        public bool FlagClear { get; set; }
        public string TrackNumber { get; set; }
        public int User { get; set; }
    }
}
