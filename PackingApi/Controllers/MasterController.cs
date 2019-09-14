using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackingApi.Models.DB;

namespace PackingApi.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private PackingDBContext db;
        public MasterController(PackingDBContext packingDBContext)
        {
            this.db = packingDBContext;
        }
        // GET api/values
        [HttpGet]
        //[ResponseCache(Duration = 3600*4)]
        public async Task<ActionResult> selectRegion()
        {
            try
            {
                var data = await (from invoice in db.TbtInvoice
                                  select new { invoice.Descript }).Distinct().ToListAsync();
                List<String> response = new List<string>();
                data.ForEach(i => response.Add(i.Descript));

                if (response.Count != 0)
                {
                    return Ok(response);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }
       
    }
}
