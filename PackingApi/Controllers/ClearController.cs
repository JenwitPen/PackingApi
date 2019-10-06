using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackingApi.Helpers;
using PackingApi.Models.DB;
using PackingApi.Models.Requests;
using PackingApi.Models.Responses;

namespace PackingApi.Controllers
{

    [Route("api/[controller]/[action]")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    public class ClearController : ControllerBase
    {
        private PackingDBContext db;
        public ClearController(PackingDBContext packingDBContext)
        {
            this.db = packingDBContext;
        }

        [HttpPost]
        public async Task<ActionResult> selectPostConfirmList([FromBody] selectPostConfirmListRequest selectPostConfirmListRequest)
        {
            try
            {
                var data = await (from o in db.TbtOrder
                                  join p in db.TbtPackItem on new { o.ItemCode, o.DocNum } equals new { p.ItemCode, p.DocNum }
                                  join c in db.TbtClearItem on new { o.ItemCode, o.DocNum } equals new { c.ItemCode, c.DocNum } into cgroup
                                  from cl in cgroup.DefaultIfEmpty()
                                  where p.FlagPack == true &&
                                   (String.IsNullOrEmpty(selectPostConfirmListRequest.PackNo) || p.PackNo == selectPostConfirmListRequest.PackNo) &&
                                  (String.IsNullOrEmpty(selectPostConfirmListRequest.DocNum) || o.DocNum == selectPostConfirmListRequest.DocNum) &&
                                  (String.IsNullOrEmpty(selectPostConfirmListRequest.CardName) || o.CardName.Contains(selectPostConfirmListRequest.CardName)) &&
                                  (String.IsNullOrEmpty(selectPostConfirmListRequest.County) || o.County == selectPostConfirmListRequest.County) &&
                                  (String.IsNullOrEmpty(selectPostConfirmListRequest.Region) || o.Descript == selectPostConfirmListRequest.Region)
                                  select new { o, p, cl }).ToListAsync();
                if (selectPostConfirmListRequest.StartDocDueDate != null)
                {
                    data = data.Where(i => i.o.DocDueDate >= selectPostConfirmListRequest.StartDocDueDate).ToList();
                }
                if (selectPostConfirmListRequest.EndDocDueDate != null)
                {
                    data = data.Where(i => i.o.DocDueDate <= selectPostConfirmListRequest.EndDocDueDate).ToList();
                }
                var response = (from x in data
                                group x by new
                                {
                                    x.o.DocNum,
                                    x.p.PackNo,
                                    x.o.DocDate,
                                    x.o.DocDueDate,
                                    x.o.CardCode,
                                    x.o.CardName,
                                    x.o.County,
                                    x.o.Descript,
                                    x.o.ShipToCode,
                                    x.o.Transporter,
                                    x.o.Address,
                                    x.o.Remark,
                                    x.cl.TrackNumber
                                } into g
                                orderby g.Key.DocDueDate
                                select new selectPostConfirmListResponse
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
                                    PackNo = g.Key.PackNo,
                                    TackNumber = g.Key.TrackNumber,
                                    Price = g.Sum(y => y.o.Price * y.o.Quantity)
                                }).Skip((selectPostConfirmListRequest.page - 1) * selectPostConfirmListRequest.size).Take(selectPostConfirmListRequest.size).ToList();


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
        public async Task<ActionResult> updatePostConfirm([FromBody] updatePostConfirmRequest updatePostConfirmRequest)
        {
            try
            {
                var clearitem = await (from p in db.TbtClearItem
                                       where p.DocNum == updatePostConfirmRequest.DocNum
                                       select p).ToListAsync();
                if (clearitem.Count > 0)
                {
                    clearitem.ForEach(x =>
                    {
                        x.UpdateDate = DateTime.Now;
                        x.UpdateUser = updatePostConfirmRequest.User;
                        x.FlagClear = updatePostConfirmRequest.FlagClear;
                        x.TrackNumber = updatePostConfirmRequest.TrackNumber;
                    }
                    );
                    db.Update(clearitem);
                }
                else
                {
                    var order = await (from o in db.TbtOrder
                                       where o.DocNum == updatePostConfirmRequest.DocNum
                                       select new TbtClearItem
                                       {
                                           CreateDate = DateTime.Now,
                                           CreateUser = updatePostConfirmRequest.User,
                                           DocNum = updatePostConfirmRequest.DocNum,
                                           ItemCode = o.ItemCode,
                                           FlagClear = updatePostConfirmRequest.FlagClear,
                                           TrackNumber = updatePostConfirmRequest.TrackNumber

                                       }).ToListAsync();
                    db.AddRange(order);

                }
                db.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }
    }
}
