using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackingApi.Models.DB;
using PackingApi.Models.Requests;
using PackingApi.Models.Responses;

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


                db.TbtOrder.AddRange(data.Select(x => new TbtOrder
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
                  Isbn=x.Isbn
                }));
                List<TbtPickItem> tbtPickItems = new List<TbtPickItem>();
                foreach (var i in data)
                {
                    tbtPickItems.Add(
                    new TbtPickItem
                    {
                        PickNo = PackNo,
                        CreateUser = updateInvoicePickRequest.UserId,
                        CreateDate = DateTime.Now,
                        ItemCode=i.ItemCode
                    });

                }
                db.TbtPickItem.AddRange(tbtPickItems);
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
                var response = await (from p in db.TbtPickItem
                                      join o in db.TbtOrder on p.ItemCode equals o.ItemCode
                                      where (string.IsNullOrEmpty(selectInvoicePickRequest.PickNo) || p.PickNo == selectInvoicePickRequest.PickNo)
                                      select new { p.PickNo,o.DocNum, o.Quantity, o.Price,p.FlagPick }).GroupBy(x => new {  x.PickNo })
                                      .Select(g => new {
                                          Status = g.Sum(x=>x.FlagPick==true?1:0)==0?"Open": g.Sum(x => x.FlagPick == true ? 0 : 1) == 0? "Complate" : "In Progress",
                                          PickNo = g.Key.PickNo,
                                          TotalPrice = g.Sum(x => x.Quantity * x.Price),
                                          TotalDocNum=g.Select(x=>x.DocNum).Distinct().Count()
                                      }
                                      )
                                      .Skip((selectInvoicePickRequest.page - 1) * selectInvoicePickRequest.size).Take(selectInvoicePickRequest.size).ToListAsync();
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

        [HttpGet("{PickNo}")]
        public async Task<ActionResult> selectPickItemGroup(String PickNo)
        {

            try
            {
                var response = await (from Pi in db.TbtPickItem
                                      join iG in db.TbmItemGroup on Pi.ItemCode.Trim().Substring(0, 1) equals iG.ItemGrpPrefix.Trim()
                                      where Pi.PickNo == PickNo
                                      select new { iG.ItemGrpCode, iG.ItemGrpName, Pi.FlagPick }).GroupBy(x => new {x.ItemGrpCode,x.ItemGrpName}).
                                      Select(y=> new selectPickItemGroupResponse {
                                          ItemGrpCode = y.Key.ItemGrpCode,
                                       ItemGrpName=   y.Key.ItemGrpName,
                                       Status= y.Sum(x => x.FlagPick == true ? 1 : 0) == 0 ? "Open" : y.Sum(x => x.FlagPick == true ? 0 : 1) == 0 ? "Confirm" : "In Progress",
                                      }).ToListAsync();
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

        [HttpPost]
        public async Task<ActionResult> selectPickItemByGroup([FromBody]  selectPickItemByGroupRequest selectPickItemByGroupRequest)
        {

            try
            {
                var iGroup = await db.TbmItemGroup.Where(w => w.ItemGrpCode == selectPickItemByGroupRequest.ItemGrpCode).FirstOrDefaultAsync();
                var data = await (from PItem in db.TbtPickItem
                                  join O in db.TbtOrder on PItem.ItemCode equals O.ItemCode into gj
                                  from x in gj.DefaultIfEmpty()
                                  where PItem.PickNo == selectPickItemByGroupRequest.PickNo && PItem.ItemCode.Trim().Substring(0, 1) == iGroup.ItemGrpPrefix.Trim()
                                  select new { x.DocDueDate, x.BinCode, x.Dscription, PItem.ItemCode, x.Quantity, x.Isbn, PItem.FlagPick }).Skip((selectPickItemByGroupRequest.page - 1) * selectPickItemByGroupRequest.size).Take(selectPickItemByGroupRequest.size).ToListAsync();
                selectPickItemByGroupReponse selectPickItemByGroup = new selectPickItemByGroupReponse();
                if (data != null)
                {

                    selectPickItemByGroup.ItemGrpName = iGroup.ItemGrpName;
                    selectPickItemByGroup.PickNo = selectPickItemByGroupRequest.PickNo;
                    selectPickItemByGroup.DocDueDate = data.FirstOrDefault().DocDueDate;

                    selectPickItemByGroup.selectPickItems = data.Select(i => new selectPickItem
                    {
                        BinCode = i.BinCode,
                        Dscription = i.Dscription,
                        Isbn = i.Isbn,
                        ItemCode = i.ItemCode,
                        Quantity = (int)i.Quantity,
                        FlagPick = i.FlagPick != null ? (bool)i.FlagPick : false
                    }).OrderBy(x => x.BinCode).ToList();
                }


                if (selectPickItemByGroup != null)
                {
                    return Ok(selectPickItemByGroup);
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
        //[HttpPost]
        //public async Task<ActionResult> selectPickItem([FromBody] selectPickItemRequest selectPickItemRequest)
        //{

        //    try
        //    {
        //        var itemg = (from i in db.TbmItemGroup
        //                     where i.ItemGrpCode == selectPickItemRequest.ItemGrpCode
        //                     select i).FirstOrDefault();

        //        var response = await (from Piv in db.TbtPickInvoice
        //                              join PItem in db.TbtPickItem on Piv.ItemCode equals PItem.ItemCode into ps
        //                              from p in ps.DefaultIfEmpty()
        //                              where Piv.PickNo == selectPickItemRequest.PickNo && Piv.ItemCode.Trim().Substring(0, 1) == itemg.ItemGrpPrefix
        //                              select new { Location = Piv.BinCode, Piv.ItemCode, ItemName = Piv.Dscription, Piv.Quantity, p.Isbn }
        //                              ).ToListAsync();
        //        if (response.Count != 0)
        //        {
        //            return Ok(response);
        //        }
        //        else
        //        {
        //            return NotFound();
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        return StatusCode(500, ex);
        //    }
        //}
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
