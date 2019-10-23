using PackingApi.PDF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.AspNetCore.Hosting;


namespace PackingApi.Models.PDF.Process
{
    public class OutOfStockDocument
    {
        private IHostingEnvironment _hostingEnvironment;

        Document _document;
        string[] headers;
        BaseFont _bf;
        public OutOfStockDocument(IHostingEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment;
            Config config = new Config(this._hostingEnvironment);
            this._document = config.setPageSize();
            this._bf = config.getTHSarabun();
        }

        public Stream CreatePDF(OutOfStockDocumentModel outOfStockDocumentModel)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            Document document = this._document;
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            float[] widthBoxs = new float[] { 15f, 20f, 20f, 15f, 15f, 15f };
            PdfPTable tableTitle = new PdfPTable(widthBoxs.Count());
            tableTitle.SetWidths(widthBoxs);

            tableTitle.WidthPercentage = 95f;
            tableTitle.AddCell(bulidColumnTitleBox(@"ใบแจ้งรายการสินค้าขาดคราว", 20, 6, Element.ALIGN_CENTER, 0));
            tableTitle.AddCell(bulidEmptryRows(widthBoxs.Count()));

            tableTitle.AddCell(bulidColumnTitleBox(@"ลงวันที่ :", 16, 1, Element.ALIGN_LEFT, 0));
            tableTitle.AddCell(bulidColumnTitleBox(outOfStockDocumentModel.DocDueDate == null ? "" : outOfStockDocumentModel.DocDueDate.Value.ToString("dd/MM/yyyy"), 14, 2, Element.ALIGN_LEFT, 0));


            tableTitle.AddCell(bulidColumnTitleBox(@"พนักงานขาย :", 16, 1, Element.ALIGN_LEFT, 0));
            tableTitle.AddCell(bulidColumnTitleBox("", 14, 2, Element.ALIGN_LEFT, 0));


            tableTitle.AddCell(bulidColumnTitleBox(@"ชื่อลูกค้า :", 16, 1, Element.ALIGN_LEFT, 0));
            tableTitle.AddCell(bulidColumnTitleBox(outOfStockDocumentModel.CardName, 14, 2, Element.ALIGN_LEFT, 0));

            tableTitle.AddCell(bulidColumnTitleBox(@"เลขที่ใบสั่งขาย :", 16, 1, Element.ALIGN_LEFT, 0));
            tableTitle.AddCell(bulidColumnTitleBox("", 14, 2, Element.ALIGN_LEFT, 0));

            tableTitle.AddCell(bulidColumnTitleBox(@"ที่อยู่ :", 16, 1, Element.ALIGN_LEFT, 0));
            tableTitle.AddCell(bulidColumnTitleBox(outOfStockDocumentModel.Address, 14, 5, Element.ALIGN_LEFT, 0));



            tableTitle.AddCell(bulidColumnTitleBox(@"สถานที่ส่งของ :", 16, 1, Element.ALIGN_LEFT, 0));
            tableTitle.AddCell(bulidColumnTitleBox(outOfStockDocumentModel.Address, 14, 5, Element.ALIGN_LEFT, 0));

            tableTitle.AddCell(bulidColumnTitleBox(@"หมายเหตุ :", 16, 1, Element.ALIGN_LEFT, 0));
            tableTitle.AddCell(bulidColumnTitleBox(outOfStockDocumentModel.Remark, 14, 5, Element.ALIGN_LEFT, 0));
            tableTitle.AddCell(bulidEmptryRows(widthBoxs.Count()));
            document.Add(tableTitle);
            string[] headers = new string[] { "No", "รหัส", "รายการ", "ค่าส่ง", "ราคา", "จำนวนเงิน" };
            float[] withheaders = new float[] { 5f, 20f, 45f, 10f, 10f, 10f };
            PdfPTable tableDetail = new PdfPTable(headers.Count());
            tableDetail.SetWidths(withheaders);
            tableDetail.WidthPercentage = 95f;
            tableDetail.HeaderRows = 1;

            foreach (string h in headers)
            {
                tableDetail.AddCell(bulidColumnHeader(h, 16));
            }

            foreach (var group in outOfStockDocumentModel.outOfStocktems.Select(x => new { x.ItemGrpCode, x.ItemGrpName }).Distinct())
            {
                tableDetail.AddCell(bulidColumnDetail(@"ประเภทสินค้า : " + group.ItemGrpCode + "-" + group.ItemGrpName, 14, 6, Element.ALIGN_CENTER));
                int i = 1;
                foreach (var item in outOfStockDocumentModel.outOfStocktems.Where(x => x.ItemGrpCode == group.ItemGrpCode))
                {
                    tableDetail.AddCell(bulidColumnDetail(i.ToString(), 14, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail(item.ItemCode, 14, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail(item.Dscription, 14, 1, Element.ALIGN_LEFT));
                    tableDetail.AddCell(bulidColumnDetail((item.Qty).ToString(), 14, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail((item.Price).ToString(), 14, 1, Element.ALIGN_CENTER));
                    tableDetail.AddCell(bulidColumnDetail((item.Price * item.Qty).ToString(), 14, 1, Element.ALIGN_CENTER));
                    i++;
                }

            }
            tableDetail.AddCell(bulidEmptryRows(6));
            document.Add(tableDetail);
            float[] withfooster = new float[] { 15f, 85f };
            PdfPTable tableFooster = new PdfPTable(withfooster.Count());
            tableFooster.SetWidths(withfooster);
            tableFooster.WidthPercentage = 95f;
            tableFooster.AddCell(bulidColumnFooster("หมายเหตุ :", 14, 1));
            tableFooster.AddCell(bulidColumnFooster("สินค้าข้างต้นขาดคราว บริษัทจะจัดส่งให้เมื่อพิมพ์เรียบร้อยแล้ว หากมีข้อสงสัย หรือต้องการยกเลิกรายการขาดคราว", 14, 1));
            tableFooster.AddCell(bulidColumnFooster("", 14, 1));
            tableFooster.AddCell(bulidColumnFooster("โปรดติดต่อแผนกลูกค้าสัมพัรธ์ โทร 0-2512-0661 ต่อ 2511-15ม2521-25 ภายใน 15 วัน", 14, 1));
            document.Add(tableFooster);

            document.Close();
            return memoryStream;
        }
        private PdfPCell bulidEmptryRows(int Col)
        {
            PdfPCell cell = new PdfPCell();
            cell.Colspan = Col;
            cell.Border = 0;
            cell.FixedHeight = 20f;
            return cell;
        }
        private PdfPCell bulidColumnTitleBox(string str, int size, int Colspan, int Align, int border)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = Colspan;
            cell.Border = border;
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.HorizontalAlignment = Align;
            return cell;
        }
        private PdfPCell bulidColumnFooster(string str, int size, int Colspan)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = Colspan;
            cell.Border = 0;
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            return cell;
        }

        private PdfPCell bulidColumnDetail(string str, int size, int col, int Align)
        {

            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);


            PdfPCell cell = new PdfPCell(pharse);
            cell.Colspan = col;
            cell.Border = 0;
            cell.HorizontalAlignment = Align;
            return cell;
        }
        private PdfPCell bulidColumnHeader(string str, int size)
        {
            Font font = new Font(this._bf, size);
            Phrase pharse = new Phrase(str, font);

            PdfPCell cell = new PdfPCell(pharse);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.FixedHeight = 25f;
            cell.Border = Rectangle.BOTTOM_BORDER;
            return cell;
        }

    }

}
