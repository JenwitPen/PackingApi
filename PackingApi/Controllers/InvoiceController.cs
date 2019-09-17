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
        [HttpPost]
        public async Task<ActionResult> selectInvoice([FromBody]selectInvoiceRequest selectInvoiceRequest)
        {
            try
            {

                var data = await (from x in db.TbtInvoice
                                  where x.FlagPick != true &&
                                  (String.IsNullOrEmpty(selectInvoiceRequest.DocNum) || x.DocNum == selectInvoiceRequest.DocNum) &&
                                  (String.IsNullOrEmpty(selectInvoiceRequest.CardName) || x.CardName.Contains(selectInvoiceRequest.CardName)) &&
                                  (String.IsNullOrEmpty(selectInvoiceRequest.County) || x.County == selectInvoiceRequest.County) &&
                                  (String.IsNullOrEmpty(selectInvoiceRequest.Region) || x.Descript == selectInvoiceRequest.Region)
                                  select x).ToListAsync();
                if (selectInvoiceRequest.StartDocDueDate != null)
                {
                    data= data.Where(i => i.DocDueDate >= selectInvoiceRequest.StartDocDueDate).ToList();
                }
                if (selectInvoiceRequest.EndDocDueDate != null)
                {
                    data= data.Where(i => i.DocDueDate <= selectInvoiceRequest.EndDocDueDate).ToList();
                }
                var response = (from x in data
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
                                orderby g.Key.DocDueDate
                                select new selectInvoiceRespone
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
                                }).Skip((selectInvoiceRequest.page - 1) * selectInvoiceRequest.size).Take(selectInvoiceRequest.size).ToList();

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
