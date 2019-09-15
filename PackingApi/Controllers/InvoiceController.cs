using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackingApi.Models.DB;
using PackingApi.Models.Requests;

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
                var response =await (from x in db.TbtInvoice
                                group x by new
                                {
                                    x.DocNum,
                                    x.DocDate,
                                    x.DocDueDate,
                                    x.CardCode,
                                    x.CardName,
                                    x.County,
                                    x.Descript,
                                    x.ShipToCode,
                                    x.Transporter,
                                    x.Address,
                                    x.Remark
                                } into g
                                select new selectInvoiceRequest
                                {
                                    DocNum = g.Key.DocNum,
                                    DocDate = g.Key.DocDate,
                                    DocDueDate = g.Key.DocDueDate,
                                    CardCode = g.Key.CardCode,
                                    CardName = g.Key.CardName,
                                    County = g.Key.County,
                                    Descript = g.Key.Descript,
                                    ShipToCode = g.Key.ShipToCode,
                                    Transporter = g.Key.Transporter,
                                    Address = g.Key.Address,
                                    Remark = g.Key.Remark,
                                    Price = g.Sum(y => y.Price * y.Quantity)
                                }).Skip((page - 1) * size).Take(size).ToListAsync(); 

                if (response.Count!=0)
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
