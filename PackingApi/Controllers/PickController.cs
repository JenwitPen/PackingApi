using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackingApi.Helpers;
using PackingApi.Models.DB;
using PackingApi.Models.PDF.Process;
using PackingApi.Models.Requests;
using PackingApi.Models.Responses;
using PackingApi.PDF.Models;

namespace PackingApi.Controllers
{

    [Route("api/[controller]/[action]")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    public class PickController : ControllerBase
    {
        private PackingDBContext db;
        private IHostingEnvironment _hostingEnvironment;
        public PickController(PackingDBContext packingDBContext,  IHostingEnvironment hostingEnvironment)
        {
            this.db = packingDBContext;
            this._hostingEnvironment=hostingEnvironment;
    }
        // GET api/values
        [HttpPost]
        public async Task<ActionResult> updateInvoicePick([FromBody] updateInvoicePickRequest updateInvoicePickRequest)
        {
            String PickNo = "";
            try
            {
                var invoice = await (from iv in db.TbtInvoice
                                     where updateInvoicePickRequest.DocNums.Contains(iv.DocNum)
                                     select iv).ToListAsync();
                invoice.ForEach(i => i.FlagPick = true);
                var data = invoice;

                var RunNo = (from f in db.TbmRunNo
                             where f.Type == "PickNo"
                             select f).FirstOrDefault();
                int? mRun = RunNo.RunNo;
                PickNo = RunningNo.GetRunNoYYYYMMdd((int)mRun);


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
                    Isbn = x.Isbn
                }));
                List<TbtPickItem> tbtPickItems = new List<TbtPickItem>();
                foreach (var i in data)
                {
                    tbtPickItems.Add(
                    new TbtPickItem
                    {
                        PickNo = PickNo,
                        CreateUser = updateInvoicePickRequest.UserId,
                        CreateDate = DateTime.Now,
                        ItemCode = i.ItemCode,
                        DocNum = i.DocNum,

                    });

                }
                db.TbtPickItem.AddRange(tbtPickItems);
                RunNo.RunNo = mRun + 1;
                db.SaveChanges();
                return Ok(new updateInvoicePackRespone { PickNo = PickNo });

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
                                      join o in db.TbtOrder on new { p.ItemCode, p.DocNum } equals new { o.ItemCode,o.DocNum }
                                      where (string.IsNullOrEmpty(selectInvoicePickRequest.PickNo) || p.PickNo == selectInvoicePickRequest.PickNo)
                                      select new { p.PickNo, o.DocNum, o.Quantity, o.Price, p.FlagPick }).GroupBy(x => new { x.PickNo })
                                      .Select(g => new
                                      {
                                          Status = g.Sum(x => x.FlagPick == true ? 1 : 0) == 0 ? "Open" : g.Sum(x => x.FlagPick == true ? 0 : 1) == 0 ? "Complate" : "In Progress",
                                          PickNo = g.Key.PickNo,
                                          TotalPrice = g.Sum(x => x.Quantity * x.Price),
                                          TotalDocNum = g.Select(x => x.DocNum).Distinct().Count()
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
                                      select new { iG.ItemGrpCode, iG.ItemGrpName, Pi.FlagPick }).GroupBy(x => new { x.ItemGrpCode, x.ItemGrpName }).
                                      Select(y => new selectPickItemGroupResponse
                                      {
                                          ItemGrpCode = y.Key.ItemGrpCode,
                                          ItemGrpName = y.Key.ItemGrpName,
                                          Status = y.Sum(x => x.FlagPick == true ? 1 : 0) == 0 ? "Open" : y.Sum(x => x.FlagPick == true ? 0 : 1) == 0 ? "Confirm" : "In Progress",
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
                                  join O in db.TbtOrder on new { PItem.ItemCode, PItem.DocNum } equals new{ O.ItemCode,O.DocNum} into gj
                                  from x in gj.DefaultIfEmpty()
                                  where PItem.PickNo == selectPickItemByGroupRequest.PickNo && PItem.ItemCode.Trim().Substring(0, 1) == iGroup.ItemGrpPrefix.Trim()
                                  select new { x.DocDueDate, x.BinCode, x.Dscription, PItem.ItemCode, x.Quantity, x.Isbn, PItem.FlagPick }).Skip((selectPickItemByGroupRequest.page - 1) * selectPickItemByGroupRequest.size).Take(selectPickItemByGroupRequest.size).ToListAsync();
                PickDocumentModel selectPickItemByGroup = new PickDocumentModel();
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
                        FlagPick=i.FlagPick==null?false: (bool)i.FlagPick

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
        [HttpPost]
        public async Task<ActionResult> updatePickConfirm([FromBody]  updatePickConfirmRequest updatePickConfirmRequest)
        {

            try
            {
                var iGroup = await db.TbmItemGroup.Where(w => w.ItemGrpCode == updatePickConfirmRequest.ItemGrpCode).FirstOrDefaultAsync();
                var tbtPickItem = db.TbtPickItem.Where(x => x.PickNo == updatePickConfirmRequest.PickNo && x.ItemCode.Trim().Substring(0, 1) == iGroup.ItemGrpPrefix.Trim());
                foreach (var i in tbtPickItem)
                {
                    i.FlagPick = true;
                }
                db.SaveChanges();

                return Ok(new updatePickConfirmResponse { PickNo = updatePickConfirmRequest.PickNo });


            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult> printPickItemByGroup([FromBody]  printPickItemByGroup printPickItemByGroup)
        {

            try
            {
                var iGroup = await db.TbmItemGroup.Where(w => w.ItemGrpCode == printPickItemByGroup.ItemGrpCode).FirstOrDefaultAsync();
                var data = await (from PItem in db.TbtPickItem
                                  join O in db.TbtOrder on new { PItem.ItemCode, PItem.DocNum } equals new { O.ItemCode, O.DocNum } into gj
                                  from x in gj.DefaultIfEmpty()
                                  where PItem.PickNo == printPickItemByGroup.PickNo && PItem.ItemCode.Trim().Substring(0, 1) == iGroup.ItemGrpPrefix.Trim()
                                  select new { x.DocDueDate, x.BinCode, x.Dscription, PItem.ItemCode, x.Quantity,x.Price, x.Isbn, PItem.FlagPick }).ToListAsync();
                PickDocumentModel pickDocumentModel = new PickDocumentModel();
                if (data != null)
                {

                    pickDocumentModel.ItemGrpName = iGroup.ItemGrpName;
                    pickDocumentModel.ItemGrpCode = iGroup.ItemGrpCode;
                    pickDocumentModel.PickNo = printPickItemByGroup.PickNo;
                    pickDocumentModel.DocDueDate = data.FirstOrDefault().DocDueDate;

                    pickDocumentModel.selectPickItems = data.Select(i => new selectPickItem
                    {
                        BinCode = i.BinCode,
                        Dscription = i.Dscription,
                        Isbn = i.Isbn,
                        ItemCode = i.ItemCode,
                        Quantity = (int)i.Quantity,
                        Price =i.Price == null ?0.00:(double) i.Price,
                        FlagPick = i.FlagPick == null ? false : (bool)i.FlagPick

                    }).OrderBy(x => x.BinCode).ToList();
                }
                PickDocument pDoc = new PickDocument(this._hostingEnvironment);
            
                Stream resultPDFStream = pDoc.CreatePDF(pickDocumentModel);
                resultPDFStream.Position = 0;
                if (resultPDFStream.Length != 0)
                {
                    FileStreamResult fileStreamResult = new FileStreamResult(resultPDFStream, "application/pdf");
                    fileStreamResult.FileDownloadName = "pick_"+ printPickItemByGroup.PickNo+".pdf";


                    return fileStreamResult;
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
