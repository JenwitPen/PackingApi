﻿using System;
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


                db.TbtPickInvoice.AddRange(data.Select(x => new TbtPickInvoice
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
                    CreateUser = updateInvoicePickRequest.UserId,
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
                var response = await (from p in db.TbtPick
                                      join inP in db.TbtPickInvoice on p.PickNo equals inP.PickNo
                                      where p.Active == true && p.CreateUser == selectInvoicePickRequest.UserID &&
                                      (string.IsNullOrEmpty(selectInvoicePickRequest.PickNo) || p.PickNo == selectInvoicePickRequest.PickNo)
                                      select new { p.PickNo, p.Status, inP.Quantity, inP.Price }).GroupBy(x => new { x.PickNo, x.Status }).Select(g => new { Status = g.Key.Status, PickNo = g.Key.PickNo, TotalPrice = g.Sum(x => x.Quantity * x.Price) }).Skip((selectInvoicePickRequest.page - 1) * selectInvoicePickRequest.size).Take(selectInvoicePickRequest.size).ToListAsync();
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
                var response = await (from Piv in db.TbtPickInvoice
                                      join iG in db.TbmItemGroup on Piv.ItemCode.Trim().Substring(0, 1) equals iG.ItemGrpPrefix.Trim()
                                      where Piv.PickNo == PickNo
                                      select new { Piv, iG }).GroupBy(p => new { p.iG.ItemGrpCode, p.iG.ItemGrpName })
                                      .Select(g => new selectPickItemGroupResponse
                                      {
                                          ItemGrpCode = g.Key.ItemGrpCode,
                                          ItemGrpName = g.Key.ItemGrpName,
                                          Qty = (int)g.Sum(s => s.Piv.Quantity),
                                          Price = (double)g.Sum(s => s.Piv.Price * s.Piv.Quantity)
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
                var data = await (from Piv in db.TbtPickInvoice
                                  where Piv.PickNo == selectPickItemByGroupRequest.PickNo && Piv.ItemCode.Trim().Substring(0, 1) == iGroup.ItemGrpPrefix.Trim()
                                  select Piv).ToListAsync();
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
                        Isbn = "",
                        ItemCode = i.ItemCode,
                        Quantity = (int)i.Quantity
                    }).ToList();
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
        [HttpPost]
        public async Task<ActionResult> selectPickItem([FromBody] selectPickItemRequest selectPickItemRequest)
        {

            try
            {
                var itemg = (from i in db.TbmItemGroup
                             where i.ItemGrpCode == selectPickItemRequest.ItemGrpCode
                             select i).FirstOrDefault();

                var response = await (from Piv in db.TbtPickInvoice
                                      join PItem in db.TbtPickItem on Piv.ItemCode equals PItem.ItemCode into ps
                                      from p in ps.DefaultIfEmpty()
                                      where Piv.PickNo == selectPickItemRequest.PickNo && Piv.ItemCode.Trim().Substring(0, 1) == itemg.ItemGrpPrefix
                                      select new { Location = Piv.BinCode, Piv.ItemCode, ItemName = Piv.Dscription, Piv.Quantity, p.Isbn }
                                      ).ToListAsync();
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
