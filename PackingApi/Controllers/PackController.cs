using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackingApi.Helpers;
using PackingApi.Models.DB;
using PackingApi.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    public class PackController : ControllerBase
    {
        private PackingDBContext db;
        public PackController(PackingDBContext packingDBContext)
        {
            this.db = packingDBContext;
        }

        [HttpPost]
        public async Task<ActionResult> selectPickForPack([FromBody]  selectPickForPackRequest selectPickForPackRequest)
        {

            try
            {
                var data = await (from o in db.TbtOrder
                                  join p in db.TbtPickItem on new { o.ItemCode, o.DocNum } equals new { p.ItemCode, p.DocNum }
                                  join pack in db.TbtPackItem on o.ItemCode equals pack.ItemCode into gpack
                                  where p.FlagPick == true && gpack.Count()==0 &&
                                   (String.IsNullOrEmpty(selectPickForPackRequest.PickNo) || p.PickNo == selectPickForPackRequest.PickNo) &&
                                  (String.IsNullOrEmpty(selectPickForPackRequest.DocNum) || o.DocNum == selectPickForPackRequest.DocNum) &&
                                  (String.IsNullOrEmpty(selectPickForPackRequest.CardName) || o.CardName.Contains(selectPickForPackRequest.CardName)) &&
                                  (String.IsNullOrEmpty(selectPickForPackRequest.County) || o.County == selectPickForPackRequest.County) &&
                                  (String.IsNullOrEmpty(selectPickForPackRequest.Region) || o.Descript == selectPickForPackRequest.Region)
                                  select new { o, p }).ToListAsync();
                if (selectPickForPackRequest.StartDocDueDate != null)
                {
                    data = data.Where(i => i.o.DocDueDate >= selectPickForPackRequest.StartDocDueDate).ToList();
                }
                if (selectPickForPackRequest.EndDocDueDate != null)
                {
                    data = data.Where(i => i.o.DocDueDate <= selectPickForPackRequest.EndDocDueDate).ToList();
                }
                var response = (from x in data
                                group x by new
                                {
                                    x.o.DocNum,
                                    x.p.PickNo,
                                    x.o.DocDate,
                                    x.o.DocDueDate,
                                    x.o.CardCode,
                                    x.o.CardName,
                                    x.o.County,
                                    x.o.Descript,
                                    x.o.ShipToCode,
                                    x.o.Transporter,
                                    x.o.Address,
                                    x.o.Remark
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
                                    PickNo = g.Key.PickNo,
                                    Price = g.Sum(y => y.o.Price * y.o.Quantity)
                                }).Skip((selectPickForPackRequest.page - 1) * selectPickForPackRequest.size).Take(selectPickForPackRequest.size).ToList();


                if (response != null)
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
        public async Task<ActionResult> updateOrderPack([FromBody] updateOrderPackRequest updateOrderPackRequest)
        {
            String PackNo = "";
            try
            {
                var order = await (from iv in db.TbtOrder
                                   where updateOrderPackRequest.DocNums.Contains(iv.DocNum)
                                   select iv).ToListAsync();



                var RunNo = (from f in db.TbmRunNo
                             where f.Type == "PackNo"
                             select f).FirstOrDefault();
                int? mRun = RunNo.RunNo;
                PackNo = RunningNo.GetRunNoYYYYMMdd((int)mRun);



                List<TbtPackItem> tbtPackItem = new List<TbtPackItem>();
                foreach (var i in order)
                {
                    tbtPackItem.Add(
                    new TbtPackItem
                    {
                        PackNo = PackNo,
                        CreateUser = updateOrderPackRequest.UserId,
                        CreateDate = DateTime.Now,
                        ItemCode = i.ItemCode,
                        DocNum = i.DocNum
                    });

                }
                db.TbtPackItem.AddRange(tbtPackItem);
                RunNo.RunNo = mRun + 1;
                db.SaveChanges();
                return Ok(new { PackNo = PackNo });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }

        [HttpPost]
        public async Task<ActionResult> selectPackList([FromBody] selectPackListRequest selectPackListRequest)
        {
            try
            {
                var response = await (from p in db.TbtPackItem
                                      join o in db.TbtOrder on new { p.ItemCode, p.DocNum } equals new { o.ItemCode, o.DocNum }
                                      where (string.IsNullOrEmpty(selectPackListRequest.PackNo) || p.PackNo == selectPackListRequest.PackNo)
                                      select new { p.PackNo, o.DocNum, o.Quantity, o.Price, p.FlagPack }).GroupBy(x => new { x.PackNo })
                                      .Select(g => new
                                      {
                                          Status = g.Sum(x => x.FlagPack == true ? 1 : 0) == 0 ? "Open" : g.Sum(x => x.FlagPack == true ? 0 : 1) == 0 ? "Complate" : "In Progress",
                                          PackNo = g.Key.PackNo,
                                          TotalPrice = g.Sum(x => x.Quantity * x.Price),
                                          TotalDocNum = g.Select(x => x.DocNum).Distinct().Count()
                                      }
                                      )
                                      .Skip((selectPackListRequest.page - 1) * selectPackListRequest.size).Take(selectPackListRequest.size).ToListAsync();
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
        public async Task<ActionResult> selectPackListForConfirm([FromBody]  selectPackListForConfirmRequest selectPackListForConfirmRequest)
        {
            try
            {
                var response = await (from p in db.TbtPackItem
                                      join o in db.TbtOrder on new { p.ItemCode, p.DocNum } equals new { o.ItemCode, o.DocNum }
                                      where  p.PackNo == selectPackListForConfirmRequest.PackNo
                                      select new { p.ItemCode,p.DocNum, o.Dscription,o.Quantity, o.Isbn, p.IsbnRecheck,p.Unit,p.Package }
                                      )
                                      .Skip((selectPackListForConfirmRequest.page - 1) * selectPackListForConfirmRequest.size).Take(selectPackListForConfirmRequest.size).ToListAsync();
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
        public async Task<ActionResult> updatePackConfirm([FromBody]List< updatePackConfirmRequest> updatePackConfirmRequests)
        {
            String PackNo = "";
            try
            {
                var packs = await (from P in db.TbtPackItem
                                   join req in updatePackConfirmRequests on new {P.PackNo,P.ItemCode,P.DocNum} equals new { req .PackNo, req .ItemCode, req .DocNum}
                                   select new TbtPackItem {
                                       DocNum= P.DocNum,
                                       ItemCode= P.ItemCode,
                                       PackNo=P.PackNo,
                                       FlagPack=true,
                                       Package=req.Package,
                                       Unit=req.Unit,
                                       IsbnRecheck=req.ISBN_Recheck,
                                       UpdateDate=DateTime.Now,
                                       UpdateUser=req.UserId
                                       
                                   }).ToListAsync();
                db.UpdateRange(packs);
                db.SaveChanges();
                return Ok(new { PackNo = packs.FirstOrDefault().PackNo });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }


    }
}
