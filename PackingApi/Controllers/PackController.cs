using GreatFriends.ThaiBahtText;
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        private IHostingEnvironment _hostingEnvironment;
        public PackController(PackingDBContext packingDBContext, IHostingEnvironment environment)
        {
            this.db = packingDBContext;
            this._hostingEnvironment = environment;
        }

        [HttpPost]
        public async Task<ActionResult> selectPickForPack([FromBody]  selectPickForPackRequest selectPickForPackRequest)
        {

            try
            {
                var data = await (from o in db.TbtOrder
                                  join p in db.TbtPickItem on new { o.ItemCode, o.DocNum } equals new { p.ItemCode, p.DocNum }
                                  join pack in db.TbtPackItem on o.ItemCode equals pack.ItemCode into gpack
                                  where p.FlagPick == true && gpack.Count() == 0 &&
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
                selectPackListForConfirmResponse response = new selectPackListForConfirmResponse();
                var packlist = await (from p in db.TbtPackItem
                                      join o in db.TbtOrder on new { p.ItemCode, p.DocNum } equals new { o.ItemCode, o.DocNum }
                                      where p.PackNo == selectPackListForConfirmRequest.PackNo
                                      select new PackListForConfirm
                                      {
                                          ItemCode = p.ItemCode,
                                          DocNum = p.DocNum,
                                          Dscription = o.Dscription,
                                          Quantity = o.Quantity,
                                          Isbn = o.Isbn,
                                          IsbnRecheck = p.IsbnRecheck,
                                          Unit = p.Unit,
                                          Package = p.Package
                                      }
                                       )
                                       .Skip((selectPackListForConfirmRequest.page - 1) * selectPackListForConfirmRequest.size).Take(selectPackListForConfirmRequest.size).ToListAsync();
                response.PackListForConfirms = packlist;
                response.PackNo = selectPackListForConfirmRequest.PackNo;
                var tbtpack = db.TbtPack.Where(x => x.PackNo == selectPackListForConfirmRequest.PackNo).FirstOrDefault();
                if (tbtpack != null)
                {
                    response.Package = tbtpack.Package;
                    response.Unit = tbtpack.Unit;
                }

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
        public async Task<ActionResult> updatePackConfirm([FromBody] updatePackConfirmRequest updatePackConfirmRequest)
        {

            try
            {
                var tbtpack = db.TbtPack.Where(x => x.PackNo == updatePackConfirmRequest.PackNo).FirstOrDefault();
                if (tbtpack != null)
                {
                    tbtpack.Unit = updatePackConfirmRequest.Unit;
                    tbtpack.Package = updatePackConfirmRequest.Package;
                    tbtpack.UpdateDate = DateTime.Now;
                    tbtpack.UpdateUser = updatePackConfirmRequest.UserId;
                }
                else
                {
                    TbtPack tbtPack = new TbtPack
                    {
                        Package = updatePackConfirmRequest.Package,
                        PackNo = updatePackConfirmRequest.PackNo,
                        Unit = updatePackConfirmRequest.Unit,
                        CreateUser = updatePackConfirmRequest.UserId,
                        CreateDate = DateTime.Now
                    };
                    db.TbtPack.Add(tbtPack);
                }

                var packs = await (from P in db.TbtPackItem
                                   where P.PackNo == updatePackConfirmRequest.PackNo
                                   select P).ToListAsync();

                if (updatePackConfirmRequest.Package != "")
                {
                    packs.ForEach(x =>
                    {
                        x.Package = "";
                        x.Unit = null;
                    });
                }
                packs.ForEach(x =>
                {
                    x.UpdateDate = DateTime.Now;
                    x.UpdateUser = updatePackConfirmRequest.UserId;
                    x.FlagPack = true;
                });

                db.UpdateRange(packs);

                db.SaveChanges();
                return Ok(new { PackNo = updatePackConfirmRequest.PackNo });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }

        [HttpPost]
        public async Task<ActionResult> updatePackRecheckIsbn([FromBody] updatePackRecheckIsbnRequest updatePackRecheckIsbnRequest)
        {
            try
            {
                var packitem = await (from p in db.TbtPackItem
                                      where p.PackNo == updatePackRecheckIsbnRequest.PackNo &&
                            p.ItemCode == updatePackRecheckIsbnRequest.ItemCode &&
                            p.DocNum == updatePackRecheckIsbnRequest.DocNum
                                      select p).FirstOrDefaultAsync();
                if (updatePackRecheckIsbnRequest.IsbnRecheck != null) { packitem.IsbnRecheck = updatePackRecheckIsbnRequest.IsbnRecheck; }


                if (updatePackRecheckIsbnRequest.Unit != null) { packitem.Unit = updatePackRecheckIsbnRequest.Unit; }


                if (updatePackRecheckIsbnRequest.Package != null) { packitem.Package = updatePackRecheckIsbnRequest.Package; }
                packitem.UpdateUser = updatePackRecheckIsbnRequest.UserId;

                db.Update(packitem);
                db.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }
        [HttpPost]
        public async Task<ActionResult> selectDocumentPrintingList([FromBody] selectDocumentPrintingListRequest selectDocumentPrintingListRequest)
        {
            try
            {
                var data = await (from o in db.TbtOrder
                                  join p in db.TbtPackItem on new { o.ItemCode, o.DocNum } equals new { p.ItemCode, p.DocNum }
                                  where p.FlagPack == true &&
                                   (String.IsNullOrEmpty(selectDocumentPrintingListRequest.PackNo) || p.PackNo == selectDocumentPrintingListRequest.PackNo) &&
                                  (String.IsNullOrEmpty(selectDocumentPrintingListRequest.DocNum) || o.DocNum == selectDocumentPrintingListRequest.DocNum) &&
                                  (String.IsNullOrEmpty(selectDocumentPrintingListRequest.CardName) || o.CardName.Contains(selectDocumentPrintingListRequest.CardName)) &&
                                  (String.IsNullOrEmpty(selectDocumentPrintingListRequest.County) || o.County == selectDocumentPrintingListRequest.County) &&
                                  (String.IsNullOrEmpty(selectDocumentPrintingListRequest.Region) || o.Descript == selectDocumentPrintingListRequest.Region)
                                  select new { o, p }).ToListAsync();
                if (selectDocumentPrintingListRequest.StartDocDueDate != null)
                {
                    data = data.Where(i => i.o.DocDueDate >= selectDocumentPrintingListRequest.StartDocDueDate).ToList();
                }
                if (selectDocumentPrintingListRequest.EndDocDueDate != null)
                {
                    data = data.Where(i => i.o.DocDueDate <= selectDocumentPrintingListRequest.EndDocDueDate).ToList();
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
                                    x.o.Remark
                                } into g
                                orderby g.Key.DocDueDate
                                select new selectDocumentPrintingListResponse
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
                                    Price = g.Sum(y => y.o.Price * y.o.Quantity)
                                }).Skip((selectDocumentPrintingListRequest.page - 1) * selectDocumentPrintingListRequest.size).Take(selectDocumentPrintingListRequest.size).ToList();


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
        public async Task<ActionResult> printLabel([FromBody]  printLabelRequest printLabelRequest)
        {

            try
            {

                var items = await (from o in db.TbtOrder
                                   where printLabelRequest.DocNums.Contains(o.DocNum)
                                   select o).ToListAsync();
                List<LabelItem> labelItems = new List<LabelItem>();
                items.ForEach(x =>
                {
                    LabelItem l = new LabelItem
                    {
                        Dscription = x.Dscription,
                        Quantity = x.Quantity,
                    };
                    labelItems.Add(l);
                });
                LabelDocumentModel labelDocumentModel = new LabelDocumentModel
                {
                    Address = items.FirstOrDefault().Address,
                    CardName = items.FirstOrDefault().CardName,
                    DocDate = items.FirstOrDefault().DocDate,
                    DocDueDate = items.FirstOrDefault().DocDueDate,
                    DocNum = string.Join(",", printLabelRequest.DocNums.ToArray()),
                    labelItems = labelItems,
                    Remark = items.FirstOrDefault().Remark
                };


                LabelDocument pDoc = new LabelDocument(this._hostingEnvironment);

                Stream resultPDFStream = pDoc.CreatePDF(labelDocumentModel);
                resultPDFStream.Position = 0;
                if (resultPDFStream.Length != 0)
                {
                    FileStreamResult fileStreamResult = new FileStreamResult(resultPDFStream, "application/pdf");
                    fileStreamResult.FileDownloadName = "label_" + "" + ".pdf";


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
        [HttpPost]
        public async Task<ActionResult> printOutOfStock([FromBody]  printOutOfStockRequest printOutOfStockRequest)
        {

            try
            {

                var items = await (from o in db.TbtOrder
                                   where o.DocNum == printOutOfStockRequest.DocNum
                                   join g in db.TbmItemGroup on o.ItemCode.Trim().Substring(0, 1) equals g.ItemGrpPrefix.Trim()
                                   select new { o, g }).ToListAsync();
                List<OutOfStock> outOfStocktems = new List<OutOfStock>();
                items.ForEach(x =>
                {
                    OutOfStock l = new OutOfStock
                    {
                        Dscription = x.o.Dscription,
                        ItemCode = x.o.ItemCode,
                        Price = (double)x.o.Price,
                        Qty = (double)x.o.Quantity,
                        ItemGrpCode = x.g.ItemGrpCode,
                        ItemGrpName = x.g.ItemGrpName

                    };
                    outOfStocktems.Add(l);
                });
                OutOfStockDocumentModel outOfStockDocumentModel = new OutOfStockDocumentModel
                {
                    Address = items.FirstOrDefault().o.Address,
                    CardName = items.FirstOrDefault().o.CardName,
                    DocDate = items.FirstOrDefault().o.DocDate,
                    DocDueDate = items.FirstOrDefault().o.DocDueDate,
                    DocNum = printOutOfStockRequest.DocNum,
                    outOfStocktems = outOfStocktems,
                    Remark = items.FirstOrDefault().o.Remark,
                };


                OutOfStockDocument pDoc = new OutOfStockDocument(this._hostingEnvironment);

                Stream resultPDFStream = pDoc.CreatePDF(outOfStockDocumentModel);
                resultPDFStream.Position = 0;
                if (resultPDFStream.Length != 0)
                {
                    FileStreamResult fileStreamResult = new FileStreamResult(resultPDFStream, "application/pdf");
                    fileStreamResult.FileDownloadName = "OutOfStock_" + "" + ".pdf";


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
        [HttpPost]
        public ActionResult printInvoice()
        {
            try
            {
                decimal amount = 121.50M;
                string s = amount.ThaiBahtText();

                var invoice = (from i in db.TbtOrder
                               where i.DocNum == "1519010001"
                               select i).FirstOrDefault();

                string path = _hostingEnvironment.ContentRootPath + "\\Pdf_template\\01.1.12 HR02 HR08 101_Foxit.pdf";
                Stream pdfInputStream = new FileStream(path: path, mode: FileMode.Open);

                FillOutPdf fillOutPdf = new FillOutPdf(_hostingEnvironment.ContentRootPath);


                var data = new Models.PDF.Invoice
                {
                    CardCode = invoice.CardCode.ToString() + " text Foxit Free",
                    CardName = invoice.CardName + "  ทดลองภาษาไทย",
                    Docnum = invoice.DocNum
                };
                Stream resultPDFStream = fillOutPdf.FillForm(pdfInputStream, data);
                resultPDFStream.Position = 0;
                //Download the PDF document in the browser.


                if (resultPDFStream.Length != 0)
                {
                    FileStreamResult fileStreamResult = new FileStreamResult(resultPDFStream, "application/pdf");
                    fileStreamResult.FileDownloadName = "packing_invoice_foxit.pdf";


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

        [HttpPost]
        public async Task<ActionResult> selectPackageStock(selectPackageStock selectPackageStock)
        {
            try
            {
                var response = await (from p in db.TbmPackage
                                      where p.Active == true && p.Qty > 0
                                      select new TbmPackage
                                      {
                                          Active = p.Active,
                                          CreateDate = p.CreateDate,
                                          CreateUser = p.CreateUser,
                                          PackageId = p.PackageId,
                                          PackageName = p.PackageName,
                                          Qty = p.Qty == null ? 0 : p.Qty,
                                          UpdateDate = p.UpdateDate,
                                          UpdateUser = p.UpdateUser
                                      }
                                      ).Skip((selectPackageStock.page - 1) * selectPackageStock.size).Take(selectPackageStock.size).ToListAsync();
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
        public ActionResult updatePackageStock([FromBody] updatePackageStockRequest updatePackageStockRequest)
        {

            try
            {

                if (updatePackageStockRequest.flagNew)
                {
                    TbmPackage addtbmPackage = new TbmPackage
                    {

                        Active = updatePackageStockRequest.Active,
                        PackageName = updatePackageStockRequest.PackageName,
                        Qty = updatePackageStockRequest.Qty,
                        CreateDate = DateTime.Now,
                        CreateUser = updatePackageStockRequest.UpdateUser
                    };
                    db.TbmPackage.AddRange(addtbmPackage);
                }
                else
                {
                    var tbmPackage = (from p in db.TbmPackage
                                      where p.PackageId == updatePackageStockRequest.PackageID
                                      select p).FirstOrDefault();
                    tbmPackage.Active = updatePackageStockRequest.Active;
                    tbmPackage.PackageId = updatePackageStockRequest.PackageID;
                    tbmPackage.PackageName = updatePackageStockRequest.PackageName;
                    tbmPackage.Qty = updatePackageStockRequest.Qty;
                    tbmPackage.UpdateDate = DateTime.Now;
                    tbmPackage.UpdateUser = updatePackageStockRequest.UpdateUser;

                    db.TbmPackage.UpdateRange(tbmPackage);
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