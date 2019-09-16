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
    public class PickController : ControllerBase
    {
        private PackingDBContext db;
        public PickController(PackingDBContext packingDBContext)
        {
            this.db = packingDBContext;
        }
        // GET api/values
        [HttpPost]
        public async Task<ActionResult> updateInvoicePick([FromBody] updateInvoicePickRequest updateInvoicePickRequest)
        {
            String PackNo = "";
            try
            {
                var invoice = await (from iv in db.TbtInvoice
                                     where updateInvoicePickRequest.DocNums.Contains(iv.DocNum)
                                     select iv).ToListAsync();
                invoice.ForEach(i => i.FlagPick = true);
                var data = invoice;

                var RunNo = (from f in db.TbmRunNo
                             where f.Type == "PackNo"
                             select f).FirstOrDefault();
                int? mRun = RunNo.RunNo;
                PackNo = getRunNo((int)mRun);


                db.TbtPickInvoice.AddRange(data.Select( x => new TbtPickInvoice
                {
                    Address = x.Address,
                    BinCode = x.BinCode,
                    CardCode = x.CardCode,
                    CardName = x.CardName,
                    County = x.County,
                    CreateUser = updateInvoicePickRequest.UserId,
                    CreateDate = DateTime.Now,
                    Descript = x.Descript,
                    DocDate = x.DocDate,
                    DocDueDate = x.DocDueDate,
                    DocNum = x.DocNum,
                    Dscription = x.Dscription,
                    ItemCode = x.ItemCode,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    Remark = x.Remark,
                    ShipToCode = x.ShipToCode,
                    Transporter = x.Transporter,
                    WhsCode = x.WhsCode,
                    PickNo = PackNo
                }));

                db.TbtPick.Add(new TbtPick
                {
                    PickNo = PackNo,
                    CrateUser = updateInvoicePickRequest.UserId,
                    Active = true,
                    CreateDate = DateTime.Now,
                    Status = "Open"
                });
                RunNo.RunNo = mRun + 1;
                db.SaveChanges();
                return Ok(new updateInvoicePackRespone { PackNo = PackNo });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }
        [HttpPost]
        public async Task<ActionResult> selectInvoicePick([FromBody] selectInvoicePickRequest selectInvoicePickRequest)
        {

            try
            {
                var response = await (from ivP in db.TbtPick
                                      where ivP.Active == true && ivP.CrateUser == selectInvoicePickRequest.UserID &&
                                      (string.IsNullOrEmpty(selectInvoicePickRequest.PickNo) || ivP.PickNo == selectInvoicePickRequest.PickNo)
                                      select ivP).Skip((selectInvoicePickRequest.page - 1) * selectInvoicePickRequest.size).Take(selectInvoicePickRequest.size).ToListAsync();
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
        private String getRunNo(int runNo)
        {
            String strZero = "";
            if (runNo < 999)
            {
                strZero = "0";
            }
            if (runNo < 99)
            {
                strZero += "0";
            }
            if (runNo < 9)
            {
                strZero += "0";
            }
            string strNum = DateTime.Now.Year.ToString() + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString()) + strZero + runNo;
            return strNum;
        }
    }
}
