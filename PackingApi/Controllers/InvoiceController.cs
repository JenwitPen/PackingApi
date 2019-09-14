using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackingApi.Models.DB;

namespace PackingApi.Controllers
{

    [Route("api/[controller]/[action]")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private PackingDBContext db;
        public InvoiceController(PackingDBContext packingDBContext)
        {
            this.db = packingDBContext;
        }
        // GET api/values
        [HttpGet]
        public async Task<ActionResult> selectInvoice(int page = 1, int size = 10)
        {
            try
            {
                var response =await (from user in db.TbtInvoice
                                select user).Skip((page - 1) * size).Take(size).ToListAsync();

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
